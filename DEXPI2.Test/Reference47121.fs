module DEXPI2.Reference47121


/// 参考 P&ID 47121 线：界外入口 → 离心泵 P4711 吸入口 N1
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                CentrifugalPump(tag = "P4711", nozzles = [ "N1" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "47121"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 界外入口连接器 → 泵吸入口 N1（无中间管件）
                                Items =
                                    [
                                        PipingNodeOwner.FlowInPipeOffPageConnector(connector = "FlowInPipeOffPageConnector1")
                                        PipingNodeOwner.Nozzle(equipment = "P4711", nozzle = "N1")
                                    ]
                            }
                        ]
                }
            ]
    }
