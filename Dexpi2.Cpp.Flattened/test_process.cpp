// test_process.cpp - proves dexpi2_process.hpp compiles standalone.
#include "dexpi2_process.hpp"

#include <memory>

void test_process()
{
    dexpi2::process::ProcessModel pm;
    (void)pm.PerformedRoles;
    (void)pm.Compositions;
    // ProcessStep is abstract in DEXPI (protected ctor): reference only.
    std::shared_ptr<dexpi2::process::process::ProcessStep> stepRef;
    (void)stepRef;
    // Emitting is a concrete subclass: its flat body must inline ProcessStep's members.
    dexpi2::process::process::Emitting step;
    (void)step.PerformedRoles;
    (void)step.Description;
    std::shared_ptr<dexpi2::auxiliaries::QualifiedValueOfDouble> qv;
    (void)qv;
}
