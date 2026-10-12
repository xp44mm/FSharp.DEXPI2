namespace DEXPI2

/// <summary>阀门执行机构（actuator）类型。</summary>
/// DEXPI 2.0 标准数据模型无此闭枚举：执行机构种类写在
/// <c>ControlledActuator.DeviceTypeName</c> 的自由文本中（如 general / diaphragm / piston / motor / solenoid），
/// 手动 / 气动 / 电动 / 电磁等主要由 P&amp;ID 图形符号区分。本类型为 F# 侧的强类型映射。
type ActuatorType =
    /// 手动（手轮 / 杠杆）
    | Manual
    /// 气动
    | Pneumatic
    /// 电动
    | Electric
    /// 电磁阀
    | Solenoid
    /// 自动
    | Auto

module ActuatorType =
    /// 序列化为 SQLite 存储字符串（对应 SegmentItem.tag 中 actuator 部分）。
    let toSqlite (a: ActuatorType) : string =
        match a with
        | Manual -> "Manual"
        | Pneumatic -> "Pneumatic"
        | Electric -> "Electric"
        | Solenoid -> "Solenoid"
        | Auto -> "Auto"

    /// 从 SQLite 存储字符串还原。
    let fromSqlite (s: string) : ActuatorType =
        match s with
        | "Manual" -> Manual
        | "Pneumatic" -> Pneumatic
        | "Electric" -> Electric
        | "Solenoid" -> Solenoid
        | "Auto" -> Auto
        | _ -> failwithf "unknown ActuatorType: %s" s
