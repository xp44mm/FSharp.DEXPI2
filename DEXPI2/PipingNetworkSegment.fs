namespace DEXPI2

/// <summary>
/// 管段：常规段或枢纽段，让 P&amp;ID 拓扑更抽象。
/// Segment：常规管段，段号 + 段内项列表（项见 PipingNodeOwner）；
/// HubSegment：枢纽段（仅段号），抽象的多节点分叉/变径点（如异径管、三通等共享管件），
/// 被其他段以 PipingNodeOwner.HubSegmentRef(segmentNumber) 引用（不允许跨系统引用），
/// 不区分具体管件类型与端点。
/// </summary>
type PipingNetworkSegment =
    | Segment of SegmentNumber: string * Items: list<PipingNodeOwner>
    | HubSegment of SegmentNumber: string

module PipingNetworkSegment =
    /// 输出 (segmentNumber, kind)，对应 Segment 表的 segment_number/kind 列。
    /// Items 不参与本 tuple，由 SegmentItem 表单独存储。
    let toSqlite (seg: PipingNetworkSegment) : string * string =
        match seg with
        | Segment(num, _) -> num, "Segment"
        | HubSegment(num) -> num, "HubSegment"

    /// 从 (segmentNumber, kind, items) 还原。items 由 SegmentItem 表按段聚合得到。
    let fromSqlite (segmentNumber: string) (kind: string) (items: list<PipingNodeOwner>) : PipingNetworkSegment =
        match kind with
        | "Segment" -> Segment(segmentNumber, items)
        | "HubSegment" -> HubSegment(segmentNumber)
        | _ -> failwithf "unknown PipingNetworkSegment kind: %s" kind
