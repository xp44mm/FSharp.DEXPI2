// test_core.cpp - proves dexpi2_core.hpp compiles standalone.
#include "dexpi2_core.hpp"

#include <memory>

void test_core()
{
    dexpi2::core::diagram::MetaData md;
    md.ProjectName = "core";
    dexpi2::core::datatypes::MultiLanguageString mls;
    dexpi2::core::physicalquantities::PhysicalQuantity pq;
    pq.Value = 1.0;
    dexpi2::core::physicalquantities::PhysicalQuantityVector pqv;
    pqv.Values = {1.0};
    std::shared_ptr<dexpi2::core::ConceptualObject> p;
    (void)p;
    (void)md;
    (void)mls;
    (void)pq;
    (void)pqv;
}
