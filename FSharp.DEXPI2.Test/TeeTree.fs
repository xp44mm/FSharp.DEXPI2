namespace FSharp.DEXPI2.Test

open Xunit

open FSharp.DEXPI2
open FSharp.DEXPI2.Plant
open FSharp.DEXPI2.Plant.ProcessEquipment
open FSharp.DEXPI2.Plant.Piping

/// <summary>按图（罐 V-101 → dn100 管道 → 截止阀 XV-101 → 球阀 XV-102 → 三通 T-101 → 两路 dn80 管道）构建三通管线树。
/// 图形：罐（矩形框）→ 管线 dn100 → 截止阀（圆形带翼符号）→ 球阀（圆形符号）→ 三通（T 形符号）→ 两路 dn80 → 向右箭头。
/// 段按规范（reference_pid.xml PipeTee2 拓扑）：三通有 3 个节点（入口 + 直通 + 支管），
/// 是来流段（S1）的 TargetItem，同时是直通段（S2）与支管段（S3）两个段的 SourceItem——一分二的分叉点；
/// 与异径管/阀门（2 节点、一进一出）不同。图中截止阀/球阀在 C# 中为 GlobeValve/BallValve（OperatedValve 子类），
/// F# 侧暂以 OperatedValve 表示。</summary>
module TeeTree =
    /// 管径节点（三通入口 dn100，两路出口 dn80）
    let dn100 = { NominalDiameterStandard = "100" }
    let dn80 = { NominalDiameterStandard = "80" }

    /// 罐 V-101 的出口喷嘴 N1（dn100 侧）
    let n1 =
        {
            SubTagName = "N1"
            NominalPressureStandard = "PN16"
            Nodes = [ dn100 ]
        }

    let tank: Tank = { TagName = "V-101"; Nozzles = [ n1 ] }

    /// 截止阀 XV-101（OperatedValve；C# 类型 GlobeValve，dn100 管上、三通前）
    let valve: OperatedValve =
        {
            PipingComponentNumber = "XV-101"
            Nodes = [ dn100 ]
        }

    /// 三通 T-101（PipeTee；DN100×DN80×DN80，入口 / 直通 / 支管各一个节点）
    let tee: PipeTee =
        {
            PipingComponentNumber = "T-101"
            Nodes = [ dn100; dn80; dn80 ]
        }

    /// 直通下游出口（PipeOffPageConnector，图面右侧箭头）
    let outlet1: PipeOffPageConnector =
        {
            PipeConnectorNumber = "Outlet1"
            Nodes = [ dn80 ]
        }

    /// 支管下游出口（PipeOffPageConnector，图面右侧箭头）
    let outlet2: PipeOffPageConnector =
        {
            PipeConnectorNumber = "Outlet2"
            Nodes = [ dn80 ]
        }

    let seg1 =
        {
            SegmentNumber = "S1"
            NominalDiameterStandard = "100"

            SourceItem = PipingSourceItem.Nozzle n1
            SourceNode = dn100
            TargetItem = PipingTargetItem.PipeTee tee
            TargetNode = dn100

            Items =
                [
                    PipingNetworkSegmentItem.OperatedValve valve
                    PipingNetworkSegmentItem.PipeTee tee
                ]

            Connections =
                [
                    {
                        SourceItem = PipingSourceItem.Nozzle n1
                        SourceNode = dn100
                        TargetItem = PipingTargetItem.OperatedValve valve
                        TargetNode = dn100
                        IsDirectPipingConnection = false
                    }

                    {
                        SourceItem = PipingSourceItem.OperatedValve valve
                        SourceNode = dn100
                        TargetItem = PipingTargetItem.PipeTee tee
                        TargetNode = dn100
                        IsDirectPipingConnection = false
                    }
                ]
        }

    let seg2 =
        {
            SegmentNumber = "S2"
            NominalDiameterStandard = "80"

            SourceItem = PipingSourceItem.PipeTee tee
            SourceNode = dn80
            TargetItem = PipingTargetItem.PipeOffPageConnector outlet1
            TargetNode = dn80

            Items =
                [
                    PipingNetworkSegmentItem.PipeOffPageConnector outlet1
                ]

            Connections =
                [
                    {
                        SourceItem = PipingSourceItem.PipeTee tee
                        SourceNode = dn80
                        TargetItem = PipingTargetItem.PipeOffPageConnector outlet1
                        TargetNode = dn80
                        IsDirectPipingConnection = false
                    }
                ]
        }

    let seg3 =
        {
            SegmentNumber = "S3"
            NominalDiameterStandard = "80"
            SourceItem = PipingSourceItem.PipeTee tee
            SourceNode = dn80
            TargetItem = PipingTargetItem.PipeOffPageConnector outlet2
            TargetNode = dn80
            Items =
                [
                    PipingNetworkSegmentItem.PipeOffPageConnector outlet2
                ]
            Connections =
                [
                    {
                        SourceItem = PipingSourceItem.PipeTee tee
                        SourceNode = dn80
                        TargetItem = PipingTargetItem.PipeOffPageConnector outlet2
                        TargetNode = dn80
                        IsDirectPipingConnection = false
                    }
                ]
        }

    /// 工厂模型入口（tank 在位内联，段 S1~S3）
    let plantModel: PlantModel =
        {
            TaggedPlantItems = [ TaggedPlantItem.Tank tank ]
            PipingNetworkSystems =
                [
                    {
                        LineNumber = "PL-301"
                        Segments = [ seg1; seg2; seg3 ]
                    }
                ]
        }
