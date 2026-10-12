namespace DEXPI2

open System

/// <summary>DEXPI 2.0 模型类 PipingNodeOwner（XMI id ID996）的可区分联合：全部派生类型（含抽象中间类）</summary>
/// 规则：不允许跨系统（跨 LineNumber）引用管段；SegmentRef 引用的枢纽段
/// 必须与引用段同属一个 PipingNetworkSystem，故只保留 segmentNumber 一个句柄
type PipingNodeOwner =
    | SegmentRef of segmentNumber: string
    // Nozzle 支
    | Nozzle of equipment: string * nozzle: string

    //| AccessNozzle of ProcessEquipment * string
    //| InstrumentNozzle of ProcessEquipment * string
    //| ProcessNozzle of ProcessEquipment * string

    // PipeOffPageConnector 支（含抽象中间类）
    | PipeOffPageConnector of pipeConnectorDescription: string
    | FlowInPipeOffPageConnector of pipeConnectorDescription: string
    | FlowOutPipeOffPageConnector of pipeConnectorDescription: string

    // PipingComponent 支（含抽象中间类）
    | PipingComponent of tag: string
    // CheckValve 支
    | CheckValve of tag: string
    | GlobeCheckValve of tag: string
    | SwingCheckValve of tag: string
    // InlineMeasuringElement 支
    | InlineMeasuringElement of tag: string
    | ElectromagneticFlowMeter of tag: string
    | FlowMeasuringElement of tag: string
    | FlowNozzle of tag: string
    | MassFlowMeasuringElement of tag: string
    | PositiveDisplacementFlowMeter of tag: string
    | TurbineFlowMeter of tag: string
    | VariableAreaFlowMeter of tag: string
    | VenturiTube of tag: string
    | VolumeFlowMeasuringElement of tag: string
    // OperatedValve 支
    | OperatedValve of tag: string * actuatorType: ActuatorType
    | AngleBallValve of tag: string * actuatorType: ActuatorType
    | AngleGlobeValve of tag: string * actuatorType: ActuatorType
    | AnglePlugValve of tag: string * actuatorType: ActuatorType
    | AngleValve of tag: string * actuatorType: ActuatorType
    | BallValve of tag: string * actuatorType: ActuatorType
    | ButterflyValve of tag: string * actuatorType: ActuatorType
    | GateValve of tag: string * actuatorType: ActuatorType
    | GlobeValve of tag: string * actuatorType: ActuatorType
    | NeedleValve of tag: string * actuatorType: ActuatorType
    | PlugValve of tag: string * actuatorType: ActuatorType
    | StraightwayValve of tag: string * actuatorType: ActuatorType
    // PipeFitting 支
    | PipeFitting
    | BlindFlange
    | ClampedFlangeCoupling
    | Compensator
    | ConicalStrainer
    | Flange
    | FlangedConnection
    | Funnel
    | Hose
    | IlluminatedSightGlass
    | InLineMixer
    | LineBlind
    | Penetration
    | PipeCoupling
    | PipeFlangeSpacer
    | PipeFlangeSpade
    | RestrictionOrifice
    | Sensorwell of typeRepresentation: string
    | SightGlass
    | Silencer
    | SteamTrap
    | Strainer
    | VentLine
    // SafetyValveOrFitting 支
    | SafetyValveOrFitting of tag: string

    | SafetyValve of tag: string // 手动增加，不符合dexpi2规范

    | BreatherValve of tag: string
    | FlameArrestor of tag: string
    | RuptureDisc of tag: string
    | SpringLoadedAngleGlobeSafetyValve of tag: string
    | SpringLoadedGlobeSafetyValve of tag: string
    // 叶类
    | PropertyBreak of tag: string

module PipingNodeOwner =
    /// 输出 (nodeKind, tag)，对应 SegmentItem 表的 node_kind/tag 列。
    /// 规则：
    ///   * nodeKind 为变体名；tag+actuator 项的 nodeKind 形如 "Manual OperatedValve"（actuator 在前）
    ///   * tag 为纯业务参数，不拼变体名前缀：
    ///       单 tag 项 -> tag 值
    ///       Nozzle(eq,nz) -> "eq nz"
    ///       SegmentRef(num) -> num
    ///       PipeOffPageConnector 支 -> description
    ///       Sensorwell(typeRep) -> nodeKind 形如 "Sensorwell <typeRep>"，tag 为空串
    ///       无参数项 -> tag 为空串 ""
    let toSqlite (node: PipingNodeOwner) : string * string =
        let act a = ActuatorType.toSqlite a
        let valveKind opvalve a = sprintf "%s %s" (act a) opvalve

        match node with
        | SegmentRef(num) -> "SegmentRef", num
        | Nozzle(eq, nz) -> "Nozzle", sprintf "%s %s" eq nz
        | PipeOffPageConnector(desc) -> "PipeOffPageConnector", desc
        | FlowInPipeOffPageConnector(desc) -> "FlowInPipeOffPageConnector", desc
        | FlowOutPipeOffPageConnector(desc) -> "FlowOutPipeOffPageConnector", desc
        | PipingComponent(tag) -> "PipingComponent", tag
        | CheckValve(tag) -> "CheckValve", tag
        | GlobeCheckValve(tag) -> "GlobeCheckValve", tag
        | SwingCheckValve(tag) -> "SwingCheckValve", tag
        | InlineMeasuringElement(tag) -> "InlineMeasuringElement", tag
        | ElectromagneticFlowMeter(tag) -> "ElectromagneticFlowMeter", tag
        | FlowMeasuringElement(tag) -> "FlowMeasuringElement", tag
        | FlowNozzle(tag) -> "FlowNozzle", tag
        | MassFlowMeasuringElement(tag) -> "MassFlowMeasuringElement", tag
        | PositiveDisplacementFlowMeter(tag) -> "PositiveDisplacementFlowMeter", tag
        | TurbineFlowMeter(tag) -> "TurbineFlowMeter", tag
        | VariableAreaFlowMeter(tag) -> "VariableAreaFlowMeter", tag
        | VenturiTube(tag) -> "VenturiTube", tag
        | VolumeFlowMeasuringElement(tag) -> "VolumeFlowMeasuringElement", tag

        | OperatedValve(tag, a) -> valveKind "OperatedValve" a, tag
        | AngleBallValve(tag, a) -> valveKind "AngleBallValve" a, tag
        | AngleGlobeValve(tag, a) -> valveKind "AngleGlobeValve" a, tag
        | AnglePlugValve(tag, a) -> valveKind "AnglePlugValve" a, tag
        | AngleValve(tag, a) -> valveKind "AngleValve" a, tag
        | BallValve(tag, a) -> valveKind "BallValve" a, tag
        | ButterflyValve(tag, a) -> valveKind "ButterflyValve" a, tag
        | GateValve(tag, a) -> valveKind "GateValve" a, tag
        | GlobeValve(tag, a) -> valveKind "GlobeValve" a, tag
        | NeedleValve(tag, a) -> valveKind "NeedleValve" a, tag
        | PlugValve(tag, a) -> valveKind "PlugValve" a, tag
        | StraightwayValve(tag, a) -> valveKind "StraightwayValve" a, tag
        | PipeFitting -> "PipeFitting", ""
        | BlindFlange -> "BlindFlange", ""
        | ClampedFlangeCoupling -> "ClampedFlangeCoupling", ""
        | Compensator -> "Compensator", ""
        | ConicalStrainer -> "ConicalStrainer", ""
        | Flange -> "Flange", ""
        | FlangedConnection -> "FlangedConnection", ""
        | Funnel -> "Funnel", ""
        | Hose -> "Hose", ""
        | IlluminatedSightGlass -> "IlluminatedSightGlass", ""
        | InLineMixer -> "InLineMixer", ""
        | LineBlind -> "LineBlind", ""
        | Penetration -> "Penetration", ""
        | PipeCoupling -> "PipeCoupling", ""
        | PipeFlangeSpacer -> "PipeFlangeSpacer", ""
        | PipeFlangeSpade -> "PipeFlangeSpade", ""
        | RestrictionOrifice -> "RestrictionOrifice", ""
        | Sensorwell(typeRep) -> String.concat " " ["Sensorwell"; typeRep], ""
        | SightGlass -> "SightGlass", ""
        | Silencer -> "Silencer", ""
        | SteamTrap -> "SteamTrap", ""
        | Strainer -> "Strainer", ""
        | VentLine -> "VentLine", ""
        | SafetyValveOrFitting(tag) -> "SafetyValveOrFitting", tag
        | SafetyValve(tag) -> "SafetyValve", tag
        | BreatherValve(tag) -> "BreatherValve", tag
        | FlameArrestor(tag) -> "FlameArrestor", tag
        | RuptureDisc(tag) -> "RuptureDisc", tag
        | SpringLoadedAngleGlobeSafetyValve(tag) -> "SpringLoadedAngleGlobeSafetyValve", tag
        | SpringLoadedGlobeSafetyValve(tag) -> "SpringLoadedGlobeSafetyValve", tag
        | PropertyBreak(tag) -> "PropertyBreak", tag

    /// 从 (nodeKind, tag) 还原 PipingNodeOwner。
    /// nodeKind 可能带 actuator 前缀（如 "Manual BallValve"，与 toSqlite 输出一致）或 Sensorwell 的
    /// typeRep 后缀（如 "Sensorwell Thermowell"）；tag 为 null 时仅按 nodeKind 构造无参数项。
    let fromSqlite (nodeKind: string) (tag: string) : PipingNodeOwner =
        // 拆出可能的 actuator 前缀："Manual BallValve" -> ("BallValve", Some Manual)
        // 以及 Sensorwell 的 typeRep 后缀："Sensorwell Thermowell" -> ("Sensorwell", Some "Thermowell")
        let kindName, actuator, sensorwellTypeRep =
            let parts = nodeKind.Split(' ')
            if parts.Length > 1 && List.contains parts.[0]
                [ "Manual"; "Pneumatic"; "Electric"; "Solenoid"; "Auto" ] then
                parts.[1], Some (ActuatorType.fromSqlite parts.[0]), None
            elif parts.Length > 1 && parts.[0] = "Sensorwell" then
                "Sensorwell", None, Some (String.Join(" ", parts.[1..]))
            else
                nodeKind, None, None
        let valve ctor =
            match actuator with
            | Some a -> ctor (tag, a)
            | None -> failwithf "valve kind %s missing actuator" nodeKind
        match kindName with
        | "SegmentRef" -> SegmentRef(tag)
        | "Nozzle" ->
            let np = if isNull tag then [||] else tag.Split(' ')
            Nozzle(np.[0], np.[1])
        | "PipeOffPageConnector" -> PipeOffPageConnector(tag)
        | "FlowInPipeOffPageConnector" -> FlowInPipeOffPageConnector(tag)
        | "FlowOutPipeOffPageConnector" -> FlowOutPipeOffPageConnector(tag)
        | "PipingComponent" -> PipingComponent(tag)
        | "CheckValve" -> CheckValve(tag)
        | "GlobeCheckValve" -> GlobeCheckValve(tag)
        | "SwingCheckValve" -> SwingCheckValve(tag)
        | "InlineMeasuringElement" -> InlineMeasuringElement(tag)
        | "ElectromagneticFlowMeter" -> ElectromagneticFlowMeter(tag)
        | "FlowMeasuringElement" -> FlowMeasuringElement(tag)
        | "FlowNozzle" -> FlowNozzle(tag)
        | "MassFlowMeasuringElement" -> MassFlowMeasuringElement(tag)
        | "PositiveDisplacementFlowMeter" -> PositiveDisplacementFlowMeter(tag)
        | "TurbineFlowMeter" -> TurbineFlowMeter(tag)
        | "VariableAreaFlowMeter" -> VariableAreaFlowMeter(tag)
        | "VenturiTube" -> VenturiTube(tag)
        | "VolumeFlowMeasuringElement" -> VolumeFlowMeasuringElement(tag)
        | "OperatedValve" -> valve OperatedValve
        | "AngleBallValve" -> valve AngleBallValve
        | "AngleGlobeValve" -> valve AngleGlobeValve
        | "AnglePlugValve" -> valve AnglePlugValve
        | "AngleValve" -> valve AngleValve
        | "BallValve" -> valve BallValve
        | "ButterflyValve" -> valve ButterflyValve
        | "GateValve" -> valve GateValve
        | "GlobeValve" -> valve GlobeValve
        | "NeedleValve" -> valve NeedleValve
        | "PlugValve" -> valve PlugValve
        | "StraightwayValve" -> valve StraightwayValve
        | "PipeFitting" -> PipeFitting
        | "BlindFlange" -> BlindFlange
        | "ClampedFlangeCoupling" -> ClampedFlangeCoupling
        | "Compensator" -> Compensator
        | "ConicalStrainer" -> ConicalStrainer
        | "Flange" -> Flange
        | "FlangedConnection" -> FlangedConnection
        | "Funnel" -> Funnel
        | "Hose" -> Hose
        | "IlluminatedSightGlass" -> IlluminatedSightGlass
        | "InLineMixer" -> InLineMixer
        | "LineBlind" -> LineBlind
        | "Penetration" -> Penetration
        | "PipeCoupling" -> PipeCoupling
        | "PipeFlangeSpacer" -> PipeFlangeSpacer
        | "PipeFlangeSpade" -> PipeFlangeSpade
        | "RestrictionOrifice" -> RestrictionOrifice
        | "Sensorwell" ->
            match sensorwellTypeRep with
            | Some tr -> Sensorwell(tr)
            | None -> failwithf "Sensorwell node_kind missing typeRepresentation: %s" nodeKind
        | "SightGlass" -> SightGlass
        | "Silencer" -> Silencer
        | "SteamTrap" -> SteamTrap
        | "Strainer" -> Strainer
        | "VentLine" -> VentLine
        | "SafetyValveOrFitting" -> SafetyValveOrFitting(tag)
        | "SafetyValve" -> SafetyValve(tag)
        | "BreatherValve" -> BreatherValve(tag)
        | "FlameArrestor" -> FlameArrestor(tag)
        | "RuptureDisc" -> RuptureDisc(tag)
        | "SpringLoadedAngleGlobeSafetyValve" -> SpringLoadedAngleGlobeSafetyValve(tag)
        | "SpringLoadedGlobeSafetyValve" -> SpringLoadedGlobeSafetyValve(tag)
        | "PropertyBreak" -> PropertyBreak(tag)
        | _ -> failwithf "unknown PipingNodeOwner node_kind: %s" nodeKind
