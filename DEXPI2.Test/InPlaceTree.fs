module DEXPI2.InPlaceTree

/// 工厂模型入口（罐 / 管线 PL-101 在位内联，段 S1、S3）
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                Tank(tag = "V-101", nozzles = [ "N1" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "PL-101"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 设备的管嘴，和管道段，不包括在本段，尽管在本段的Items中。
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(
                                            equipment = "V-101",
                                            nozzle = "N1"
                                        )
                                        PipingNodeOwner.OperatedValve(tag = "XV-101")
                                        PipingNetworkSegmentSingleton(tag = "S2")
                                    ]
                            }

                            {
                                SegmentNumber = "S2"
                                Items =
                                    [
                                        PipingNodeOwner.PipeReducer
                                    ]

                            }

                            {
                                SegmentNumber = "S3"
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(tag = "S2")
                                        PipingNodeOwner.PipeOffPageConnector(
                                            connector = "To Sewer"
                                        )
                                    ]
                            }
                        ]
                }
            ]
    }
