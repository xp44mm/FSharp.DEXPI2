// test_auxiliaries.cpp - proves dexpi2_auxiliaries.hpp compiles standalone.
#include "dexpi2_auxiliaries.hpp"

#include <memory>

void test_auxiliaries()
{
    // all auxiliaries classes are abstract reference types
    std::shared_ptr<dexpi2::auxiliaries::QualifiedValueOfDouble> qv;
    std::shared_ptr<dexpi2::auxiliaries::QualifiedValueOfPhysicalQuantity> qvp;
    std::shared_ptr<dexpi2::auxiliaries::QualifiedValueOfPhysicalQuantitywithUnitTypeMassFlowRateUnit> qvm;
    (void)qv;
    (void)qvp;
    (void)qvm;
}
