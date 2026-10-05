namespace FSharp.DEXPI2.Plant.Piping

open FSharp.DEXPI2
open FSharp.DEXPI2.Plant.ProcessEquipment

type OperatedValve =
    {
        PipingComponentNumber: string
        Nodes: list<PipingNode>
    }

type PipeOffPageConnector =
    {
        PipeConnectorNumber: string
        Nodes: list<PipingNode>

    }

type PipeReducer =
    {
        PipingComponentNumber: string

        Nodes: list<PipingNode>
    }

type PipeTee =
    {
        PipingComponentNumber: string

        Nodes: list<PipingNode>
    }

type Sensorwell =
    {
        SensorwellTypeRepresentation: string

        PipingComponentNumber: string

        Nodes: list<PipingNode>
    }

type CheckValve =
    {
        PipingComponentNumber: string

        Nodes: list<PipingNode>
    }

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

type PipingConnection =
    {
        SourceItem: PipingSourceItem
        SourceNode: PipingNode
        TargetItem: PipingTargetItem
        TargetNode: PipingNode

        IsDirectPipingConnection: bool
    }

type PipingNetworkSegment =
    {
        NominalDiameterStandard: string
        SegmentNumber: string

        SourceItem: PipingSourceItem
        SourceNode: PipingNode
        TargetItem: PipingTargetItem
        TargetNode: PipingNode

        Items: list<PipingNetworkSegmentItem>
        Connections: list<PipingConnection>
    }

type PipingNetworkSystem =
    {
        LineNumber: string
        Segments: list<PipingNetworkSegment>
    }
