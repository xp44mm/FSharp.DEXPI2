module DEXPI2.Reference47130


/// 参考 P&ID 47130 线（冷却水 WKa）：界外入口 → 板式换热器 H1007 管嘴 N3
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                PlateHeatExchanger(tag = "H1007", nozzles = [ "N3" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "47130"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 开口线：源端为空（界外来流），仅连换热器管嘴 N3
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "H1007", nozzle = "N3")
                                    ]
                            }
                        ]
                }
            ]
    }
