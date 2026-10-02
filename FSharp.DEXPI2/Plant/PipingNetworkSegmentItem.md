# PipingNetworkSegmentItem 继承树（非抽象子类）

DEXPI 2.0 模型类 **PipingNetworkSegmentItem**（XMI id ID995，`dexpi2::plant::piping`，**抽象**）的全部派生类型。

- 传递后代：**61 个**
- 其中抽象：**2 个**（PipeOffPageConnector、PipingComponent，不属“非抽象”）
- **非抽象子类：59 个**（含非叶类），全部位于 `Dexpi2.Plant.Piping`

## 继承树

```
PipingNetworkSegmentItem (ID995) [抽象]
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

- 59 个非抽象子类中，**非叶类也包含在内**：`CheckValve`、`InlineMeasuringElement`、
  `OperatedValve`、`PipeFitting`、`SafetyValveOrFitting`、`PropertyBreak` 等中间类本身
  也可实例化；
- 分支统计：`PipeOffPageConnector` 支 2、`CheckValve` 支 3、`InlineMeasuringElement`
  支 10、`OperatedValve` 支 12、`PipeFitting` 支 25、`SafetyValveOrFitting` 支 6、
  `PropertyBreak` 1，合计 59。

## 数据来源

`Dexpi2.CSharp.Generator/input/*.hpp` 中 `[flattened; bases inlined: ...]`
直系基类注释的传递闭包（与原始 `Dexpi2.Cpp/Generated/dexpi2.hpp` 的多继承声明一致）。
