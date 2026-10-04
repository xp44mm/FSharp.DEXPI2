namespace DEXPI2

/// <summary>DEXPI 2.0 模型类 PipingNodeOwner（XMI id ID996）的可区分联合：全部派生类型（含抽象中间类）</summary>
type PipingNodeOwner =
    // Nozzle 支
    | Nozzle of nozzle: Nozzle
    | AccessNozzle
    | InstrumentNozzle
    | ProcessNozzle
    // PipeOffPageConnector 支（含抽象中间类）
    | PipeOffPageConnector of pipeOffPageConnector: PipeOffPageConnector
    | FlowInPipeOffPageConnector
    | FlowOutPipeOffPageConnector
    // PipingComponent 支（含抽象中间类）
    | PipingComponent
    // CheckValve 支
    | CheckValve of checkValve: CheckValve
    | GlobeCheckValve
    | SwingCheckValve
    // InlineMeasuringElement 支
    | InlineMeasuringElement
    | ElectromagneticFlowMeter
    | FlowMeasuringElement
    | FlowNozzle
    | MassFlowMeasuringElement
    | PositiveDisplacementFlowMeter
    | TurbineFlowMeter
    | VariableAreaFlowMeter
    | VenturiTube
    | VolumeFlowMeasuringElement
    // OperatedValve 支
    | OperatedValve of operatedValve: OperatedValve
    | AngleBallValve
    | AngleGlobeValve
    | AnglePlugValve
    | AngleValve
    | BallValve
    | ButterflyValve
    | GateValve
    | GlobeValve
    | NeedleValve
    | PlugValve
    | StraightwayValve
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
    | PipeReducer of pipeReducer: PipeReducer
    | PipeTee of pipeTee: PipeTee
    | RestrictionOrifice
    | Sensorwell of sensorwell: Sensorwell
    | SightGlass
    | Silencer
    | SteamTrap
    | Strainer
    | VentLine
    // SafetyValveOrFitting 支
    | SafetyValveOrFitting
    | BreatherValve
    | FlameArrestor
    | RuptureDisc
    | SpringLoadedAngleGlobeSafetyValve
    | SpringLoadedGlobeSafetyValve
    // 叶类
    | PropertyBreak
