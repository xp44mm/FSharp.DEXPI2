namespace DEXPI2.Test

open Xunit

open DEXPI2

module ProcessEquipmentTest =
    /// 穷举全部 99 个案例（编译期穷尽性检查），返回案例名。
    let private caseName (detail: ProcessEquipmentDetail) : string =
        match detail with
        | Agglomerator -> "Agglomerator"
        | ReciprocatingPressureAgglomerator -> "ReciprocatingPressureAgglomerator"
        | RotatingGrowthAgglomerator -> "RotatingGrowthAgglomerator"
        | RotatingPressureAgglomerator -> "RotatingPressureAgglomerator"
        | Agitator -> "Agitator"
        | Blower -> "Blower"
        | AxialBlower -> "AxialBlower"
        | CentrifugalBlower -> "CentrifugalBlower"
        | Burner -> "Burner"
        | Centrifuge -> "Centrifuge"
        | FilteringCentrifuge -> "FilteringCentrifuge"
        | SedimentalCentrifuge -> "SedimentalCentrifuge"
        | Compressor -> "Compressor"
        | AirEjector -> "AirEjector"
        | AxialCompressor -> "AxialCompressor"
        | CentrifugalCompressor -> "CentrifugalCompressor"
        | ReciprocatingCompressor -> "ReciprocatingCompressor"
        | RotaryCompressor -> "RotaryCompressor"
        | CoolingTower -> "CoolingTower"
        | DryCoolingTower -> "DryCoolingTower"
        | SprayCooler -> "SprayCooler"
        | WetCoolingTower -> "WetCoolingTower"
        | Dryer -> "Dryer"
        | ConvectionDryer -> "ConvectionDryer"
        | HeatedSurfaceDryer -> "HeatedSurfaceDryer"
        | ElectricGenerator -> "ElectricGenerator"
        | AlternatingCurrentGenerator -> "AlternatingCurrentGenerator"
        | DirectCurrentGenerator -> "DirectCurrentGenerator"
        | Extruder -> "Extruder"
        | ReciprocatingExtruder -> "ReciprocatingExtruder"
        | RotatingExtruder -> "RotatingExtruder"
        | Fan -> "Fan"
        | AxialFan -> "AxialFan"
        | RadialFan -> "RadialFan"
        | Feeder -> "Feeder"
        | Filter -> "Filter"
        | GasFilter -> "GasFilter"
        | LiquidFilter -> "LiquidFilter"
        | Heater -> "Heater"
        | Boiler -> "Boiler"
        | ElectricHeater -> "ElectricHeater"
        | Furnace -> "Furnace"
        | SteamGenerator -> "SteamGenerator"
        | HeatExchanger -> "HeatExchanger"
        | AirCoolingSystem -> "AirCoolingSystem"
        | PlateHeatExchanger -> "PlateHeatExchanger"
        | SpiralHeatExchanger -> "SpiralHeatExchanger"
        | ThinFilmEvaporator -> "ThinFilmEvaporator"
        | TubularHeatExchanger -> "TubularHeatExchanger"
        | Mill -> "Mill"
        | Crusher -> "Crusher"
        | Grinder -> "Grinder"
        | Mixer -> "Mixer"
        | Kneader -> "Kneader"
        | RotaryMixer -> "RotaryMixer"
        | StaticMixer -> "StaticMixer"
        | MobileTransportSystem -> "MobileTransportSystem"
        | ForkliftTruck -> "ForkliftTruck"
        | RailWaggon -> "RailWaggon"
        | Ship -> "Ship"
        | TransportableContainer -> "TransportableContainer"
        | Truck -> "Truck"
        | Motor -> "Motor"
        | AlternatingCurrentMotor -> "AlternatingCurrentMotor"
        | CombustionEngine -> "CombustionEngine"
        | DirectCurrentMotor -> "DirectCurrentMotor"
        | PackagingSystem -> "PackagingSystem"
        | ProcessColumn -> "ProcessColumn"
        | Pump -> "Pump"
        | CentrifugalPump -> "CentrifugalPump"
        | EjectorPump -> "EjectorPump"
        | ReciprocatingPump -> "ReciprocatingPump"
        | RotaryPump -> "RotaryPump"
        | Separator -> "Separator"
        | ElectricalSeparator -> "ElectricalSeparator"
        | GravitationalSeparator -> "GravitationalSeparator"
        | MechanicalSeparator -> "MechanicalSeparator"
        | ScrubbingSeparator -> "ScrubbingSeparator"
        | Sieve -> "Sieve"
        | RevolvingSieve -> "RevolvingSieve"
        | StationarySieve -> "StationarySieve"
        | VibratingSieve -> "VibratingSieve"
        | StationaryTransportSystem -> "StationaryTransportSystem"
        | Conveyor -> "Conveyor"
        | Lift -> "Lift"
        | LoadingUnloadingSystem -> "LoadingUnloadingSystem"
        | Turbine -> "Turbine"
        | GasTurbine -> "GasTurbine"
        | SteamTurbine -> "SteamTurbine"
        | Vessel -> "Vessel"
        | PressureVessel -> "PressureVessel"
        | Silo -> "Silo"
        | Tank -> "Tank"
        | WasteGasEmitter -> "WasteGasEmitter"
        | Chimney -> "Chimney"
        | Flare -> "Flare"
        | Weigher -> "Weigher"
        | BatchWeigher -> "BatchWeigher"
        | ContinuousWeigher -> "ContinuousWeigher"

    /// 全部 99 个案例各构造一次（29 直系 + 70 二级）。
    let allCases: ProcessEquipmentDetail list =
        [
            Agglomerator
            ReciprocatingPressureAgglomerator
            RotatingGrowthAgglomerator
            RotatingPressureAgglomerator
            Agitator
            Blower
            AxialBlower
            CentrifugalBlower
            Burner
            Centrifuge
            FilteringCentrifuge
            SedimentalCentrifuge
            Compressor
            AirEjector
            AxialCompressor
            CentrifugalCompressor
            ReciprocatingCompressor
            RotaryCompressor
            CoolingTower
            DryCoolingTower
            SprayCooler
            WetCoolingTower
            Dryer
            ConvectionDryer
            HeatedSurfaceDryer
            ElectricGenerator
            AlternatingCurrentGenerator
            DirectCurrentGenerator
            Extruder
            ReciprocatingExtruder
            RotatingExtruder
            Fan
            AxialFan
            RadialFan
            Feeder
            Filter
            GasFilter
            LiquidFilter
            Heater
            Boiler
            ElectricHeater
            Furnace
            SteamGenerator
            HeatExchanger
            AirCoolingSystem
            PlateHeatExchanger
            SpiralHeatExchanger
            ThinFilmEvaporator
            TubularHeatExchanger
            Mill
            Crusher
            Grinder
            Mixer
            Kneader
            RotaryMixer
            StaticMixer
            MobileTransportSystem
            ForkliftTruck
            RailWaggon
            Ship
            TransportableContainer
            Truck
            Motor
            AlternatingCurrentMotor
            CombustionEngine
            DirectCurrentMotor
            PackagingSystem
            ProcessColumn
            Pump
            CentrifugalPump
            EjectorPump
            ReciprocatingPump
            RotaryPump
            Separator
            ElectricalSeparator
            GravitationalSeparator
            MechanicalSeparator
            ScrubbingSeparator
            Sieve
            RevolvingSieve
            StationarySieve
            VibratingSieve
            StationaryTransportSystem
            Conveyor
            Lift
            LoadingUnloadingSystem
            Turbine
            GasTurbine
            SteamTurbine
            Vessel
            PressureVessel
            Silo
            Tank
            WasteGasEmitter
            Chimney
            Flare
            Weigher
            BatchWeigher
            ContinuousWeigher
        ]

    let private names = allCases |> List.map caseName

    [<Fact>]
    let ``ProcessEquipmentDetail 联合包含全部 99 个案例``() =
        Assert.Equal(99, allCases.Length)

    [<Fact>]
    let ``99 个案例名互不重复``() =
        Assert.Equal(99, names |> List.distinct |> List.length)

    [<Fact>]
    let ``非叶中间类（Agglomerator、HeatExchanger、Pump、Vessel、Separator）有案例``() =
        Assert.Contains("Agglomerator", names)
        Assert.Contains("HeatExchanger", names)
        Assert.Contains("Pump", names)
        Assert.Contains("Vessel", names)
        Assert.Contains("Separator", names)

    [<Fact>]
    let ``叶类（Agitator、Burner、Feeder、PackagingSystem、ProcessColumn）有案例``() =
        Assert.Contains("Agitator", names)
        Assert.Contains("Burner", names)
        Assert.Contains("Feeder", names)
        Assert.Contains("PackagingSystem", names)
        Assert.Contains("ProcessColumn", names)

    [<Fact>]
    let ``TaggedColumnSection 不属 ProcessEquipmentDetail``() =
        Assert.DoesNotContain("TaggedColumnSection", names)

    [<Fact>]
    let ``ProcessEquipment 记录绑定 TagName 与细节``() =
        let pump: ProcessEquipment = { TagName = "P-101"; detail = ProcessEquipmentDetail.Pump }
        Assert.Equal("P-101", pump.TagName)
        Assert.Equal(ProcessEquipmentDetail.Pump, pump.detail)
        match pump.detail with
        | ProcessEquipmentDetail.Pump -> ()
        | _ -> failwith "expected Pump"
