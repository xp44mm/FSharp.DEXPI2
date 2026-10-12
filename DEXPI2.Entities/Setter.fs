module DEXPI2.Setter

open System
open System.Data.SQLite
open System.IO
open DEXPI2.PipingNodeOwnerCategory
open DEXPI2.ProcessEquipmentCategory

/// 参数数值传递方向：equipment -> nozzle -> segment -> segmentItem
/// 求最终值类似于css算法，后来的具体的，局部的覆盖先来的，父容器的，全局的参数
/// pn: 压力等级，单位kPa
/// dn: 公称直径，单位mm
/// fluid: 介质，string;如"Water","Steam","Air","Oil","Gas"等
///
/// 分层：
///   实体层（ModelEntities.fs）：type ModelEntities + 同名 module ModelEntities。
///     设置 dn/pn/fluid（set*）与 CSS 覆盖求值（get*Final）都在实体层完成，纯内存，不碰数据库。
///     段 → 父级(设备,管嘴) 映射由 modelToEntities 建立（plant 级传递），get*Final 沿覆盖链动态求值。
///   数据库层（本文件 Setter.fs）：只负责建库与裸读写——
///     newDatabase       新建库（每个 PID 完全重建）
///     modelToEntities   PlantModel → 数据库实体（纯函数）
///     writeEntities     数据库实体 → 库（UPSERT，实体为权威：把实体投影到五张表）
///     createFromModel   = modelToEntities + writeEntities，返回实体供业务 set/get
///     tryGet*Row        库行读取
///
/// 数据库：SQLite。五张表，与 DEXPI2 命名空间下五个 record 类型一一对应（每个类型一个文件）：
///   EquipmentRow.fs —— Equipment  —— 设备（tag）级全局参数
///   NozzleRow.fs    —— Nozzle     —— 设备管嘴级参数
///   SegmentRow.fs   —— Segment    —— 常规管段级参数
///   HubSegmentRow.fs—— HubSegment —— 枢纽管段级参数：多节点分叉/变径点（reducer / tee 等共享管件）的单例段；
///                 端点不必区分，DN 为端点列表（按端点顺序），Fluid/PN 与其他层一样为单值
///   SegmentItemRow.fs —— SegmentItem —— 常规段内的各项（每项一行），主键 (LineNumber, SegmentNumber, Tag)：
///                  tag 有名管件（如 OperatedValve(XV-101)）以真实 tag 为 Tag；
///                  无名管件（无 tag，如 BlindFlange、Sensorwell）以 ItemIndex.ToString() 为 Tag；
///                  Type 列存管件类型名称。引用项（Nozzle、HubSegmentRef）只标记段边界，不入库。
/// 所有非 key 字段（fluid/pn/dn）可变，默认值 Fluid=""、PN=0、DN=0；未设置即默认值。
/// 覆盖链：equipment（最全局）→ nozzle → segment → segmentItem（最局部）。

// ============================================================
// 2. SQLite 数据库：打开、建表
// ============================================================

/// 建表 SQL（每个 PID 完全重建数据库，无需 IF NOT EXISTS；值列 NOT NULL DEFAULT 0 / ''）。
/// 各表的建表语句定义在其表实体模块中（EquipmentTable / NozzleTable / SegmentTable /
/// HubSegmentTable / SegmentItemTable，见各 *Row.fs），此处按顺序拼接执行。
let private schemaSql =
    String.concat
        "\n"
        [
            EquipmentTable.schemaSql
            NozzleTable.schemaSql
            SegmentTable.schemaSql
            HubSegmentTable.schemaSql
            SegmentItemTable.schemaSql
        ]

/// <summary>
/// 新建一个参数数据库：每个 PID 完全重建数据库（如同保存对话框选一个新文件名，
/// 重名库在选择时即已排除），建表语句不带 IF NOT EXISTS。
/// 打开（不存在则创建）SQLite 数据库文件并创建五张表；对已含同名表的库文件会报错，属预期。
/// 连接字符串风格与参考用法一致：Data Source=...;Version=3;。
/// 传入 ":memory:" 可打开内存数据库（仅本连接可见）。
/// 调用方负责在结束时关闭连接。
/// </summary>
let newDatabase (path: string) : SQLiteConnection =
    let dir = Path.GetDirectoryName path
    if
        not(String.IsNullOrEmpty dir)
        && not(Directory.Exists dir)
    then
        Directory.CreateDirectory dir |> ignore
    let conn =
        new SQLiteConnection(sprintf "Data Source=%s;Version=3;" path)
    try
        conn.Open()
        use cmd = new SQLiteCommand(schemaSql, conn)
        cmd.ExecuteNonQuery() |> ignore
        conn
    with e ->
        // 建表失败（如重名库文件已含同名表）时释放连接，避免泄漏的文件句柄占用库文件
        conn.Dispose()
        reraise()

// ============================================================
// 4. PlantModel → 数据库实体；数据库实体写入数据库
// ============================================================

/// <summary>
/// 从 PlantModel 生成数据库实体（纯函数，不碰数据库）：
/// 五张表的主键行（值列默认 0 / ""）+ 段 → 父级(设备,管嘴) 映射。
/// 实现委托给 ModelEntities.from：构建逻辑在 ModelEntities 层与各表模块，不扯到数据库。
/// 结果类型 ModelEntities 见 ModelEntities.fs；测试逻辑可直接比较此结果（结构相等）。
/// </summary>
let modelToEntities (model: PlantModel) : ModelEntities = ModelEntities.from model

/// <summary>
/// 把数据库实体投影到数据库（UPSERT 五张表，实体为权威：已存在的行用实体的值覆盖，不存在的行插入）。
/// 与旧版 INSERT OR IGNORE 的区别：实体层 set* 修改实体后调用本函数，新值必须落库，
/// 故冲突时更新值列（Fluid/PN/DN），主键行数与插入顺序不受影响。
/// </summary>
let writeEntities (conn: SQLiteConnection) (entities: ModelEntities) : unit =
    for eq in entities.Equipments do
        use cmd =
            new SQLiteCommand(
                "INSERT INTO Equipment (Equipment, Fluid, PN, DN) VALUES (@equipment, @fluid, @pn, @dn) ON CONFLICT(Equipment) DO UPDATE SET Fluid = excluded.Fluid, PN = excluded.PN, DN = excluded.DN;",
                conn
            )
        cmd.Parameters.AddWithValue("@equipment", eq.Equipment)
        |> ignore
        cmd.Parameters.AddWithValue("@fluid", eq.Fluid)
        |> ignore
        cmd.Parameters.AddWithValue("@pn", eq.PN)
        |> ignore
        cmd.Parameters.AddWithValue("@dn", eq.DN)
        |> ignore
        cmd.ExecuteNonQuery() |> ignore

    for nz in entities.Nozzles do
        use cmd =
            new SQLiteCommand(
                "INSERT INTO Nozzle (Equipment, Nozzle, Fluid, PN, DN) VALUES (@equipment, @nozzle, @fluid, @pn, @dn) ON CONFLICT(Equipment, Nozzle) DO UPDATE SET Fluid = excluded.Fluid, PN = excluded.PN, DN = excluded.DN;",
                conn
            )
        cmd.Parameters.AddWithValue("@equipment", nz.Equipment)
        |> ignore
        cmd.Parameters.AddWithValue("@nozzle", nz.Nozzle)
        |> ignore
        cmd.Parameters.AddWithValue("@fluid", nz.Fluid)
        |> ignore
        cmd.Parameters.AddWithValue("@pn", nz.PN)
        |> ignore
        cmd.Parameters.AddWithValue("@dn", nz.DN)
        |> ignore
        cmd.ExecuteNonQuery() |> ignore

    for seg in entities.Segments do
        use cmd =
            new SQLiteCommand(
                "INSERT INTO Segment (LineNumber, SegmentNumber, Fluid, PN, DN) VALUES (@lineNumber, @segmentNumber, @fluid, @pn, @dn) ON CONFLICT(LineNumber, SegmentNumber) DO UPDATE SET Fluid = excluded.Fluid, PN = excluded.PN, DN = excluded.DN;",
                conn
            )
        cmd.Parameters.AddWithValue("@lineNumber", seg.LineNumber)
        |> ignore
        cmd.Parameters.AddWithValue("@segmentNumber", seg.SegmentNumber)
        |> ignore
        cmd.Parameters.AddWithValue("@fluid", seg.Fluid)
        |> ignore
        cmd.Parameters.AddWithValue("@pn", seg.PN)
        |> ignore
        cmd.Parameters.AddWithValue("@dn", seg.DN)
        |> ignore
        cmd.ExecuteNonQuery() |> ignore

    for hub in entities.HubSegments do
        use cmd =
            new SQLiteCommand(
                "INSERT INTO HubSegment (LineNumber, SegmentNumber, Fluid, PN, DN) VALUES (@lineNumber, @segmentNumber, @fluid, @pn, @dn) ON CONFLICT(LineNumber, SegmentNumber) DO UPDATE SET Fluid = excluded.Fluid, PN = excluded.PN, DN = excluded.DN;",
                conn
            )
        cmd.Parameters.AddWithValue("@lineNumber", hub.LineNumber)
        |> ignore
        cmd.Parameters.AddWithValue("@segmentNumber", hub.SegmentNumber)
        |> ignore
        cmd.Parameters.AddWithValue("@fluid", hub.Fluid)
        |> ignore
        cmd.Parameters.AddWithValue("@pn", hub.PN)
        |> ignore
        cmd.Parameters.AddWithValue("@dn", HubSegmentTable.dnsToString hub.DNs)
        |> ignore
        cmd.ExecuteNonQuery() |> ignore

    for item in entities.SegmentItems do
        use cmd =
            new SQLiteCommand(
                "INSERT INTO SegmentItem (LineNumber, SegmentNumber, Tag, Type, Fluid, PN, DN) VALUES (@lineNumber, @segmentNumber, @tag, @type, @fluid, @pn, @dn) ON CONFLICT(LineNumber, SegmentNumber, Tag) DO UPDATE SET Fluid = excluded.Fluid, PN = excluded.PN, DN = excluded.DN;",
                conn
            )
        cmd.Parameters.AddWithValue("@lineNumber", item.LineNumber)
        |> ignore
        cmd.Parameters.AddWithValue("@segmentNumber", item.SegmentNumber)
        |> ignore
        cmd.Parameters.AddWithValue("@tag", item.Tag)
        |> ignore
        cmd.Parameters.AddWithValue("@type", item.Type)
        |> ignore
        cmd.Parameters.AddWithValue("@fluid", item.Fluid)
        |> ignore
        cmd.Parameters.AddWithValue("@pn", item.PN)
        |> ignore
        cmd.Parameters.AddWithValue("@dn", item.DN)
        |> ignore
        cmd.ExecuteNonQuery() |> ignore

/// <summary>
/// 从拓扑模型生成数据库实体、写入数据库（= modelToEntities + writeEntities）并返回实体，
/// 供业务在此实体上 set*/get*Final（见 ModelEntities 模块）。
/// 注意：每次调用都会以模型默认值重新投影（UPSERT），已设置的参数如需保留，
/// 应先取得实体完成设置后再 writeEntities 落库。
/// </summary>
let createFromModel (conn: SQLiteConnection) (model: PlantModel) : ModelEntities =
    let entities = modelToEntities model
    writeEntities conn entities
    entities

// ============================================================
// 6. 行读取
// ============================================================

/// <summary>读 Equipment 行；不存在返回 None。</summary>
let tryGetEquipmentRow
    (conn: SQLiteConnection)
    (equipment: string)
    : EquipmentRow option
    =
    use cmd =
        new SQLiteCommand(
            "SELECT Equipment, Fluid, PN, DN FROM Equipment WHERE Equipment=@equipment;",
            conn
        )
    cmd.Parameters.AddWithValue("@equipment", equipment)
    |> ignore
    use r = cmd.ExecuteReader()
    if r.Read() then
        Some
            {
                Equipment = r.GetString 0
                Fluid = r.GetString 1
                PN = r.GetInt32 2
                DN = r.GetInt32 3
            }
    else
        None

/// <summary>读 Nozzle 行；不存在返回 None。</summary>
let tryGetNozzleRow
    (conn: SQLiteConnection)
    (equipment: string)
    (nozzle: string)
    : NozzleRow option
    =
    use cmd =
        new SQLiteCommand(
            "SELECT Equipment, Nozzle, Fluid, PN, DN FROM Nozzle WHERE Equipment=@equipment AND Nozzle=@nozzle;",
            conn
        )
    cmd.Parameters.AddWithValue("@equipment", equipment)
    |> ignore
    cmd.Parameters.AddWithValue("@nozzle", nozzle)
    |> ignore
    use r = cmd.ExecuteReader()
    if r.Read() then
        Some
            {
                Equipment = r.GetString 0
                Nozzle = r.GetString 1
                Fluid = r.GetString 2
                PN = r.GetInt32 3
                DN = r.GetInt32 4
            }
    else
        None

/// <summary>读 Segment（常规段）行；不存在返回 None。</summary>
let tryGetSegmentRow
    (conn: SQLiteConnection)
    (lineNumber: string)
    (segmentNumber: string)
    : SegmentRow option
    =
    use cmd =
        new SQLiteCommand(
            "SELECT LineNumber, SegmentNumber, Fluid, PN, DN FROM Segment WHERE LineNumber=@lineNumber AND SegmentNumber=@segmentNumber;",
            conn
        )
    cmd.Parameters.AddWithValue("@lineNumber", lineNumber)
    |> ignore
    cmd.Parameters.AddWithValue("@segmentNumber", segmentNumber)
    |> ignore
    use r = cmd.ExecuteReader()
    if r.Read() then
        Some
            {
                LineNumber = r.GetString 0
                SegmentNumber = r.GetString 1
                Fluid = r.GetString 2
                PN = r.GetInt32 3
                DN = r.GetInt32 4
            }
    else
        None

/// <summary>读 HubSegment（枢纽段）行；不存在返回 None。</summary>
let tryGetHubSegmentRow
    (conn: SQLiteConnection)
    (lineNumber: string)
    (segmentNumber: string)
    : HubSegmentRow option
    =
    use cmd =
        new SQLiteCommand(
            "SELECT LineNumber, SegmentNumber, Fluid, PN, DN FROM HubSegment WHERE LineNumber=@lineNumber AND SegmentNumber=@segmentNumber;",
            conn
        )
    cmd.Parameters.AddWithValue("@lineNumber", lineNumber)
    |> ignore
    cmd.Parameters.AddWithValue("@segmentNumber", segmentNumber)
    |> ignore
    use r = cmd.ExecuteReader()
    if r.Read() then
        Some
            {
                LineNumber = r.GetString 0
                SegmentNumber = r.GetString 1
                Fluid = r.GetString 2
                PN = r.GetInt32 3
                DNs = HubSegmentTable.dnsFromString(r.GetString 4)
            }
    else
        None

/// <summary>读 SegmentItem 行；不存在返回 None。Tag：tag 有名管件用真实 tag，无名管件用 ItemIndex.ToString()。</summary>
let tryGetSegmentItemRow
    (conn: SQLiteConnection)
    (lineNumber: string)
    (segmentNumber: string)
    (tag: string)
    : SegmentItemRow option
    =
    use cmd =
        new SQLiteCommand(
            "SELECT LineNumber, SegmentNumber, Tag, Type, Fluid, PN, DN FROM SegmentItem WHERE LineNumber=@lineNumber AND SegmentNumber=@segmentNumber AND Tag=@tag;",
            conn
        )
    cmd.Parameters.AddWithValue("@lineNumber", lineNumber)
    |> ignore
    cmd.Parameters.AddWithValue("@segmentNumber", segmentNumber)
    |> ignore
    cmd.Parameters.AddWithValue("@tag", tag)
    |> ignore
    use r = cmd.ExecuteReader()
    if r.Read() then
        Some
            {
                LineNumber = r.GetString 0
                SegmentNumber = r.GetString 1
                Tag = r.GetString 2
                Type = r.GetString 3
                Fluid = r.GetString 4
                PN = r.GetInt32 5
                DN = r.GetInt32 6
            }
    else
        None
