namespace DEXPI2

/// <summary>DEXPI 2.0 模型类 PipingNodeOwner（XMI id ID996）的可区分联合：全部派生类型（含抽象中间类）</summary>
type PipingNodeOwner =
    | PipingNetworkSegmentSingleton of lineNumber: string * segmentNumber: string
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
    | OperatedValve of tag: string
    | AngleBallValve of tag: string
    | AngleGlobeValve of tag: string
    | AnglePlugValve of tag: string
    | AngleValve of tag: string
    | BallValve of tag: string
    | ButterflyValve of tag: string
    | GateValve of tag: string
    | GlobeValve of tag: string
    | NeedleValve of tag: string
    | PlugValve of tag: string
    | StraightwayValve of tag: string
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
