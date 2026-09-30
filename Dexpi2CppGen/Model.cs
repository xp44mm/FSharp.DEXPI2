namespace Dexpi2CppGen;

/// <summary>Parsed DEXPI 2.0 UML model, from the standard XMI 2.1 export (Dexpi.xmi).</summary>
public sealed class DexpiModel
{
    public List<DexpiPackage> Packages { get; } = new();
    public Dictionary<string, DexpiType> TypesById { get; } = new(StringComparer.Ordinal);
    public List<DexpiAssociation> Associations { get; } = new();
    public int PrimitiveTypeCount { get; set; }
}

/// <summary>A UML package (top-level model or nested package) that maps to one C# namespace.</summary>
public sealed class DexpiPackage
{
    public DexpiPackage(string fullPath) => FullPath = fullPath;

    /// <summary>Package path, e.g. "Core.DataTypes"; "_Auxiliaries" keeps its leading underscore.</summary>
    public string FullPath { get; }

    public string Namespace => "Dexpi." + string.Join(".", FullPath.Split('.').Select(p => p.TrimStart('_')));

    /// <summary>Generated types in document order.</summary>
    public List<DexpiType> Types { get; } = new();
}

/// <summary>Any UML classifier referenced by xmi:id.</summary>
public abstract class DexpiType
{
    protected DexpiType(string id, string name, DexpiPackage package)
    {
        Id = id;
        Name = name;
        Package = package;
    }

    public string Id { get; }
    public string Name { get; }
    public DexpiPackage Package { get; }
    public string CSharpName { get; set; } = "";
    public bool IsGenerated { get; set; }
}

/// <summary>UML Class, or a plain-named DataType that is materialized as a C# class.</summary>
public sealed class DexpiClass : DexpiType
{
    public DexpiClass(string id, string name, DexpiPackage package, bool isAbstract)
        : base(id, name, package) => IsAbstract = isAbstract;

    public bool IsAbstract { get; }
    public List<string> GeneralizationIds { get; } = new();
    public List<DexpiProperty> Properties { get; } = new();
}

/// <summary>UML Enumeration.</summary>
public sealed class DexpiEnum : DexpiType
{
    public DexpiEnum(string id, string name, DexpiPackage package) : base(id, name, package) { }

    public List<string> Literals { get; } = new();
}

/// <summary>Special-named DataType (e.g. "Undefined | String") or PrimitiveType; mapped to a C# builtin, not generated.</summary>
public sealed class DexpiBuiltin : DexpiType
{
    public DexpiBuiltin(string id, string name, DexpiPackage package) : base(id, name, package) { }
}

/// <summary>One UML property (ownedAttribute of a class, or a named reverse association end).</summary>
public sealed class DexpiProperty
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? TypeId { get; set; }
    public bool IsCollection { get; set; }
    public bool IsOptional { get; set; }
    public bool IsComposite { get; set; }
    public string? RedefinedPropertyId { get; set; }
}

/// <summary>UML Association; member ends are either class attributes or ownedEnds.</summary>
public sealed class DexpiAssociation
{
    public string Id { get; set; } = "";
    public List<DexpiEnd> Ends { get; } = new();
}

public sealed class DexpiEnd
{
    public string Id { get; set; } = "";
    public bool IsOwnedEnd { get; set; }
    public string Name { get; set; } = "";
    public string? TypeId { get; set; }
    public bool IsOptional { get; set; }
    public bool IsCollection { get; set; }
}

/// <summary>Deterministic generation report: counts, mapping decisions, skips, warnings.</summary>
public sealed class GenerationReport
{
    public int ClassCount { get; set; }
    public int AbstractCount { get; set; }
    public int EnumCount { get; set; }
    public int PropertyCount { get; set; }
    public int AssociationCount { get; set; }
    public List<string> Notes { get; } = new();
    public List<string> Skipped { get; } = new();
    public List<string> Warnings { get; } = new();

    public void AddNote(string s)
    {
        if (!Notes.Contains(s)) Notes.Add(s);
    }

    public void AddSkipped(string s)
    {
        if (!Skipped.Contains(s)) Skipped.Add(s);
    }

    public void AddWarning(string s)
    {
        if (!Warnings.Contains(s)) Warnings.Add(s);
    }
}
