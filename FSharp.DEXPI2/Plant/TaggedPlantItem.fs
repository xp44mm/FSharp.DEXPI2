namespace FSharp.DEXPI2.Plant.ProcessEquipment

/// <summary>可区分联合：TaggedPlantItem 的所有非抽象派生类型（含非叶类）。每个案例荷载对应的 C# 类型。</summary>
type TaggedPlantItem =
    | Tank of Tank
    | Pump of Pump
