namespace DEXPI2

/// <summary>SegmentItem 表的一行：常规段内一个节点一条记录，主键 (LineNumber, SegmentNumber, Tag)。</summary>
/// Tag：tag 有名管件（如 OperatedValve(XV-101)）用真实 tag；无名管件（无 tag）用 ItemIndex.ToString()。
type SegmentItemRow =
    {
        /// 所属系统线号（主键）
        LineNumber: string
        /// 段号（主键）
        SegmentNumber: string
        /// 管件标识（主键）：tag 管件为真实 tag；无名管件为 ItemIndex.ToString()，如 "1"
        Tag: string
        /// 管件类型名称，如 OperatedValve(Manual)、CheckValve、BlindFlange、Sensorwell(PT)、PipeOffPageConnector
        Type: string

        /// 介质，默认 ""（未设置）
        mutable Fluid: string // = ""
        /// 压力等级 kPa，默认 0（未设置）
        mutable PN: int // = 0
        /// 公称直径 mm，默认 0（未设置）
        mutable DN: int // = 0
    }

/// SegmentItem 表建表 SQL（每个 PID 完全重建数据库，无需 IF NOT EXISTS；值列 NOT NULL DEFAULT 0 / ''）
/// Id：自动递增整数主键，保持插入顺序；(LineNumber, SegmentNumber, Tag) 为业务唯一键（UNIQUE）。
module SegmentItemTable =
    let schemaSql =
        """
    CREATE TABLE SegmentItem (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        LineNumber TEXT NOT NULL,
        SegmentNumber TEXT NOT NULL,
        Tag TEXT NOT NULL,
        Type TEXT NOT NULL,
        Fluid TEXT NOT NULL DEFAULT '',
        PN INTEGER NOT NULL DEFAULT 0,
        DN INTEGER NOT NULL DEFAULT 0,
        UNIQUE (LineNumber, SegmentNumber, Tag)
    );
    """

    /// 段项 (tag, 类型名) 对：tag 管件用真实 tag，无名管件用 ItemIndex.ToString()；引用项不入库。
    let private tagTypeOfItems (items: PipingNodeOwner list) : (string * string) list =
        items
        |> List.mapi(fun i item ->
            match PipingNodeOwnerCategory.tagItemOf item with
            | Some(typ, tag) -> Some(tag, typ)
            | None ->
                match PipingNodeOwnerCategory.unnamedTypeOf item with
                | Some typ -> Some(string i, typ)
                | None -> None)
        |> List.choose id

    /// 从 PlantModel 提取 SegmentItem 行（tag 有名管件用真实 tag，无名管件用 ItemIndex.ToString()，
    /// 引用项（Nozzle、HubSegmentRef）不入库）。纯函数，不碰数据库。
    let from (model: PlantModel) : SegmentItemRow list =
        [
            for sys in model.PipingNetworkSystems do
                for seg in sys.Segments do
                    match seg with
                    | Segment(segmentNumber, items) ->
                        for tag, typ in tagTypeOfItems items do
                            yield
                                {
                                    LineNumber = sys.LineNumber
                                    SegmentNumber = segmentNumber
                                    Tag = tag
                                    Type = typ
                                    Fluid = ""
                                    PN = 0
                                    DN = 0
                                }
                    | HubSegment _ -> ()
        ]

    /// 行 → (Fluid, PN, DN) 三元组（按影响范围从大到小）
    let toTuple (r: SegmentItemRow) : string * int * int = r.Fluid, r.PN, r.DN

    /// 在行列表中按主键 (LineNumber, SegmentNumber, Tag) 查找行；不存在返回 None。
    /// 输入行列表（如实体 e 的 e.SegmentItems），不引用 ModelEntities 类型，避免循环引用。
    let tryGet
        (rows: SegmentItemRow list)
        (lineNumber: string)
        (segmentNumber: string)
        (tag: string)
        : SegmentItemRow option
        =
        rows
        |> List.tryFind(fun r ->
            r.LineNumber = lineNumber
            && r.SegmentNumber = segmentNumber
            && r.Tag = tag
        )

    /// 在行列表中按主键 (LineNumber, SegmentNumber, Tag) 设置 PN（kPa）；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.SegmentItems），不引用 ModelEntities 类型，避免循环引用。
    let setPN
        (rows: SegmentItemRow list)
        (pn: int)
        (lineNumber: string)
        (segmentNumber: string)
        (tag: string)
        : unit
        =
        match tryGet rows lineNumber segmentNumber tag with
        | Some r -> r.PN <- pn
        | None ->
            invalidOp (
                sprintf
                    "No item '%s' in segment '%s/%s'; call createFromModel first (ref items like Nozzle/HubSegmentRef are not stored)."
                    tag
                    lineNumber
                    segmentNumber
            )

    /// 在行列表中按主键 (LineNumber, SegmentNumber, Tag) 设置 DN（mm）；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.SegmentItems），不引用 ModelEntities 类型，避免循环引用。
    let setDN
        (rows: SegmentItemRow list)
        (dn: int)
        (lineNumber: string)
        (segmentNumber: string)
        (tag: string)
        : unit
        =
        match tryGet rows lineNumber segmentNumber tag with
        | Some r -> r.DN <- dn
        | None ->
            invalidOp (
                sprintf
                    "No item '%s' in segment '%s/%s'; call createFromModel first (ref items like Nozzle/HubSegmentRef are not stored)."
                    tag
                    lineNumber
                    segmentNumber
            )

    /// 在行列表中按主键 (LineNumber, SegmentNumber, Tag) 设置流体介质；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.SegmentItems），不引用 ModelEntities 类型，避免循环引用。
    let setFluid
        (rows: SegmentItemRow list)
        (fluid: string)
        (lineNumber: string)
        (segmentNumber: string)
        (tag: string)
        : unit
        =
        match tryGet rows lineNumber segmentNumber tag with
        | Some r -> r.Fluid <- fluid
        | None ->
            invalidOp (
                sprintf
                    "No item '%s' in segment '%s/%s'; call createFromModel first (ref items like Nozzle/HubSegmentRef are not stored)."
                    tag
                    lineNumber
                    segmentNumber
            )
