module DEXPI2.Reference47125

/// 弹簧安全阀：S1 终点，亦为 S2 起点（模块级共享引用）
let springLoadedGlobeSafetyValve1 = PipingNodeOwner.SpringLoadedGlobeSafetyValve(tag = "SpringLoadedGlobeSafetyValve1")


/// 参考 P&ID 47125 线：47126 线三通 C3（单例段 S3@47126）→ 弹簧安全阀 → 罐 T4750 入口 N5
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                Tank(tag = "T4750", nozzles = [ "N5" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "47125"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 三通（属于 47126 线的单例段 S3）→ 弹簧安全阀
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(tag = "S3@47126")
                                        springLoadedGlobeSafetyValve1
                                    ]
                            }

                            {
                                SegmentNumber = "S2"
                                // 弹簧安全阀 → 罐入口 N5
                                Items =
                                    [
                                        springLoadedGlobeSafetyValve1
                                        PipingNodeOwner.Nozzle(equipment = "T4750", nozzle = "N5")
                                    ]
                            }
                        ]
                }
            ]
    }
