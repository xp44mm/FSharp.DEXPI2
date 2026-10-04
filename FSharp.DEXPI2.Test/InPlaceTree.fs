namespace FSharp.DEXPI2.Test

open Xunit

open FSharp.DEXPI2
open FSharp.DEXPI2.Core
open FSharp.DEXPI2.Plant
open FSharp.DEXPI2.Plant.ProcessEquipment
open FSharp.DEXPI2.Plant.Piping

/// <summary>按图（罐 V-101 → dn100 管道 → 阀门 XV-101 → 异径管 XR-101 → dn80 管道 → 排出口 To Sewer）构建当前模型的实例。
/// 图形：罐（矩形框）→ 管线 dn100 → 阀门（圆形带翼符号）→ 异径管（锥形符号）→ 管线 dn80 → 向右箭头。
/// 段按规范（Piping.py PropertyBreak / PipingNetworkSegment；reference_pid.xml 47124）：异径管独立成段——
/// 它只出现在自己段（S2）的 Items 中，不在前段（S1）也不在后段（S3）的 Items 中；新段紧接异径管之后开始且管径不同。
/// 段标识 = SegmentNumber，段无 PN；压力等级隐含于 PipingClassCode（值取自规范参考模型 47124）。</summary>
module InPlaceTree =
    /// dn100 / dn80 管径节点（异径管两侧各一段）
    let dn100 = { NominalDiameterStandard = "100" }
    let dn80 = { NominalDiameterStandard = "80" }

    /// 罐 V-101 的出口喷嘴 N1（dn100 侧）
    let n1 =
        {
            SubTagName = "N1"
            NominalPressureStandard = "PN16"
            Nodes = [ dn100 ]
        }

    let tank = Tank { TagName = "V-101"; Nozzles = [ n1 ] }

    /// 阀门 XV-101（OperatedValve；同时是 PipingNetworkSegmentItem / PipingSourceItem / PipingTargetItem）
    let valve: OperatedValve =
        {
            //PipingComponentName = "XV-101"
            PipingComponentNumber = "XV-101"
            Nodes = [ dn100 ]
        }

    /// 异径管 XR-101（PipeReducer；dn100 → dn80，入口 / 出口各一个节点）
    let reducer: PipeReducer =
        {
            //PipingComponentName = "XR-101"
            PipingComponentNumber = "XR-101"
            Nodes = [ dn100; dn80 ]
        }

    /// 排出口 To Sewer（PipeOffPageConnector）
    let discharge: PipeOffPageConnector =
        {
            PipeConnectorNumber = "To Sewer"
            Nodes = [ dn80 ]
        }

    let segments =
        [
            {
                SegmentNumber = "S1"
                //FluidCode = "W"
                NominalDiameterStandard = "100"
                //PipingClassCode = "75HB13"

                SourceItem = PipingSourceItem.Nozzle n1
                SourceNode = dn100
                TargetItem = PipingTargetItem.PipeReducer reducer
                TargetNode = dn100

                Items =
                    [
                        PipingNetworkSegmentItem.OperatedValve valve
                        PipingNetworkSegmentItem.PipeReducer reducer

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
                            TargetItem = PipingTargetItem.PipeReducer reducer
                            TargetNode = dn100
                            IsDirectPipingConnection = false
                        }

                    ]
            }

            //{
            //    SegmentNumber = "S2"
            //    FluidCode = "W"
            //    NominalDiameterStandard = "100"
            //    PipingClassCode = "73HG12"

            //    SourceItem = PipingSourceItem.PipeReducer reducer
            //    SourceNode = dn100
            //    TargetItem = PipingTargetItem.PipeReducer reducer
            //    TargetNode = dn80

            //    Items =
            //        [
            //        ]

            //    Connections = []
            //}

            {
                SegmentNumber = "S3"
                //FluidCode = "W"
                NominalDiameterStandard = "80"
                //PipingClassCode = "73HG12"

                SourceItem = PipingSourceItem.PipeReducer reducer
                SourceNode = dn80
                TargetItem = PipingTargetItem.PipeOffPageConnector discharge
                TargetNode = dn80

                Items =
                    [
                        PipingNetworkSegmentItem.PipeOffPageConnector discharge
                    ]

                Connections =
                    [
                        {
                            SourceItem = PipingSourceItem.PipeReducer reducer
                            SourceNode = dn80
                            TargetItem = PipingTargetItem.PipeOffPageConnector discharge
                            TargetNode = dn80
                            IsDirectPipingConnection = false
                        }
                    ]
            }
        ]

    /// 工厂模型入口（tank / pns1 在位内联，段 seg1~seg3）
    let plantModel: PlantModel =
        {
            TaggedPlantItems = [ tank ]
            PipingNetworkSystems =
                [
                    {
                        LineNumber = "PL-101"
                        //FluidCode = "W"
                        //NominalDiameterStandard = "100"
                        //PipingClassCode = "75HB13"
                        //PipingNetworkSystemGroupNumber = ""
                        Segments = segments
                    }
                ]
        }
