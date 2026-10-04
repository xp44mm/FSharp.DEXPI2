namespace FSharp.DEXPI2.Plant.ProcessEquipment

open FSharp.DEXPI2
open FSharp.DEXPI2.Plant.Piping

type Tank = {
    TagName: string
    Nozzles: list<Nozzle>
}

/// 泵（C# Pump，XMI ID1345；superTypes：ProcessEquipment → TaggedPlantItem → ...）
type Pump = {
    //own members
    //DesignPressureHead: PhysicalQuantity
    //DesignVolumeFlowRate: PhysicalQuantity
    //DifferentialPressure: PhysicalQuantity

    //inherited from TaggedPlantItem
    TagName: string

    //inherited from NozzleOwner
    Nozzles: list<Nozzle>
}
