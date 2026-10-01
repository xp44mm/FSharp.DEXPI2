namespace FSharp.DEXPI2.Core

open FSharp.DEXPI2

type EngineeringModel = {
    Id: ObjectId
    ConceptualModel: ObjectId
}

namespace FSharp.DEXPI2.Plant

open FSharp.DEXPI2

type PlantModel = {
    Id: ObjectId
    TaggedPlantItems: list<ObjectId>
    PipingNetworkSystems: list<ObjectId>
}

namespace FSharp.DEXPI2.Plant.ProcessEquipment

open FSharp.DEXPI2

type Tank = {
    Id: ObjectId
    TagName: string
    Nozzles: list<ObjectId>
}

type Nozzle = {
    Id: ObjectId
    SubTagName: string
    Nodes: list<ObjectId>
}

namespace FSharp.DEXPI2.Plant.Piping

open FSharp.DEXPI2

type PipingNode = {
    Id: ObjectId
}

type ShutOffValve = {
    Id: ObjectId
    TagName: string
    Nodes: list<ObjectId>
}

type PipeOffPageConnector = {
    Id: ObjectId
    TagName: string
}

type PipingNetworkSystem = {
    Id: ObjectId
    LineNumber: string
    Segments: list<ObjectId>
    OffPageConnectors: list<ObjectId>
}

type PipingNetworkSegment = {
    Id: ObjectId
    Items: list<ObjectId>
}

type PipingConnection = {
    Id: ObjectId
    SourceItem: ObjectId
    TargetItem: ObjectId
}
