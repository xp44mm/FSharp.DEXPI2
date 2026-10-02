namespace FSharp.DEXPI2.Test

open FSharp.DEXPI2
open FSharp.DEXPI2.Core
open FSharp.DEXPI2.Plant
open FSharp.DEXPI2.Plant.ProcessEquipment
open FSharp.DEXPI2.Plant.Piping

module InPlaceTree =

    let plantModel : PlantModel = {
        Id = ObjectId "PlantModel1"
        TaggedPlantItems = [ObjectId "V101"]
        PipingNetworkSystems = [
            {
                Id = ObjectId "PNS1"
                LineNumber = "PL-101"
                FluidCode = "W"
                PipingClassCode = "15S1"
                PipingNetworkSystemGroupNumber = ""
                Segments = [
                    {
                        Id = ObjectId "Seg1"
                        SegmentNumber = "S1"
                        NominalDiameterNumericalValueRepresentation = 50
                        Items = [
                            ValveItem {
                                Id = ObjectId "XV101"
                                PipingComponentNumber = "XV-101"
                                NominalDiameterNumericalValueRepresentation = 50; PN = 16
                                Nodes = [
                                    { Id = ObjectId "XV101In";  NominalDiameterNumericalValueRepresentation = 50 }
                                    { Id = ObjectId "XV101Out"; NominalDiameterNumericalValueRepresentation = 50 }
                                ]
                            }
                            OffPageConnectorItem {
                                Id = ObjectId "Discharge"
                                TagName = "To Sewer"
                            }
                        ]
                        Connections = [
                            { Id = ObjectId "Conn1"
                              SourceItem = ObjectId "N1Node"
                              TargetItem = ObjectId "XV101In" }
                            { Id = ObjectId "Conn2"
                              SourceItem = ObjectId "XV101Out"
                              TargetItem = ObjectId "Discharge" }
                        ]
                    }
                ]
            }
        ]
    }

    let tank : Tank = {
        Id = ObjectId "V101"
        TagName = "V-101"
        Nozzles = [
            {
                Id = ObjectId "N1"
                SubTagName = "N1"
                PN = 16
                Node = { Id = ObjectId "N1Node"; NominalDiameterNumericalValueRepresentation = 50 }
            }
        ]
    }
