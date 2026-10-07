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

/// <summary>DEXPI 2.0 模型类 PipingNodeOwner（XMI id ID996）的可区分联合：全部派生类型（含抽象中间类）</summary>
/// 规则：不允许跨系统（跨 LineNumber）引用管段；PipingNetworkSegmentSingleton 引用的单例段
/// 必须与引用段同属一个 PipingNetworkSystem，故只保留 segmentNumber 一个句柄
type PipingNodeOwner =
    | PipingNetworkSegmentSingleton of segmentNumber: string
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
    | PipeReducer
    | PipeTee
    | RestrictionOrifice
    | Sensorwell of typeRepresentation: string
    | SightGlass
    | Silencer
    | SteamTrap
    | Strainer
    | VentLine
    // SafetyValveOrFitting 支
    | SafetyValveOrFitting of tag: string
    | BreatherValve of tag: string
    | FlameArrestor of tag: string
    | RuptureDisc of tag: string
    | SpringLoadedAngleGlobeSafetyValve of tag: string
    | SpringLoadedGlobeSafetyValve of tag: string
    // 叶类
    | PropertyBreak of tag: string

//type PipingNodeOwner =
//    {
//        tag: string
//        detail: PipingNodeOwnerDetail
//    }
