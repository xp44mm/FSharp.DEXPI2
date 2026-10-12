namespace DEXPI2


type PipingNetworkSystem =
    {
        LineNumber: string
        Segments: list<PipingNetworkSegment>
    }

module PipingNetworkSystem =
    /// 输出 lineNumber，对应 PipingNetworkSystem 表的 line_number 列。
    /// Segments 不参与本 tuple，由 Segment 表按 line_number 聚合得到。
    let toSqlite (sys: PipingNetworkSystem) : string = sys.LineNumber

    /// 从 (lineNumber, segments) 还原。
    let fromSqlite (lineNumber: string) (segments: list<PipingNetworkSegment>) : PipingNetworkSystem =
        { LineNumber = lineNumber; Segments = segments }
