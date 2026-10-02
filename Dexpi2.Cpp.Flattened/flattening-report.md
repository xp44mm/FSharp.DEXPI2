# DEXPI 2.0 C++ -> Flattened Project - Report

- Input: dexpi2.hpp (SHA-256: e81ff83c9093ec5e8464c61cfaff00cbf74b303f7da45423de0d19e3204b9f67)
- Tool: Dexpi2CppFlatten (rule version 1.0)
- Output directory: C:\Application Data\GitHub\xp44mm\FSharp.DEXPI2\Dexpi2.Cpp.Flattened

## Counts (unchanged by flattening)

- Classes: 527 (abstract: 86)
- Enumerations: 89

## Flattening effect

- Inlined inherited member instances: 6116
- Inherited duplicates removed (diamond / repeated mixins, same name): 0
- Inherited members dropped because the derived class declares the same name (C++ shadowing wins): 33
- Members re-qualified with fully qualified type names (cross-namespace inlining): 2132

## Files

| File | Top-level namespace |
| --- | --- |
| dexpi2.hpp | aggregate (includes all below in dependency order) |
| dexpi2_core.hpp | dexpi2::core |
| dexpi2_auxiliaries.hpp | dexpi2::auxiliaries |
| dexpi2_plant.hpp | dexpi2::plant |
| dexpi2_process.hpp | dexpi2::process |

## Shadowing resolutions (derived member wins)

- QualifiedValueOfPhysicalQuantitywithUnitTypeForceUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeRotationalFrequencyUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypePowerUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypePercentageUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfDouble: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeMassFlowRateUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantityVectorwithUnitTypePercentageUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeMoleFlowRateUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfInteger: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeLengthUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypePressureAbsoluteUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeTemperatureUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeAreaUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeElectricCurrentUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeElectricalFrequencyUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeVoltageUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeVolumeFlowRateUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeHeatTransferCoefficientUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeHeatTransferResistanceUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeMassUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeParticleSizeUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeDensityUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeMassSpecificEnergyUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeDynamicViscosityUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantity: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeMomentOfForceUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeTimeIntervalUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeVolumeUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeVelocityUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeMagneticFieldIntensityUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypepHUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeEnergyUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)
- QualifiedValueOfPhysicalQuantitywithUnitTypeEnergyDensityUnit: inherited 'std::shared_ptr<dexpi2::core::physicalquantities::PhysicalQuantity> Value' from QualifiedValue dropped (own member with same name wins)

## Verification

- Model invariant checked: for every class, the flattened member-name set equals own names + all ancestor names (most-derived wins).
- No class in the output declares any base (" : public" absent).
- Each file is self-sufficient: it includes exactly the top-level files it references.

## MSVC compile verification (2026-10-02)

- Toolchain: VS 18 Community (MSVC 14.51.36231, cl), /std:c++20 /EHsc /W3.
- test_core.cpp, test_auxiliaries.cpp, test_plant.cpp, test_process.cpp compile clean (each includes its dexpi2_<ns>.hpp standalone).
- smoke_test.cpp compiles and runs (asserts on inlined abstract-base members, cross-namespace re-qualification, shadowing resolution; static_asserts pass).
- Output files are UTF-8 with BOM, CRLF line endings, trailing newline.
