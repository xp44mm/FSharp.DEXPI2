namespace FSharp.DEXPI2.Plant.ProcessEquipment

open FSharp.DEXPI2
open FSharp.DEXPI2.Plant.Piping

type Nozzle = {
    Id: ObjectId
    SubTagName: string
    PN: int
    Node: PipingNode
}

type Tank = {
    Id: ObjectId
    TagName: string
    Nozzles: list<Nozzle>
}
