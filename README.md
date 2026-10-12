# FSharp.DEXPI2

用 F# 落地 [DEXPI 2.0](https://dexpi.org) 信息模型的实验仓库，专注**拓扑对象模型**。
DEXPI（Data Exchange in the Process Industry）是过程工业 P&ID 数据交换标准；
本仓库从官方 UML/XMI 出发，用 F# 判别联合手工映射模型类型（注释保留 XMI id），
并围绕"一个箱子 + 一个管嘴 + 一个阀门 + 一段管道 + 排放"这条最小管线做验证。

本仓库是原合并仓库**拆分后的 F# 部分**：官方 UML/XMI、C++ 原生类库与 C# 生成器
留在同级目录的兄弟仓库 `DEXPI2`。

## 与兄弟仓库 DEXPI2 的关系

| | FSharp.DEXPI2（本仓库） | DEXPI2（兄弟仓库） |
|---|---|---|
| 内容 | F# 拓扑对象模型 + 参数库层 + xUnit 测试 | C++ 类库（`dexpi2.hpp`）、C# 扁平化类库及生成器、官方 UML/XMI、规范文档 |
| 解决方案 | `FSharp.DEXPI2.slnx`（2 个 F# 工程） | `DEXPI2.slnx`（4 个 C++/C# 工程） |
| 角色 | 拓扑关系建模与验证 | 全量模型定义与代码生成 |

- **全量模型定义在兄弟仓库**：`Dexpi2.Cpp/dexpi2.hpp`（约 280 KB，纯头文件，
  与 UML 类图一一对应，每个类带 XMI id 注释，可回 EA 定位）。
- 本仓库的 F# 类型是**手工映射**的判别联合，注释保留 XMI id
  （例：`PipingNodeOwner` ← ID996、`ProcessEquipment` ← ID1226）。

## 怎么读这个模型（重要）

按顺序看三样东西：

1. **`DEXPI2/readme.md`**——先读规则：段的定义（段内 fluid/pn/dn 相同，任何改变导致段终止）、
   共享管件（异径管/三通）抽象为枢纽段、参数覆盖链（CSS 式）。
2. **模型文件链**——按对象链从顶到底：
   `PlantModel.fs` → `PipingNetworkSystem.fs`（线号）→ `PipingNetworkSegment.fs`
   （`Segment | HubSegment`）→ `PipingNodeOwner.fs`（段内节点判别联合）。
3. **全量 UML 映射**——对照兄弟仓库的 `dexpi2.hpp`；F# 侧只覆盖拓扑所需的派生类型。

## 仓库结构

```
FSharp.DEXPI2/
├── FSharp.DEXPI2.slnx            # 解决方案（XML 格式），聚合下面两个 F# 工程
│
├── DEXPI2/                       # 【F#】DEXPI 2.0 拓扑对象模型（netstandard2.0）
│   ├── readme.md                 #   ★ 分段与共享规则、参数覆盖链——读模型先看它
│   ├── PlantModel.fs             #   顶层：ProcessEquipments + PipingNetworkSystems
│   ├── ProcessEquipment.fs       #   设备判别联合（全部具体派生类型，tag + nozzles）
│   ├── PipingNodeOwner.fs        #   段内节点判别联合（管嘴引用 / 管件 / 枢纽段引用）
│   ├── PipingNetworkSegment.fs   #   Segment | HubSegment
│   ├── PipingNetworkSystem.fs    #   LineNumber + Segments
│   ├── ActuatorType.fs           #   阀门执行机构强类型（Manual/Pneumatic/…）
│   ├── ModelEntities.fs          #   五张表实体行 + 级联传播（cascade*）
│   ├── PlantModelProperty.fs     #   展平属性变更（一条 = 实体 + 参数新值）
│   ├── *Row.fs                   #   五张表行类型、建表 SQL、纯函数提取
│   └── Setter.fs                 #   SQLite 数据库层（建库/投影/读写）
│
├── DEXPI2.Test/                  # 【F#】xUnit v3 测试（net10.0，MTP 直跑 exe）
│   ├── InPlaceTree.fs            #   罐 → 手阀 → 异径管枢纽段 → 排出口
│   ├── PumpTree.fs               #   泵回路（罐进出口两个异径管枢纽段）
│   ├── TeeTree.fs                #   三通一分二
│   ├── SensorwellTree.fs         #   两罐间传感器套管测点（PT）
│   ├── ReferencePid.fs           #   参考 P&ID 完整拓扑（5 台设备 + 10 条管线系统）
│   ├── SetterTest.fs             #   SQLite 库层测试
│   └── xunit.runner.json
│
├── pid.png                       # 参考 P&ID 图（箱+嘴+阀+管+排放）
└── README.md
```

## 模型核心

顶层对象链：

```
PlantModel
  ├── ProcessEquipments: list<ProcessEquipment>        // 设备（tag + nozzles 管嘴列表）
  └── PipingNetworkSystems: list<PipingNetworkSystem>  // 线号系统（LineNumber）
        └── Segments: list<PipingNetworkSegment>       // Segment | HubSegment
              └── Items: list<PipingNodeOwner>         // 段内节点
```

- **段（Segment）**：`SegmentNumber + Items`，节点见 `PipingNodeOwner`；
- **枢纽段（HubSegment）**：只有段号的单例段，抽象异径管、三通等被多段共享的
  分叉/变径点，被其他段以 `SegmentRef(segmentNumber = …)` 引用（不允许跨线号引用）；
- **连接（Connections）隐含**：由 `Items` 相邻节点对推导（N 个节点 N-1 条连接），
  引用项（管嘴、段引用）同样参与推导；
- **参数（fluid / pn / dn）**：CSS 式覆盖链 `equipment → nozzle → segment → segmentItem`，
  沿链取第一个非默认值（Fluid≠""、PN≠0、DN≠0），局部覆盖父级；
  设置通过 `PlantModelProperty` 展平为有序动作列表，`ModelEntities` 的 `cascade*`
  完成级联传播（段级联写还会回写被引用管嘴，冲突抛异常）；
- **持久化**：`Setter` 把实体投影到 SQLite 五张表
  （Equipment / Nozzle / Segment / HubSegment / SegmentItem，UPSERT，实体为权威）。
- 完整规则见 `DEXPI2/readme.md`。

## 解决方案里包含哪些工程

`FSharp.DEXPI2.slnx` 当前聚合：

| 工程 | 语言 | 目标框架 | 角色 |
|---|---|---|---|
| `DEXPI2/DEXPI2.fsproj` | F# | netstandard2.0 | DEXPI 2.0 拓扑对象模型 + SQLite 参数库层 |
| `DEXPI2.Test/DEXPI2.Test.fsproj` | F# | net10.0 | xUnit v3 测试（MTP 直跑 exe，引用 DEXPI2） |

## 最小管线（pid.png → 概念模型）

参照图里的走向：

```
V-101 (Tank)
  └─ N1 (bottom nozzle)
       └─ XV-101 (ShutOffValve)
            └─ PL-101 (DN50)
                 └─ To Sewer / Atmosphere
```

验证入口在 `DEXPI2.Test`：按 P&ID 图构建的拓扑树实例
（`InPlaceTree` 罐 → 阀门 → 异径管单例段 → 排出口；`PumpTree` 泵回路；
`TeeTree` 三通一分二；`SensorwellTree` 两罐间传感器套管测点；
`ReferencePid` 参考 P&ID 完整拓扑：5 台设备 + 10 条管线系统），
每个树都以 `PlantModel` 为工厂入口，断言段边界、共享管件引用、
参数级联传播与隐含连接（由 `Items` 推导）。

## 构建与测试

```sh
dotnet build FSharp.DEXPI2.slnx     # 构建两个 F# 工程
dotnet run --project DEXPI2.Test    # 运行测试（xUnit v3 MTP 直跑 exe）
```

> 测试工程是 `net10.0` + `xunit.v3.core.mtp-v2` 的可执行程序，直接 `dotnet run`
> 即运行全部测试；.NET 10 SDK 已不再支持 VSTest target。
