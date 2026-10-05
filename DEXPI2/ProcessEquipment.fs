namespace DEXPI2

/// <summary>DEXPI 2.0 模型类 ProcessEquipment（XMI id ID1226）的可区分联合：全部具体派生类型（含非叶类）</summary>
type ProcessEquipment =
    // Agglomerator 支
    | Agglomerator of TagName: string
    | ReciprocatingPressureAgglomerator of TagName: string
    | RotatingGrowthAgglomerator of TagName: string
    | RotatingPressureAgglomerator of TagName: string
    // Agitator 支（叶类）
    | Agitator of TagName: string
    // Blower 支
    | Blower of TagName: string
    | AxialBlower of TagName: string
    | CentrifugalBlower of TagName: string
    // Burner 支（叶类）
    | Burner of TagName: string
    // Centrifuge 支
    | Centrifuge of TagName: string
    | FilteringCentrifuge of TagName: string
    | SedimentalCentrifuge of TagName: string
    // Compressor 支
    | Compressor of TagName: string
    | AirEjector of TagName: string
    | AxialCompressor of TagName: string
    | CentrifugalCompressor of TagName: string
    | ReciprocatingCompressor of TagName: string
    | RotaryCompressor of TagName: string
    // CoolingTower 支
    | CoolingTower of TagName: string
    | DryCoolingTower of TagName: string
    | SprayCooler of TagName: string
    | WetCoolingTower of TagName: string
    // Dryer 支
    | Dryer of TagName: string
    | ConvectionDryer of TagName: string
    | HeatedSurfaceDryer of TagName: string
    // ElectricGenerator 支
    | ElectricGenerator of TagName: string
    | AlternatingCurrentGenerator of TagName: string
    | DirectCurrentGenerator of TagName: string
    // Extruder 支
    | Extruder of TagName: string
    | ReciprocatingExtruder of TagName: string
    | RotatingExtruder of TagName: string
    // Fan 支
    | Fan of TagName: string
    | AxialFan of TagName: string
    | RadialFan of TagName: string
    // Feeder 支（叶类）
    | Feeder of TagName: string
    // Filter 支
    | Filter of TagName: string
    | GasFilter of TagName: string
    | LiquidFilter of TagName: string
    // Heater 支
    | Heater of TagName: string
    | Boiler of TagName: string
    | ElectricHeater of TagName: string
    | Furnace of TagName: string
    | SteamGenerator of TagName: string
    // HeatExchanger 支
    | HeatExchanger of TagName: string
    | AirCoolingSystem of TagName: string
    | PlateHeatExchanger of TagName: string
    | SpiralHeatExchanger of TagName: string
    | ThinFilmEvaporator of TagName: string
    | TubularHeatExchanger of TagName: string
    // Mill 支
    | Mill of TagName: string
    | Crusher of TagName: string
    | Grinder of TagName: string
    // Mixer 支
    | Mixer of TagName: string
    | Kneader of TagName: string
    | RotaryMixer of TagName: string
    | StaticMixer of TagName: string
    // MobileTransportSystem 支
    | MobileTransportSystem of TagName: string
    | ForkliftTruck of TagName: string
    | RailWaggon of TagName: string
    | Ship of TagName: string
    | TransportableContainer of TagName: string
    | Truck of TagName: string
    // Motor 支
    | Motor of TagName: string
    | AlternatingCurrentMotor of TagName: string
    | CombustionEngine of TagName: string
    | DirectCurrentMotor of TagName: string
    // PackagingSystem 支（叶类）
    | PackagingSystem of TagName: string
    // ProcessColumn 支（叶类）
    | ProcessColumn of TagName: string
    // Pump 支
    | Pump of TagName: string
    | CentrifugalPump of TagName: string
    | EjectorPump of TagName: string
    | ReciprocatingPump of TagName: string
    | RotaryPump of TagName: string
    // Separator 支
    | Separator of TagName: string
    | ElectricalSeparator of TagName: string
    | GravitationalSeparator of TagName: string
    | MechanicalSeparator of TagName: string
    | ScrubbingSeparator of TagName: string
    // Sieve 支
    | Sieve of TagName: string
    | RevolvingSieve of TagName: string
    | StationarySieve of TagName: string
    | VibratingSieve of TagName: string
    // StationaryTransportSystem 支
    | StationaryTransportSystem of TagName: string
    | Conveyor of TagName: string
    | Lift of TagName: string
    | LoadingUnloadingSystem of TagName: string
    // Turbine 支
    | Turbine of TagName: string
    | GasTurbine of TagName: string
    | SteamTurbine of TagName: string
    // Vessel 支
    | Vessel of TagName: string
    | PressureVessel of TagName: string
    | Silo of TagName: string
    | Tank of TagName: string
    // WasteGasEmitter 支
    | WasteGasEmitter of TagName: string
    | Chimney of TagName: string
    | Flare of TagName: string
    // Weigher 支
    | Weigher of TagName: string
    | BatchWeigher of TagName: string
    | ContinuousWeigher of TagName: string
