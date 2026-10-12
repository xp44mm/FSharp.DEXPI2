namespace DEXPI2

/// <summary>Segment 表的一行：一个常规管段一条记录，主键 (LineNumber, SegmentNumber)。</summary>
type SegmentRow =
    {
        /// 所属系统线号（主键）
        LineNumber: string
        /// 段号（主键）
        SegmentNumber: string
        /// 介质，默认 ""（未设置）
        mutable Fluid: string
        /// 压力等级 kPa，默认 0（未设置）
        mutable PN: int
        /// 公称直径 mm，默认 0（未设置）
        mutable DN: int
    }

/// Segment 表建表 SQL（每个 PID 完全重建数据库，无需 IF NOT EXISTS；值列 NOT NULL DEFAULT 0 / ''）
/// Id：自动递增整数主键，保持插入顺序；(LineNumber, SegmentNumber) 为业务唯一键（UNIQUE）。
module SegmentTable =
    let schemaSql =
        """
    CREATE TABLE Segment (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        LineNumber TEXT NOT NULL,
        SegmentNumber TEXT NOT NULL,
        Fluid TEXT NOT NULL DEFAULT '',
        PN INTEGER NOT NULL DEFAULT 0,
        DN INTEGER NOT NULL DEFAULT 0,
        UNIQUE (LineNumber, SegmentNumber)
    );
    """

    /// 从 PlantModel 提取 Segment 行（每常规段一行，值列默认；枢纽段不入本表）。纯函数，不碰数据库。
    let from (model: PlantModel) : SegmentRow list =
        [
            for sys in model.PipingNetworkSystems do
                for seg in sys.Segments do
                    match seg with
                    | Segment(segmentNumber, _) ->
                        yield
                            {
                                LineNumber = sys.LineNumber
                                SegmentNumber = segmentNumber
                                Fluid = ""
                                PN = 0
                                DN = 0
                            }
                    | HubSegment _ -> ()
        ]

    /// 行 → (Fluid, PN, DN) 三元组（按影响范围从大到小）
    let toTuple (r: SegmentRow) : string * int * int = r.Fluid, r.PN, r.DN

    /// 在行列表中按主键 (LineNumber, SegmentNumber) 查找行；不存在返回 None。
    /// 输入行列表（如实体 e 的 e.Segments），不引用 ModelEntities 类型，避免循环引用。
    let tryGet
        (rows: SegmentRow list)
        (lineNumber: string)
        (segmentNumber: string)
        : SegmentRow option
        =
        rows
        |> List.tryFind(fun r ->
            r.LineNumber = lineNumber
            && r.SegmentNumber = segmentNumber
        )

    /// 在行列表中按主键 (LineNumber, SegmentNumber) 设置 PN（kPa）；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.Segments），不引用 ModelEntities 类型，避免循环引用。
    let setPN
        (rows: SegmentRow list)
        (pn: int)
        (lineNumber: string)
        (segmentNumber: string)
        : unit
        =
        match tryGet rows lineNumber segmentNumber with
        | Some r -> r.PN <- pn
        | None ->
            invalidOp (
                sprintf
                    "Segment '%s/%s' does not exist; call createFromModel first."
                    lineNumber
                    segmentNumber
            )

    /// 在行列表中按主键 (LineNumber, SegmentNumber) 设置 DN（mm）；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.Segments），不引用 ModelEntities 类型，避免循环引用。
    let setDN
        (rows: SegmentRow list)
        (dn: int)
        (lineNumber: string)
        (segmentNumber: string)
        : unit
        =
        match tryGet rows lineNumber segmentNumber with
        | Some r -> r.DN <- dn
        | None ->
            invalidOp (
                sprintf
                    "Segment '%s/%s' does not exist; call createFromModel first."
                    lineNumber
                    segmentNumber
            )

    /// 在行列表中按主键 (LineNumber, SegmentNumber) 设置流体介质；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.Segments），不引用 ModelEntities 类型，避免循环引用。
    let setFluid
        (rows: SegmentRow list)
        (fluid: string)
        (lineNumber: string)
        (segmentNumber: string)
        : unit
        =
        match tryGet rows lineNumber segmentNumber with
        | Some r -> r.Fluid <- fluid
        | None ->
            invalidOp (
                sprintf
                    "Segment '%s/%s' does not exist; call createFromModel first."
                    lineNumber
                    segmentNumber
            )
