namespace FSharp.DEXPI2.Plant

//open FSharp.DEXPI2
//open FSharp.DEXPI2.Plant.Piping
//open FSharp.DEXPI2.Plant.ProcessEquipment

type PlantModel = {
    Id: FSharp.DEXPI2.ObjectId
    TaggedPlantItems: list<FSharp.DEXPI2.Plant.ProcessEquipment.TaggedPlantItem>
    PipingNetworkSystems: list<FSharp.DEXPI2.Plant.Piping.PipingNetworkSystem>
}
