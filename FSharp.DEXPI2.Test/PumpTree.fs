namespace FSharp.DEXPI2.Test

open Xunit

open FSharp.DEXPI2
open FSharp.DEXPI2.Core
open FSharp.DEXPI2.Plant
open FSharp.DEXPI2.Plant.ProcessEquipment
open FSharp.DEXPI2.Plant.Piping

module PumpTree =
    /// 管径节点（各段）
    let dn125 = { NominalDiameterStandard = "125" }
    let dn100 = { NominalDiameterStandard = "100" }
    let dn80 = { NominalDiameterStandard = "80" }
    let dn150 = { NominalDiameterStandard = "150" }

    /// 罐 V-101 的出口喷嘴 N1（dn125 侧）
    let n1 =
        {
            SubTagName = "N1"
            NominalPressureStandard = "PN16"
            Nodes = [ dn125 ]
        }

    let tank: Tank = { TagName = "V-101"; Nozzles = [ n1 ] }

    /// 泵 P-101：入口喷嘴 N2（dn100）、出口喷嘴 N3（dn80）
    let n2 =
        {
            SubTagName = "N2"
            NominalPressureStandard = "PN16"
            Nodes = [ dn100 ]
        }

    let n3 =
        {
            SubTagName = "N3"
            NominalPressureStandard = "PN16"
            Nodes = [ dn80 ]
        }

    let pump: Pump = { TagName = "P-101"; Nozzles = [ n2; n3 ] }

    /// 阀门 XV-101（OperatedValve；dn125 管上，罐出口 N1 与异径管 XR-101 之间）
    let valve: OperatedValve =
        {
            PipingComponentNumber = "XV-101"
            Nodes = [ dn125 ]
        }

    /// 止回阀 CV-101（CheckValve；dn150 管上，异径管 XR-102 之后、排污管道之前）
    let checkValve: CheckValve =
        {
            PipingComponentNumber = "CV-101"
            Nodes = [ dn150 ]
        }

    /// 异径管 XR-101（PipeReducer；dn125 → dn100，阀门 XV-101 之后、泵入口前）
    let reducer1: PipeReducer =
        {
            PipingComponentNumber = "XR-101"
            Nodes = [ dn125; dn100 ]
        }

    /// 异径管 XR-102（PipeReducer；dn80 → dn150，泵出口）
    let reducer2: PipeReducer =
        {
            PipingComponentNumber = "XR-102"
            Nodes = [ dn150; dn80 ]
        }


    /// dn150 管上的阀门 XV-201（OperatedValve；已定义，暂未接入任何段的 Items/Connections）
    let valve2: OperatedValve =
        {
            //PipingComponentName = "XV-201"
            PipingComponentNumber = "XV-201"
            Nodes = [ dn150 ]
        }

    /// 排污管道（PipeOffPageConnector，矩形标注框）
    let discharge: PipeOffPageConnector =
        {
            PipeConnectorNumber = "排污管道"
            Nodes = [ dn150 ]
        }

    let segments =
        [
            {
                SegmentNumber = "S1"
                //FluidCode = "W"
                NominalDiameterStandard = "125"
                //PipingClassCode = "75HB13"

                SourceItem = PipingSourceItem.Nozzle n1
                SourceNode = dn125
                //TargetItem = PipingTargetItem.OperatedValve valve
                //TargetNode = dn125
                TargetItem = PipingTargetItem.PipeReducer reducer1
                TargetNode = dn125

                Items =
                    [
                        PipingNetworkSegmentItem.OperatedValve valve
                        PipingNetworkSegmentItem.PipeReducer reducer1
                    ]

                Connections =
                    [
                        {
                            SourceItem = PipingSourceItem.Nozzle n1
                            SourceNode = dn125

                            TargetItem = PipingTargetItem.OperatedValve valve
                            TargetNode = dn125

                            IsDirectPipingConnection = true
                        }
                        {
                            SourceItem = PipingSourceItem.OperatedValve valve
                            SourceNode = dn125

                            TargetItem = PipingTargetItem.PipeReducer reducer1
                            TargetNode = dn125

                            IsDirectPipingConnection = false
                        }

                    ]
            }

            {
                SegmentNumber = "S2"
                NominalDiameterStandard = "100"

                SourceItem = PipingSourceItem.PipeReducer reducer1
                SourceNode = dn100
                TargetItem = PipingTargetItem.Nozzle n2
                TargetNode = dn100

                Items =
                    [
                    ]

                Connections =
                    [
                        {
                            SourceItem = PipingSourceItem.PipeReducer reducer1
                            SourceNode = dn100
                            TargetItem = PipingTargetItem.Nozzle n2
                            TargetNode = dn100

                            IsDirectPipingConnection = false
                        }

                    ]
            }

            {
                SegmentNumber = "S3"
                NominalDiameterStandard = "80"

                SourceItem = PipingSourceItem.Nozzle n3
                SourceNode = dn80
                TargetItem = PipingTargetItem.PipeReducer reducer2
                TargetNode = dn150

                Items =
                    [
                        PipingNetworkSegmentItem.PipeReducer reducer2
                    ]

                Connections =
                    [
                        {
                            SourceItem = PipingSourceItem.Nozzle n3
                            SourceNode = dn80
                            TargetItem = PipingTargetItem.PipeReducer reducer2
                            TargetNode = dn150
                            IsDirectPipingConnection = false
                        }
                    ]
            }

            {
                SegmentNumber = "S4"
                NominalDiameterStandard = "150"

                SourceItem = PipingSourceItem.PipeReducer reducer2
                SourceNode = dn150
                TargetItem = PipingTargetItem.PipeOffPageConnector discharge
                TargetNode = dn150

                Items =
                    [
                        PipingNetworkSegmentItem.CheckValve checkValve
                        PipingNetworkSegmentItem.PipeOffPageConnector discharge
                    ]

                Connections =
                    [
                        {
                            SourceItem = PipingSourceItem.PipeReducer reducer2
                            SourceNode = dn150
                            TargetItem = PipingTargetItem.CheckValve checkValve
                            TargetNode = dn150
                            IsDirectPipingConnection = false
                        }

                        {
                            SourceItem = PipingSourceItem.CheckValve checkValve
                            SourceNode = dn150
                            TargetItem = PipingTargetItem.PipeOffPageConnector discharge
                            TargetNode = dn150
                            IsDirectPipingConnection = false
                        }
                    ]
            }
        ]

    /// 工厂模型入口（tank / pump 在位内联，段 seg1~seg4）
    let plantModel: PlantModel =
        {
            TaggedPlantItems = [ TaggedPlantItem.Tank tank; TaggedPlantItem.Pump pump ]
            PipingNetworkSystems =
                [
                    {
                        LineNumber = "PL-201"
                        Segments = segments
                    }
                ]
        }

