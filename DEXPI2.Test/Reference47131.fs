module DEXPI2.Reference47131


/// 参考 P&ID 47131 线（冷却水 WKb）：板式换热器 H1007 管嘴 N4 → 界外出口
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                PlateHeatExchanger(tag = "H1007", nozzles = [ "N4" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "47131"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 开口线：目标端为空（流出界外），仅连换热器管嘴 N4
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "H1007", nozzle = "N4")
                                    ]
                            }
                        ]
                }
            ]
    }
