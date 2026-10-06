module DEXPI2.Reference47141


/// 参考 P&ID 47141 线（淬火液 QSb）：管壳式换热器 H1008 管嘴 N3 → 截止阀 C1 → 界外出口
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                TubularHeatExchanger(tag = "H1008", nozzles = [ "N3" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "47141"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 换热器管嘴 N3 → 截止阀 C1 → 界外出口（目标端为空）
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "H1008", nozzle = "N3")
                                        PipingNodeOwner.GlobeValve(tag = "C1")
                                    ]
                            }
                        ]
                }
            ]
    }
