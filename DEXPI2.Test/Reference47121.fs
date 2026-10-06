module DEXPI2.Reference47121


/// 参考 P&ID 47121/47122 线：离心泵 P4711 的吸入线与排出线
/// 47121：界外入口 → 泵吸入口 N1；47122：泵出口 N2 → 板式换热器 H1007 入口 N1
/// 泵 P4711 为一个实体，管嘴 N1、N2 完整定义于 ProcessEquipments
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                CentrifugalPump(tag = "P4711", nozzles = [ "N1"; "N2" ])
                PlateHeatExchanger(tag = "H1007", nozzles = [ "N1" ])
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

                {
                    LineNumber = "47122"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 泵出口 N2 → 换热器入口 N1（无中间管件）
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "P4711", nozzle = "N2")
                                        PipingNodeOwner.Nozzle(equipment = "H1007", nozzle = "N1")
                                    ]
                            }
                        ]
                }
            ]
    }
