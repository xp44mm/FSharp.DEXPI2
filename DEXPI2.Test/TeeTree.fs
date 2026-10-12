module DEXPI2.TeeTree

open Xunit
open System.IO

/// 工厂模型入口（罐在位内联，段 S1~S4；S2 为三通单例段）
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                Tank(tag = "V-101", nozzles = [ "N1" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "PL-301"

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
                                        pipeConnectorDescription = "Outlet1"
                                    )
                                ]
                            )

                            Segment(
                                "S4",
                                [
                                    SegmentRef(segmentNumber = "S2")

                                    PipingNodeOwner.PipeOffPageConnector(
                                        pipeConnectorDescription = "Outlet2"
                                    )
                                ]
                            )
                        ]
                }
            ]
    }

/// 本树设置语句：罐 V-101 级联 fluid/pn；N1 管嘴 dn=100 沿 S1 级联（S2 追加 100）；
/// S3、S4 出口同为 dn=80（S2 再追加 80、80，三通三端点即 [80; 80; 100]）。
let setprops () =
    PlantModelProperty.getEntities
        plantModel
        [
            PlantModelProperty.EquipmentFluid("V-101", "Water")
            PlantModelProperty.EquipmentPN("V-101", 1000)

            PlantModelProperty.NozzleDN("V-101", "N1", 100)

            PlantModelProperty.SegmentDN("PL-301", "S3", 80)
            PlantModelProperty.SegmentDN("PL-301", "S4", 80)
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

    // 常规段：fluid/pn 全链级联；dn：S1←N1（NozzleDN 沿引用段级联），S3/S4←SegmentDN 显式
    let seg num = e.Segments |> List.find(fun r -> r.SegmentNumber = num)
    for num in [ "S1"; "S3"; "S4" ] do
        Assert.Equal("Water", (seg num).Fluid)
        Assert.Equal(1000, (seg num).PN)
    Assert.Equal(100, (seg "S1").DN)
    Assert.Equal(80, (seg "S3").DN)
    Assert.Equal(80, (seg "S4").DN)

    // 枢纽段 S2（三通）：fluid/pn 级联；DNs 按追加次序——S1 引用先追 100，S3 追 80，S4 追 80 → [80; 80; 100]
    let hub = e.HubSegments |> List.find(fun r -> r.SegmentNumber = "S2")
    Assert.Equal("Water", hub.Fluid)
    Assert.Equal(1000, hub.PN)
    Assert.Equal<int list>([ 80; 80; 100 ], hub.DNs)

    // 段项：S1/XV-101 由 N1 经 S1 级联；S3/1、S4/1（Outlet1/Outlet2）由各自 SegmentDN 级联
    let item num tag = e.SegmentItems |> List.find(fun r -> r.SegmentNumber = num && r.Tag = tag)
    let xv = item "S1" "XV-101"
    Assert.Equal("Water", xv.Fluid)
    Assert.Equal(1000, xv.PN)
    Assert.Equal(100, xv.DN)
    let o1 = item "S3" "1"
    Assert.Equal("Water", o1.Fluid)
    Assert.Equal(1000, o1.PN)
    Assert.Equal(80, o1.DN)
    let o2 = item "S4" "1"
    Assert.Equal("Water", o2.Fluid)
    Assert.Equal(1000, o2.PN)
    Assert.Equal(80, o2.DN)

/// 级联传播测试：应用 setprops 后，ModelEntities 各表属性符合预期。
type TeeTreeTest(output: ITestOutputHelper) =

    [<Fact>]
    member _.``setprops 后 ModelEntities 各表属性符合级联预期``() =
        assertExpected (setprops ())

    [<Fact>]
    member _.``PlantModel 保存到 SQLite``() =
        // 数据库放到本测试项目根目录，测试结束保留文件供查看
        let dbPath = Path.GetFullPath(Path.Combine("..", "..", "..", "tee_tree.db"))
        if File.Exists dbPath then
            try File.Delete dbPath with _ -> ()
        use conn = PlantModelDb.createDatabase dbPath
        PlantModelDb.saveModel conn plantModel
        output.WriteLine(sprintf "数据库已生成：%s" dbPath)

    [<Fact>]
    member _.``往返测试：独立模型保存后加载与原模型完全一致``() =
        let dbPath = Path.GetFullPath(Path.Combine("..", "..", "..", "tee_roundtrip.db"))
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
