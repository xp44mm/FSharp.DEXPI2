namespace DEXPI2

open System
open System.Data.SQLite
open System.IO
open System.Text
/// <summary>
/// PlantModel 拓扑库的建库、保存与加载。
/// 表结构见 schema.sql；本模块内嵌同一 DDL，避免运行时依赖外部文件。
/// 与 DEXPI2.Entities 项目的参数库无关。
/// </summary>
module PlantModelDb =
    /// schema.sql 路径：优先从程序集所在目录读取（发布后），否则回退到源码目录。
    let private schemaPath =
        let asmDir = IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        let candidate = IO.Path.Combine(asmDir, "schema.sql")
        if IO.File.Exists candidate then candidate
        else Path.Combine(__SOURCE_DIRECTORY__, "schema.sql")

    /// 建库 DDL：从 schema.sql 读取（UTF-8 BOM）。
    let private schemaSql = IO.File.ReadAllText(schemaPath, UTF8Encoding(true))

    /// <summary>
    /// 新建拓扑库：打开（不存在则创建）SQLite 文件并建表。
    /// 调用方负责在结束时关闭连接。
    /// </summary>
    let createDatabase (path: string) : SQLiteConnection =
        let dir = IO.Path.GetDirectoryName path
        if not (String.IsNullOrEmpty dir) && not (IO.Directory.Exists dir) then
            IO.Directory.CreateDirectory dir |> ignore
        // 全新建库：已存在则删除（占用中则忽略，后续建表会报错）
        if IO.File.Exists path then
            try IO.File.Delete path with _ -> ()
        let conn = new SQLiteConnection(sprintf "Data Source=%s;Version=3;" path)
        try
            conn.Open()
            use pragma = conn.CreateCommand()
            pragma.CommandText <- "PRAGMA foreign_keys = ON;"
            pragma.ExecuteNonQuery() |> ignore
            use cmd = conn.CreateCommand()
            cmd.CommandText <- schemaSql
            cmd.ExecuteNonQuery() |> ignore
            conn
        with e ->
            conn.Dispose()
            reraise()

    /// 保存 PlantModel：清空业务表后按外键顺序重插。
    let saveModel (conn: SQLiteConnection) (model: PlantModel) : unit =
        let delete cmdText =
            use c = conn.CreateCommand()
            c.CommandText <- cmdText
            c.ExecuteNonQuery() |> ignore
        delete "DELETE FROM SegmentItem;"
        delete "DELETE FROM Segment;"
        delete "DELETE FROM Nozzle;"
        delete "DELETE FROM PipingNetworkSystem;"
        delete "DELETE FROM Equipment;"

        // Equipment + Nozzle
        for eq in model.ProcessEquipments do
            let tag, kind = ProcessEquipment.toSqlite eq
            use cEq = conn.CreateCommand()
            cEq.CommandText <- "INSERT INTO Equipment(tag, kind) VALUES(@tag, @kind);"
            cEq.Parameters.AddWithValue("@tag", tag) |> ignore
            cEq.Parameters.AddWithValue("@kind", kind) |> ignore
            cEq.ExecuteNonQuery() |> ignore

            let _, nozzles =
                let _, fields = Microsoft.FSharp.Reflection.FSharpValue.GetUnionFields(eq, typeof<ProcessEquipment>)
                fields.[0] :?> string, fields.[1] :?> string list
            for i, nz in List.indexed nozzles do
                use cNz = conn.CreateCommand()
                cNz.CommandText <- "INSERT INTO Nozzle(equipment_tag, tag, position) VALUES(@eq, @nz, @pos);"
                cNz.Parameters.AddWithValue("@eq", tag) |> ignore
                cNz.Parameters.AddWithValue("@nz", nz) |> ignore
                cNz.Parameters.AddWithValue("@pos", i) |> ignore
                cNz.ExecuteNonQuery() |> ignore

        // PipingNetworkSystem + Segment + SegmentItem
        for sys in model.PipingNetworkSystems do
            let lineNumber = PipingNetworkSystem.toSqlite sys
            use cSys = conn.CreateCommand()
            cSys.CommandText <- "INSERT INTO PipingNetworkSystem(line_number) VALUES(@ln);"
            cSys.Parameters.AddWithValue("@ln", lineNumber) |> ignore
            cSys.ExecuteNonQuery() |> ignore

            for seg in sys.Segments do
                let segNum, kind = PipingNetworkSegment.toSqlite seg
                use cSeg = conn.CreateCommand()
                cSeg.CommandText <- "INSERT INTO Segment(line_number, segment_number, kind) VALUES(@ln, @sn, @kind);"
                cSeg.Parameters.AddWithValue("@ln", lineNumber) |> ignore
                cSeg.Parameters.AddWithValue("@sn", segNum) |> ignore
                cSeg.Parameters.AddWithValue("@kind", kind) |> ignore
                cSeg.ExecuteNonQuery() |> ignore

                let items =
                    match seg with
                    | Segment(_, its) -> its
                    | HubSegment(_) -> []
                for i, node in List.indexed items do
                    let nodeKind, nodeTag = PipingNodeOwner.toSqlite node
                    use cIt = conn.CreateCommand()
                    cIt.CommandText <- "INSERT INTO SegmentItem(line_number, segment_number, position, node_kind, tag) VALUES(@ln, @sn, @pos, @nk, @tag);"
                    cIt.Parameters.AddWithValue("@ln", lineNumber) |> ignore
                    cIt.Parameters.AddWithValue("@sn", segNum) |> ignore
                    cIt.Parameters.AddWithValue("@pos", i) |> ignore
                    cIt.Parameters.AddWithValue("@nk", nodeKind) |> ignore
                    if isNull nodeTag then
                        cIt.Parameters.AddWithValue("@tag", DBNull.Value) |> ignore
                    else
                        cIt.Parameters.AddWithValue("@tag", nodeTag) |> ignore
                    cIt.ExecuteNonQuery() |> ignore

    /// 从库读回 PlantModel。
    let loadModel (conn: SQLiteConnection) : PlantModel =
        // 读 Nozzle：(equipment_tag, nozzle, position)
        let nozzleRows =
            use c = conn.CreateCommand()
            c.CommandText <- "SELECT equipment_tag, tag, position FROM Nozzle ORDER BY equipment_tag, position;"
            use r = c.ExecuteReader()
            let acc = System.Collections.Generic.List<string * string * int>()
            while r.Read() do
                acc.Add(r.GetString 0, r.GetString 1, r.GetInt32 2)
            acc |> List.ofSeq
        let nozzlesByEq =
            nozzleRows
            |> List.groupBy (fun (eq, _, _) -> eq)
            |> List.map (fun (eq, rows) -> eq, rows |> List.sortBy (fun (_, _, p) -> p) |> List.map (fun (_, nz, _) -> nz))
            |> Map.ofList

        // 读 Equipment
        let equipments =
            use c = conn.CreateCommand()
            c.CommandText <- "SELECT tag, kind FROM Equipment ORDER BY id;"
            use r = c.ExecuteReader()
            let acc = System.Collections.Generic.List<ProcessEquipment>()
            while r.Read() do
                let tag = r.GetString 0
                let kind = r.GetString 1
                let nozzles = match nozzlesByEq.TryFind tag with Some ns -> ns | None -> []
                acc.Add(ProcessEquipment.fromSqlite tag kind nozzles)
            acc |> List.ofSeq

        // 读 SegmentItem：(line, segment, position, node_kind, tag)
        let itemRows =
            use c = conn.CreateCommand()
            c.CommandText <- "SELECT line_number, segment_number, position, node_kind, tag FROM SegmentItem ORDER BY line_number, segment_number, position;"
            use r = c.ExecuteReader()
            let acc = System.Collections.Generic.List<string * string * int * string * string>()
            while r.Read() do
                let tag = if r.IsDBNull 4 then null else r.GetString 4
                acc.Add(r.GetString 0, r.GetString 1, r.GetInt32 2, r.GetString 3, tag)
            acc |> List.ofSeq
        let itemsBySeg =
            itemRows
            |> List.groupBy (fun (ln, sn, _, _, _) -> ln, sn)
            |> List.map (fun ((ln, sn), rows) ->
                (ln, sn),
                rows |> List.sortBy (fun (_, _, p, _, _) -> p)
                     |> List.map (fun (_, _, _, nk, tag) -> PipingNodeOwner.fromSqlite nk tag))
            |> Map.ofList

        // 读 Segment：(line, segment, kind)
        let segRows =
            use c = conn.CreateCommand()
            c.CommandText <- "SELECT line_number, segment_number, kind FROM Segment ORDER BY line_number, id;"
            use r = c.ExecuteReader()
            let acc = System.Collections.Generic.List<string * string * string>()
            while r.Read() do
                acc.Add(r.GetString 0, r.GetString 1, r.GetString 2)
            acc |> List.ofSeq
        let segsByLine =
            segRows
            |> List.groupBy (fun (ln, _, _) -> ln)
            |> List.map (fun (ln, rows) ->
                ln,
                rows |> List.map (fun (_, sn, kind) ->
                    let items = match itemsBySeg.TryFind (ln, sn) with Some its -> its | None -> []
                    PipingNetworkSegment.fromSqlite sn kind items))
            |> Map.ofList

        // 读 PipingNetworkSystem
        let systems =
            use c = conn.CreateCommand()
            c.CommandText <- "SELECT line_number FROM PipingNetworkSystem ORDER BY id;"
            use r = c.ExecuteReader()
            let acc = System.Collections.Generic.List<PipingNetworkSystem>()
            while r.Read() do
                let ln = r.GetString 0
                let segs = match segsByLine.TryFind ln with Some ss -> ss | None -> []
                acc.Add(PipingNetworkSystem.fromSqlite ln segs)
            acc |> List.ofSeq

        { ProcessEquipments = equipments
          PipingNetworkSystems = systems }
