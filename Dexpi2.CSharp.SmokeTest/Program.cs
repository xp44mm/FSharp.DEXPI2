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
// 排除生成/编译基础设施，仅统计扁平模型类型：
//  - netstandard2.0 嵌入的可空注解特性类（Nullable* / RefSafetyRules / Embedded，基类 Attribute）
//  - IsExternalInit 垫片（netstandard2.0 record 支撑）
//  - 可区分联合容器（*Union）及其嵌套 Case 记录（不属于扁平模型）
var types = asm.GetTypes()
    .Where(t => t.DeclaringType is null
        && !t.Name.EndsWith("Union")
        && !t.IsSubclassOf(typeof(Attribute))
        && t.Name != "IsExternalInit")
    .ToArray();
var classes = types.Where(t => t.IsClass).ToArray();
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

// Discriminated union for TaggedPlantItem: all non-abstract descendants
// (non-leaf classes included); one distinct case per class, carrying the C# type.
var unionType = typeof(TaggedPlantItemUnion);
var unionAll = (System.Collections.IEnumerable)unionType.GetField("All", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
var cases = unionAll.Cast<object>().ToArray();
var payloads = cases
    .Select(c => (Type)c.GetType().GetProperty("Type")!.GetValue(c)!)
    .ToArray();
Check(cases.Length == 100, $"TaggedPlantItemUnion case count {cases.Length} != 100");
Check(cases.Select(c => c.GetType()).Distinct().Count() == 100,
    "TaggedPlantItemUnion cases must be distinct record types (one per derived class)");
Check(cases.All(c => c.GetType().Name == ((Type)c.GetType().GetProperty("Type")!.GetValue(c)!).Name + "Case"),
    "TaggedPlantItemUnion case type name must be '<class>Case'");
Check(payloads.All(t => t.IsClass && !t.IsAbstract), "TaggedPlantItemUnion payloads must be concrete classes");
Check(payloads.All(t => t.Assembly == asm), "TaggedPlantItemUnion payloads must live in this assembly");
foreach (var expected in new[] { typeof(CentrifugalPump), typeof(BatchWeigher), typeof(TaggedColumnSection) })
    Check(payloads.Contains(expected), $"TaggedPlantItemUnion missing case {expected.Name}");
Console.WriteLine($"TaggedPlantItemUnion cases : {cases.Length}");

if (failures.Count == 0)
{
    Console.WriteLine("SMOKE TEST PASSED");
    return 0;
}

Console.WriteLine("SMOKE TEST FAILED:");
foreach (var f in failures) Console.WriteLine("  " + f);
return 1;
