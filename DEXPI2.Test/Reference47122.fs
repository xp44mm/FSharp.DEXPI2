module DEXPI2.Reference47122


/// 参考 P&ID 47122 线：离心泵 P4711 出口 N2 → 板式换热器 H1007 入口 N1
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                CentrifugalPump(tag = "P4711", nozzles = [ "N2" ])
                PlateHeatExchanger(tag = "H1007", nozzles = [ "N1" ])
            ]
        PipingNetworkSystems =
            [
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
