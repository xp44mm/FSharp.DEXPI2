module DEXPI2.Reference47127


/// 参考 P&ID 47127 线：管壳式换热器 H1008 出口 N2 → 罐 T4750 入口 N6，中间截止阀 C1
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                TubularHeatExchanger(tag = "H1008", nozzles = [ "N2" ])
                Tank(tag = "T4750", nozzles = [ "N6" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "47127"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 换热器出口 N2 → 截止阀 C1 → 罐入口 N6
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "H1008", nozzle = "N2")
                                        PipingNodeOwner.GlobeValve(tag = "C1")
                                        PipingNodeOwner.Nozzle(equipment = "T4750", nozzle = "N6")
                                    ]
                            }
                        ]
                }
            ]
    }
