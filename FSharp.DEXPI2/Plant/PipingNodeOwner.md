# PipingNodeOwner 继承树（非抽象子类）

DEXPI 2.0 模型类 **PipingNodeOwner**（XMI id ID996，`dexpi2::plant::piping`，**抽象**）的全部派生类型。

- 传递后代：**65 个**
- 其中抽象：**2 个**（`PipeOffPageConnector`、`PipingComponent`，不属"非抽象"）
- **非抽象子类：63 个**（含非叶类），分布在 `Dexpi2.Plant.Piping` 命名空间

## 继承树

```
PipingNodeOwner (ID996) [抽象]
├── Nozzle (ID1224)
│   ├── AccessNozzle (ID1223)
│   ├── InstrumentNozzle (ID1576)
│   └── ProcessNozzle (ID1685)
├── PipeOffPageConnector (ID958) [抽象]
│   ├── FlowInPipeOffPageConnector (ID957)
│   └── FlowOutPipeOffPageConnector (ID962)
├── PipingComponent (ID939) [抽象]
│   ├── CheckValve (ID938)
│   │   ├── GlobeCheckValve (ID966)
│   │   └── SwingCheckValve (ID1132)
│   ├── InlineMeasuringElement (ID802)
│   │   ├── ElectromagneticFlowMeter (ID950)
│   │   ├── FlowMeasuringElement (ID960)
│   │   ├── FlowNozzle (ID961)
│   │   ├── MassFlowMeasuringElement (ID976)
│   │   ├── PositiveDisplacementFlowMeter (ID1103)
│   │   ├── TurbineFlowMeter (ID1133)
│   │   ├── VariableAreaFlowMeter (ID1134)
│   │   ├── VenturiTube (ID1137)
│   │   └── VolumeFlowMeasuringElement (ID1138)
│   ├── OperatedValve (ID851)
│   │   ├── AngleBallValve (ID928)
│   │   ├── AngleGlobeValve (ID929)
│   │   ├── AnglePlugValve (ID930)
│   │   ├── AngleValve (ID931)
│   │   ├── BallValve (ID932)
│   │   ├── ButterflyValve (ID937)
│   │   ├── GateValve (ID965)
│   │   ├── GlobeValve (ID967)
│   │   ├── NeedleValve (ID977)
│   │   ├── PlugValve (ID1102)
│   │   └── StraightwayValve (ID1130)
│   ├── PipeFitting (ID934)
│   │   ├── BlindFlange (ID933)
│   │   ├── ClampedFlangeCoupling (ID945)
│   │   ├── Compensator (ID946)
│   │   ├── ConicalStrainer (ID947)
│   │   ├── Flange (ID955)
│   │   ├── FlangedConnection (ID956)
│   │   ├── Funnel (ID964)
│   │   ├── Hose (ID968)
│   │   ├── IlluminatedSightGlass (ID969)
│   │   ├── InLineMixer (ID970)
│   │   ├── LineBlind (ID975)
│   │   ├── Penetration (ID985)
│   │   ├── PipeCoupling (ID987)
│   │   ├── PipeFlangeSpacer (ID993)
│   │   ├── PipeFlangeSpade (ID994)
│   │   ├── PipeReducer (ID1010)
│   │   ├── PipeTee (ID1011)
│   │   ├── RestrictionOrifice (ID1109)
│   │   ├── Sensorwell (ID897)
│   │   ├── SightGlass (ID1125)
│   │   ├── Silencer (ID1126)
│   │   ├── SteamTrap (ID1129)
│   │   ├── Strainer (ID1131)
│   │   └── VentLine (ID1135)
│   └── SafetyValveOrFitting (ID936)
│       ├── BreatherValve (ID935)
│       ├── FlameArrestor (ID951)
│       ├── RuptureDisc (ID1110)
│       ├── SpringLoadedAngleGlobeSafetyValve (ID1127)
│       └── SpringLoadedGlobeSafetyValve (ID1128)
└── PropertyBreak (ID1104)
```
## 要点
- **4 个直接子类**：`Nozzle`、`PipeOffPageConnector`[抽象]、`PipingComponent`[抽象]、`PropertyBreak`；
- **分支规模**：
  - `PipingComponent` 支 57 个（最大的一支：阀门 11 + 管件 24 + 测量元件 9 + 安全阀 5 + 止回阀 3 + PipingComponent 自身）；
  - `Nozzle` 支 4 个（Nozzle 自身 + Access/Instrument/Process 三个专用接管口）；
  - `PipeOffPageConnector` 支 3 个（抽象基类 + 进/出两个方向）；
  - `PropertyBreak` 支 1 个（叶类，无后代）；
- 抽象后代 2 个（`PipeOffPageConnector`、`PipingComponent`），不进入"非抽象子类"集合；
- 继承成员：`Nodes`（UML 0..* 聚合 `PipingNode`），所有 65 个后代展平后均携带该成员
  （C# 中为 `List<PipingNode>`，元素非空）；
- `PipingNode` 本身**不**继承自 PipingNodeOwner——它是被聚合的"连接点"，
  公称压力等属性由持有它的 Nozzle/Valve 决定，不自带；
- 与 `NozzleOwner` 的关系：`Nozzle` 同时是两者的后代——
  `NozzleOwner` 给 Nozzle 带来 `Nozzles` 列表（罐/设备拥有哪些接管口），
  `PipingNodeOwner` 给 Nozzle 带来 `Nodes` 列表（接管口在管道拓扑里的节点）。
## 数据来源

`Dexpi2.Cpp/Generated/dexpi2.hpp`的多继承。
