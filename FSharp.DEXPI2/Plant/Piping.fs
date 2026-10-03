namespace FSharp.DEXPI2.Plant.Piping

open FSharp.DEXPI2

type PipingNode = {
    //NominalDiameterNumericalValueRepresentation: string
    //NominalDiameterRepresentation: string
    NominalDiameterStandard: string
    //NominalDiameterTypeRepresentation: string
}

type OperatedValve = {
    //own members（与 C# own 分区字段一一对应）
    //InsulationThickness: string
    //InsulationType: string
    //NumberOfPorts: string
    //Operation: string
    //PipingClassCode: string
    PipingComponentName: string
    PipingComponentNumber: string

    //inherited from PipingNodeOwner
    Nodes: list<PipingNode>
}

type PipeOffPageConnector = {
    //ConnectorReference: PipeOffPageConnectorReference
    //PipeConnectorDescription: string
    PipeConnectorNumber: string
    Nodes: list<PipingNode>

}

type PipingNetworkSegmentItem =
    | PipeOffPageConnector of PipeOffPageConnector
    | OperatedValve of OperatedValve

type PipingSourceItem =
    | PipeOffPageConnector of PipeOffPageConnector
    | OperatedValve of OperatedValve

type PipingTargetItem =
    | PipeOffPageConnector of PipeOffPageConnector
    | OperatedValve of OperatedValve

type PipingConnection = {
    SourceItem: PipingSourceItem
    SourceNode: PipingNode
    TargetItem: PipingTargetItem
    TargetNode: PipingNode
}

type PipingNetworkSegment = {
    //ColorCode: string
    //FlowDirection: string
    FluidCode: string
    //HeatTracingType: string
    //HeatTracingTypeRepresentation: string
    //InsulationType: string
    //JacketedPipe: string
    //NominalDiameterNumericalValueRepresentation: string
    //NominalDiameterRepresentation: string
    NominalDiameterStandard: string
    //NominalDiameterTypeRepresentation: string
    //OnHold: string
    //PipingClassCode: string
    //PressureTestCircuitNumber: string
    //PrimarySecondaryPipingNetworkSegment: string
    SegmentNumber: string
    //Siphon: string
    //Slope: string

    SourceItem: PipingSourceItem
    SourceNode: PipingNode
    TargetItem: PipingTargetItem
    TargetNode: PipingNode

    Items: list<PipingNetworkSegmentItem>
    Connections: list<PipingConnection>
}

type PipingNetworkSystem = {
    FluidCode: string
    //HeatTracingType: string
    //HeatTracingTypeRepresentation: string
    //InsulationType: string
    //JacketLineNumber: string
    //JacketedLineNumber: string
    //JacketedPipe: string
    LineNumber: string
    //NominalDiameterNumericalValueRepresentation: string
    //NominalDiameterRepresentation: string
    NominalDiameterStandard: string
    //NominalDiameterTypeRepresentation: string
    //OnHold: string
    //PipingClassCode: string
    PipingNetworkSystemGroupNumber: string
    Segments: list<PipingNetworkSegment>
}
