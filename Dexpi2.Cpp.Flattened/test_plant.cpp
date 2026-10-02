// test_plant.cpp - proves dexpi2_plant.hpp compiles standalone.
#include "dexpi2_plant.hpp"

#include <memory>

void test_plant()
{
    dexpi2::plant::PlantModel pm;
    pm.MetaData = std::make_shared<dexpi2::core::diagram::MetaData>();
    (void)pm.PerformedRoles;
    dexpi2::plant::piping::VentLine vl;
    (void)vl.PerformedRoles;
    dexpi2::plant::plantstructure::Site site;
    (void)site.PerformedRoles;
}
