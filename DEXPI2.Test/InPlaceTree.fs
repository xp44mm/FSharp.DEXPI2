namespace DEXPI2.Test

open Xunit

open DEXPI2

/// <summary>按图（罐 V-101 → 管线 → 阀门 XV-101 → 异径管 XR-101 → 管线 → 排出口 To Sewer）构建拓扑实例。
/// 只保留拓扑与 tag：罐（ProcessEquipment 记录）→ 段 S1（喷嘴 N1、阀门 XV-101、异径管 XR-101）→ 段 S3（排出口 To Sewer）。
/// 其余属性（DN、PN、节点、连接）忽略。</summary>
module InPlaceTree =
    /// 罐 V-101（设备记录：TagName + detail）
    let tank: ProcessEquipment = { TagName = "V-101"; detail = ProcessEquipmentDetail.Tank }

    /// 出口喷嘴 N1（原模型为段 S1 的 SourceItem，新模型以段首 Item 表示）
    let nozzle: PipingNodeOwner = { tag = "N1"; detail = PipingNodeOwnerDetail.Nozzle }

    /// 阀门 XV-101（OperatedValve）
    let valve: PipingNodeOwner = { tag = "XV-101"; detail = PipingNodeOwnerDetail.OperatedValve }

    /// 异径管 XR-101（PipeReducer；并入段 S1 的 Items）
    let reducer: PipingNodeOwner = { tag = "XR-101"; detail = PipingNodeOwnerDetail.PipeReducer }

    /// 排出口 To Sewer（PipeOffPageConnector）
    let discharge: PipingNodeOwner = { tag = "To Sewer"; detail = PipingNodeOwnerDetail.PipeOffPageConnector }

    let segments =
        [
            {
                SegmentNumber = "S1"
                // 段是否包含整条管线的首/末件
                includeFirstItem = true
                includeLastItem = false
                Items = [ nozzle; valve; reducer ]
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
