namespace Dexpi2CppGen;

/// <summary>Result of a type mapping: either a C# primitive/builtin name, or a model type to qualify by the emitter.</summary>
public readonly record struct TypeMapping(bool IsPrimitive, string PrimitiveName, DexpiType? Target)
{
    public static TypeMapping Primitive(string name) => new(true, name, null);
    public static TypeMapping Of(DexpiType target) => new(false, "", target);
}

/// <summary>
/// Maps DEXPI 2.0 builtin DataType / PrimitiveType names to C# types.
/// Plain-named DataTypes are materialized as classes and mapped by their C# name.
/// </summary>
public static class TypeMapper
{
    /// <summary>Maps a referenced UML type to a C# type (cross-namespace qualification is done by the emitter).</summary>
    public static TypeMapping Map(DexpiModel model, DexpiType? type, string? typeId, GenerationReport report)
    {
        if (type == null)
        {
            report.AddWarning($"Unresolved type reference: '{typeId}' -> object.");
            return TypeMapping.Primitive("object");
        }

        switch (type)
        {
            case DexpiEnum e:
                return TypeMapping.Of(e);
            case DexpiClass c:
                return TypeMapping.Of(c);
            case DexpiBuiltin b:
                return MapBuiltin(model, b, report);
            default:
                return TypeMapping.Primitive("object");
        }
    }

    private static TypeMapping MapBuiltin(DexpiModel model, DexpiBuiltin b, GenerationReport report)
    {
        var name = b.Name.Trim();
        switch (name)
        {
            case "Undefined | String":
            case "String": return TypeMapping.Primitive("string");
            case "Undefined | Integer":
            case "Integer": return TypeMapping.Primitive("int");
            case "Undefined | Double":
            case "Double": return TypeMapping.Primitive("double");
            case "Undefined | Boolean":
            case "Boolean": return TypeMapping.Primitive("bool");
            case "Undefined | DateTime":
            case "DateTime": return TypeMapping.Primitive("DateTime");
            case "Undefined | AnyURI":
            case "AnyURI": return TypeMapping.Primitive("string"); // interchange-friendly; DEXPI AnyURI is serialized as text
            case "UnsignedByte": return TypeMapping.Primitive("byte");
        }

        if (name.StartsWith("Undefined | (PhysicalQuantityVector", StringComparison.Ordinal) ||
            name.StartsWith("PhysicalQuantityVector", StringComparison.Ordinal))
            return ResolveByName(model, "PhysicalQuantityVector", b, report);
        if (name.StartsWith("Undefined | (PhysicalQuantity", StringComparison.Ordinal) ||
            name.StartsWith("Undefined | PhysicalQuantity", StringComparison.Ordinal) ||
            name.StartsWith("PhysicalQuantity", StringComparison.Ordinal))
            return ResolveByName(model, "PhysicalQuantity", b, report);
        if (name.StartsWith("Undefined | ", StringComparison.Ordinal))
        {
            var rest = name.Substring("Undefined | ".Length).Trim();
            if (rest.StartsWith("(", StringComparison.Ordinal) && rest.EndsWith(")", StringComparison.Ordinal))
                rest = rest.Substring(1, rest.Length - 2).Trim();
            return ResolveByName(model, rest, b, report);
        }

        report.AddWarning($"Builtin type '{name}' has no mapping -> object.");
        return TypeMapping.Primitive("object");
    }

    private static TypeMapping ResolveByName(DexpiModel model, string name, DexpiBuiltin origin, GenerationReport report)
    {
        var matches = new List<DexpiType>();
        foreach (var t in model.TypesById.Values)
            if (t.Name == name && !ReferenceEquals(t, origin))
                matches.Add(t);
        if (matches.Count == 0)
        {
            report.AddWarning($"Cannot resolve type named '{name}' (referenced by '{origin.Name}') -> object.");
            return TypeMapping.Primitive("object");
        }
        if (matches.Count > 1)
            report.AddNote($"Ambiguous name '{name}' resolved to first occurrence '{matches[0].CSharpName}' ({matches.Count} candidates).");

        return matches[0] switch
        {
            DexpiEnum e => TypeMapping.Of(e),
            DexpiClass c => TypeMapping.Of(c),
            _ => TypeMapping.Primitive("object"),
        };
    }
}
