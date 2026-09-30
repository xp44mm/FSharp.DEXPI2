# Dexpi2.Python

DEXPI 2.0 信息模型的 Python 落地项目。

## 目标

- 用 Python `dataclass` 生成 DEXPI 2.0 模型类，**保留全部泛化父类（原生多继承）**，
  直接表达 XMI 的 mixin 结构：`ProcessEquipment(ChamberOwner, NozzleOwner, TaggedPlantItem, TransmissionDriver)`、
  `Nozzle(ConceptualObject, SensingLocation, ActuatingElectricalLocation, PipingNodeOwner, PipingSourceItem, PipingTargetItem)`
- 用 `lxml` / `xml.etree` 解析官方 `reference_pid.xml`（DEXPI XML 交换格式）
- 类图：沿用仓库现有 HTML 类关系图机制，或 pyreverse

## 结构

    Dexpi2.Python/
      dexpi2/
        gen.py          # XMI 解析器 + Python 生成器（确定性，UTF-8 BOM + CRLF）
        dexpi_model.py  # 生成的模型（运行 gen.py 后产生）
        dexpi_model-report.md  # 生成报告
      README.md

## 运行生成器

    python dexpi2/gen.py ..\DEXPI2.0-UML\Dexpi.xmi dexpi2\dexpi_model.py

（在 `Dexpi2.Python` 目录下执行；XMI 位于仓库 `DEXPI2.0-UML\Dexpi.xmi`）

## 状态

- [x] 生成器（XMI → dataclass，多继承）
- [ ] 生成并验证（需 CPython 解释器）
- [ ] XML 反序列化（reference_pid.xml 验证 Nozzles 落位）
- [ ] 类关系提取（ast）与 HTML 类图
