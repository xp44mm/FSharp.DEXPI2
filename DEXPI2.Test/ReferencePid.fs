module DEXPI2.ReferencePid

open Xunit
open System.IO

/// 参考 P&ID（reference_pid.xml）完整拓扑：5 台设备 + 10 条管线系统（47125 安全阀支路已并入 47126）
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
                            Segment(
                                "S1",
                                [
                                    PipingNodeOwner.FlowInPipeOffPageConnector(
                                        pipeConnectorDescription = ""
                                    )
                                    PipingNodeOwner.Nozzle(
                                        equipment = "P4711",
                                        nozzle = "N1"
                                    )
                                ]
                            )
                        ]
                }

                {
                    LineNumber = "47122"
                    Segments =
                        [
                            Segment(
                                "S1",
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
                            )
                        ]
                }

                {
                    LineNumber = "47123"

                    Segments =
                        [
                            Segment(
                                "S1",
                                [
                                    PipingNodeOwner.Nozzle(
                                        equipment = "H1007",
                                        nozzle = "N2"
                                    )
                                    PipingNodeOwner.GlobeValve(
                                        tag = "C1",
                                        actuatorType = Manual
                                    )
                                    PipingNodeOwner.Nozzle(
                                        equipment = "T4750",
                                        nozzle = "N1"
                                    )
                                ]
                            )
                        ]
                }

                {
                    LineNumber = "47124"

                    Segments =
                        [
                            Segment(
                                "S1",
                                [
                                    PipingNodeOwner.Nozzle(
                                        equipment = "T4750",
                                        nozzle = "N2"
                                    )
                                    PipingNodeOwner.ButterflyValve(
                                        tag = "C1",
                                        actuatorType = Manual
                                    )
                                    PipingNodeOwner.SwingCheckValve(tag = "C2")
                                    SegmentRef(segmentNumber = "S2")
                                ]
                            )

                            HubSegment "S2"

                            Segment(
                                "S3",

                                [
                                    SegmentRef(segmentNumber = "S2")
                                    PipingNodeOwner.BallValve(
                                        tag = "C4",
                                        actuatorType = Manual
                                    )
                                    PipingNodeOwner.Nozzle(
                                        equipment = "P4712",
                                        nozzle = "N1"
                                    )
                                ]
                            )
                        ]
                }

                {
                    LineNumber = "47126"

                    Segments =
                        [
                            Segment(
                                "S12",

                                [
                                    PipingNodeOwner.Nozzle(
                                        equipment = "P4712",
                                        nozzle = "N2"
                                    )
                                    SegmentRef(segmentNumber = "S1")
                                ]
                            )
                            HubSegment "S1"

                            Segment(
                                "S2",

                                [
                                    SegmentRef(segmentNumber = "S1")
                                    PipingNodeOwner.BallValve(
                                        tag = "C2",
                                        actuatorType = Manual
                                    )
                                ]
                            )

                            Segment(
                                "S13",
                                [
                                    SegmentRef(segmentNumber = "S1")
                                    PipingNodeOwner.SpringLoadedGlobeSafetyValve(
                                        tag = "SpringLoadedGlobeSafetyValve1"
                                    )
                                    PipingNodeOwner.Nozzle(
                                        equipment = "T4750",
                                        nozzle = "N5"
                                    )
                                ]
                            )

                            Segment(
                                "S5",
                                [
                                    SegmentRef(segmentNumber = "S1")
                                    PipingNodeOwner.BallValve(
                                        tag = "C5",
                                        actuatorType = Manual
                                    )
                                    PipingNodeOwner.BlindFlange
                                ]
                            )

                            Segment(
                                "S6",
                                [
                                    SegmentRef(segmentNumber = "S1")
                                    PipingNodeOwner.BallValve(
                                        tag = "C7",
                                        actuatorType = Manual
                                    )
                                    SegmentRef(segmentNumber = "S11")
                                ]
                            )

                            HubSegment "S11"

                            Segment(
                                "S7",

                                [
                                    SegmentRef(segmentNumber = "S11")
                                    PipingNodeOwner.Nozzle(
                                        equipment = "H1008",
                                        nozzle = "N1"
                                    )
                                ]
                            )

                            Segment(
                                "S9",

                                [
                                    SegmentRef(segmentNumber = "S11")
                                    PipingNodeOwner.BallValve(
                                        tag = "C10",
                                        actuatorType = Manual
                                    )
                                    PipingNodeOwner.BlindFlange
                                ]
                            )

                            Segment(
                                "S10",

                                [
                                    SegmentRef(segmentNumber = "S11")
                                    PipingNodeOwner.FlowOutPipeOffPageConnector(
                                        pipeConnectorDescription = ""
                                    )
                                ]
                            )
                        ]
                }

                {
                    LineNumber = "47127"

                    Segments =
                        [
                            Segment(
                                "S1",

                                [
                                    PipingNodeOwner.Nozzle(
                                        equipment = "H1008",
                                        nozzle = "N2"
                                    )
                                    PipingNodeOwner.GlobeValve(
                                        tag = "C1",
                                        actuatorType = Manual
                                    )
                                    PipingNodeOwner.Nozzle(
                                        equipment = "T4750",
                                        nozzle = "N6"
                                    )
                                ]
                            )
                        ]
                }

                {
                    LineNumber = "47130"

                    Segments =
                        [
                            Segment(
                                "S1",

                                [
                                    PipingNodeOwner.FlowInPipeOffPageConnector(
                                        pipeConnectorDescription = ""
                                    )
                                    PipingNodeOwner.Nozzle(
                                        equipment = "H1007",
                                        nozzle = "N3"
                                    )
                                ]
                            )
                        ]
                }

                {
                    LineNumber = "47131"

                    Segments =
                        [
                            Segment(
                                "S1",

                                [
                                    PipingNodeOwner.Nozzle(
                                        equipment = "H1007",
                                        nozzle = "N4"
                                    )
                                    PipingNodeOwner.FlowOutPipeOffPageConnector(
                                        pipeConnectorDescription = ""
                                    )
                                ]
                            )
                        ]
                }

                {
                    LineNumber = "47140"

                    Segments =
                        [
                            Segment(
                                "S1",

                                [
                                    PipingNodeOwner.FlowInPipeOffPageConnector(
                                        pipeConnectorDescription = ""
                                    )
                                    PipingNodeOwner.Nozzle(
                                        equipment = "H1008",
                                        nozzle = "N4"
                                    )
                                ]
                            )
                        ]
                }

                {
                    LineNumber = "47141"

                    Segments =
                        [
                            Segment(
                                "S1",

                                [
                                    PipingNodeOwner.Nozzle(
                                        equipment = "H1008",
                                        nozzle = "N3"
                                    )
                                    PipingNodeOwner.GlobeValve(
                                        tag = "C1",
                                        actuatorType = Manual
                                    )
                                    PipingNodeOwner.FlowOutPipeOffPageConnector(
                                        pipeConnectorDescription = ""
                                    )
                                ]
                            )
                        ]
                }
            ]
    }

/// PN 由 PipingClassCode 等级相对大小取值：75HB13（较高）→ 1600 kPa；73HG12（较低）→ 1000 kPa。
/// 各线 fluid/pn/dn 来自 reference_pid.xml 的系统级属性；73HG12 低压段（47124/S2 异径管、S3、
/// 47126/S13 原 47125 安全阀支路）用段级覆盖。
let setprops () =
    PlantModelProperty.getEntities
        plantModel
        [
            // ---- 47121 泵吸入线：MNb / 75HB13 / DN80
            PlantModelProperty.NetworkFluid("47121", "MNb")
            PlantModelProperty.NetworkPN("47121", 1600)
            PlantModelProperty.NetworkDN("47121", 80)
            // ---- 47122 泵出→换热器：MNb / 75HB13 / DN80
            PlantModelProperty.NetworkFluid("47122", "MNb")
            PlantModelProperty.NetworkPN("47122", 1600)
            PlantModelProperty.NetworkDN("47122", 80)
            // ---- 47123 换热器→罐：MNb / 75HB13 / DN80
            PlantModelProperty.NetworkFluid("47123", "MNb")
            PlantModelProperty.NetworkPN("47123", 1600)
            PlantModelProperty.NetworkDN("47123", 80)
            // ---- 47124 罐→泵：MNc / 75HB13 / DN80；S2 异径管、S3 球阀段为 73HG12 低压
            PlantModelProperty.NetworkFluid("47124", "MNc")
            PlantModelProperty.NetworkPN("47124", 1600)
            PlantModelProperty.NetworkDN("47124", 80)
            PlantModelProperty.SegmentPN("47124", "S2", 1000)
            PlantModelProperty.SegmentPN("47124", "S3", 1000)
            PlantModelProperty.SegmentDN("47124", "S3", 50)
            // ---- 47126 泵出支路：MNc / 75HB13 / DN50；S13（原 47125 安全阀支路）为 73HG12 / DN25
            PlantModelProperty.NetworkFluid("47126", "MNc")
            PlantModelProperty.NetworkPN("47126", 1600)
            PlantModelProperty.NetworkDN("47126", 50)
            PlantModelProperty.SegmentPN("47126", "S13", 1000)
            PlantModelProperty.SegmentDN("47126", "S13", 25)
            // ---- 47127 换热器→罐：MNc / 75HB13 / DN50
            PlantModelProperty.NetworkFluid("47127", "MNc")
            PlantModelProperty.NetworkPN("47127", 1600)
            PlantModelProperty.NetworkDN("47127", 50)
            // ---- 47130/47131 H1007 公用工程线：WKa/WKb / 75HB13 / DN50
            PlantModelProperty.NetworkFluid("47130", "WKa")
            PlantModelProperty.NetworkPN("47130", 1600)
            PlantModelProperty.NetworkDN("47130", 50)
            PlantModelProperty.NetworkFluid("47131", "WKb")
            PlantModelProperty.NetworkPN("47131", 1600)
            PlantModelProperty.NetworkDN("47131", 50)
            // ---- 47140/47141 H1008 公用工程线：QSa/QSb / 75HB13 / DN50
            PlantModelProperty.NetworkFluid("47140", "QSa")
            PlantModelProperty.NetworkPN("47140", 1600)
            PlantModelProperty.NetworkDN("47140", 50)
            PlantModelProperty.NetworkFluid("47141", "QSb")
            PlantModelProperty.NetworkPN("47141", 1600)
            PlantModelProperty.NetworkDN("47141", 50)
        ]

/// 断言 ModelEntities 各表属性符合 reference_pid.xml 参考值（setprops 应用后的正确结果）。
let assertExpected (e: ModelEntities) =
    // 常规段：fluid/pn/dn（pn 由 NetworkPN 1600 打底，73HG12 低压段被 SegmentPN 覆盖 1000；
    // dn 由 NetworkDN 打底，47124/S3 与 47126/S13 被 SegmentDN 覆盖 50/25）
    let seg line num =
        e.Segments
        |> List.find(fun r ->
            r.LineNumber = line
            && r.SegmentNumber = num
        )
    for line, fluid, dn in
        [
            "47121", "MNb", 80
            "47122", "MNb", 80
            "47123", "MNb", 80
            "47127", "MNc", 50
            "47130", "WKa", 50
            "47131", "WKb", 50
            "47140", "QSa", 50
            "47141", "QSb", 50
        ] do
        let r = seg line "S1"
        Assert.Equal(fluid, r.Fluid)
        Assert.Equal(1600, r.PN)
        Assert.Equal(dn, r.DN)

    // 47124：S1 主段 1600/80；S3（73HG12 球阀段）覆盖 1000/50
    Assert.Equal("MNc", (seg "47124" "S1").Fluid)
    Assert.Equal(1600, (seg "47124" "S1").PN)
    Assert.Equal(80, (seg "47124" "S1").DN)
    Assert.Equal("MNc", (seg "47124" "S3").Fluid)
    Assert.Equal(1000, (seg "47124" "S3").PN)
    Assert.Equal(50, (seg "47124" "S3").DN)

    // 47126 各段（S12/S2/S5/S6/S7/S9/S10 主段 1600/50；S13 覆盖 1000/25）
    for num in
        [
            "S12"
            "S2"
            "S5"
            "S6"
            "S7"
            "S9"
            "S10"
        ] do
        let r = seg "47126" num
        Assert.Equal("MNc", r.Fluid)
        Assert.Equal(1600, r.PN)
        Assert.Equal(50, r.DN)
    Assert.Equal("MNc", (seg "47126" "S13").Fluid)
    Assert.Equal(1000, (seg "47126" "S13").PN)
    Assert.Equal(25, (seg "47126" "S13").DN)

    // 枢纽段：
    // 47124/S2 异径管：NetworkDN 复制 [80;80]，S3 段级联追加 50 → [50;80;80]；PN 覆盖 1000
    // 47126/S1 三通：NetworkDN 复制 [50×5]，S13 段级联追加 25 → [25;50;50;50;50;50]；
    //   PN 被 S13 递归覆盖 1000（SegmentRef S1）
    // 47126/S11 三通：NetworkDN 复制 [50×4]，无段级覆盖 → [50;50;50;50]/1600
    let hub line num =
        e.HubSegments
        |> List.find(fun r ->
            r.LineNumber = line
            && r.SegmentNumber = num
        )
    let h2 = hub "47124" "S2"
    Assert.Equal("MNc", h2.Fluid)
    Assert.Equal(1000, h2.PN)
    Assert.Equal<int list>([ 50; 80; 80 ], h2.DNs)
    let h1 = hub "47126" "S1"
    Assert.Equal("MNc", h1.Fluid)
    Assert.Equal(1000, h1.PN)
    Assert.Equal<int list>([ 25; 50; 50; 50; 50; 50 ], h1.DNs)
    let h11 = hub "47126" "S11"
    Assert.Equal("MNc", h11.Fluid)
    Assert.Equal(1600, h11.PN)
    Assert.Equal<int list>([ 50; 50; 50; 50 ], h11.DNs)

    // 管嘴：Network* 级联不写管嘴行，仅段级级联写入被引用管嘴——
    // P4712/N1 由 47124/S3 级联写 1000/50；T4750/N5 由 47126/S13 级联写 1000/25；其余保持默认
    let nz eq n =
        e.Nozzles
        |> List.find(fun r -> r.Equipment = eq && r.Nozzle = n)
    let p4712n1 = nz "P4712" "N1"
    Assert.Equal(1000, p4712n1.PN)
    Assert.Equal(50, p4712n1.DN)
    let t4750n5 = nz "T4750" "N5"
    Assert.Equal(1000, t4750n5.PN)
    Assert.Equal(25, t4750n5.DN)
    let p4711n1 = nz "P4711" "N1"
    Assert.Equal("", p4711n1.Fluid)
    Assert.Equal(0, p4711n1.PN)
    Assert.Equal(0, p4711n1.DN)

    // 代表性段项：S13 弹簧安全阀与 S3 球阀 C4 随段级联写（MNc/1000/25、MNc/1000/50）
    let item line num tag =
        e.SegmentItems
        |> List.find(fun r ->
            r.LineNumber = line
            && r.SegmentNumber = num
            && r.Tag = tag
        )
    let sv = item "47126" "S13" "SpringLoadedGlobeSafetyValve1"
    Assert.Equal("MNc", sv.Fluid)
    Assert.Equal(1000, sv.PN)
    Assert.Equal(25, sv.DN)
    let bv = item "47124" "S3" "C4"
    Assert.Equal("MNc", bv.Fluid)
    Assert.Equal(1000, bv.PN)
    Assert.Equal(50, bv.DN)

/// 级联传播测试：应用 setprops 后，ModelEntities 各表属性符合 XML 参考值。
type ReferencePidTest(output: ITestOutputHelper) =

    [<Fact>]
    member _.``setprops 后 ModelEntities 各表属性符合 reference_pid.xml 参考值``() =
        assertExpected(setprops())

    [<Fact>]
    member _.``PlantModel 保存到 SQLite``() =
        // 数据库放到本测试项目根目录，测试结束保留文件供查看
        let dbPath = Path.GetFullPath(Path.Combine("..", "..", "..", "reference_pid.db"))
        if File.Exists dbPath then
            try File.Delete dbPath with _ -> ()
        use conn = PlantModelDb.createDatabase dbPath
        PlantModelDb.saveModel conn plantModel
        output.WriteLine(sprintf "数据库已生成：%s" dbPath)

    [<Fact>]
    member _.``往返测试：独立模型保存后加载与原模型完全一致``() =
        let dbPath = Path.GetFullPath(Path.Combine("..", "..", "..", "referencepid_roundtrip.db"))
        if File.Exists dbPath then
            try File.Delete dbPath with _ -> ()
        try
            use conn = PlantModelDb.createDatabase dbPath
            PlantModelDb.saveModel conn plantModel
            let loaded = PlantModelDb.loadModel conn
            Assert.Equal(plantModel, loaded)
        finally
            if File.Exists dbPath then
                try File.Delete dbPath with _ -> ()
