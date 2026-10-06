namespace DEXPI2

type PipingNetworkSegment =
    {
        SegmentNumber: string
        Items: list<PipingNodeOwner>
    }

/// 单例管段（singleton 模式）：段内仅含一个管件，供其他段以 PipingNodeOwner.PipingNetworkSegmentSingleton 按 tag 引用
module PipingNetworkSegment =
    /// 三通单例管段：Items 仅含一个 PipeTee
    let tee segmentNumber =
        {
            SegmentNumber = segmentNumber
            Items = [ PipingNodeOwner.PipeTee ]
        }

    /// 异径管单例管段：Items 仅含一个 PipeReducer
    let reducer segmentNumber =
        {
            SegmentNumber = segmentNumber
            Items = [ PipingNodeOwner.PipeReducer ]
        }
