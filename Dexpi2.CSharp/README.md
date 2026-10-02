# Dexpi2.CSharp

DEXPI 2.0 信息模型的 **C# 扁平化（零继承）** 落地库，参照已移除的 `Dexpi2.Cpp.Flattened`（C++ 扁平化版）1:1 生成。

## DEXPI 是什么

DEXPI（**Data Exchange in the Process Industry**，流程工业数据交换）是一个行业倡议，
目标是为流程工业（化工、石化、制药、能源等）的 **P&ID（管道仪表流程图）** 建立
**供应商中立**的数据交换信息模型，让不同工程软件（设计、仿真、采购、运维）之间的
工厂数据可以无损互转。DEXPI 2.0 是官方发布的 UML 信息模型：

- 官方模型文件：`DEXPI2.0-UML/Dexpi.xmi`（Sparx Enterprise Architect 导出），
  另附 `Dexpi for Modelio.xmi` 与 EA HTML 导出（<https://dexpi.org/>）。
- 模型分四个顶层包，对应四个 C++ 命名空间：
  - **core**：概念基础（`ConceptualObject`、`Role`、`Note`、`QualifiedValue`、
    图形元素、物理量/单位等）；
  - **auxiliaries**：限定值变体（`QualifiedValueOfDouble`、
    `QualifiedValueOfPhysicalQuantitywithUnitType<Unit>` 等，全部为抽象类）；
  - **plant**：工厂侧——仪器仪表、管道、厂区结构、工艺设备、图元与标注；
  - **process**：工艺侧——物性/物流/物料状态、过程连接、过程步骤与单元操作、
    `ProcessModel` 根对象。
- 与 ISO 15926 / CFIHOS 生态同源，强调“标签（Tag）+ 分类 + 限定值”的数据组织方式。

## 为什么“消除继承”

原始 UML 大量使用 **mixin 多重继承**，例如：

```text
ProcessEquipment : ChamberOwner, NozzleOwner, TaggedPlantItem, TransmissionDriver
```

- C++ 可以用原生多重继承直接映射（见 `Dexpi2.Cpp`），但会带来菱形继承、
  虚基类、成员遮蔽等复杂度；
- **C# 不支持多重继承**——这正是“如果不使用多重继承，完全可以用 C#”的原因：
  只要把继承链彻底摊平，C# 就能 1:1 承载整个模型；
- 扁平化本身也有工程收益：每个类自包含，无虚表布局问题，序列化/反序列化
  （XML/JSON）直接按成员表走，避免“基类成员去哪了”的歧义。

`Dexpi2.Cpp.Flattened` 已完成摊平：内联继承成员 6116 个、跨命名空间重限定
2132 处、按 C++ 遮蔽规则保留派生类同名成员 33 处。本库在其产物上直接转译，
**不重新做一次展平**，保证两套库语义完全一致。

## 映射规则（C++ → C#）

| C++（扁平化头文件） | C# |
| --- | --- |
| `std::string` | `string`（默认 `= ""`） |
| `std::shared_ptr<T>` | `T?`（可空引用，默认 null） |
| `std::optional<T>` | `T?`（可空值类型/引用） |
| `std::vector<T>` | `List<T>`（默认 `= new()`） |
| `int` / `double` / `bool` | `int` / `double` / `bool`（保留默认值） |
| `enum class E` | `enum E` |
| 抽象类（`[abstract in DEXPI]`） | `abstract class`（不可直接实例化） |
| 同命名空间引用 | 短名 |
| 跨命名空间引用 | 全限定名 |

命名空间对照：

| C++ | C# |
| --- | --- |
| `dexpi2::core` | `Dexpi2.Core` |
| `dexpi2::core::datatypes` | `Dexpi2.Core.Datatypes` |
| `dexpi2::core::diagram` | `Dexpi2.Core.Diagram` |
| `dexpi2::core::physicalquantities` | `Dexpi2.Core.Physicalquantities` |
| `dexpi2::auxiliaries` | `Dexpi2.Auxiliaries` |
| `dexpi2::plant::*` | `Dexpi2.Plant.*` |
| `dexpi2::process::*` | `Dexpi2.Process.*` |

> 命名空间段只做首字母大写，保留 C++ 的拼写（如 `plantstructure` → `Plantstructure`），
> 便于与 C++ 头文件一一对照。

## 统计（与 C++ 扁平化报告一致）

| 文件 | 类 | 抽象类 | 枚举 |
| --- | --- | --- | --- |
| Dexpi2.Core.cs | 46 | 11 | 48 |
| Dexpi2.Auxiliaries.cs | 33 | 33 | 0 |
| Dexpi2.Plant.cs | 305 | 31 | 27 |
| Dexpi2.Process.cs | 143 | 11 | 14 |
| **合计** | **527** | **86** | **89** |

## 结构

```
Dexpi2.CSharp/
  Dexpi2.CSharp.csproj    # net10.0, Nullable enable
  Dexpi2.Core.cs          # 对应 dexpi2_core.hpp
  Dexpi2.Auxiliaries.cs   # 对应 dexpi2_auxiliaries.hpp
  Dexpi2.Plant.cs         # 对应 dexpi2_plant.hpp
  Dexpi2.Process.cs       # 对应 dexpi2_process.hpp

Dexpi2.CSharp.Generator/  # F# 生成器（解析 input/ 下 4 个扁平化 hpp，输出上述 .cs）
  input/                  # 扁平化 C++ 头文件快照（原 Dexpi2.Cpp.Flattened 产物，已随其删除）
Dexpi2.CSharp.SmokeTest/  # 反射冒烟测试（527/86/89、零继承、成员抽查）
```

## 重新生成

```text
dotnet run --project Dexpi2.CSharp.Generator
```

生成器从 `Dexpi2.CSharp.Generator/input/*.hpp` 读取模型（该快照源自已删除的
`Dexpi2.Cpp.Flattened` 扁平化产物），输出到 `Dexpi2.CSharp/`；
所有输出文件为 UTF-8 (BOM) + CRLF。

## 编译与测试

```text
dotnet build  Dexpi2.CSharp\Dexpi2.CSharp.csproj -c Release
dotnet run --project Dexpi2.CSharp.SmokeTest -c Release
```

冒烟测试断言：527 个类、86 个抽象类、89 个枚举；**所有类的基类均为
`object`（零继承）**；抽查 `ProcessModel` 等类的内联成员与默认值。
