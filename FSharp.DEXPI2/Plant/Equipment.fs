namespace FSharp.DEXPI2.Plant.ProcessEquipment

open FSharp.DEXPI2
open FSharp.DEXPI2.Plant.Piping

type Tank = {
    TagName: string
    Nozzles: list<Nozzle>
}
