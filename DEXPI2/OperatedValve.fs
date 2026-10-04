namespace FSharp.DEXPI2

open FSharp.DEXPI2.Plant.Piping

type OperatedValve = {
    //own members（与 C# own 分区字段一一对应）
    //InsulationThickness: string
    //InsulationType: string
    //NumberOfPorts: string
    //Operation: string
    //PipingClassCode: string
    //PipingComponentName: string
    PipingComponentNumber: string

    //inherited from PipingNodeOwner
    Nodes: list<PipingNode>
}
