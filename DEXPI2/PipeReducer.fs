namespace FSharp.DEXPI2

open FSharp.DEXPI2.Plant.Piping

type PipeReducer = {
    //own members（none）
    //inherited from PipeFitting（与 C# inherited 分区字段一一对应）
    //InsulationThickness: string
    //InsulationType: string
    //PipingClassCode: string
    //PipingComponentName: string
    PipingComponentNumber: string

    //inherited from PipingNodeOwner
    Nodes: list<PipingNode>
}
