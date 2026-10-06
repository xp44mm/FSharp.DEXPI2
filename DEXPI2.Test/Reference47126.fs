module DEXPI2.Reference47126


/// 参考 P&ID 47126 线：往复泵 P4712 出口 N2 进入三通管网，界外出口结束
/// 三通单例段：PipeTee2=S1、PipeTee1=S3、PipeTee3=S4、PipeTee4=S11（由 S6 拆出）、PipeTee5=S8
/// 连接段 S12~S15 由"仅含三通"的 XML 段拆出，链路段 S1~S10 保留 XML 段号
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                ReciprocatingPump(tag = "P4712", nozzles = [ "N2" ])
                TubularHeatExchanger(tag = "H1008", nozzles = [ "N1" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "47126"

                    Segments =
                        [
                            // 三通 C1 单例段：星型连接，被 S12、S2、S13 引用
                            PipingNetworkSegment.tee "S1"

                            {
                                SegmentNumber = "S12"
                                // 泵出口 N2 → 三通 C1（原 XML 段 S1 的连接部分）
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "P4712", nozzle = "N2")
                                        PipingNetworkSegmentSingleton(tag = "S1")
                                    ]
                            }

                            {
                                SegmentNumber = "S2"
                                // 三通 C1 → 球阀 C2
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(tag = "S1")
                                        PipingNodeOwner.BallValve(tag = "C2")
                                    ]
                            }

                            // 三通 C3 单例段：被 S13、S14 及 47125 线引用
                            PipingNetworkSegment.tee "S3"

                            {
                                SegmentNumber = "S13"
                                // 三通 C1 → 三通 C3（原 XML 段 S3 的连接部分）
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(tag = "S1")
                                        PipingNetworkSegmentSingleton(tag = "S3")
                                    ]
                            }

                            // 三通 C4 单例段：被 S14、S5、S6 引用
                            PipingNetworkSegment.tee "S4"

                            {
                                SegmentNumber = "S14"
                                // 三通 C3 → 三通 C4（原 XML 段 S4 的连接部分）
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(tag = "S3")
                                        PipingNetworkSegmentSingleton(tag = "S4")
                                    ]
                            }

                            {
                                SegmentNumber = "S5"
                                // 三通 C4 → 球阀 C5 → 盲板 C6
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(tag = "S4")
                                        PipingNodeOwner.BallValve(tag = "C5")
                                        PipingNodeOwner.BlindFlange
                                    ]
                            }

                            {
                                SegmentNumber = "S6"
                                // 三通 C4 → 球阀 C7 → 三通 C8（C8 拆出为单例段 S11）
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(tag = "S4")
                                        PipingNodeOwner.BallValve(tag = "C7")
                                        PipingNetworkSegmentSingleton(tag = "S11")
                                    ]
                            }

                            // 三通 C8 单例段（由原 S6 拆出）：被 S6、S7、S15 引用
                            PipingNetworkSegment.tee "S11"

                            {
                                SegmentNumber = "S7"
                                // 三通 C8 → 换热器 H1008 入口 N1
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(tag = "S11")
                                        PipingNodeOwner.Nozzle(equipment = "H1008", nozzle = "N1")
                                    ]
                            }

                            // 三通 C9 单例段：被 S15、S9、S10 引用
                            PipingNetworkSegment.tee "S8"

                            {
                                SegmentNumber = "S15"
                                // 三通 C8 → 三通 C9（原 XML 段 S8 的连接部分）
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(tag = "S11")
                                        PipingNetworkSegmentSingleton(tag = "S8")
                                    ]
                            }

                            {
                                SegmentNumber = "S9"
                                // 三通 C9 → 球阀 C10 → 盲板 C11
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(tag = "S8")
                                        PipingNodeOwner.BallValve(tag = "C10")
                                        PipingNodeOwner.BlindFlange
                                    ]
                            }

                            {
                                SegmentNumber = "S10"
                                // 三通 C9 → 界外出口连接器
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(tag = "S8")
                                        PipingNodeOwner.FlowOutPipeOffPageConnector(connector = "FlowOutPipeOffPageConnector1")
                                    ]
                            }
                        ]
                }
            ]
    }
