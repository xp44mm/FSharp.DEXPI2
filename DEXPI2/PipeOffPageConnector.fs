namespace FSharp.DEXPI2

open FSharp.DEXPI2.Plant.Piping

type PipeOffPageConnector = {
    //ConnectorReference: PipeOffPageConnectorReference
    //PipeConnectorDescription: string
    PipeConnectorNumber: string
    Nodes: list<PipingNode>

}
