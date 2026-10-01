namespace FSharp.DEXPI2.Test

open Xunit

open FSharp.DEXPI2
open FSharp.DEXPI2.Core
open FSharp.DEXPI2.Plant
open FSharp.DEXPI2.Plant.ProcessEquipment
open FSharp.DEXPI2.Plant.Piping

module MinimalTankDischarge =

    // ---- 实例化 MinimalTankDischarge.xml ----
    let tank    = { Id = ObjectId "V101";  TagName = "V-101";  Nozzles = [ObjectId "N1"] }
    let nozzle  = { Id = ObjectId "N1";    SubTagName = "N1";  Nodes = [ObjectId "N1Node"] }
    let nodeN1  = { Id = ObjectId "N1Node" }

    let valve   = { Id = ObjectId "XV101"; TagName = "XV-101"
                    Nodes = [ObjectId "XV101In"; ObjectId "XV101Out"] }
    let nodeIn  = { Id = ObjectId "XV101In" }
    let nodeOut = { Id = ObjectId "XV101Out" }

    let discharge = { Id = ObjectId "Discharge"; TagName = "To Sewer" }

    let conn1 = { Id = ObjectId "Conn1"
                  SourceItem = ObjectId "N1Node"; TargetItem = ObjectId "XV101In" }
    let conn2 = { Id = ObjectId "Conn2"
                  SourceItem = ObjectId "XV101Out"; TargetItem = ObjectId "Discharge" }

    let seg = { Id = ObjectId "Seg1"; Items = [ObjectId "Conn1"; ObjectId "Conn2"] }
    let pns = { Id = ObjectId "PNS1"; LineNumber = "PL-101"
                Segments = [ObjectId "Seg1"]
                OffPageConnectors = [ObjectId "Discharge"] }

    let plant = { Id = ObjectId "PlantModel1"
                  TaggedPlantItems = [ObjectId "V101"; ObjectId "XV101"]
                  PipingNetworkSystems = [ObjectId "PNS1"] }

    let em = { Id = ObjectId "EM1"; ConceptualModel = ObjectId "PlantModel1" }

    [<Fact>]
    let ``EngineeringModel 指向 PlantModel``() =
        Assert.Equal(ObjectId "PlantModel1", em.ConceptualModel)

    [<Fact>]
    let ``Tank V-101 带一个底嘴 N1``() =
        Assert.Equal("V-101", tank.TagName)
        Assert.Equal<list<ObjectId>>([ObjectId "N1"], tank.Nozzles)

    [<Fact>]
    let ``Nozzle N1 挂一个 PipingNode N1Node``() =
        Assert.Equal("N1", nozzle.SubTagName)
        Assert.Equal<list<ObjectId>>([ObjectId "N1Node"], nozzle.Nodes)

    [<Fact>]
    let ``ShutOffValve XV-101 带进出两个节点``() =
        Assert.Equal("XV-101", valve.TagName)
        Assert.Equal<list<ObjectId>>([ObjectId "XV101In"; ObjectId "XV101Out"], valve.Nodes)

    [<Fact>]
    let ``管线 PL-101 的两条连接形成 N1 -> 阀 -> 排放``() =
        Assert.Equal("PL-101", pns.LineNumber)
        Assert.Equal(ObjectId "N1Node", conn1.SourceItem)
        Assert.Equal(ObjectId "XV101In", conn1.TargetItem)
        Assert.Equal(ObjectId "XV101Out", conn2.SourceItem)
        Assert.Equal(ObjectId "Discharge", conn2.TargetItem)

    [<Fact>]
    let ``PlantModel 列出箱和阀作为 TaggedPlantItems``() =
        Assert.Contains(ObjectId "V101", plant.TaggedPlantItems)
        Assert.Contains(ObjectId "XV101", plant.TaggedPlantItems)
        Assert.Equal<list<ObjectId>>([ObjectId "PNS1"], plant.PipingNetworkSystems)
