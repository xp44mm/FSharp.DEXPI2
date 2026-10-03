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

type Tank = {
    TagName: string
    Nozzles: list<Nozzle>
}

/// <summary>可区分联合：TaggedPlantItem 的所有非抽象派生类型（含非叶类）。每个案例荷载对应的 C# 类型。</summary>
type TaggedPlantItem =
    | Tank of Tank
