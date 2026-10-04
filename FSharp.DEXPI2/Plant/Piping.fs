namespace FSharp.DEXPI2.Plant.Piping

open FSharp.DEXPI2
open FSharp.DEXPI2.Plant.ProcessEquipment

type PipingNetworkSegmentItem =
    | PipeOffPageConnector of PipeOffPageConnector
    | PipeReducer of PipeReducer
    | PipeTee of PipeTee
    | Sensorwell of Sensorwell
    | OperatedValve of OperatedValve
    | CheckValve of CheckValve

type PipingSourceItem =
    | Nozzle of Nozzle
    | PipeOffPageConnector of PipeOffPageConnector
    | PipeReducer of PipeReducer
    | PipeTee of PipeTee
    | Sensorwell of Sensorwell
    | OperatedValve of OperatedValve
    | CheckValve of CheckValve

type PipingTargetItem =
    | Nozzle of Nozzle
    | PipeOffPageConnector of PipeOffPageConnector
    | PipeReducer of PipeReducer
    | PipeTee of PipeTee
    | Sensorwell of Sensorwell
    | OperatedValve of OperatedValve
    | CheckValve of CheckValve

type PipingConnection = {
    SourceItem: PipingSourceItem
    SourceNode: PipingNode
    TargetItem: PipingTargetItem
    TargetNode: PipingNode

    IsDirectPipingConnection: bool
}

type PipingNetworkSegment = {
    //ColorCode: string
    //FlowDirection: string
    //FluidCode: string
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
    //FluidCode: string
    //HeatTracingType: string
    //HeatTracingTypeRepresentation: string
    //InsulationType: string
    //JacketLineNumber: string
    //JacketedLineNumber: string
    //JacketedPipe: string
    LineNumber: string
    //NominalDiameterNumericalValueRepresentation: string
    //NominalDiameterRepresentation: string
    //NominalDiameterStandard: string
    //NominalDiameterTypeRepresentation: string
    //OnHold: string
    //PipingClassCode: string
    //PipingNetworkSystemGroupNumber: string
    Segments: list<PipingNetworkSegment>
}
