namespace FSharp.DEXPI2.Plant.Piping

open FSharp.DEXPI2

type PipingNode = {
    Id: ObjectId
    NominalDiameterNumericalValueRepresentation: int
}

type OperatedValve = {
    Id: ObjectId
    PipingComponentNumber: string
    NominalDiameterNumericalValueRepresentation: int
    PN: int
    Nodes: list<PipingNode>
}

type PipeOffPageConnector = {
    Id: ObjectId
    TagName: string
}

type SegmentItem =
    | ValveItem of OperatedValve
    | OffPageConnectorItem of PipeOffPageConnector

type PipingConnection = {
    Id: ObjectId
    SourceItem: ObjectId
    TargetItem: ObjectId
}

type PipingNetworkSegment = {
    Id: ObjectId
    SegmentNumber: string
    NominalDiameterNumericalValueRepresentation: int
    Items: list<SegmentItem>
    Connections: list<PipingConnection>
}

type PipingNetworkSystem = {
    Id: ObjectId
    LineNumber: string
    FluidCode: string
    PipingClassCode: string
    PipingNetworkSystemGroupNumber: string
    Segments: list<PipingNetworkSegment>
}
