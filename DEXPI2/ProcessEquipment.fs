namespace DEXPI2

/// <summary>DEXPI 2.0 模型类 ProcessEquipment（XMI id ID1226）的可区分联合：全部具体派生类型（含非叶类）</summary>
type ProcessEquipment =
    // Agglomerator 支
    | Agglomerator of tag:string * nozzles: string list
    | ReciprocatingPressureAgglomerator of tag:string * nozzles: string list
    | RotatingGrowthAgglomerator of tag:string * nozzles: string list
    | RotatingPressureAgglomerator of tag:string * nozzles: string list
    // Agitator 支（叶类）
    | Agitator of tag:string * nozzles: string list
    // Blower 支
    | Blower of tag:string * nozzles: string list
    | AxialBlower of tag:string * nozzles: string list
    | CentrifugalBlower of tag:string * nozzles: string list
    // Burner 支（叶类）
    | Burner of tag:string * nozzles: string list
    // Centrifuge 支
    | Centrifuge of tag:string * nozzles: string list
    | FilteringCentrifuge of tag:string * nozzles: string list
    | SedimentalCentrifuge of tag:string * nozzles: string list
    // Compressor 支
    | Compressor of tag:string * nozzles: string list
    | AirEjector of tag:string * nozzles: string list
    | AxialCompressor of tag:string * nozzles: string list
    | CentrifugalCompressor of tag:string * nozzles: string list
    | ReciprocatingCompressor of tag:string * nozzles: string list
    | RotaryCompressor of tag:string * nozzles: string list
    // CoolingTower 支
    | CoolingTower of tag:string * nozzles: string list
    | DryCoolingTower of tag:string * nozzles: string list
    | SprayCooler of tag:string * nozzles: string list
    | WetCoolingTower of tag:string * nozzles: string list
    // Dryer 支
    | Dryer of tag:string * nozzles: string list
    | ConvectionDryer of tag:string * nozzles: string list
    | HeatedSurfaceDryer of tag:string * nozzles: string list
    // ElectricGenerator 支
    | ElectricGenerator of tag:string * nozzles: string list
    | AlternatingCurrentGenerator of tag:string * nozzles: string list
    | DirectCurrentGenerator of tag:string * nozzles: string list
    // Extruder 支
    | Extruder of tag:string * nozzles: string list
    | ReciprocatingExtruder of tag:string * nozzles: string list
    | RotatingExtruder of tag:string * nozzles: string list
    // Fan 支
    | Fan of tag:string * nozzles: string list
    | AxialFan of tag:string * nozzles: string list
    | RadialFan of tag:string * nozzles: string list
    // Feeder 支（叶类）
    | Feeder of tag:string * nozzles: string list
    // Filter 支
    | Filter of tag:string * nozzles: string list
    | GasFilter of tag:string * nozzles: string list
    | LiquidFilter of tag:string * nozzles: string list
    // Heater 支
    | Heater of tag:string * nozzles: string list
    | Boiler of tag:string * nozzles: string list
    | ElectricHeater of tag:string * nozzles: string list
    | Furnace of tag:string * nozzles: string list
    | SteamGenerator of tag:string * nozzles: string list
    // HeatExchanger 支
    | HeatExchanger of tag:string * nozzles: string list
    | AirCoolingSystem of tag:string * nozzles: string list
    | PlateHeatExchanger of tag:string * nozzles: string list
    | SpiralHeatExchanger of tag:string * nozzles: string list
    | ThinFilmEvaporator of tag:string * nozzles: string list
    | TubularHeatExchanger of tag:string * nozzles: string list
    // Mill 支
    | Mill of tag:string * nozzles: string list
    | Crusher of tag:string * nozzles: string list
    | Grinder of tag:string * nozzles: string list
    // Mixer 支
    | Mixer of tag:string * nozzles: string list
    | Kneader of tag:string * nozzles: string list
    | RotaryMixer of tag:string * nozzles: string list
    | StaticMixer of tag:string * nozzles: string list
    // MobileTransportSystem 支
    | MobileTransportSystem of tag:string * nozzles: string list
    | ForkliftTruck of tag:string * nozzles: string list
    | RailWaggon of tag:string * nozzles: string list
    | Ship of tag:string * nozzles: string list
    | TransportableContainer of tag:string * nozzles: string list
    | Truck of tag:string * nozzles: string list
    // Motor 支
    | Motor of tag:string * nozzles: string list
    | AlternatingCurrentMotor of tag:string * nozzles: string list
    | CombustionEngine of tag:string * nozzles: string list
    | DirectCurrentMotor of tag:string * nozzles: string list
    // PackagingSystem 支（叶类）
    | PackagingSystem of tag:string * nozzles: string list
    // ProcessColumn 支（叶类）
    | ProcessColumn of tag:string * nozzles: string list
    // Pump 支
    | Pump of tag:string * nozzles: string list
    | CentrifugalPump of tag:string * nozzles: string list
    | EjectorPump of tag:string * nozzles: string list
    | ReciprocatingPump of tag:string * nozzles: string list
    | RotaryPump of tag:string * nozzles: string list
    // Separator 支
    | Separator of tag:string * nozzles: string list
    | ElectricalSeparator of tag:string * nozzles: string list
    | GravitationalSeparator of tag:string * nozzles: string list
    | MechanicalSeparator of tag:string * nozzles: string list
    | ScrubbingSeparator of tag:string * nozzles: string list
    // Sieve 支
    | Sieve of tag:string * nozzles: string list
    | RevolvingSieve of tag:string * nozzles: string list
    | StationarySieve of tag:string * nozzles: string list
    | VibratingSieve of tag:string * nozzles: string list
    // StationaryTransportSystem 支
    | StationaryTransportSystem of tag:string * nozzles: string list
    | Conveyor of tag:string * nozzles: string list
    | Lift of tag:string * nozzles: string list
    | LoadingUnloadingSystem of tag:string * nozzles: string list
    // Turbine 支
    | Turbine of tag:string * nozzles: string list
    | GasTurbine of tag:string * nozzles: string list
    | SteamTurbine of tag:string * nozzles: string list
    // Vessel 支
    | Vessel of tag:string * nozzles: string list
    | PressureVessel of tag:string * nozzles: string list
    | Silo of tag:string * nozzles: string list
    | Tank of tag:string * nozzles: string list
    // WasteGasEmitter 支
    | WasteGasEmitter of tag:string * nozzles: string list
    | Chimney of tag:string * nozzles: string list
    | Flare of tag:string * nozzles: string list
    // Weigher 支
    | Weigher of tag:string * nozzles: string list
    | BatchWeigher of tag:string * nozzles: string list
    | ContinuousWeigher of tag:string * nozzles: string list

