module DEXPI2.TeeTree

/// 工厂模型入口（罐在位内联，段 S1~S4；S2 为三通单例段）
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                Tank(tag = "V-101", nozzles = [ "N1" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "PL-301"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 罐出口至三通入口：喷嘴 N1 → 阀门 XV-101 → 三通
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "V-101", nozzle = "N1")
                                        PipingNodeOwner.OperatedValve(tag = "XV-101")

                                        PipingNetworkSegmentSingleton(lineNumber = "", segmentNumber = "S2")
                                    ]
                            }

                            // 三通 T-101 单例段：星型连接（一个入口分叉为直通 + 支管），
                            // 不对应实体的一个三通管件；被 S1、S3、S4 引用
                            PipingNetworkSegment.tee "S2"

                            {
                                SegmentNumber = "S3"
                                // 三通直通 → 出口 Outlet1
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(lineNumber = "", segmentNumber = "S2")

                                        PipingNodeOwner.PipeOffPageConnector(pipeConnectorDescription = "Outlet1")
                                    ]
                            }

                            {
                                SegmentNumber = "S4"
                                // 三通支管 → 出口 Outlet2
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(lineNumber = "", segmentNumber = "S2")

                                        PipingNodeOwner.PipeOffPageConnector(pipeConnectorDescription = "Outlet2")
                                    ]
                            }
                        ]
                }
            ]
    }
