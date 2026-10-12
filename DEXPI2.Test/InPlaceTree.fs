module DEXPI2.InPlaceTree

open Xunit
open System.IO

/// 工厂模型入口（罐 / 管线 PL-101 在位内联，段 S1、S3）
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                Tank(tag = "V-101", nozzles = [ "N1" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "PL-101"

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
                                    PipingNodeOwner.PipeOffPageConnector(
                                        pipeConnectorDescription = "To Sewer"
                                    )
                                ]
                            )

                        ]
                }
            ]
    }

/// 顺序应用一组 PlantModelProperty 到同一个 ModelEntities（累积、级联传播），返回最终内存态。
/// 委托给 PlantModelProperty.getEntities：其内部 List.sort 按 DU 声明顺序
/// （Equipment → … → SegmentItem）全局值先落、局部值后落，局部值才不被父级级联写覆盖。
let setprops () =
    PlantModelProperty.getEntities
        plantModel
        [
            PlantModelProperty.EquipmentFluid("V-101", "Water")
            PlantModelProperty.EquipmentPN("V-101", 1000)
            PlantModelProperty.NozzleDN("V-101", "N1", 100)
            PlantModelProperty.SegmentDN("PL-101", "S3", 150)
        ]

/// 断言 ModelEntities 各表属性符合级联预期（setprops 应用后的正确结果）。
let assertExpected (e: ModelEntities) =
    // 设备行：fluid/pn 级联设置，dn 无设备级参数保持 0
    let eq = e.Equipments |> List.find(fun r -> r.Equipment = "V-101")
    Assert.Equal("Water", eq.Fluid)
    Assert.Equal(1000, eq.PN)
    Assert.Equal(0, eq.DN)

    // 管嘴行：fluid/pn 继承设备级，dn 由 NozzleDN 显式设 100
    let nz = e.Nozzles |> List.find(fun r -> r.Nozzle = "N1")
    Assert.Equal("Water", nz.Fluid)
    Assert.Equal(1000, nz.PN)
    Assert.Equal(100, nz.DN)

    // 常规段：fluid/pn 全链级联；S1 dn=100（NozzleDN 沿引用段级联），S3 dn=150（SegmentDN 显式）
    let seg (line: string) = e.Segments |> List.find(fun r -> r.SegmentNumber = line)
    for line in [ "S1"; "S3" ] do
        Assert.Equal("Water", (seg line).Fluid)
        Assert.Equal(1000, (seg line).PN)
    Assert.Equal(100, (seg "S1").DN)
    Assert.Equal(150, (seg "S3").DN)

    // 枢纽段 S2：fluid/pn 级联；DNs 按追加次序 [150; 100]（先 NozzleDN 追加 100，后 SegmentDN 追加 150）
    let hub = e.HubSegments |> List.find(fun r -> r.SegmentNumber = "S2")
    Assert.Equal("Water", hub.Fluid)
    Assert.Equal(1000, hub.PN)
    Assert.Equal<int list>([ 150; 100 ], hub.DNs)

    // 段项：XV-101（S1）由 NozzleDN 经 S1 级联；S3/1 由 SegmentDN 级联
    let item (line: string) (tag: string) =
        e.SegmentItems |> List.find(fun r -> r.SegmentNumber = line && r.Tag = tag)
    let xv = item "S1" "XV-101"
    Assert.Equal("Water", xv.Fluid)
    Assert.Equal(1000, xv.PN)
    Assert.Equal(100, xv.DN)
    let po = item "S3" "1"
    Assert.Equal("Water", po.Fluid)
    Assert.Equal(1000, po.PN)
    Assert.Equal(150, po.DN)

/// 级联传播测试：应用 setprops 后，ModelEntities 各表属性符合预期。
type InPlaceTreeTest(output: ITestOutputHelper) =

    [<Fact>]
    member _.``setprops 后 ModelEntities 各表属性符合级联预期``() =
        assertExpected (setprops ())

    [<Fact>]
    member _.``倒叙设置列表经 getEntities 内部 List.sort 后仍按 DU 顺序执行，结果与正序一致``() =
        // 与 setprops 相同的 4 条设置，仅列表顺序颠倒，直接交给 getEntities。
        // 其内部 List.sort 按 DU 声明顺序（Equipment → … → SegmentItem）重排，全局值先落、局部值后落；
        // 若去掉排序，倒序执行会使 NozzleDN 先于 SegmentDN，枢纽段 S2 的 DNs 追加次序颠倒为 [100; 150]，
        // 且后执行的父级级联（EquipmentPN/EquipmentFluid）会覆盖先设的局部值，断言将失败。
        let e =
            PlantModelProperty.getEntities
                plantModel
                [
                    PlantModelProperty.SegmentDN("PL-101", "S3", 150)
                    PlantModelProperty.NozzleDN("V-101", "N1", 100)
                    PlantModelProperty.EquipmentPN("V-101", 1000)
                    PlantModelProperty.EquipmentFluid("V-101", "Water")
                ]
        assertExpected e

    [<Fact>]
    member _.``PlantModel 保存到 SQLite``() =
        // 数据库放到本测试项目根目录，测试结束保留文件供查看
        let dbPath = Path.GetFullPath(Path.Combine("..", "..", "..", "in_place_tree.db"))
        if File.Exists dbPath then
            try File.Delete dbPath with _ -> ()
        use conn = PlantModelDb.createDatabase dbPath
        PlantModelDb.saveModel conn plantModel
        output.WriteLine(sprintf "数据库已生成：%s" dbPath)

    [<Fact>]
    member _.``往返测试：独立模型保存后加载与原模型完全一致``() =
        let dbPath = Path.GetFullPath(Path.Combine("..", "..", "..", "roundtrip.db"))
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
