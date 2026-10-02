namespace FSharp.DEXPI2.Plant

open FSharp.DEXPI2
open FSharp.DEXPI2.Plant.Piping

type PlantModel = {
    Id: ObjectId
    TaggedPlantItems: list<ObjectId>
    PipingNetworkSystems: list<PipingNetworkSystem>
}
