namespace FSharp.DEXPI2

open FSharp.DEXPI2.Plant.Piping

type CheckValve = {
    //own members
    //InsulationThickness: PhysicalQuantity
    //InsulationType: string
    //PipingClassCode: string
    //PipingComponentName: string
    PipingComponentNumber: string

    //inherited from PipingNodeOwner
    Nodes: list<PipingNode>
}
