namespace DEXPI2.Test

open Xunit

open DEXPI2

/// <summary>按图（罐 V-101 → 管线 → 阀门 XV-101 → 异径管 XR-101 → 管线 → 排出口 To Sewer）构建拓扑实例。
/// 只保留拓扑与 tag：罐（ProcessEquipment 记录，带喷嘴 N1）→ 段 S1（阀门 XV-101、异径管 XR-101）→ 段 S3（排出口 To Sewer）。
/// 其余属性（DN、PN、节点、连接）忽略。</summary>
module InPlaceTree =
    /// 出口喷嘴 N1（挂接在罐 V-101 的 Nozzles 上，即整条管线的首件）
    let nozzle: PipingNodeOwner =
        {
            tag = "N1"
            detail = PipingNodeOwnerDetail.Nozzle
        }

    /// 罐 V-101（设备记录：TagName + Nozzles + detail）
    let tank: ProcessEquipment =
        {
            TagName = "V-101"
            Nozzles = [ nozzle ]
            detail = ProcessEquipmentDetail.Tank
        }

    /// 阀门 XV-101（OperatedValve）
    let valve: PipingNodeOwner =
        {
            tag = "XV-101"
            detail = PipingNodeOwnerDetail.OperatedValve
        }

    /// 异径管 XR-101（PipeReducer；并入段 S1 的 Items）
    let reducer: PipingNodeOwner =
        {
            tag = "XR-101"
            detail = PipingNodeOwnerDetail.PipeReducer
        }

    /// 排出口 To Sewer（PipeOffPageConnector）
    let discharge: PipingNodeOwner =
        {
            tag = "To Sewer"
            detail = PipingNodeOwnerDetail.PipeOffPageConnector
        }

    let segments =
        [
            {
                SegmentNumber = "S1"
                // 段是否包含整条管线的首/末件（首件喷嘴 N1 挂在罐上，不在段 Items 中）
                includeFirstItem = false
                includeLastItem = false
                Items = [ valve; reducer ]
            }

            {
                SegmentNumber = "S3"
                includeFirstItem = false
                includeLastItem = true
                Items = [ discharge ]
            }
        ]

    /// 工厂模型入口（罐 / 管线 PL-101 在位内联，段 S1、S3）
    let plantModel: PlantModel =
        {
            ProcessEquipments = [ tank.detail ]
            PipingNetworkSystems =
                [
                    {
                        LineNumber = "PL-101"
                        Segments = segments
                    }
                ]
        }
