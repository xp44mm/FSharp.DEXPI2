// smoke_test.cpp
// Compile-and-run verification for the flattened DEXPI 2.0 C++ model
// (Dexpi2CppFlatten output). Exercises inlined abstract-base members,
// cross-namespace re-qualification and the shadowing resolution.
#include "dexpi2.hpp"

#include <cassert>
#include <memory>
#include <string>
#include <vector>

#define TOUCH(x) ((void)(x))

int main()
{
    using namespace dexpi2;

    // ---- core ----
    core::diagram::MetaData md;
    md.ProjectName = "Test";
    md.TotalNumberOfSheets = 3;

    core::datatypes::MultiLanguageString mls;
    core::datatypes::SingleLanguageString sls;
    sls.Language = "en";
    sls.Value = "hello";
    mls.SingleLanguageStrings.push_back(
        std::make_shared<core::datatypes::SingleLanguageString>(sls));

    core::physicalquantities::PhysicalQuantity pq;
    pq.Value = 42.0;
    core::physicalquantities::PhysicalQuantityVector pqv;
    pqv.Values = {1.0, 2.0, 3.0};

    // ---- plant ----
    plant::PlantModel pm;
    // inlined from core::ConceptualModel (abstract base, fully expanded):
    pm.MetaData = std::make_shared<core::diagram::MetaData>(md);
    TOUCH(pm.Notes);
    TOUCH(pm.Roles);
    // inlined from core::ConceptualObject through ConceptualModel:
    TOUCH(pm.PerformedRoles);
    TOUCH(pm.PersistentIdentifiers);
    TOUCH(pm.ReferencedNotes);
    TOUCH(pm.PlantStructureItems);
    TOUCH(pm.PipingNetworkSystems);

    plant::plantstructure::Enterprise ent;
    TOUCH(ent.PerformedRoles); // via PlantStructureItem -> ConceptualObject

    plant::plantstructure::Site site;
    TOUCH(site.PerformedRoles);

    plant::processequipment::MotorAsComponent motor;
    motor.NominalPower = std::make_shared<core::physicalquantities::PhysicalQuantity>(pq);
    motor.NominalRotationalFrequency = std::make_shared<core::physicalquantities::PhysicalQuantity>(pq);
    motor.SubTagName = "M-101";
    TOUCH(motor.PerformedRoles); // inlined from ConceptualObject

    plant::processequipment::TransmissionSystem ts;
    TOUCH(ts.Driver); // shared_ptr<TransmissionDriver> (abstract reference type)
    TOUCH(ts.GearBoxes);
    TOUCH(ts.SubTagName);

    plant::instrumentation::FlowInSignalOffPageConnector fi;
    TOUCH(fi.ConnectorReference); // from SignalOffPageConnector
    TOUCH(fi.SignalConnectorDescription);
    TOUCH(fi.SignalConnectorNumber);
    TOUCH(fi.PerformedRoles); // from ConceptualObject

    plant::piping::VentLine vl;
    TOUCH(vl.InsulationThickness); // from PipeFitting
    TOUCH(vl.PipingClassCode);
    TOUCH(vl.PerformedRoles); // from ConceptualObject via PipingComponent

    plant::piping::FlowInPipeOffPageConnector fp;
    TOUCH(fp.PerformedRoles);

    plant::processequipment::TaggedColumnSection tcs;
    TOUCH(tcs.Height);  // from ColumnSection
    TOUCH(tcs.TagName); // from TaggedPlantItem
    TOUCH(tcs.PerformedRoles);

    // ---- process ----
    process::ProcessModel prm;
    TOUCH(prm.Compositions);
    TOUCH(prm.ProcessSteps);
    TOUCH(prm.MetaData); // inlined from core::ConceptualModel
    TOUCH(prm.PerformedRoles);

    // ---- auxiliaries (abstract reference types; shadowing resolved) ----
    std::shared_ptr<auxiliaries::QualifiedValueOfDouble> qv;
    TOUCH(qv);
    std::shared_ptr<auxiliaries::QualifiedValueOfPhysicalQuantity> qvp;
    TOUCH(qvp);

    static_assert(sizeof(plant::PlantModel) > 0, "PlantModel must be complete");
    static_assert(sizeof(process::ProcessModel) > 0, "ProcessModel must be complete");
    static_assert(sizeof(plant::processequipment::ProcessEquipment) > 0, "ProcessEquipment must be complete");
    static_assert(sizeof(core::physicalquantities::PhysicalQuantity) > 0, "PhysicalQuantity must be complete");

    return 0;
}
