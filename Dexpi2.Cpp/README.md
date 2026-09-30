# Dexpi2.Cpp

DEXPI 2.0 信息模型的 C++ 落地项目（骨架）。

## 目标

- 解析 `Dexpi.xmi` → C++ 类库，**C++ 原生多继承**直接映射 XMI mixin：
  `class ProcessEquipment : public ChamberOwner, public NozzleOwner,
                            public TaggedPlantItem, public TransmissionDriver`
- 多值属性：`std::vector<T>`（列表）、`std::optional<T>`（0..1 可选）
- XML 交换格式解析（官方 `reference_pid.xml`）：pugixml / tinyxml2
- VS 类设计器支持 C++：可对生成的类链出 `.cd` 类图（模板/STL 显示偏弱）

## 结构

    Dexpi2.Cpp/
      Dexpi2.Cpp.vcxproj   # VS C++ 项目（StaticLibrary，MSVC v145 / C++20，x64）
      dexpi2.hpp           # 命名空间与规划说明
      dexpi2.cpp           # 占位实现
      README.md

## 状态

- [x] 项目骨架（可编译）
- [ ] 模型生成器（XMI → C++ 类，多继承）
- [ ] XML 反序列化（reference_pid.xml 验证）
- [ ] VS 类设计器类图（.cd）

## 编译（命令行验证）

    "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" Dexpi2.Cpp.vcxproj /p:Configuration=Release /p:Platform=x64
