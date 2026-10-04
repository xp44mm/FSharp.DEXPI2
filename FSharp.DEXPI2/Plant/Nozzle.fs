namespace FSharp.DEXPI2.Plant.ProcessEquipment

open FSharp.DEXPI2
open FSharp.DEXPI2.Plant.Piping

type Nozzle = {
    SubTagName: string

    //NominalPressureNumericalValueRepresentation: string
    //NominalPressureRepresentation: string
    NominalPressureStandard: string
    //NominalPressureTypeRepresentation: string

    Nodes: list<PipingNode>
    //Chamber: ObjectId
}
