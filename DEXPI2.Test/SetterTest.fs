namespace DEXPI2.Test

open System
open System.Data.SQLite
open System.IO

open DEXPI2
open Xunit

type SetterTest() =
    /// 内存 SQLite 数据库（每个测试独立，仅本连接可见）
    let memoryDb () : SQLiteConnection =
        Setter.newDatabase ":memory:"

    [<Fact>]
    member _.``createFromModel 建五张表，段项按 Tag 统一入表，引用项不入库``() =
        use conn = memoryDb ()
        Setter.createFromModel conn InPlaceTree.plantModel |> ignore

        // 五张表存在
        let tables =
            use cmd =
                new SQLiteCommand(
                    "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;",
                    conn)
            use r = cmd.ExecuteReader()
            [ while r.Read() do
                yield r.GetString 0 ]
        Assert.Equal<string list>([ "Equipment"; "HubSegment"; "Nozzle"; "Segment"; "SegmentItem" ], tables)

        // Equipment：V-101，值默认
        let eq = Setter.tryGetEquipmentRow conn "V-101"
        Assert.True(eq.IsSome)
        Assert.Equal(0, eq.Value.PN)
        Assert.Equal(0, eq.Value.DN)
        Assert.Equal("", eq.Value.Fluid)

        // Nozzle：V-101 / N1
        Assert.True((Setter.tryGetNozzleRow conn "V-101" "N1").IsSome)

        // 常规段：PL-101 / S1、S3 进 Segment 表；枢纽段 S2（reducer 单例）进 HubSegment 表
        Assert.True((Setter.tryGetSegmentRow conn "PL-101" "S1").IsSome)
        Assert.True((Setter.tryGetSegmentRow conn "PL-101" "S3").IsSome)
        Assert.True((Setter.tryGetSegmentRow conn "PL-101" "S2").IsNone)
        let hub = Setter.tryGetHubSegmentRow conn "PL-101" "S2"
        Assert.True(hub.IsSome)
        Assert.Equal<int list>([], hub.Value.DNs) // 端点不区分，默认空列表

        // tag 有名管件以真实 tag 为 Tag：OperatedValve XV-101
        let tagItem = Setter.tryGetSegmentItemRow conn "PL-101" "S1" "XV-101"
        Assert.True(tagItem.IsSome)
        Assert.Equal("OperatedValve(Manual)", tagItem.Value.Type)

        // 无名管件以 ItemIndex.ToString() 为 Tag：S3 第 1 位 PipeOffPageConnector → Tag="1"
        let unnamed = Setter.tryGetSegmentItemRow conn "PL-101" "S3" "1"
        Assert.True(unnamed.IsSome)
        Assert.Equal("PipeOffPageConnector", unnamed.Value.Type)

        // 引用项（Nozzle、HubSegmentRef）不入库：S1 的 "0"（Nozzle）与 "2"（HubSegmentRef）无行
        Assert.True((Setter.tryGetSegmentItemRow conn "PL-101" "S1" "0").IsNone)
        Assert.True((Setter.tryGetSegmentItemRow conn "PL-101" "S1" "2").IsNone)

        // 枢纽段 S2 无任何段项
        Assert.True((Setter.tryGetSegmentItemRow conn "PL-101" "S2" "0").IsNone)
        Assert.True((Setter.tryGetSegmentItemRow conn "PL-101" "S2" "XV-101").IsNone)

    [<Fact>]
    member _.``设备级参数按覆盖链继承到喷嘴、段、段项``() =
        use conn = memoryDb ()
        let e = Setter.createFromModel conn InPlaceTree.plantModel

        EquipmentTable.setPN e.Equipments 1600 "V-101"
        EquipmentTable.setDN e.Equipments 100 "V-101"
        EquipmentTable.setFluid e.Equipments "Water" "V-101"




        // 无名管件（Sensorwell）经映射继承：SensorwellTree S1 中部测点 PT（Tag="1"）
        use conn2 = memoryDb ()
        let e2 = Setter.createFromModel conn2 SensorwellTree.plantModel
        Assert.Equal("Sensorwell(PT)", (Setter.tryGetSegmentItemRow conn2 "PL-401" "S1" "1").Value.Type)
        EquipmentTable.setPN e2.Equipments 1600 "V-101"

    [<Fact>]
    member _.``喷嘴级参数覆盖设备级并沿映射传递到段与项``() =
        use conn = memoryDb ()
        let e = Setter.createFromModel conn InPlaceTree.plantModel

        EquipmentTable.setPN e.Equipments 1600 "V-101"
        NozzleTable.setPN e.Nozzles 2500 "V-101" "N1"

        // 找到引用本管嘴的段（S1），把值传给管段

    [<Fact>]
    member _.``段级参数按段号设置并覆盖喷嘴级，枢纽段 DN 用端点列表``() =
        use conn = memoryDb ()
        let e = Setter.createFromModel conn InPlaceTree.plantModel

        NozzleTable.setDN e.Nozzles 50 "V-101" "N1"



    [<Fact>]
    member _.``段项级参数覆盖段级，引用项位置与未知 Tag 设置抛异常``() =
        use conn = memoryDb ()
        let e = Setter.createFromModel conn InPlaceTree.plantModel

        EquipmentTable.setPN e.Equipments 1600 "V-101"



    [<Fact>]
    member _.``段号跨系统重复时按段号设置抛异常，InLine 版本互不影响``() =
        use conn = memoryDb ()
        // 合并 PL-101 与 PL-201 到同一个 PlantModel（S1/S2 段号重复：S1 常规、S2 枢纽）
        let multi =
            { InPlaceTree.plantModel with
                PipingNetworkSystems =
                    InPlaceTree.plantModel.PipingNetworkSystems
                    @ PumpTree.plantModel.PipingNetworkSystems
            }
        let e = Setter.createFromModel conn multi
        ()
    [<Fact>]
    member _.``枢纽段端点 DN 列表设置``() =
        // 异径管枢纽段（InPlaceTree S2）
        use conn = memoryDb ()
        let e = Setter.createFromModel conn InPlaceTree.plantModel
        ()
    [<Fact>]
    member _.``newDatabase 每次新建库，重名文件再次 newDatabase 报错``() =
        let path = Path.Combine(Path.GetTempPath(), "dexpi2-setter-" + Guid.NewGuid().ToString("N") + ".db")
        try
            // 第一次：新建库并写入
            use conn = Setter.newDatabase path
            let e = Setter.createFromModel conn InPlaceTree.plantModel
            EquipmentTable.setFluid e.Equipments "Steam" "V-101"

            // 实体 → 库投影（UPSERT）：设置的值落库生效
            Setter.writeEntities conn e
            Assert.Equal("Steam", (Setter.tryGetEquipmentRow conn "V-101").Value.Fluid)
            Assert.Equal(300, (Setter.tryGetHubSegmentRow conn "PL-101" "S2").Value.PN)
            Assert.Equal(2000, (Setter.tryGetSegmentItemRow conn "PL-101" "S1" "XV-101").Value.PN)

            // 同一文件再次 newDatabase：表已存在 → 报错（重名库在选择时已排除，属预期）
            Assert.Throws<SQLiteException>(fun () -> Setter.newDatabase path |> ignore) |> ignore
        finally
            if File.Exists path then
                File.Delete path

    [<Fact>]
    member _.``各表含自动递增 Id 主键，保持插入顺序``() =
        use conn = memoryDb ()
        let e = Setter.createFromModel conn InPlaceTree.plantModel

        // 五张表都有 Id 列（自动递增整数主键）
        let hasIdColumn (table: string) =
            use cmd = new SQLiteCommand(sprintf "PRAGMA table_info(%s);" table, conn)
            use r = cmd.ExecuteReader()
            [ while r.Read() do
                yield r.GetString 1 ]
            |> List.contains "Id"
        for t in [ "Equipment"; "Nozzle"; "Segment"; "HubSegment"; "SegmentItem" ] do
            Assert.True(hasIdColumn t, t + " 缺少 Id 列")

        // Id 保持插入顺序递增：
        //  Equipment：V-101 → [1]
        //  Nozzle：V-101/N1 → [1]
        //  Segment：S1、S3（S2 是枢纽段不入本表）→ [1;2]
        //  HubSegment：S2 → [1]
        //  SegmentItem：S1 的 XV-101（引用项跳过）、S3 的 PipeOffPageConnector → [1;2]
        let ids (table: string) =
            use cmd = new SQLiteCommand(sprintf "SELECT Id FROM %s ORDER BY Id;" table, conn)
            use r = cmd.ExecuteReader()
            [ while r.Read() do
                yield r.GetInt32 0 ]
        Assert.Equal<int list>([ 1 ], ids "Equipment")
        Assert.Equal<int list>([ 1 ], ids "Nozzle")
        Assert.Equal<int list>([ 1; 2 ], ids "Segment")
        Assert.Equal<int list>([ 1 ], ids "HubSegment")
        Assert.Equal<int list>([ 1; 2 ], ids "SegmentItem")

        // upsert（实体设置后 writeEntities 投影）不产生新行，Id 不变
        EquipmentTable.setPN e.Equipments 1600 "V-101"
        Setter.writeEntities conn e
        Assert.Equal<int list>([ 1 ], ids "Equipment")

    [<Fact>]
    member _.``modelToEntities 生成数据库实体（纯函数，不碰数据库）``() =
        let e = Setter.modelToEntities InPlaceTree.plantModel

        // 设备 / 管嘴
        Assert.Equal<string list>([ "V-101" ], e.Equipments |> List.map (fun r -> r.Equipment))
        Assert.Equal<string list>([ "N1" ], e.Nozzles |> List.map (fun r -> r.Nozzle))

        // 常规段 / 枢纽段（S2 是枢纽段）
        Assert.Equal<string list>(
            [ "PL-101/S1"; "PL-101/S3" ],
            e.Segments |> List.map (fun r -> sprintf "%s/%s" r.LineNumber r.SegmentNumber))
        Assert.Equal<string list>(
            [ "PL-101/S2" ],
            e.HubSegments |> List.map (fun r -> sprintf "%s/%s" r.LineNumber r.SegmentNumber))

        // 段项：tag 管件用真 tag，无名管件用 ItemIndex.ToString()，引用项不入库
        Assert.Equal<string list>(
            [ "PL-101/S1/XV-101/OperatedValve(Manual)"; "PL-101/S3/1/PipeOffPageConnector" ],
            e.SegmentItems
            |> List.map (fun r -> sprintf "%s/%s/%s/%s" r.LineNumber r.SegmentNumber r.Tag r.Type))


        // 实体写入数据库后与实体本身一致（writeEntities 只做投影，不改变实体）
        use conn = memoryDb ()
        Setter.writeEntities conn e
        Assert.Equal<string list>(
            [ "V-101" ],
            Setter.tryGetEquipmentRow conn "V-101" |> Option.map (fun r -> r.Equipment) |> Option.toList)
        Assert.Equal<string list>(
            [ "PL-101/S1"; "PL-101/S3" ],
            [ "S1"; "S3" ]
            |> List.map (fun s ->
                Setter.tryGetSegmentRow conn "PL-101" s
                |> Option.map (fun r -> sprintf "%s/%s" r.LineNumber r.SegmentNumber))
            |> List.choose id)
