# DEXPI2

本项目目标是只描述拓扑关系，实体的其他属性是下一轮的事情。

其他属性如pn,dn,材质，保温可以根据拓扑关系在关键点上设置，推导出相邻的实体属性。

## 分段规则：共享管件（异径管 / 三通）必须单独成段

`PipingNodeOwner` 的每一项只属于**一个**段的 `Items`（值语义，不是引用）。
而异径管（`PipeReducer`）和三通（`PipeTee`）是**被多段共享的节点**：

- 异径管：一个入口 + 一个出口，同时是上游段的末端和下游段的始端，被两个段引用；
- 三通：一个入口 + 直通 + 支管，最多同时被三个段引用。
- 拓扑语义：本拓扑中的 `PipeTee`（含 tee 单例段）表达**星型连接**——一个入口分叉出多个出口的分叉点，
  **不对应实体的一个三通管件**；实体上是否真有一个三通（也可能用焊接支管、集合管等方式实现分叉）
  是物理细节，属后续轮次。
- **连续分叉合并**：中间没有其他管件、直接首尾相连的多个三通，合并为**一个**星型连接
  （取链首段的 `SegmentNumber` 作为代表句柄，其余三通不再单独成段），
  合并后所有原分支统一挂到该星型点上；判定只看两个三通之间是否有其他管件。

因此分段规则是：

1. 共享管件必须**单独放进自己的段**（单例段），段内 `Items` 只含这一个管件；
2. 该段必须有一个**名称**：`SegmentNumber` 即名称，作为被其他段引用的句柄；
3. 其他段用 `PipingNodeOwner.PipingNetworkSegmentSingleton(lineNumber = "线号", segmentNumber = "段名")`
   引用它，而不是把管件重复内联进多个段的 `Items`；
4. 引用需要**两级**（`LineNumber`, `SegmentNumber`）：
   **`LineNumber` 为空字符串表示"在本 line 内"**——省略线号，只指同一
   `LineNumber` 下的单例段；跨线引用时填目标线的 `LineNumber`。

构造单例段用 `PipingNetworkSegment.tee` / `PipingNetworkSegment.reducer`
（`PipingNetworkSegment.fs`）：

```fsharp
PipingNetworkSegment.reducer "S2"  // 异径管单例段
PipingNetworkSegment.tee "T1"      // 三通单例段
```

实例：

- `InPlaceTree`：S2 是异径管单例段，S1、S3 用
  `PipingNetworkSegmentSingleton(lineNumber = "", segmentNumber = "S2")` 引用；
- `PumpTree`：S2（XR-101）、S5（XR-102）分别是罐出口侧、泵出口侧异径管的单例段，
  分别由 S1/S3、S4/S6 引用；
- `ReferencePid`：47126 线的 C1/C3/C4 三个连续三通（中间无其他管件）合并为一个
  星型连接，代表段为 S1；47125 线从该星型点分出安全阀支路，
  用 `PipingNetworkSegmentSingleton(lineNumber = "47126", segmentNumber = "S1")`。

## 共享组件：是否改变流道决定合并还是单独成段

段的界限上被多段引用的共享组件，按它是否改变流道 / 结构分两类处理：

1. **不改变流道**（一进一出、不分支）的组件——如止回阀、安全阀：
   相邻段**合并为一个管段**，组件作为普通项内联进合并段的 `Items`，
   不再作为段边界，模型里它只出现一次。
2. **改变流道 / 结构**的组件——保持上一条"单独成段"规则：
   - 三通（`PipeTee`）：一分二、星型连接，单个三通最多被三段引用；
     连续三通合并后的代表段可被更多段引用（如 47126 的 S1 被五段引用）；
   - 异径管（`PipeReducer`）：改变管径，上下游段管径不同；
   - 判断标准：是否分叉（改变流道）或改变管径（改变结构）。

实例：

- `ReferencePid` 47124 的止回阀 C2：一进一出、不分支，原 S1/S2 交界，
  两段合并为一个段，C2 内联进 S1；
- `ReferencePid` 47125 的弹簧安全阀：一进一出（分支在其上游星型连接处，
  即 47126 合并后的单例段 S1，原 C1/C3/C4 连续三通），原 S1/S2 合并为一个段，安全阀内联；
- `ReferencePid` 47124 的异径管 C3：改变管径，仍为单例段 S2，
  S1/S3 用 `PipingNetworkSegmentSingleton(lineNumber = "", segmentNumber = "S2")` 引用。

## 段的界限：Items 中的引用只连接、不包含

`Items` 中列出的两类引用项，虽然写在 Items 里、充当本段的起始 / 目标
（SourceItem / TargetItem），但段的界限只是**连接到**它们，**不包括**它们：

- `PipingNodeOwner.Nozzle(equipment = ..., nozzle = ...)`：设备管嘴的引用。
  管嘴由设备拥有（`ProcessEquipments` 的 `nozzles`），本段从它出发或到它为止；
- `PipingNodeOwner.PipingNetworkSegmentSingleton(lineNumber = ..., segmentNumber = ...)`：
  其他管段的引用。引用分**两级**：`segmentNumber` 是被引用的单例段名，
  `lineNumber` 是被引用的线号；**`lineNumber` 为空字符串表示同一 `LineNumber` 内**。
  被引用的段有自己的 `SegmentNumber` 与 `Items`，本段只与它首尾相连。

判断归属：一项是否属于本段，看它是否在本段内**定义**。引用项只是借用
`PipingNodeOwner` 的槽位来标记段的边界，段真正包含的是它自己定义的管件。

实例：`InPlaceTree` 的 S1 以 `Nozzle(V-101, N1)` 为起点、以
`PipingNetworkSegmentSingleton(lineNumber = "", segmentNumber = "S2")` 为终点——N1 属于罐 V-101、
S2 段属于它自己，S1 只与它们相连，不包含它们。

## 连接（Connections）是隐含的，由 Items 推导

模型没有显式的 `Connections` 字段（旧 record 模型才有 PipingConnection 列表），
连接是**隐含**的，由 `Items` 计算出来：

- `Items` 中**每相邻两个节点之间**有一条连接；
- 有 N 个节点就有 N-1 条连接，依次组成连接列表；
- 即连接列表 = 相邻对的序列：`(Items[0] → Items[1])`、`(Items[1] → Items[2])`、……

实例：`InPlaceTree` 的 S1 `Items = [Nozzle(V-101, N1); OperatedValve(XV-101); PipingNetworkSegmentSingleton(lineNumber = "", segmentNumber = "S2")]`，
隐含连接为 `N1 → XV-101`、`XV-101 → S2` 两条——引用项（喷嘴、段引用）同样参与推导。
