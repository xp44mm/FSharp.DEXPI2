module DEXPI2.ReferencePid

/// 参考 P&ID（reference_pid.xml）完整拓扑：5 台设备 + 11 条管线系统
/// 设备均为完整实体（管嘴全量）；三通/异径管单例段按 DEXPI2/readme.md 规则拆分
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                PlateHeatExchanger(tag = "H1007", nozzles = [ "N1"; "N2"; "N3"; "N4" ])
                TubularHeatExchanger(tag = "H1008", nozzles = [ "N1"; "N2"; "N3"; "N4" ])
                CentrifugalPump(tag = "P4711", nozzles = [ "N1"; "N2" ])
                ReciprocatingPump(tag = "P4712", nozzles = [ "N1"; "N2" ])
                Tank(
                    tag = "T4750",
                    nozzles =
                        [
                            "N1"
                            "N2"
                            "N3"
                            "N5"
                            "N6"
                            "N7"
                            "N8"
                        ]
                )
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "47121"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 界外入口连接器 → 泵吸入口 N1（无中间管件）
                                Items =
                                    [
                                        PipingNodeOwner.FlowInPipeOffPageConnector(
                                            connector = "FlowInPipeOffPageConnector1"
                                        )
                                        PipingNodeOwner.Nozzle(
                                            equipment = "P4711",
                                            nozzle = "N1"
                                        )
                                    ]
                            }
                        ]
                }

                {
                    LineNumber = "47122"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 泵出口 N2 → 换热器入口 N1（无中间管件）
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(
                                            equipment = "P4711",
                                            nozzle = "N2"
                                        )
                                        PipingNodeOwner.Nozzle(
                                            equipment = "H1007",
                                            nozzle = "N1"
                                        )
                                    ]
                            }
                        ]
                }

                {
                    LineNumber = "47123"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 换热器出口 N2 → 截止阀 C1 → 罐入口 N1
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(
                                            equipment = "H1007",
                                            nozzle = "N2"
                                        )
                                        PipingNodeOwner.GlobeValve(tag = "C1")
                                        PipingNodeOwner.Nozzle(
                                            equipment = "T4750",
                                            nozzle = "N1"
                                        )
                                    ]
                            }
                        ]
                }

                {
                    LineNumber = "47124"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 罐出口 N2 → 蝶阀 C1 → 止回阀 C2 → 异径管单例段 S2
                                // （止回阀一进一出、不改变流道，原 XML 段 S1/S2 合并，内联本段）
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(
                                            equipment = "T4750",
                                            nozzle = "N2"
                                        )
                                        PipingNodeOwner.ButterflyValve(tag = "C1")
                                        PipingNodeOwner.SwingCheckValve(tag = "C2")
                                        PipingNetworkSegmentSingleton(tag = "S2")
                                    ]
                            }

                            // 异径管 C3 单例段（改变管径，须单独成段）
                            PipingNetworkSegment.reducer "S2"

                            {
                                SegmentNumber = "S3"
                                // 异径管 → 球阀 C4 → 泵入口 N1
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(tag = "S2")
                                        PipingNodeOwner.BallValve(tag = "C4")
                                        PipingNodeOwner.Nozzle(
                                            equipment = "P4712",
                                            nozzle = "N1"
                                        )
                                    ]
                            }
                        ]
                }

                {
                    LineNumber = "47125"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 三通（47126 线单例段 S3）→ 弹簧安全阀 → 罐入口 N5
                                // （安全阀一进一出、不分支，原 XML 段 S1/S2 合并，内联本段）
                                Items =
                                    [
                                        PipingNetworkSegmentSingleton(tag = "S3@47126")
                                        PipingNodeOwner.SpringLoadedGlobeSafetyValve(
                                            tag = "SpringLoadedGlobeSafetyValve1"
                                        )
                                        PipingNodeOwner.Nozzle(
                                            equipment = "T4750",
                                            nozzle = "N5"
                                        )
                                    ]
                            }
                        ]
                }

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
                                        PipingNodeOwner.Nozzle(
                                            equipment = "P4712",
                                            nozzle = "N2"
                                        )
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
                                        PipingNodeOwner.Nozzle(
                                            equipment = "H1008",
                                            nozzle = "N1"
                                        )
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
                                        PipingNodeOwner.FlowOutPipeOffPageConnector(
                                            connector = "FlowOutPipeOffPageConnector1"
                                        )
                                    ]
                            }
                        ]
                }

                {
                    LineNumber = "47127"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 换热器出口 N2 → 截止阀 C1 → 罐入口 N6
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(
                                            equipment = "H1008",
                                            nozzle = "N2"
                                        )
                                        PipingNodeOwner.GlobeValve(tag = "C1")
                                        PipingNodeOwner.Nozzle(
                                            equipment = "T4750",
                                            nozzle = "N6"
                                        )
                                    ]
                            }
                        ]
                }

                {
                    LineNumber = "47130"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 开口线：源端为空（界外来流），仅连换热器管嘴 N3
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(
                                            equipment = "H1007",
                                            nozzle = "N3"
                                        )
                                    ]
                            }
                        ]
                }

                {
                    LineNumber = "47131"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 开口线：目标端为空（流出界外），仅连换热器管嘴 N4
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(
                                            equipment = "H1007",
                                            nozzle = "N4"
                                        )
                                    ]
                            }
                        ]
                }

                {
                    LineNumber = "47140"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 开口线：源端为空（界外来流），仅连换热器管嘴 N4
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(
                                            equipment = "H1008",
                                            nozzle = "N4"
                                        )
                                    ]
                            }
                        ]
                }

                {
                    LineNumber = "47141"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 换热器管嘴 N3 → 截止阀 C1 → 界外出口（目标端为空）
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(
                                            equipment = "H1008",
                                            nozzle = "N3"
                                        )
                                        PipingNodeOwner.GlobeValve(tag = "C1")
                                    ]
                            }
                        ]
                }
            ]
    }
