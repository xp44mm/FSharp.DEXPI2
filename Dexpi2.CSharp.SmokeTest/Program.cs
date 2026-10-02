using System.Reflection;
using Dexpi2.Auxiliaries;
using Dexpi2.Core;
using Dexpi2.Core.Datatypes;
using Dexpi2.Core.Physicalquantities;
using Dexpi2.Plant.Processequipment;
using Dexpi2.Process;

var failures = new List<string>();

void Check(bool cond, string what)
{
    if (!cond) failures.Add(what);
}

var asm = typeof(EngineeringModel).Assembly;
var types = asm.GetTypes();
// netstandard2.0 会把可空注解用的 NullableAttribute 等编译期类型嵌入程序集
// （它们基类是 System.Attribute），统计模型时排除。
var classes = types.Where(t => t.IsClass && !t.IsSubclassOf(typeof(Attribute))).ToArray();
var enums = types.Where(t => t.IsEnum).ToArray();
var abstracts = classes.Where(t => t.IsAbstract).ToArray();
var nonObjectBase = classes.Where(t => t.BaseType != typeof(object)).ToArray();

Console.WriteLine($"classes   : {classes.Length} (report: 527)");
Console.WriteLine($"abstracts : {abstracts.Length} (report: 86)");
Console.WriteLine($"enums     : {enums.Length} (report: 89)");
Console.WriteLine($"non-object base types: {nonObjectBase.Length} (must be 0)");

Check(classes.Length == 527, $"class count {classes.Length} != 527");
Check(abstracts.Length == 86, $"abstract count {abstracts.Length} != 86");
Check(enums.Length == 89, $"enum count {enums.Length} != 89");
Check(nonObjectBase.Length == 0, "inheritance detected: some class has a non-object base");

// Flatness: none of the 527 classes may declare a base type.
foreach (var t in nonObjectBase) failures.Add($"  {t.FullName} : base {t.BaseType}");

// Representative cross-namespace types must exist.
Check(typeof(Dexpi2.Core.Diagram.Diagram).IsClass, "missing Dexpi2.Core.Diagram.Diagram");
Check(typeof(PhysicalQuantity).IsClass, "missing PhysicalQuantity");
Check(typeof(QualifiedValueOfPhysicalQuantitywithUnitTypeForceUnit).IsAbstract, "auxiliaries class not abstract");

// Core class members (own + none inherited).
var eng = new EngineeringModel
{
    OriginatingSystemName = "test",
    ExportDateTime = "2026-10-02",
};
Check(eng.ShapeCatalogues is { Count: 0 }, "EngineeringModel.ShapeCatalogues not initialized");
Check(eng.ConceptualModel is null, "EngineeringModel.ConceptualModel should be null by default");

// Flattening: pick the first concrete class (any namespace) that carries the
// inlined ConceptualObject members directly, and verify them.
var flatDemo = classes
    .First(c => !c.IsAbstract
                && c.GetProperty("PerformedRoles") is not null
                && c.GetProperty("PersistentIdentifiers") is not null
                && c.GetProperty("ReferencedNotes") is not null);
Console.WriteLine($"flattening demo class: {flatDemo.FullName}");
var demo = Activator.CreateInstance(flatDemo)!;
var demoProps = flatDemo.GetProperties().Select(p => p.Name).ToHashSet();
foreach (var expected in new[] { "PerformedRoles", "PersistentIdentifiers", "ReferencedNotes" })
    Check(demoProps.Contains(expected), $"{flatDemo.Name} missing inlined member {expected}");

// Process model: ProcessModel in Dexpi2.Process with inlined members.
var pm = new ProcessModel();
var pmProps = pm.GetType().GetProperties().Select(p => p.Name).ToHashSet();
foreach (var expected in new[] { "PerformedRoles", "MetaData", "ProcessSteps", "Compositions", "MaterialTemplates" })
    Check(pmProps.Contains(expected), $"ProcessModel missing inlined member {expected}");

// Qualified value: value property typed PhysicalQuantity (shadowing resolution: own Value wins).
var qvProp = typeof(QualifiedValueOfPhysicalQuantitywithUnitTypeForceUnit).GetProperty("Value");
Check(qvProp?.PropertyType == typeof(PhysicalQuantity), $"Value property type {qvProp?.PropertyType}");

// Enum sanity.
Check(typeof(QuantityProvenance).GetEnumNames().Length == 5, "QuantityProvenance member count");

if (failures.Count == 0)
{
    Console.WriteLine("SMOKE TEST PASSED");
    return 0;
}

Console.WriteLine("SMOKE TEST FAILED:");
foreach (var f in failures) Console.WriteLine("  " + f);
return 1;
