namespace DEXPI2.Test

open Xunit

open DEXPI2

module PipingNodeOwnerTest =
    /// 穷举全部 65 个案例（编译期穷尽性检查），返回 (案例名, 标识荷载)。
    /// 仅 Sensorwell 携带 SensorwellTypeRepresentation 荷载，其余返回 None。
    let private describe (detail: PipingNodeOwnerDetail) : string * string option =
        match detail with
        | Nozzle -> "Nozzle", None
        | AccessNozzle -> "AccessNozzle", None
        | InstrumentNozzle -> "InstrumentNozzle", None
        | ProcessNozzle -> "ProcessNozzle", None
        | PipeOffPageConnector -> "PipeOffPageConnector", None
        | FlowInPipeOffPageConnector -> "FlowInPipeOffPageConnector", None
        | FlowOutPipeOffPageConnector -> "FlowOutPipeOffPageConnector", None
        | PipingComponent -> "PipingComponent", None
        | CheckValve -> "CheckValve", None
        | GlobeCheckValve -> "GlobeCheckValve", None
        | SwingCheckValve -> "SwingCheckValve", None
        | InlineMeasuringElement -> "InlineMeasuringElement", None
        | ElectromagneticFlowMeter -> "ElectromagneticFlowMeter", None
        | FlowMeasuringElement -> "FlowMeasuringElement", None
        | FlowNozzle -> "FlowNozzle", None
        | MassFlowMeasuringElement -> "MassFlowMeasuringElement", None
        | PositiveDisplacementFlowMeter -> "PositiveDisplacementFlowMeter", None
        | TurbineFlowMeter -> "TurbineFlowMeter", None
        | VariableAreaFlowMeter -> "VariableAreaFlowMeter", None
        | VenturiTube -> "VenturiTube", None
        | VolumeFlowMeasuringElement -> "VolumeFlowMeasuringElement", None
        | OperatedValve -> "OperatedValve", None
        | AngleBallValve -> "AngleBallValve", None
        | AngleGlobeValve -> "AngleGlobeValve", None
        | AnglePlugValve -> "AnglePlugValve", None
        | AngleValve -> "AngleValve", None
        | BallValve -> "BallValve", None
        | ButterflyValve -> "ButterflyValve", None
        | GateValve -> "GateValve", None
        | GlobeValve -> "GlobeValve", None
        | NeedleValve -> "NeedleValve", None
        | PlugValve -> "PlugValve", None
        | StraightwayValve -> "StraightwayValve", None
        | PipeFitting -> "PipeFitting", None
        | BlindFlange -> "BlindFlange", None
        | ClampedFlangeCoupling -> "ClampedFlangeCoupling", None
        | Compensator -> "Compensator", None
        | ConicalStrainer -> "ConicalStrainer", None
        | Flange -> "Flange", None
        | FlangedConnection -> "FlangedConnection", None
        | Funnel -> "Funnel", None
        | Hose -> "Hose", None
        | IlluminatedSightGlass -> "IlluminatedSightGlass", None
        | InLineMixer -> "InLineMixer", None
        | LineBlind -> "LineBlind", None
        | Penetration -> "Penetration", None
        | PipeCoupling -> "PipeCoupling", None
        | PipeFlangeSpacer -> "PipeFlangeSpacer", None
        | PipeFlangeSpade -> "PipeFlangeSpade", None
        | PipeReducer -> "PipeReducer", None
        | PipeTee -> "PipeTee", None
        | RestrictionOrifice -> "RestrictionOrifice", None
        | Sensorwell tag -> "Sensorwell", Some tag
        | SightGlass -> "SightGlass", None
        | Silencer -> "Silencer", None
        | SteamTrap -> "SteamTrap", None
        | Strainer -> "Strainer", None
        | VentLine -> "VentLine", None
        | SafetyValveOrFitting -> "SafetyValveOrFitting", None
        | BreatherValve -> "BreatherValve", None
        | FlameArrestor -> "FlameArrestor", None
        | RuptureDisc -> "RuptureDisc", None
        | SpringLoadedAngleGlobeSafetyValve -> "SpringLoadedAngleGlobeSafetyValve", None
        | SpringLoadedGlobeSafetyValve -> "SpringLoadedGlobeSafetyValve", None
        | PropertyBreak -> "PropertyBreak", None

    /// 全部 65 个案例各构造一次。
    let allCases: PipingNodeOwnerDetail list =
        [
            Nozzle
            AccessNozzle
            InstrumentNozzle
            ProcessNozzle
            PipeOffPageConnector
            FlowInPipeOffPageConnector
            FlowOutPipeOffPageConnector
            PipingComponent
            CheckValve
            GlobeCheckValve
            SwingCheckValve
            InlineMeasuringElement
            ElectromagneticFlowMeter
            FlowMeasuringElement
            FlowNozzle
            MassFlowMeasuringElement
            PositiveDisplacementFlowMeter
            TurbineFlowMeter
            VariableAreaFlowMeter
            VenturiTube
            VolumeFlowMeasuringElement
            OperatedValve
            AngleBallValve
            AngleGlobeValve
            AnglePlugValve
            AngleValve
            BallValve
            ButterflyValve
            GateValve
            GlobeValve
            NeedleValve
            PlugValve
            StraightwayValve
            PipeFitting
            BlindFlange
            ClampedFlangeCoupling
            Compensator
            ConicalStrainer
            Flange
            FlangedConnection
            Funnel
            Hose
            IlluminatedSightGlass
            InLineMixer
            LineBlind
            Penetration
            PipeCoupling
            PipeFlangeSpacer
            PipeFlangeSpade
            PipeReducer
            PipeTee
            RestrictionOrifice
            Sensorwell "SW1"
            SightGlass
            Silencer
            SteamTrap
            Strainer
            VentLine
            SafetyValveOrFitting
            BreatherValve
            FlameArrestor
            RuptureDisc
            SpringLoadedAngleGlobeSafetyValve
            SpringLoadedGlobeSafetyValve
            PropertyBreak
        ]

    let private names = allCases |> List.map (describe >> fst)

    [<Fact>]
    let ``PipingNodeOwnerDetail 联合包含全部 65 个案例``() =
        Assert.Equal(65, allCases.Length)

    [<Fact>]
    let ``65 个案例名互不重复``() =
        Assert.Equal(65, names |> List.distinct |> List.length)

    [<Fact>]
    let ``仅 Sensorwell 携带荷载，其余 64 个案例无荷载``() =
        let tags = allCases |> List.map (describe >> snd)
        Assert.Equal(1, tags |> List.choose id |> List.length)
        Assert.Equal(64, tags |> List.filter Option.isNone |> List.length)
        Assert.Equal("SW1", tags |> List.choose id |> List.head)

    [<Fact>]
    let ``抽象中间类（PipingComponent、InlineMeasuringElement、OperatedValve、PipeFitting、SafetyValveOrFitting）有案例``() =
        Assert.Contains("PipingComponent", names)
        Assert.Contains("InlineMeasuringElement", names)
        Assert.Contains("OperatedValve", names)
        Assert.Contains("PipeFitting", names)
        Assert.Contains("SafetyValveOrFitting", names)

    [<Fact>]
    let ``Sensorwell 携带 SensorwellTypeRepresentation``() =
        match Sensorwell "SW-Sensorwell" with
        | Sensorwell tag -> Assert.Equal("SW-Sensorwell", tag)
        | _ -> failwith "expected Sensorwell"

    [<Fact>]
    let ``PipingNodeOwner 记录绑定 tag 与细节``() =
        let valve: PipingNodeOwner = { tag = "XV-101"; detail = PipingNodeOwnerDetail.OperatedValve }
        Assert.Equal("XV-101", valve.tag)
        Assert.Equal(PipingNodeOwnerDetail.OperatedValve, valve.detail)
        match valve.detail with
        | PipingNodeOwnerDetail.OperatedValve -> ()
        | _ -> failwith "expected OperatedValve"
