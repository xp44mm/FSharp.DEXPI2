module DEXPI2.Reference47140


/// 参考 P&ID 47140 线（淬火液 QSa）：界外入口 → 管壳式换热器 H1008 管嘴 N4
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                TubularHeatExchanger(tag = "H1008", nozzles = [ "N4" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "47140"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 开口线：源端为空（界外来流），仅连换热器管嘴 N4
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "H1008", nozzle = "N4")
                                    ]
                            }
                        ]
                }
            ]
    }
