namespace DEXPI2

open Microsoft.FSharp.Reflection

/// <summary>ProcessEquipment 辅助（参数数据库用）：提取设备的 (tag, nozzles)。</summary>
/// 不变式：所有 case 均为 `X of tag: string * nozzles: string list`。
module ProcessEquipmentCategory =
    let equipmentTagNozzles (eq: ProcessEquipment) : string * string list =
        let _, fields = FSharpValue.GetUnionFields(eq, typeof<ProcessEquipment>)
        fields.[0] :?> string, fields.[1] :?> string list
