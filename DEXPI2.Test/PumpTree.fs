module DEXPI2.PumpTree

open Xunit
open System.IO

/// 工厂模型入口（罐 / 泵在位内联，段 S1~S4）
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                Tank(tag = "V-101", nozzles = [ "N1" ])
                Pump(tag = "P-101", nozzles = [ "inlet"; "outlet" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "PL-201"
                    Segments =
                        [
                            Segment(
                                "S1",
                                [
                                    PipingNodeOwner.Nozzle(
                                        equipment = "V-101",
                                        nozzle = "N1"
                                    )
                                    PipingNodeOwner.OperatedValve(
                                        tag = "XV-101",
                                        actuatorType = Manual
                                    )
                                    SegmentRef(segmentNumber = "S2")
                                ]
                            )
                            HubSegment "S2"
                            Segment(
                                "S3",
                                [
                                    SegmentRef(segmentNumber = "S2")
                                    PipingNodeOwner.Nozzle(
                                        equipment = "P-101",
                                        nozzle = "inlet"
                                    )
                                ]
                            )
                            Segment(
                                "S4",
                                [
                                    PipingNodeOwner.Nozzle(
                                        equipment = "P-101",
                                        nozzle = "outlet"
                                    )
                                    SegmentRef(segmentNumber = "S5")
                                ]
                            )
                            HubSegment "S5"
                            Segment(
                                "S6",
                                [
                                    PipingNodeOwner.CheckValve(tag = "CV-101")
                                    PipingNodeOwner.PipeOffPageConnector(
                                        pipeConnectorDescription = "排污管道"
                                    )
                                ]
                            )
                        ]
                }
            ]
    }

let setprops () =
    PlantModelProperty.getEntities
        plantModel
        [
            PlantModelProperty.EquipmentFluid("V-101", "Water")
            PlantModelProperty.EquipmentPN("V-101", 1000)

            PlantModelProperty.EquipmentFluid("P-101", "Water")
            PlantModelProperty.EquipmentPN("P-101", 1600)

            PlantModelProperty.NozzleDN("V-101", "N1", 100)
            PlantModelProperty.NozzleDN("P-101", "inlet", 80)
            PlantModelProperty.NozzleDN("P-101", "outlet", 100)

            PlantModelProperty.SegmentDN("PL-201", "S6", 150)
        ]

/// 断言 ModelEntities 各表属性符合级联预期（setprops 应用后的正确结果）。
let assertExpected (e: ModelEntities) =
    // 设备行：fluid/pn 级联设置，dn 无设备级参数保持 0
    let eq tag = e.Equipments |> List.find(fun r -> r.Equipment = tag)
    Assert.Equal("Water", (eq "V-101").Fluid)
    Assert.Equal(1000, (eq "V-101").PN)
    Assert.Equal(0, (eq "V-101").DN)
    Assert.Equal("Water", (eq "P-101").Fluid)
    Assert.Equal(1600, (eq "P-101").PN)
    Assert.Equal(0, (eq "P-101").DN)

    // 管嘴行：fluid 继承设备级；pn 为各自设备管嘴行直写（N1=1000 来自 V-101，inlet/outlet=1600 来自 P-101，
    // 不受网络级联覆盖）；dn 由 NozzleDN 显式设
    let nz equip nozzle = e.Nozzles |> List.find(fun r -> r.Equipment = equip && r.Nozzle = nozzle)
    Assert.Equal("Water", (nz "V-101" "N1").Fluid)
    Assert.Equal(1000, (nz "V-101" "N1").PN)
    Assert.Equal(100, (nz "V-101" "N1").DN)
    Assert.Equal("Water", (nz "P-101" "inlet").Fluid)
    Assert.Equal(1600, (nz "P-101" "inlet").PN)
    Assert.Equal(80, (nz "P-101" "inlet").DN)
    Assert.Equal("Water", (nz "P-101" "outlet").Fluid)
    Assert.Equal(1600, (nz "P-101" "outlet").PN)
    Assert.Equal(100, (nz "P-101" "outlet").DN)

    // 常规段：fluid 全链级联；pn 由排序靠后的 V-101(1000) 级联覆盖网络全链——
    // List.sort 同 case 按字段字典序，"P-101" < "V-101"，故 P-101(1600) 先执行、V-101(1000) 后执行覆盖；
    // dn：S1←N1、S3←inlet、S4←outlet（NozzleDN 沿引用段级联），S6←SegmentDN 显式
    let seg num = e.Segments |> List.find(fun r -> r.SegmentNumber = num)
    for num in [ "S1"; "S3"; "S4"; "S6" ] do
        Assert.Equal("Water", (seg num).Fluid)
        Assert.Equal(1000, (seg num).PN)
    Assert.Equal(100, (seg "S1").DN)
    Assert.Equal(80, (seg "S3").DN)
    Assert.Equal(100, (seg "S4").DN)
    Assert.Equal(150, (seg "S6").DN)

    // 枢纽段：fluid/pn 级联；DNs 按追加次序——NozzleDN 同 case 按 equipment 字典序执行，
    // V-101/N1(100) 最后执行追加在前：S2 → [100; 80]，S5 仅 outlet → [100]
    let hub num = e.HubSegments |> List.find(fun r -> r.SegmentNumber = num)
    Assert.Equal("Water", (hub "S2").Fluid)
    Assert.Equal(1000, (hub "S2").PN)
    Assert.Equal<int list>([ 100; 80 ], (hub "S2").DNs)
    Assert.Equal("Water", (hub "S5").Fluid)
    Assert.Equal(1000, (hub "S5").PN)
    Assert.Equal<int list>([ 100 ], (hub "S5").DNs)

    // 段项：S1/XV-101 由 N1 经 S1 级联；S6/CV-101 与 S6/1 由 SegmentDN 级联；pn 同段为 1000
    let item num tag = e.SegmentItems |> List.find(fun r -> r.SegmentNumber = num && r.Tag = tag)
    let xv = item "S1" "XV-101"
    Assert.Equal("Water", xv.Fluid)
    Assert.Equal(1000, xv.PN)
    Assert.Equal(100, xv.DN)
    let cv = item "S6" "CV-101"
    Assert.Equal("Water", cv.Fluid)
    Assert.Equal(1000, cv.PN)
    Assert.Equal(150, cv.DN)
    let po = item "S6" "1"
    Assert.Equal("Water", po.Fluid)
    Assert.Equal(1000, po.PN)
    Assert.Equal(150, po.DN)

/// 级联传播测试：应用 setprops 后，ModelEntities 各表属性符合预期。
type PumpTreeTest(output: ITestOutputHelper) =

    [<Fact>]
    member _.``setprops 后 ModelEntities 各表属性符合级联预期``() =
        assertExpected (setprops ())

    [<Fact>]
    member _.``PlantModel 保存到 SQLite``() =
        // 数据库放到本测试项目根目录，测试结束保留文件供查看
        let dbPath = Path.GetFullPath(Path.Combine("..", "..", "..", "pump_tree.db"))
        if File.Exists dbPath then
            try File.Delete dbPath with _ -> ()
        use conn = PlantModelDb.createDatabase dbPath
        PlantModelDb.saveModel conn plantModel
        output.WriteLine(sprintf "数据库已生成：%s" dbPath)

    [<Fact>]
    member _.``往返测试：独立模型保存后加载与原模型完全一致``() =
        let dbPath = Path.GetFullPath(Path.Combine("..", "..", "..", "pump_roundtrip.db"))
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
