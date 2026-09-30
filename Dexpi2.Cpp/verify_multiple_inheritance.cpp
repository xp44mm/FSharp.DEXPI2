#include "Generated/dexpi2.hpp"

#include <type_traits>

// 编译期验证：C++ 原生多继承完整保留 DEXPI XMI 的 mixin 结构。
// 这些断言在头文件生成后成立；缺失任一 mixin 即编译失败。

namespace dexpi2 {

static_assert(std::is_base_of_v<plant::processequipment::ChamberOwner, plant::processequipment::ProcessEquipment>,
              "ProcessEquipment: ChamberOwner mixin missing");
static_assert(std::is_base_of_v<plant::processequipment::NozzleOwner, plant::processequipment::ProcessEquipment>,
              "ProcessEquipment: NozzleOwner mixin missing");
static_assert(std::is_base_of_v<plant::processequipment::TaggedPlantItem, plant::processequipment::ProcessEquipment>,
              "ProcessEquipment: TaggedPlantItem mixin missing");
static_assert(std::is_base_of_v<plant::processequipment::TransmissionDriver, plant::processequipment::ProcessEquipment>,
              "ProcessEquipment: TransmissionDriver mixin missing");

static_assert(std::is_base_of_v<core::ConceptualObject, plant::processequipment::Nozzle>,
              "Nozzle: ConceptualObject mixin missing");
static_assert(std::is_base_of_v<plant::instrumentation::ActuatingElectricalLocation, plant::processequipment::Nozzle>,
              "Nozzle: ActuatingElectricalLocation mixin missing");
static_assert(std::is_base_of_v<plant::instrumentation::SensingLocation, plant::processequipment::Nozzle>,
              "Nozzle: SensingLocation mixin missing");
static_assert(std::is_base_of_v<plant::piping::PipingNodeOwner, plant::processequipment::Nozzle>,
              "Nozzle: PipingNodeOwner mixin missing");
static_assert(std::is_base_of_v<plant::piping::PipingSourceItem, plant::processequipment::Nozzle>,
              "Nozzle: PipingSourceItem mixin missing");
static_assert(std::is_base_of_v<plant::piping::PipingTargetItem, plant::processequipment::Nozzle>,
              "Nozzle: PipingTargetItem mixin missing");

}  // namespace dexpi2
