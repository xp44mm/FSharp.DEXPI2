module DEXPI2.Reference47124

/// 止回阀 C2：S1 终点，亦为 S2 起点（模块级共享引用）
let swingCheckValve1 = PipingNodeOwner.SwingCheckValve(tag = "C2")


/// 参考 P&ID 47124 线：罐 T4750 出口 N2 → 往复泵 P4712 入口 N1
/// 蝶阀 C1、止回阀 C2 内联；异径管 C3 为单例段 S2，被 S1、S3 引用
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                Tank(tag = "T4750", nozzles = [ "N2" ])
                ReciprocatingPump(tag = "P4712", nozzles = [ "N1" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "47124"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 罐出口 N2 → 蝶阀 C1 → 止回阀 C2 → 异径管
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "T4750", nozzle = "N2")
                                        PipingNodeOwner.ButterflyValve(tag = "C1")
                                        swingCheckValve1
                                        PipingNetworkSegmentSingleton(tag = "S2")
                                    ]
                            }

                            // 异径管 C3 单例段
                            PipingNetworkSegment.reducer "S2"

                            {
                                SegmentNumber = "S3"
                                // 异径管 → 球阀 C4 → 泵入口 N1
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(tag = "S2")
                                        PipingNodeOwner.BallValve(tag = "C4")
                                        PipingNodeOwner.Nozzle(equipment = "P4712", nozzle = "N1")
                                    ]
                            }
                        ]
                }
            ]
    }
