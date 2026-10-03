# PipingSourceItem / PipingTargetItem 继承树

DEXPI 2.0 中两个**空标记 mixin**（无自有成员），用于声明"某类可以作为管道网络段的起点/终点"。

| 类 | XMI id | 命名空间 | 抽象 |
|---|---|---|---|
| `PipingSourceItem` | ID959 | `dexpi2::plant::piping` | 是 |
| `PipingTargetItem` | ID963 | `dexpi2::plant::piping` | 是 |

两者都没有自有成员（UML 中 `// own members (none)`），仅通过多继承被其他类实现，
用于在 `PipingNetworkSegment.SourceItem` / `TargetItem` 属性中做多态引用。

## PipingSourceItem（管道起点）

可作为 `PipingNetworkSegment.SourceItem` 的类型。

- 传递后代：**63 个**
- 其中抽象：**1 个**（PipingComponent）
- 非抽象子类：**62 个**

```
PipingSourceItem (ID959) [抽象]
├── FlowInPipeOffPageConnector (ID957)
├── Nozzle (ID1224)
│   ├── AccessNozzle (ID1223)
│   ├── InstrumentNozzle (ID1576)
│   └── ProcessNozzle (ID1685)
├── PropertyBreak (ID1104)
└── PipingComponent (ID939) [抽象]
    ├── CheckValve (ID938)
    │   ├── GlobeCheckValve (ID966)
    │   └── SwingCheckValve (ID1132)
    ├── InlineMeasuringElement (ID802)
    │   ├── ElectromagneticFlowMeter (ID950)
    │   ├── FlowMeasuringElement (ID960)
    │   ├── FlowNozzle (ID961)
    │   ├── MassFlowMeasuringElement (ID976)
    │   ├── PositiveDisplacementFlowMeter (ID1103)
    │   ├── TurbineFlowMeter (ID1133)
    │   ├── VariableAreaFlowMeter (ID1134)
    │   ├── VenturiTube (ID1137)
    │   └── VolumeFlowMeasuringElement (ID1138)
    ├── OperatedValve (ID851)
    │   ├── AngleBallValve (ID928)
    │   ├── AngleGlobeValve (ID929)
    │   ├── AnglePlugValve (ID930)
    │   ├── AngleValve (ID931)
    │   ├── BallValve (ID932)
    │   ├── ButterflyValve (ID937)
    │   ├── GateValve (ID965)
    │   ├── GlobeValve (ID967)
    │   ├── NeedleValve (ID977)
    │   ├── PlugValve (ID1102)
    │   └── StraightwayValve (ID1130)
    ├── PipeFitting (ID934)
    │   ├── BlindFlange (ID933)
    │   ├── ClampedFlangeCoupling (ID945)
    │   ├── Compensator (ID946)
    │   ├── ConicalStrainer (ID947)
    │   ├── Flange (ID955)
    │   ├── FlangedConnection (ID956)
    │   ├── Funnel (ID964)
    │   ├── Hose (ID968)
    │   ├── IlluminatedSightGlass (ID969)
    │   ├── InLineMixer (ID970)
    │   ├── LineBlind (ID975)
    │   ├── Penetration (ID985)
    │   ├── PipeCoupling (ID987)
    │   ├── PipeFlangeSpacer (ID993)
    │   ├── PipeFlangeSpade (ID994)
    │   ├── PipeReducer (ID1010)
    │   ├── PipeTee (ID1011)
    │   ├── RestrictionOrifice (ID1109)
    │   ├── Sensorwell (ID897)
    │   ├── SightGlass (ID1125)
    │   ├── Silencer (ID1126)
    │   ├── SteamTrap (ID1129)
    │   ├── Strainer (ID1131)
    │   └── VentLine (ID1135)
    └── SafetyValveOrFitting (ID936)
        ├── BreatherValve (ID935)
        ├── FlameArrestor (ID951)
        ├── RuptureDisc (ID1110)
        ├── SpringLoadedAngleGlobeSafetyValve (ID1127)
        └── SpringLoadedGlobeSafetyValve (ID1128)
```

## PipingTargetItem（管道终点）

可作为 `PipingNetworkSegment.TargetItem` 的类型。

- 传递后代：**63 个**
- 其中抽象：**1 个**（PipingComponent）
- 非抽象子类：**62 个**

```
PipingTargetItem (ID963) [抽象]
├── FlowOutPipeOffPageConnector (ID962)
├── Nozzle (ID1224)
│   ├── AccessNozzle (ID1223)
│   ├── InstrumentNozzle (ID1576)
│   └── ProcessNozzle (ID1685)
├── PropertyBreak (ID1104)
└── PipingComponent (ID939) [抽象]
    ├── CheckValve (ID938)
    │   ├── GlobeCheckValve (ID966)
    │   └── SwingCheckValve (ID1132)
    ├── InlineMeasuringElement (ID802)
    │   ├── ElectromagneticFlowMeter (ID950)
    │   ├── FlowMeasuringElement (ID960)
    │   ├── FlowNozzle (ID961)
    │   ├── MassFlowMeasuringElement (ID976)
    │   ├── PositiveDisplacementFlowMeter (ID1103)
    │   ├── TurbineFlowMeter (ID1133)
    │   ├── VariableAreaFlowMeter (ID1134)
    │   ├── VenturiTube (ID1137)
    │   └── VolumeFlowMeasuringElement (ID1138)
    ├── OperatedValve (ID851)
    │   ├── AngleBallValve (ID928)
    │   ├── AngleGlobeValve (ID929)
    │   ├── AnglePlugValve (ID930)
    │   ├── AngleValve (ID931)
    │   ├── BallValve (ID932)
    │   ├── ButterflyValve (ID937)
    │   ├── GateValve (ID965)
    │   ├── GlobeValve (ID967)
    │   ├── NeedleValve (ID977)
    │   ├── PlugValve (ID1102)
    │   └── StraightwayValve (ID1130)
    ├── PipeFitting (ID934)
    │   ├── BlindFlange (ID933)
    │   ├── ClampedFlangeCoupling (ID945)
    │   ├── Compensator (ID946)
    │   ├── ConicalStrainer (ID947)
    │   ├── Flange (ID955)
    │   ├── FlangedConnection (ID956)
    │   ├── Funnel (ID964)
    │   ├── Hose (ID968)
    │   ├── IlluminatedSightGlass (ID969)
    │   ├── InLineMixer (ID970)
    │   ├── LineBlind (ID975)
    │   ├── Penetration (ID985)
    │   ├── PipeCoupling (ID987)
    │   ├── PipeFlangeSpacer (ID993)
    │   ├── PipeFlangeSpade (ID994)
    │   ├── PipeReducer (ID1010)
    │   ├── PipeTee (ID1011)
    │   ├── RestrictionOrifice (ID1109)
    │   ├── Sensorwell (ID897)
    │   ├── SightGlass (ID1125)
    │   ├── Silencer (ID1126)
    │   ├── SteamTrap (ID1129)
    │   ├── Strainer (ID1131)
    │   └── VentLine (ID1135)
    └── SafetyValveOrFitting (ID936)
        ├── BreatherValve (ID935)
        ├── FlameArrestor (ID951)
        ├── RuptureDisc (ID1110)
        ├── SpringLoadedAngleGlobeSafetyValve (ID1127)
        └── SpringLoadedGlobeSafetyValve (ID1128)
```

## 要点

- 两个 mixin **都没有自有成员**——继承它们不会带来任何字段，纯粹是类型标记；
- **直接子类对比**：

  | 直接子类 | PipingSourceItem | PipingTargetItem |
  |---|:---:|:---:|
  | Nozzle | ✓ | ✓ |
  | PropertyBreak | ✓ | ✓ |
  | PipingComponent（含全部阀门/管件/仪表） | ✓ | ✓ |
  | FlowInPipeOffPageConnector | ✓ | ✗ |
  | FlowOutPipeOffPageConnector | ✗ | ✓ |

- 唯一区别在**跨页连接器**：`FlowIn` 只能做起点，`FlowOut` 只能做终点——
  物理意义是流体方向：流体从跨页连接器"进来"的那一页是起点，"出去"的那一页是终点；
- 其余 62 个后代（Nozzle + PropertyBreak + 全部 PipingComponent 分支）**同时**是 Source 和 Target——
  阀门、管件、接管口都可以是管段的任意一端；
- 与 `PipingNodeOwner` 的关系：所有 PipingSourceItem/PipingTargetItem 后代**同时**也是 PipingNodeOwner 后代
  （它们都自带 `Nodes: List<PipingNode>`），这不是巧合——能做管道端点的东西必然有管道节点。

## 数据来源

`Dexpi2.Cpp/Generated/dexpi2.hpp` 中各类 `class X : public Y, public Z, ...`
多继承声明的传递闭包。

直接子类经 `DEXPI 2.0 EA HTML export` 中 `PipingSourceItem`(EA551)、
`PipingTargetItem`(EA552) 页面 Links 表 Generalization 方向 "From" 交叉验证。
