namespace DEXPI2.Test

open Xunit

open DEXPI2

module PlantModelTest =
    [<Fact>]
    let ``PlantModel 由设备细节列表与管网系统组成``() =
        let pump: ProcessEquipment = { TagName = "P-101"; Nozzles = []; detail = ProcessEquipmentDetail.Pump }
        let valve: PipingNodeOwner = { tag = "XV-101"; detail = PipingNodeOwnerDetail.OperatedValve }
        let reducer: PipingNodeOwner = { tag = "XR-101"; detail = PipingNodeOwnerDetail.PipeReducer }
        let segment: PipingNetworkSegment =
            {
                SegmentNumber = "S1"
                includeFirstItem = true
                includeLastItem = true
                Items = [ valve; reducer ]
            }
        let system: PipingNetworkSystem = { LineNumber = "PL-101"; Segments = [ segment ] }
        let model: PlantModel = { ProcessEquipments = [ pump.detail ]; PipingNetworkSystems = [ system ] }

        Assert.Equal(1, model.ProcessEquipments.Length)
        Assert.Equal(ProcessEquipmentDetail.Pump, model.ProcessEquipments.Head)
        Assert.Equal("PL-101", model.PipingNetworkSystems.Head.LineNumber)
        Assert.Equal(1, model.PipingNetworkSystems.Head.Segments.Length)
        Assert.Equal(2, model.PipingNetworkSystems.Head.Segments.Head.Items.Length)

    [<Fact>]
    let ``管段记录携带段号、两端开关与 Items``() =
        let nozzle: PipingNodeOwner = { tag = "N1"; detail = PipingNodeOwnerDetail.Nozzle }
        let seg: PipingNetworkSegment =
            {
                SegmentNumber = "S1"
                includeFirstItem = false
                includeLastItem = false
                Items = [ nozzle ]
            }
        Assert.Equal("S1", seg.SegmentNumber)
        Assert.False(seg.includeFirstItem)
        Assert.False(seg.includeLastItem)
        match seg.Items.Head with
        | { tag = "N1"; detail = PipingNodeOwnerDetail.Nozzle } -> ()
        | _ -> failwith "expected Nozzle"
