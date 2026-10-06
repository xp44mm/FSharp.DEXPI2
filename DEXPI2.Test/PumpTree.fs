module DEXPI2.PumpTree


/// 工厂模型入口（罐 / 泵在位内联，段 S1~S4）
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                Tank(tag = "V-101", nozzles = [ "N1" ])
                Pump(tag = "P-101", nozzles = [ "inlet"; "outlet" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "PL-201"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 罐出口至异径管：喷嘴 N1 → 阀门 XV-101 → 异径管 XR-101
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "V-101", nozzle = "N1")
                                        PipingNodeOwner.OperatedValve(tag = "XV-101")

                                        PipingNetworkSegmentSingleton(lineNumber = "", segmentNumber = "S2")
                                    ]
                            }

                            PipingNetworkSegment.reducer "S2"

                            {
                                SegmentNumber = "S3"
                                // 异径管 XR-101 → 泵入口喷嘴 N2
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(lineNumber = "", segmentNumber = "S2")

                                        PipingNodeOwner.Nozzle(equipment = "P-101", nozzle = "inlet")
                                    ]
                            }

                            {
                                SegmentNumber = "S4"
                                // 泵出口喷嘴 N3 → 异径管 XR-102
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "P-101", nozzle = "outlet")
                                        PipingNetworkSegmentSingleton(lineNumber = "", segmentNumber = "S5")
                                    ]
                            }

                            PipingNetworkSegment.reducer "S5"

                            {
                                SegmentNumber = "S6"
                                // 异径管 XR-102 → 止回阀 CV-101 → 排污管道
                                Items =
                                    [
                                        PipingNodeOwner.CheckValve(tag = "CV-101")
                                        PipingNodeOwner.PipeOffPageConnector(pipeConnectorDescription = "排污管道")
                                    ]
                            }
                        ]
                }
            ]
    }
