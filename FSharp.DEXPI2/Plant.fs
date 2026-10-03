namespace FSharp.DEXPI2.Plant

open FSharp.DEXPI2
open FSharp.DEXPI2.Plant.Piping
open FSharp.DEXPI2.Plant.ProcessEquipment

type PlantModel = {
    TaggedPlantItems: list<TaggedPlantItem>
    PipingNetworkSystems: list<PipingNetworkSystem>
}
