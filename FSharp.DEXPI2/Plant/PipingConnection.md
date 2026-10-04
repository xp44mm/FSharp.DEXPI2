# PipingConnection 继承树（非抽象子类）

DEXPI 2.0 模型类 **PipingConnection**（XMI id ID949，`dexpi2::plant::piping`，**抽象**）的全部派生类型。

- 传递后代：**2 个**
- 其中抽象：**0 个**
- **非抽象子类：2 个**（均叶类），全部位于 `Dexpi2.Plant.Piping`

## 继承树

```
PipingConnection (ID949) [抽象]
├── DirectPipingConnection (ID948)
└── Pipe (ID986)
```

## 要点

- **只有 2 个直接子类，且都是叶类**：`dexpi2.hpp` 中无任何类继承
  `DirectPipingConnection` 或 `Pipe`（已核查全部 `class X : public …` 声明）；
- **语义分工**：
  - `DirectPipingConnection`（ID948）——"两个管道项之间的**直接**连接，
    即不通过管道实现的连接"（OPC UA 映射 8.18：*"A direct connection between
    two piping items, i.e. a connection that is not realized by a pipe"*），
    如设备法兰对法兰直连；
  - `Pipe`（ID986）——**实际管道**（`class Pipe : public ConceptualObject,
    public PipingConnection`，额外继承 `ConceptualObject` 的身份/角色/注释字段）；
- **多态引用**：`PipingNetworkSegment.Connections` 的元素类型是抽象
  `PipingConnection`，实例只能是上述两个子类——规范参考 XML
  （`Specification-V2.0.0/.../reference_pid.xml`）中段内连接全部用
  `Plant/Piping.Pipe` 实例化（如 `Pipe1`、`Pipe2`）；
- 与当前 F# 模型的关系：`PipingConnection` record 对应抽象基类的
  `SourceItem/SourceNode/TargetItem/TargetNode` 四字段；`Pipe`/`DirectPipingConnection`
  在 F# 中尚未建模（模型仅用 `PipingConnection` 直接实例化连接），
  C# 生成结果中两子类均已存在。

## 数据来源

`Dexpi2.Cpp/Generated/dexpi2.hpp` 多继承声明
（`DirectPipingConnection : public PipingConnection`、`Pipe : public
ConceptualObject, public PipingConnection`），子类清单经 `DEXPI 2.0 EA HTML
export` 中 `PipingConnection`(EA545) 页面 Links 表 Generalization 方向 "From"
交叉验证（仅 `DirectPipingConnection`、`Pipe` 两条）。
