namespace FSharp.DEXPI2

open FSharp.DEXPI2.Plant.Piping

type Sensorwell = {
    //own members（与 C# own 分区字段一一对应）
    //LocationNominalDiameterNumericalValueRepresentation: string
    //LocationNominalDiameterRepresentation: string
    //LocationNominalDiameterStandard: NominalDiameterStandardClassification
    //LocationNominalDiameterTypeRepresentation: string
    SensorwellTypeRepresentation: string

    //inherited from PipeFitting（与 C# inherited 分区字段一一对应）
    //InsulationThickness: string
    //InsulationType: string
    //PipingClassCode: string
    //PipingComponentName: string
    PipingComponentNumber: string

    //inherited from PipingNodeOwner
    Nodes: list<PipingNode>
}
