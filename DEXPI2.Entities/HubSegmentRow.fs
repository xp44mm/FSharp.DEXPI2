namespace DEXPI2

open System

/// <summary>
/// HubSegment 表的一行：一个枢纽管段一条记录，主键 (LineNumber, SegmentNumber)。
/// 枢纽段仅含一个多节点管件（reducer / tee）的单例段；端点不必区分，
/// DN 是端点列表（reducer 2 个、tee 3 个，按端点顺序），Fluid/PN 为单值。
/// </summary>
type HubSegmentRow =
    {
        /// 所属系统线号（主键）
        LineNumber: string
        /// 段号（主键）
        SegmentNumber: string
        /// 介质，默认 ""（未设置）
        mutable Fluid: string // = ""
        /// 压力等级 kPa，默认 0（未设置）
        mutable PN: int // = 0
        /// 各端点公称直径 mm，默认全 0（未设置）；如 reducer=[入口;出口]、tee=[入口;直通;支管]
        mutable DNs: int list // = []
    }

/// HubSegment 表建表 SQL（每个 PID 完全重建数据库，无需 IF NOT EXISTS；值列 NOT NULL DEFAULT 0 / ''）
/// DNs 列以逗号分隔文本存端点列表（TEXT），Fluid/PN 为单值。
/// Id：自动递增整数主键，保持插入顺序；(LineNumber, SegmentNumber) 为业务唯一键（UNIQUE）。
module HubSegmentTable =
    let schemaSql =
        """
    CREATE TABLE HubSegment (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        LineNumber TEXT NOT NULL,
        SegmentNumber TEXT NOT NULL,
        Fluid TEXT NOT NULL DEFAULT '',
        PN INTEGER NOT NULL DEFAULT 0,
        DN TEXT NOT NULL DEFAULT '',
        UNIQUE (LineNumber, SegmentNumber)
    );
    """

    /// 从 PlantModel 提取 HubSegment 行（每枢纽段一行，值列默认；常规段不入本表）。纯函数，不碰数据库。
    let from (model: PlantModel) : HubSegmentRow list =
        [
            for sys in model.PipingNetworkSystems do
                for seg in sys.Segments do
                    match seg with
                    | HubSegment segmentNumber ->
                        yield
                            {
                                LineNumber = sys.LineNumber
                                SegmentNumber = segmentNumber
                                Fluid = ""
                                PN = 0
                                DNs = []
                            }
                    | Segment _ -> ()
        ]

    /// int list → 逗号分隔文本（HubSegment.DN 列）
    let dnsToString (dns: int list) : string =
        dns |> List.map string |> String.concat ","

    /// 逗号分隔文本 → int list（HubSegment.DN 列）
    let dnsFromString (s: string) : int list =
        if String.IsNullOrEmpty s then
            []
        else
            s.Split(',')
            |> Array.toList
            |> List.map (fun t ->
                match Int32.TryParse t with
                | true, v -> v
                | _ -> 0)

    /// 在行列表中按主键 (LineNumber, SegmentNumber) 查找行；不存在返回 None。
    /// 输入行列表（如实体 e 的 e.HubSegments），不引用 ModelEntities 类型，避免循环引用。
    let tryGet
        (rows: HubSegmentRow list)
        (lineNumber: string)
        (segmentNumber: string)
        : HubSegmentRow option
        =
        rows
        |> List.tryFind(fun r ->
            r.LineNumber = lineNumber
            && r.SegmentNumber = segmentNumber
        )

    /// (LineNumber, SegmentNumber) 是否已存在于行列表（如实体 e 的 e.HubSegments）；
    /// 不引用 ModelEntities 类型，避免循环引用。
    let exists
        (rows: HubSegmentRow list)
        (lineNumber: string)
        (segmentNumber: string)
        : bool
        =
        rows
        |> List.exists(fun r ->
            r.LineNumber = lineNumber
            && r.SegmentNumber = segmentNumber
        )

    /// 在行列表中按主键 (LineNumber, SegmentNumber) 设置 PN（kPa）；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.HubSegments），不引用 ModelEntities 类型，避免循环引用。
    let setPN
        (rows: HubSegmentRow list)
        (pn: int)
        (lineNumber: string)
        (segmentNumber: string)
        : unit
        =
        match tryGet rows lineNumber segmentNumber with
        | Some h -> h.PN <- pn
        | None ->
            invalidOp (
                sprintf
                    "HubSegment '%s/%s' does not exist; call createFromModel first."
                    lineNumber
                    segmentNumber
            )

    /// 在行列表中按主键 (LineNumber, SegmentNumber) 设置各端点 DN（mm），整体替换端点列表；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.HubSegments），不引用 ModelEntities 类型，避免循环引用。
    let setDN
        (rows: HubSegmentRow list)
        (dns: int list)
        (lineNumber: string)
        (segmentNumber: string)
        : unit
        =
        match tryGet rows lineNumber segmentNumber with
        | Some h -> h.DNs <- dns
        | None ->
            invalidOp (
                sprintf
                    "HubSegment '%s/%s' does not exist; call createFromModel first."
                    lineNumber
                    segmentNumber
            )

    /// 在行列表中按主键 (LineNumber, SegmentNumber) 设置流体介质；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.HubSegments），不引用 ModelEntities 类型，避免循环引用。
    let setFluid
        (rows: HubSegmentRow list)
        (fluid: string)
        (lineNumber: string)
        (segmentNumber: string)
        : unit
        =
        match tryGet rows lineNumber segmentNumber with
        | Some h -> h.Fluid <- fluid
        | None ->
            invalidOp (
                sprintf
                    "HubSegment '%s/%s' does not exist; call createFromModel first."
                    lineNumber
                    segmentNumber
            )
