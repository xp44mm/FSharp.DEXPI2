namespace FSharp.DEXPI2.Test

open Xunit

open FSharp.DEXPI2
open FSharp.DEXPI2.Core
open FSharp.DEXPI2.Plant
open FSharp.DEXPI2.Plant.ProcessEquipment
open FSharp.DEXPI2.Plant.Piping

module MinimalTankDischarge =

    // ---- 实例化 MinimalTankDischarge.xml ----
    let nodeN1  = { Id = ObjectId "N1Node"; NominalDiameterNumericalValueRepresentation = 50 }
    let nozzle  = { Id = ObjectId "N1";    SubTagName = "N1";  PN = 16; Node = nodeN1 }
    let tank    = { Id = ObjectId "V101";  TagName = "V-101";  Nozzles = [nozzle] }

    let nodeIn  = { Id = ObjectId "XV101In"; NominalDiameterNumericalValueRepresentation = 50 }
    let nodeOut = { Id = ObjectId "XV101Out"; NominalDiameterNumericalValueRepresentation = 50 }
    let valve   = { Id = ObjectId "XV101"; PipingComponentNumber = "XV-101"
                    NominalDiameterNumericalValueRepresentation = 50; PN = 16
                    Nodes = [nodeIn; nodeOut] }

    let discharge = { Id = ObjectId "Discharge"; TagName = "To Sewer" }

    let conn1 = { Id = ObjectId "Conn1"
                  SourceItem = ObjectId "N1Node"; TargetItem = ObjectId "XV101In" }
    let conn2 = { Id = ObjectId "Conn2"
                  SourceItem = ObjectId "XV101Out"; TargetItem = ObjectId "Discharge" }

    let seg = { Id = ObjectId "Seg1"
                SegmentNumber = "S1"; NominalDiameterNumericalValueRepresentation = 50
                Items = [ValveItem valve; OffPageConnectorItem discharge]
                Connections = [conn1; conn2] }
    let pns = { Id = ObjectId "PNS1"; LineNumber = "PL-101"
                FluidCode = "W"; PipingClassCode = "15S1"
                PipingNetworkSystemGroupNumber = ""
                Segments = [seg] }

    let plant = { Id = ObjectId "PlantModel1"
                  TaggedPlantItems = [ObjectId "V101"]
                  PipingNetworkSystems = [pns] }

    [<Fact>]
    let ``PlantModel 作为模型入口``() =
        Assert.Equal(ObjectId "PlantModel1", plant.Id)

    [<Fact>]
    let ``Tank V-101 带一个底嘴 N1``() =
        Assert.Equal("V-101", tank.TagName)
        Assert.Equal(1, tank.Nozzles.Length)
        Assert.Equal("N1", tank.Nozzles.[0].SubTagName)

    [<Fact>]
    let ``Nozzle N1 带 PN 并挂 PipingNode N1Node``() =
        Assert.Equal("N1", nozzle.SubTagName)
        Assert.Equal(16, nozzle.PN)
        Assert.Equal(ObjectId "N1Node", nozzle.Node.Id)

    [<Fact>]
    let ``OperatedValve XV-101 在管段内``() =
        Assert.Equal("XV-101", valve.PipingComponentNumber)
        Assert.Equal(50, valve.NominalDiameterNumericalValueRepresentation)
        Assert.Equal(16, valve.PN)
        Assert.Equal(2, valve.Nodes.Length)
        match seg.Items.[0] with
        | ValveItem v -> Assert.Equal("XV-101", v.PipingComponentNumber)
        | _ -> failwith "expected valve"

    [<Fact>]
    let ``管线 PL-101 的段包含阀、排出口和两条连接``() =
        Assert.Equal("PL-101", pns.LineNumber)
        Assert.Equal("W", pns.FluidCode)
        Assert.Equal(1, pns.Segments.Length)
        let s = pns.Segments.[0]
        Assert.Equal("S1", s.SegmentNumber)
        Assert.Equal(2, s.Items.Length)
        Assert.Equal(2, s.Connections.Length)
        Assert.Equal(ObjectId "N1Node", s.Connections.[0].SourceItem)
        Assert.Equal(ObjectId "Discharge", s.Connections.[1].TargetItem)

    [<Fact>]
    let ``PlantModel 的 TaggedPlantItems 只有罐``() =
        Assert.Equal<list<ObjectId>>([ObjectId "V101"], plant.TaggedPlantItems)
        Assert.Equal(1, plant.PipingNetworkSystems.Length)
        Assert.Equal("PL-101", plant.PipingNetworkSystems.[0].LineNumber)
