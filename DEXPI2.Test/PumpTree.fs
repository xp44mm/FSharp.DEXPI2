module DEXPI2.PumpTree

/// dn150 管上的阀门 XV-201（OperatedValve；已定义，暂未接入任何段）
let valve2 = PipingNodeOwner.OperatedValve(tag = "XV-201")

/// 工厂模型入口（罐 / 泵在位内联，段 S1~S4）
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                Tank(tag = "V-101", nozzles = [ "N1" ])
                Pump(tag = "P-101", nozzles = [ "N2"; "N3" ])
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
                                        PipingNodeOwner.PipeReducer
                                    ]
                            }

                            {
                                SegmentNumber = "S2"
                                // 异径管 XR-101 → 泵入口喷嘴 N2
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "P-101", nozzle = "N2")
                                    ]
                            }

                            {
                                SegmentNumber = "S3"
                                // 泵出口喷嘴 N3 → 异径管 XR-102
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "P-101", nozzle = "N3")
                                        PipingNodeOwner.PipeReducer
                                    ]
                            }

                            {
                                SegmentNumber = "S4"
                                // 异径管 XR-102 → 止回阀 CV-101 → 排污管道
                                Items =
                                    [
                                        PipingNodeOwner.CheckValve(tag = "CV-101")
                                        PipingNodeOwner.PipeOffPageConnector(connector = "排污管道")
                                    ]
                            }
                        ]
                }
            ]
    }
