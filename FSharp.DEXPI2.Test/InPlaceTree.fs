namespace FSharp.DEXPI2.Test

open Xunit

open FSharp.DEXPI2
open FSharp.DEXPI2.Core
open FSharp.DEXPI2.Plant
open FSharp.DEXPI2.Plant.ProcessEquipment
open FSharp.DEXPI2.Plant.Piping

/// <summary>按图（罐 V-101 → 阀门 XV-101 → 排出口 To Sewer）构建当前模型的实例。
/// 图形：罐（白色矩形框）→ 管线 → 阀门（圆形带翼符号）→ 排出口（双圆圈出页符号 + 向右箭头）。</summary>
module InPlaceTree =
    let dn50 = { NominalDiameterStandard = "50" }

    /// 阀门 XV-101 的入口 / 出口节点
    let n1 =
        {
            SubTagName = "N1"
            NominalPressureStandard = "PN16"
            Nodes = [ dn50 ]
        }

    let tank = Tank { TagName = "V-101"; Nozzles = [ n1 ] }


    /// 阀门 XV-101（OperatedValve；同时是 PipingNetworkSegmentItem / PipingSourceItem / PipingTargetItem）
    let valve: OperatedValve =
        {
            PipingComponentName = "XV-101"
            PipingComponentNumber = "XV-101"
            Nodes = [ dn50 ]
        }

    /// 排出口 To Sewer（PipeOffPageConnector）
    let discharge: PipeOffPageConnector =
        {
            PipeConnectorNumber = "To Sewer"
            Nodes = [ dn50 ]
        }

    /// 工厂模型入口（tank / pns1 / seg1 在位内联）
    let plantModel: PlantModel =
        {
            TaggedPlantItems = [ tank ]
            PipingNetworkSystems =
                [
                    {
                        LineNumber = "PL-101"
                        FluidCode = "W"
                        NominalDiameterStandard = "50"
                        PipingNetworkSystemGroupNumber = ""
                        Segments =
                            [
                                {
                                    SegmentNumber = "S1"
                                    FluidCode = "W"
                                    NominalDiameterStandard = "50"
                                    SourceItem = PipingSourceItem.Nozzle n1
                                    SourceNode = dn50
                                    TargetItem =
                                        PipingTargetItem.PipeOffPageConnector discharge
                                    TargetNode = dn50

                                    Items =
                                        [
                                            PipingNetworkSegmentItem.OperatedValve valve
                                            PipingNetworkSegmentItem.PipeOffPageConnector
                                                discharge
                                        ]

                                    Connections =
                                        [
                                            {
                                                SourceItem =
                                                    PipingSourceItem.Nozzle n1
                                                SourceNode = dn50

                                                TargetItem =
                                                    PipingTargetItem.OperatedValve
                                                        valve
                                                TargetNode = dn50

                                                IsDirectPipingConnection = false
                                            }

                                            {
                                                SourceItem =
                                                    PipingSourceItem.OperatedValve valve
                                                SourceNode = dn50
                                                TargetItem =
                                                    PipingTargetItem.PipeOffPageConnector
                                                        discharge
                                                TargetNode = dn50

                                                IsDirectPipingConnection = false
                                            }
                                        ]
                                }
                            ]
                    }
                ]
        }

    [<Fact>]
    let ``试水：罐-阀-排出口模型构建与字段访问`` () =
        // 罐 V-101
        match plantModel.TaggedPlantItems.[0] with
        | Tank t ->
            Assert.Equal("V-101", t.TagName)
            Assert.Equal(1, t.Nozzles.Length)
            Assert.Equal("N1", t.Nozzles.[0].SubTagName)
            Assert.Equal("PN16", t.Nozzles.[0].NominalPressureStandard)
            Assert.Equal(1, t.Nozzles.[0].Nodes.Length)

        // 管线 PL-101 / 段 S1
        let pns = plantModel.PipingNetworkSystems.[0]
        Assert.Equal("PL-101", pns.LineNumber)
        Assert.Equal("W", pns.FluidCode)
        Assert.Equal(1, pns.Segments.Length)
        let seg = pns.Segments.[0]
        Assert.Equal("S1", seg.SegmentNumber)
        Assert.Equal(2, seg.Items.Length)
        Assert.Equal(2, seg.Connections.Length)

        // 阀门 XV-101
        Assert.Equal("XV-101", valve.PipingComponentName)
        Assert.Equal("XV-101", valve.PipingComponentNumber)
        Assert.Equal(1, valve.Nodes.Length)

        // 排出口 To Sewer
        Assert.Equal("To Sewer", discharge.PipeConnectorNumber)

        //// 连接端点引用同一批节点对象
        //Assert.Same(box xv101Out, box seg.Connections.[0].SourceNode)
        //Assert.Same(box dischargeNode, box seg.Connections.[0].TargetNode)
