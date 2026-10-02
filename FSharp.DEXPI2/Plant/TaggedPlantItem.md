# TaggedPlantItem 继承树（非抽象子类）

DEXPI 2.0 模型类 **TaggedPlantItem**（XMI id ID1163，`dexpi2::plant::processequipment`，**抽象**）的全部派生类型。

- 传递后代：**101 个**
- 其中抽象：**1 个**（ProcessEquipment，不属“非抽象”）
- **非抽象子类：100 个**（含非叶类），全部位于 `Dexpi2.Plant.Processequipment`

## 继承树

```
TaggedPlantItem (ID1163) [抽象]
├── ProcessEquipment (ID1226) [抽象]
│   ├── Agglomerator (ID1225)
│   │   ├── ReciprocatingPressureAgglomerator (ID1704)
│   │   ├── RotatingGrowthAgglomerator (ID1744)
│   │   └── RotatingPressureAgglomerator (ID1748)
│   ├── Agitator (ID1245)
│   ├── Blower (ID1294)
│   │   ├── AxialBlower (ID1293)
│   │   └── CentrifugalBlower (ID1334)
│   ├── Burner (ID1332)
│   ├── Centrifuge (ID1351)
│   │   ├── FilteringCentrifuge (ID1509)
│   │   └── SedimentalCentrifuge (ID1758)
│   ├── Compressor (ID1270)
│   │   ├── AirEjector (ID1269)
│   │   ├── AxialCompressor (ID1298)
│   │   ├── CentrifugalCompressor (ID1338)
│   │   ├── ReciprocatingCompressor (ID1694)
│   │   └── RotaryCompressor (ID1723)
│   ├── CoolingTower (ID1409)
│   │   ├── DryCoolingTower (ID1442)
│   │   ├── SprayCooler (ID1788)
│   │   └── WetCoolingTower (ID1856)
│   ├── Dryer (ID1401)
│   │   ├── ConvectionDryer (ID1400)
│   │   └── HeatedSurfaceDryer (ID1563)
│   ├── ElectricGenerator (ID1277)
│   │   ├── AlternatingCurrentGenerator (ID1276)
│   │   └── DirectCurrentGenerator (ID1427)
│   ├── Extruder (ID1483)
│   │   ├── ReciprocatingExtruder (ID1700)
│   │   └── RotatingExtruder (ID1739)
│   ├── Fan (ID1305)
│   │   ├── AxialFan (ID1304)
│   │   └── RadialFan (ID1689)
│   ├── Feeder (ID1491)
│   ├── Filter (ID1496)
│   │   ├── GasFilter (ID1525)
│   │   └── LiquidFilter (ID1586)
│   ├── Heater (ID1327)
│   │   ├── Boiler (ID1326)
│   │   ├── ElectricHeater (ID1465)
│   │   ├── Furnace (ID1524)
│   │   └── SteamGenerator (ID1799)
│   ├── HeatExchanger (ID1261)
│   │   ├── AirCoolingSystem (ID1260)
│   │   ├── PlateHeatExchanger (ID1648)
│   │   ├── SpiralHeatExchanger (ID1787)
│   │   ├── ThinFilmEvaporator (ID1811)
│   │   └── TubularHeatExchanger (ID1837)
│   ├── Mill (ID1419)
│   │   ├── Crusher (ID1418)
│   │   └── Grinder (ID1545)
│   ├── Mixer (ID1578)
│   │   ├── Kneader (ID1577)
│   │   ├── RotaryMixer (ID1729)
│   │   └── StaticMixer (ID1795)
│   ├── MobileTransportSystem (ID1522)
│   │   ├── ForkliftTruck (ID1521)
│   │   ├── RailWaggon (ID1693)
│   │   ├── Ship (ID1772)
│   │   ├── TransportableContainer (ID1825)
│   │   └── Truck (ID1826)
│   ├── Motor (ID1283)
│   │   ├── AlternatingCurrentMotor (ID1282)
│   │   ├── CombustionEngine (ID1392)
│   │   └── DirectCurrentMotor (ID1428)
│   ├── PackagingSystem (ID1640)
│   ├── ProcessColumn (ID1655)
│   ├── Pump (ID1345)
│   │   ├── CentrifugalPump (ID1344)
│   │   ├── EjectorPump (ID1459)
│   │   ├── ReciprocatingPump (ID1713)
│   │   └── RotaryPump (ID1733)
│   ├── Separator (ID1480)
│   │   ├── ElectricalSeparator (ID1479)
│   │   ├── GravitationalSeparator (ID1542)
│   │   ├── MechanicalSeparator (ID1598)
│   │   └── ScrubbingSeparator (ID1757)
│   ├── Sieve (ID1720)
│   │   ├── RevolvingSieve (ID1719)
│   │   ├── StationarySieve (ID1797)
│   │   └── VibratingSieve (ID1851)
│   ├── StationaryTransportSystem (ID1404)
│   │   ├── Conveyor (ID1403)
│   │   ├── Lift (ID1582)
│   │   └── LoadingUnloadingSystem (ID1594)
│   ├── Turbine (ID1534)
│   │   ├── GasTurbine (ID1533)
│   │   └── SteamTurbine (ID1800)
│   ├── Vessel (ID1653)
│   │   ├── PressureVessel (ID1652)
│   │   ├── Silo (ID1786)
│   │   └── Tank (ID1809)
│   ├── WasteGasEmitter (ID1375)
│   │   ├── Chimney (ID1374)
│   │   └── Flare (ID1520)
│   └── Weigher (ID1310)
│       ├── BatchWeigher (ID1309)
│       └── ContinuousWeigher (ID1398)
└── TaggedColumnSection (ID1804)
```

## 要点

- 100 个非抽象子类中，**非叶类也包含在内**：`ProcessEquipment` 的 29 个直系具体子类
  几乎都是中间类（如 `Agglomerator`、`Pump`、`Motor`、`HeatExchanger`、`Filter`、
  `Separator` 等），其中 24 个还有下一级子类，仅 5 个为叶类
  （`Agitator`、`Burner`、`Feeder`、`PackagingSystem`、`ProcessColumn`）；
- 分支统计：`ProcessEquipment` 支 99 个具体（29 直系 + 70 二级，无第三级），
  `TaggedColumnSection` 1 个，合计 100；
- 抽象后代仅 `ProcessEquipment` 一个，不进入“非抽象子类”集合。

## 数据来源

`Dexpi2.CSharp.Generator/input/*.hpp` 中 `[flattened; bases inlined: ...]`
直系基类注释的传递闭包（与原始 `Dexpi2.Cpp/Generated/dexpi2.hpp` 的多继承声明一致）。
XMI id 逐类取自同一批头文件的类注释。
