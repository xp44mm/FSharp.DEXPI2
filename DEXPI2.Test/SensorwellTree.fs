module DEXPI2.SensorwellTree

open Xunit
open System.IO

/// 工厂模型入口（两罐在位内联，段 S1 中部挂传感器套管 SW-101 作压力测点 PT）
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                Tank(tag = "V-101", nozzles = [ "N1" ])
                Tank(tag = "V-102", nozzles = [ "N1" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "PL-401"

                    Segments =
                        [
                            Segment(
                                "S1",
                                // 罐 V-101（左）→ 传感器套管 SW-101（管道中部测点 PT）→ 罐 V-102（右）

                                    [
                                        PipingNodeOwner.Nozzle(equipment = "V-101", nozzle = "N1")
                                        PipingNodeOwner.Sensorwell(typeRepresentation = "PT")
                                        PipingNodeOwner.Nozzle(equipment = "V-102", nozzle = "N1")
                                    ]
                            )
                        ]
                }
            ]
    }

/// 本树设置语句：两罐 V-101/V-102 级联 fluid 与相同 pn=1000；两个 NozzleDN(100) 分别直写各自管嘴并沿 S1 级联——
/// N1(V-102) 先被 NozzleDN(V-101) 的段级联设为 100（默认则设），再被自身 NozzleDN 直写 100（一致静默）；
/// 段项 Sensorwell 也由级联写 100。注意：S1 段内含管嘴引用，不能用 SegmentDN 显式写段（会触发核对冲突）。
let setprops () =
    PlantModelProperty.getEntities
        plantModel
        [
            PlantModelProperty.EquipmentFluid("V-101", "Water")
            PlantModelProperty.EquipmentPN("V-101", 1000)
            PlantModelProperty.NozzleDN("V-101", "N1", 100)

            PlantModelProperty.EquipmentFluid("V-102", "Water")
            PlantModelProperty.EquipmentPN("V-102", 1000)
            PlantModelProperty.NozzleDN("V-102", "N1", 100)

        ]

/// 断言 ModelEntities 各表属性符合级联预期（setprops 应用后的正确结果）。
let assertExpected (e: ModelEntities) =
    // 设备行：fluid/pn 级联设置，dn 无设备级参数保持 0
    let eq tag = e.Equipments |> List.find(fun r -> r.Equipment = tag)
    Assert.Equal("Water", (eq "V-101").Fluid)
    Assert.Equal(1000, (eq "V-101").PN)
    Assert.Equal(0, (eq "V-101").DN)
    Assert.Equal("Water", (eq "V-102").Fluid)
    Assert.Equal(1000, (eq "V-102").PN)
    Assert.Equal(0, (eq "V-102").DN)

    // 管嘴行：fluid 继承设备级；pn 为各自设备管嘴行直写（两罐同为 1000）；
    // dn 由各自 NozzleDN(100) 直写并经 S1 级联互相传播——N1(V-102) 先被 V-101 的段级联设为 100（默认则设），
    // 再被自身 NozzleDN 直写 100（一致静默）
    let nz equip = e.Nozzles |> List.find(fun r -> r.Equipment = equip && r.Nozzle = "N1")
    Assert.Equal("Water", (nz "V-101").Fluid)
    Assert.Equal(1000, (nz "V-101").PN)
    Assert.Equal(100, (nz "V-101").DN)
    Assert.Equal("Water", (nz "V-102").Fluid)
    Assert.Equal(1000, (nz "V-102").PN)
    Assert.Equal(100, (nz "V-102").DN)

    // 常规段 S1：fluid/pn 全链级联（两罐 pn 同为 1000）；dn=100 由 NozzleDN 沿引用段级联
    let seg = e.Segments |> List.find(fun r -> r.SegmentNumber = "S1")
    Assert.Equal("Water", seg.Fluid)
    Assert.Equal(1000, seg.PN)
    Assert.Equal(100, seg.DN)

    // 段项 S1/1（Sensorwell PT，无名管件 Tag="1"）：fluid/pn 同段；dn=100 由 NozzleDN 沿 S1 级联写入
    let sw = e.SegmentItems |> List.find(fun r -> r.Tag = "1")
    Assert.Equal("Sensorwell(PT)", sw.Type)
    Assert.Equal("Water", sw.Fluid)
    Assert.Equal(1000, sw.PN)
    Assert.Equal(100, sw.DN)

/// 级联传播测试：应用 setprops 后，ModelEntities 各表属性符合预期。
type SensorwellTreeTest(output: ITestOutputHelper) =

    [<Fact>]
    member _.``setprops 后 ModelEntities 各表属性符合级联预期``() =
        assertExpected (setprops ())

    [<Fact>]
    member _.``PlantModel 保存到 SQLite``() =
        // 数据库放到本测试项目根目录，测试结束保留文件供查看
        let dbPath = Path.GetFullPath(Path.Combine("..", "..", "..", "sensorwell_tree.db"))
        if File.Exists dbPath then
            try File.Delete dbPath with _ -> ()
        use conn = PlantModelDb.createDatabase dbPath
        PlantModelDb.saveModel conn plantModel
        output.WriteLine(sprintf "数据库已生成：%s" dbPath)

    [<Fact>]
    member _.``往返测试：独立模型保存后加载与原模型完全一致``() =
        let dbPath = Path.GetFullPath(Path.Combine("..", "..", "..", "sensorwell_roundtrip.db"))
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
