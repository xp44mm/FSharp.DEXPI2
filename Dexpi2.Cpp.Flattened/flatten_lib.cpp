// flatten_lib.cpp - single compile unit for the flattened DEXPI 2.0 C++ class library.
// The library is header-only: this unit just proves the aggregate header compiles
// and the flat classes are complete.
#include "dexpi2.hpp"

static_assert(sizeof(dexpi2::core::ConceptualObject) > 0, "core class must be complete");
static_assert(sizeof(dexpi2::core::diagram::MetaData) > 0, "core class must be complete");
static_assert(sizeof(dexpi2::plant::PlantModel) > 0, "plant class must be complete");
static_assert(sizeof(dexpi2::process::ProcessModel) > 0, "process class must be complete");
static_assert(sizeof(dexpi2::auxiliaries::QualifiedValueOfDouble) > 0, "auxiliaries class must be complete");