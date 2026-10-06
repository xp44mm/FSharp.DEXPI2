module DEXPI2.Reference47123


/// 参考 P&ID 47123 线：板式换热器 H1007 出口 N2 → 罐 T4750 入口 N1，中间截止阀 C1
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                PlateHeatExchanger(tag = "H1007", nozzles = [ "N2" ])
                Tank(tag = "T4750", nozzles = [ "N1" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "47123"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 换热器出口 N2 → 截止阀 C1 → 罐入口 N1
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "H1007", nozzle = "N2")
                                        PipingNodeOwner.GlobeValve(tag = "C1")
                                        PipingNodeOwner.Nozzle(equipment = "T4750", nozzle = "N1")
                                    ]
                            }
                        ]
                }
            ]
    }
