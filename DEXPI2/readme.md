# DEXPI2

本项目目标是只描述拓扑关系，实体的其他属性是下一轮的事情。

其他属性如fluid,pn,dn可以根据拓扑关系在关键点上设置，推导出相邻的实体属性。

## 段的定义：段内 fluid/pn/dn 相同，任何改变导致段终止

在参数数据库（`Setter` / SQLite）中，**段**是 fluid、pn、dn 三个参数均一的连续管段：

- 段内任意位置的 fluid、pn、dn 必须**相同**；三个参数只在段级设置（或从父级继承），不在段内逐点变化；
- **任何改变都导致段终止**：介质 fluid 变化、压力等级 pn 变化、公称直径 dn 变化，
  改变点之前是一个段，改变点之后是另一个段（或枢纽段）；
- 因此段边界有两类来源：
  1. **拓扑边界**：共享管件（枢纽段）、设备管嘴、跨系统（线号）交界；
  2. **参数边界**：fluid / pn / dn 任一改变处。
  两类边界重合或单独出现都算段边界；参数不变的连续管件应合并为同一个段
  （如 ReferencePid 中不改变流道的止回阀内联进合并段）。

### 系统级约束：同 PipingNetworkSystem 必然同 fluid

同一 `PipingNetworkSystem`（同 `LineNumber`）内的所有段，流体介质 fluid 必然相同：
fluid 在系统内不改变，只在**跨系统（跨线号）交界**处改变。

因此 fluid 的段边界只可能出现在跨系统交界；系统内部的段边界只由 pn / dn 改变产生。

### 参数传递规则（覆盖链：equipment → nozzle → segment → segmentItem）

参数从全局到局部逐级覆盖，求最终值类似 CSS 算法——后来的、具体的、局部的覆盖先来的、父容器的、全局的：

- **覆盖链**：`equipment`（最全局）→ `nozzle` → `segment` → `segmentItem`（最局部）；
- 求最终值：沿覆盖链取第一个**非默认值**（Fluid≠""、PN≠0、DN≠0），局部覆盖父级；
- 默认值：未设置即 Fluid=""、PN=0、DN=0；
- 继承关系：段从引用它的管嘴（nozzle）继承，管嘴从所属设备（equipment）继承，
  段项（segmentItem）从所在段继承；枢纽段（HubSegment）是单例段，无父级，只存自身值；
- plant 级传递：`createFromModel` 从拓扑找到引用各管嘴的段，建立「段 → 父级(设备,管嘴)」映射，
  `get*Final` 沿覆盖链动态求值，无需逐段落库；
- 段级联写（`cascadeSegment*` 物化写）：段值写段行与段项的同时，对段内引用的管嘴（`Nozzle`）写回——
  管嘴为默认值（Fluid=""、PN=0、DN=0）则把段值写入管嘴行；
  管嘴非默认值则核对与段值一致（一致静默、不一致抛异常）；管嘴行不存在抛异常。

（对应实现：`DEXPI2/TableTypes.fs` 的表类型、`DEXPI2/Setter.fs` 的 `set*` / `get*Final` API。）

## 分段规则：共享管件（异径管 / 三通）抽象为枢纽段

**枢纽段的参数定义（多个端点、多个 dn）**：

- 枢纽段是**多端点**段：异径管 2 个端点（入口、出口），三通 3 个端点（入口、直通、支管）；
- 因此其 dn 是**端点列表**，按端点顺序存（reducer=[入口;出口]、tee=[入口;直通;支管]），
  整体设置、整体替换；fluid、pn 与其他段一样为单值（段内一致）；
- 数据库表 `HubSegment` 的 DN 列以逗号分隔文本存端点列表，Fluid/PN 为单值。

`PipingNodeOwner` 的每一项只属于**一个**段的 `Items`（值语义，不是引用）。
而异径管、三通等**被多段共享的节点**，在模型层抽象为**枢纽段（`HubSegment`）**：
只保留段号一个句柄，不区分具体管件类型（reducer / tee）与端点。

- 异径管：一个入口 + 一个出口，同时是上游段的末端和下游段的始端，被两个段引用；
- 三通：一个入口 + 直通 + 支管，最多同时被三个段引用。
- 拓扑语义：本拓扑中的枢纽段表达**星型连接 / 变径点**——一个入口分叉出多个出口的分叉点
  （如三通），或一个入口 + 一个出口的变径点（如异径管），
  **不对应实体的一个具体管件**；实体上是否真有一个三通（也可能用焊接支管、集合管等方式实现分叉）
  是物理细节，属后续轮次。
- **连续分叉合并**：中间没有其他管件、直接首尾相连的多个三通，合并为**一个**星型连接
  （取链首段的 `SegmentNumber` 作为代表句柄，其余三通不再单独成段），
  合并后所有原分支统一挂到该星型点上；判定只看两个三通之间是否有其他管件。

因此分段规则是：

1. 共享管件必须**单独放进自己的段**：枢纽段 `HubSegment`，只含 `SegmentNumber`、不含 `Items`；
2. 该段必须有一个**名称**：`SegmentNumber` 即名称，作为被其他段引用的句柄；
3. 其他段用 `PipingNodeOwner.HubSegmentRef(segmentNumber = "段名")`
   引用它，而不是把管件重复内联进多个段的 `Items`；
4. 引用为**单级**（`SegmentNumber`），且**不允许跨系统（跨 `LineNumber`）引用管段**：
   被引用的枢纽段必须与引用段同属一个 `PipingNetworkSystem`；
   若确需跨系统共享分叉，应把相关系统合并为一个 `PipingNetworkSystem`。

枢纽段直接构造（`PipingNetworkSegment.fs`）：

```fsharp
HubSegment "S2"      // 异径管枢纽段
HubSegment "T1"      // 三通枢纽段
```

实例：

- `InPlaceTree`：S2 是异径管枢纽段，S1、S3 用
  `HubSegmentRef(segmentNumber = "S2")` 引用；
- `PumpTree`：S2（XR-101）、S5（XR-102）分别是罐出口侧、泵出口侧异径管的枢纽段，
  分别由 S1/S3、S4/S6 引用；
- `ReferencePid`：47126 线的 C1/C3/C4 三个连续三通（中间无其他管件）合并为一个
  星型连接，枢纽段为 S1；原 47125 线的安全阀支路已并入本系统（段 S13），
  用 `HubSegmentRef(segmentNumber = "S1")` 同线引用。

## 共享组件：是否改变流道决定合并还是单独成段

段的界限上被多段引用的共享组件，按它是否改变流道 / 结构分两类处理：

1. **不改变流道**（一进一出、不分支）的组件——如止回阀、安全阀：
   相邻段**合并为一个管段**，组件作为普通项内联进合并段的 `Items`，
   不再作为段边界，模型里它只出现一次。
2. **改变流道 / 结构**的组件——保持上一条"单独成段"规则（枢纽段）：
   - 三通：一分二、星型连接，单个三通最多被三段引用；
     连续三通合并后的枢纽段可被更多段引用（如 47126 的 S1 被五段引用）；
   - 异径管：改变管径，上下游段管径不同；
   - 判断标准：是否分叉（改变流道）或改变管径（改变结构）。

实例：

- `ReferencePid` 47124 的止回阀 C2：一进一出、不分支，原 S1/S2 交界，
  两段合并为一个段，C2 内联进 S1；
- `ReferencePid` 47125 的弹簧安全阀：一进一出（分支在其上游星型连接处，
  即 47126 合并后的枢纽段 S1，原 C1/C3/C4 连续三通），原 S1/S2 合并为一个段，安全阀内联；
  47125 线已并入 47126 系统，安全阀段为 S13，同线引用 S1；
- `ReferencePid` 47124 的异径管 C3：改变管径，仍为枢纽段 S2，
  S1/S3 用 `HubSegmentRef(segmentNumber = "S2")` 引用。

## 段的界限：Items 中的引用只连接、不包含

`Items` 中列出的两类引用项，虽然写在 Items 里、充当本段的起始 / 目标
（SourceItem / TargetItem），但段的界限只是**连接到**它们，**不包括**它们：

- `PipingNodeOwner.Nozzle(equipment = ..., nozzle = ...)`：设备管嘴的引用。
  管嘴由设备拥有（`ProcessEquipments` 的 `nozzles`），本段从它出发或到它为止；
- `PipingNodeOwner.HubSegmentRef(segmentNumber = ...)`：
  其他管段的引用，**不允许跨系统引用**：`segmentNumber` 是被引用的枢纽段名，
  且该段必须与引用段同属一个 `PipingNetworkSystem`（`LineNumber`）。
  被引用的枢纽段有自己的 `SegmentNumber`（不含 `Items`），本段只与它首尾相连。

判断归属：一项是否属于本段，看它是否在本段内**定义**。引用项只是借用
`PipingNodeOwner` 的槽位来标记段的边界，段真正包含的是它自己定义的管件。

实例：`InPlaceTree` 的 S1 以 `Nozzle(V-101, N1)` 为起点、以
`HubSegmentRef(segmentNumber = "S2")` 为终点——N1 属于罐 V-101、
S2 段属于它自己，S1 只与它们相连，不包含它们。

## 连接（Connections）是隐含的，由 Items 推导

模型没有显式的 `Connections` 字段（旧模型才有 PipingConnection 列表），
连接是**隐含**的，由 `Items` 计算出来：

- `Items` 中**每相邻两个节点之间**有一条连接；
- 有 N 个节点就有 N-1 条连接，依次组成连接列表；
- 即连接列表 = 相邻对的序列：`(Items[0] → Items[1])`、`(Items[1] → Items[2])`、……

实例：`InPlaceTree` 的 S1 `Items = [Nozzle(V-101, N1); OperatedValve(XV-101, Manual); HubSegmentRef(segmentNumber = "S2")]`，
隐含连接为 `N1 → XV-101`、`XV-101 → S2` 两条——引用项（喷嘴、段引用）同样参与推导。
