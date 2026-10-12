namespace DEXPI2

/// <summary>
/// 段内节点归类（参数数据库用）：
/// tagItemOf —— tag 有名管件（带 tag: string 参数）返回 (类型名, tag)；引用项返回 None；
/// unnamedTypeOf —— 无名管件（无 tag）返回类型名；引用项（SegmentRef、Nozzle）返回 None（不入库）；
/// parentNozzleOf —— 段 Items 中第一个管嘴引用（CSS 覆盖链中段的父级）。
/// </summary>
module PipingNodeOwnerCategory =
    let tagItemOf (node: PipingNodeOwner) : (string * string) option =
        match node with
        | PipingComponent tag -> Some("PipingComponent", tag)
        | CheckValve tag -> Some("CheckValve", tag)
        | GlobeCheckValve tag -> Some("GlobeCheckValve", tag)
        | SwingCheckValve tag -> Some("SwingCheckValve", tag)
        | InlineMeasuringElement tag -> Some("InlineMeasuringElement", tag)
        | ElectromagneticFlowMeter tag -> Some("ElectromagneticFlowMeter", tag)
        | FlowMeasuringElement tag -> Some("FlowMeasuringElement", tag)
        | FlowNozzle tag -> Some("FlowNozzle", tag)
        | MassFlowMeasuringElement tag -> Some("MassFlowMeasuringElement", tag)
        | PositiveDisplacementFlowMeter tag -> Some("PositiveDisplacementFlowMeter", tag)
        | TurbineFlowMeter tag -> Some("TurbineFlowMeter", tag)
        | VariableAreaFlowMeter tag -> Some("VariableAreaFlowMeter", tag)
        | VenturiTube tag -> Some("VenturiTube", tag)
        | VolumeFlowMeasuringElement tag -> Some("VolumeFlowMeasuringElement", tag)
        | OperatedValve(tag, actuatorType) -> Some(sprintf "OperatedValve(%A)" actuatorType, tag)
        | AngleBallValve(tag, actuatorType) -> Some(sprintf "AngleBallValve(%A)" actuatorType, tag)
        | AngleGlobeValve(tag, actuatorType) -> Some(sprintf "AngleGlobeValve(%A)" actuatorType, tag)
        | AnglePlugValve(tag, actuatorType) -> Some(sprintf "AnglePlugValve(%A)" actuatorType, tag)
        | AngleValve(tag, actuatorType) -> Some(sprintf "AngleValve(%A)" actuatorType, tag)
        | BallValve(tag, actuatorType) -> Some(sprintf "BallValve(%A)" actuatorType, tag)
        | ButterflyValve(tag, actuatorType) -> Some(sprintf "ButterflyValve(%A)" actuatorType, tag)
        | GateValve(tag, actuatorType) -> Some(sprintf "GateValve(%A)" actuatorType, tag)
        | GlobeValve(tag, actuatorType) -> Some(sprintf "GlobeValve(%A)" actuatorType, tag)
        | NeedleValve(tag, actuatorType) -> Some(sprintf "NeedleValve(%A)" actuatorType, tag)
        | PlugValve(tag, actuatorType) -> Some(sprintf "PlugValve(%A)" actuatorType, tag)
        | StraightwayValve(tag, actuatorType) -> Some(sprintf "StraightwayValve(%A)" actuatorType, tag)
        | SafetyValveOrFitting tag -> Some("SafetyValveOrFitting", tag)
        | BreatherValve tag -> Some("BreatherValve", tag)
        | FlameArrestor tag -> Some("FlameArrestor", tag)
        | RuptureDisc tag -> Some("RuptureDisc", tag)
        | SpringLoadedAngleGlobeSafetyValve tag -> Some("SpringLoadedAngleGlobeSafetyValve", tag)
        | SpringLoadedGlobeSafetyValve tag -> Some("SpringLoadedGlobeSafetyValve", tag)
        | PropertyBreak tag -> Some("PropertyBreak", tag)
        | _ -> None

    /// 无名管件（无 tag）的类型名；引用项（SegmentRef、Nozzle）返回 None（不入库）。
    let unnamedTypeOf (node: PipingNodeOwner) : string option =
        match node with
        | Nozzle _ -> None
        | SegmentRef _ -> None
        | Sensorwell typeRepresentation -> Some(sprintf "Sensorwell(%s)" typeRepresentation)
        | PipeOffPageConnector _ -> Some "PipeOffPageConnector"
        | FlowInPipeOffPageConnector _ -> Some "FlowInPipeOffPageConnector"
        | FlowOutPipeOffPageConnector _ -> Some "FlowOutPipeOffPageConnector"
        | _ -> Some(sprintf "%A" node)

    /// 段 Items 中第一个管嘴引用（CSS 覆盖链中段的父级）；无管嘴引用时为 None
    let parentNozzleOf (items: PipingNodeOwner list) : (string * string) option =
        items
        |> List.tryPick (function Nozzle(equipment, nozzle) -> Some(equipment, nozzle) | _ -> None)
