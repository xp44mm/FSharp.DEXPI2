namespace FSharp.DEXPI2.Test

open Xunit

open FSharp.DEXPI2
open FSharp.DEXPI2.Plant
open FSharp.DEXPI2.Plant.ProcessEquipment
open FSharp.DEXPI2.Plant.Piping

/// <summary>按图（罐 V-101 ──dn50 管道── 罐 V-102，管道中部安装传感器套管 SW-101 作为压力测点 PT）构建传感器套管管线树。
/// 图形：左右两个矩形设备，中间一条水平管线，管线中部上方为方形框内圆形符号（标 PT），以竖线与管线相连。
/// 测点表达（DEXPI 2.0）：管道上的仪表测点使用 Sensorwell（传感器套管）作为 SensingLocation——
/// 套管以压力密封方式安装在管道上，供传感元件（压力变送器 PT）取压；与盲法兰方案（取压支管 + BlindFlange）不同，
/// 本图是套管直接装在主管中部。图中 PT 为 ProcessSignalGeneratingFunction（编号 PTxxxx.xx），
/// F# 侧仪表功能类尚未迁移，此处以 Sensorwell 组件 + 注释表达该测点。</summary>
module SensorwellTree =
    /// 管径节点（主管 dn50）
    let dn50 = { NominalDiameterStandard = "50" }

    /// 罐 V-101（左）出口喷嘴 N1（dn50 侧）
    let n1 =
        {
            SubTagName = "N1"
            NominalPressureStandard = "PN16"
            Nodes = [ dn50 ]
        }

    /// 罐 V-102（右）入口喷嘴 N2（dn50 侧）
    let n2 =
        {
            SubTagName = "N2"
            NominalPressureStandard = "PN16"
            Nodes = [ dn50 ]
        }

    let tank101: Tank = { TagName = "V-101"; Nozzles = [ n1 ] }
    let tank102: Tank = { TagName = "V-102"; Nozzles = [ n2 ] }

    /// 传感器套管 SW-101（Sensorwell；管道中部测点，1 个节点）
    let sensorwell: Sensorwell =
        {
            PipingComponentNumber = "SW-101"
            SensorwellTypeRepresentation = "PT"
            Nodes = [ dn50 ]
        }

    let segments =
        [
            {
                SegmentNumber = "S1"
                NominalDiameterStandard = "50"

                SourceItem = PipingSourceItem.Nozzle n1
                SourceNode = dn50
                TargetItem = PipingTargetItem.Nozzle n2
                TargetNode = dn50

                Items =
                    [
                        PipingNetworkSegmentItem.Sensorwell sensorwell
                    ]

                Connections =
                    [
                        {
                            SourceItem = PipingSourceItem.Nozzle n1
                            SourceNode = dn50
                            TargetItem = PipingTargetItem.Sensorwell sensorwell
                            TargetNode = dn50
                            IsDirectPipingConnection = false
                        }
                        {
                            SourceItem = PipingSourceItem.Sensorwell sensorwell
                            SourceNode = dn50
                            TargetItem = PipingTargetItem.Nozzle n2
                            TargetNode = dn50
                            IsDirectPipingConnection = false
                        }
                    ]
            }
        ]

    /// 工厂模型入口（两台罐在位内联，段 S1 中部挂传感器套管）
    let plantModel: PlantModel =
        {
            TaggedPlantItems = [ TaggedPlantItem.Tank tank101; TaggedPlantItem.Tank tank102 ]
            PipingNetworkSystems =
                [
                    {
                        LineNumber = "PL-401"
                        Segments = segments
                    }
                ]
        }
