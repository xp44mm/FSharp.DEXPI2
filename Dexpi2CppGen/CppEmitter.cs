using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Dexpi2CppGen;

/// <summary>
/// Deterministic DEXPI 2.0 UML -> C++ emitter.
/// Output: a single header dexpi2.hpp with every model class (C++ native multiple
/// inheritance keeps ALL generalization parents) and every enumeration.
/// Same input + same rule version always yields byte-identical output (UTF-8 BOM, CRLF, trailing CRLF).
/// </summary>
public sealed class CppEmitter
{
    public const string RulesVersion = "1.0";

    private readonly DexpiModel _model;
    private readonly GenerationReport _report;

    public CppEmitter(DexpiModel model, GenerationReport report)
    {
        _model = model;
        _report = report;
    }

    public string SourceFileName { get; private set; } = "";
    public string SourceSha256 { get; private set; } = "";
    public int MixinClassCount { get; private set; }
    public int MixinTotal { get; private set; }

    public List<string> Emit(string outputDir, string inputPath)
    {
        SourceFileName = Path.GetFileName(inputPath);
        SourceSha256 = ComputeSha256(inputPath);
        ResolveNames();
        BuildReverseProperties();

        Directory.CreateDirectory(outputDir);
        var headerPath = Path.Combine(outputDir, "dexpi2.hpp");
        File.WriteAllText(headerPath, EmitHeader(), new UTF8Encoding(true));
        var reportPath = Path.Combine(outputDir, "generation-report.md");
        ReportWriter.Write(reportPath, this, _model, _report);
        return new List<string> { headerPath, reportPath };
    }

    // ------------------------------------------------------------------
    // Name resolution: globally unique across packages (single header), C++ keywords escaped.
    // ------------------------------------------------------------------

    private static readonly HashSet<string> CppKeywords = new(StringComparer.Ordinal)
    {
        "alignas", "alignof", "and", "and_eq", "asm", "auto", "bitand", "bitor", "bool", "break", "case", "catch",
        "char", "char8_t", "char16_t", "char32_t", "class", "compl", "concept", "const", "consteval", "constexpr",
        "constinit", "const_cast", "continue", "co_await", "co_return", "co_yield", "decltype", "default", "delete",
        "do", "double", "dynamic_cast", "else", "enum", "explicit", "export", "extern", "false", "float", "for",
        "friend", "goto", "if", "inline", "int", "long", "mutable", "namespace", "new", "noexcept", "not", "not_eq",
        "nullptr", "operator", "or", "or_eq", "private", "protected", "public", "register", "reinterpret_cast",
        "requires", "return", "short", "signed", "sizeof", "static", "static_assert", "static_cast", "struct",
        "switch", "template", "this", "thread_local", "throw", "true", "try", "typedef", "typeid", "typename",
        "union", "unsigned", "using", "virtual", "void", "volatile", "wchar_t", "while", "xor", "xor_eq", "final",
        "override", "import", "module"
    };

    private static string CppEscape(string name)
    {
        var s = Identifier.Sanitize(name);
        return CppKeywords.Contains(s) ? s + "_" : s;
    }

    private static string CppNamespace(DexpiPackage p) =>
        "dexpi2::" + string.Join("::", p.FullPath.Split('.').Select(seg => seg.TrimStart('_').ToLowerInvariant()));

    private void ResolveNames()
    {
        var used = new HashSet<string>(StringComparer.Ordinal);
        foreach (var package in _model.Packages)
        {
            foreach (var t in package.Types)
            {
                string baseName;
                if (t is DexpiClass c && c.Name.Contains(" with Type=", StringComparison.Ordinal))
                    baseName = Identifier.VariantName(c.Name);
                else
                    baseName = Identifier.Sanitize(t.Name);

                var name = CppEscape(baseName);
                var i = 1;
                while (!used.Add(name))
                {
                    name = CppEscape(baseName) + "_" + i.ToString(CultureInfo.InvariantCulture);
                    i++;
                }
                t.CSharpName = name; // reused as the C++ identifier
                t.IsGenerated = true;
                if (name != baseName)
                    _report.AddNote($"Type name collision (global): '{t.Name}' -> '{name}'.");
            }
        }

        _report.ClassCount = _model.TypesById.Values.Count(t => t is DexpiClass);
        _report.AbstractCount = _model.TypesById.Values.Count(t => t is DexpiClass c && c.IsAbstract);
        _report.EnumCount = _model.TypesById.Values.Count(t => t is DexpiEnum);
        _report.PropertyCount = _model.TypesById.Values.OfType<DexpiClass>().Sum(c => c.Properties.Count);
        _report.AssociationCount = _model.Associations.Count;
    }

    /// <summary>Named reverse association ends become properties on the class at the opposite end's type.</summary>
    private void BuildReverseProperties()
    {
        foreach (var assoc in _model.Associations)
        {
            foreach (var end in assoc.Ends.Where(e => e.IsOwnedEnd))
            {
                if (string.IsNullOrEmpty(end.Name))
                {
                    _report.AddSkipped($"Association {assoc.Id}: unnamed owned end '{end.Id}' skipped (no reverse property).");
                    continue;
                }
                var other = assoc.Ends.FirstOrDefault(e => !e.IsOwnedEnd);
                if (other == null || string.IsNullOrEmpty(other.TypeId))
                {
                    _report.AddSkipped($"Association {assoc.Id}: owned end '{end.Name}' has no opposite class end; skipped.");
                    continue;
                }
                if (_model.TypesById.TryGetValue(other.TypeId, out var ot) && ot is DexpiClass owner)
                {
                    owner.Properties.Add(new DexpiProperty
                    {
                        Id = end.Id,
                        Name = end.Name,
                        TypeId = end.TypeId,
                        IsCollection = end.IsCollection,
                        IsOptional = end.IsOptional,
                    });
                    _report.AddNote($"Reverse property '{owner.CSharpName}.{end.Name}' added from association {assoc.Id}.");
                }
                else
                {
                    _report.AddSkipped($"Association {assoc.Id}: owned end '{end.Name}' - opposite end type is not a generated class; skipped.");
                }
            }
        }
    }

    // ------------------------------------------------------------------
    // Emission
    // ------------------------------------------------------------------

    private string Qualify(DexpiType t, DexpiPackage currentPkg)
    {
        if (ReferenceEquals(t.Package, currentPkg)) return t.CSharpName;
        return CppNamespace(t.Package) + "::" + t.CSharpName;
    }

    private static string CppPrimitive(string csharpPrimitive) => csharpPrimitive switch
    {
        "string" => "std::string",
        "int" => "int",
        "double" => "double",
        "bool" => "bool",
        "byte" => "int",
        // DEXPI DateTime / AnyURI / fallback: serialized as text in the XML interchange
        "DateTime" => "std::string",
        _ => "std::string",
    };

    /// <summary>Core element: ("class", name) -> shared_ptr, ("enum"|"primitive", name) -> value.</summary>
    private (string Kind, string Name) ElementCore(DexpiType? t, string? typeId, DexpiPackage currentPkg)
    {
        if (t == null)
        {
            _report.AddWarning($"Unresolved type reference: '{typeId}' -> std::string (fallback).");
            return ("primitive", "std::string");
        }
        switch (t)
        {
            case DexpiEnum e:
                return ("enum", Qualify(e, currentPkg));
            case DexpiClass c:
                return ("class", Qualify(c, currentPkg));
            case DexpiBuiltin b:
            {
                var m = TypeMapper.Map(_model, b, b.Id, _report);
                if (m.IsPrimitive) return ("primitive", CppPrimitive(m.PrimitiveName));
                return ("class", Qualify(m.Target!, currentPkg));
            }
            default:
                return ("primitive", "std::string");
        }
    }

    private string EmitHeader()
    {
        var lines = new List<string>
        {
            "// <auto-generated>",
            "// DEXPI 2.0 UML model -> C++ - deterministic conversion by Dexpi2CppGen (rule version " + RulesVersion + ").",
            "// Source: " + SourceFileName + " (SHA-256: " + SourceSha256 + ")",
            "// Regenerate: Dexpi2CppGen <input.xmi> <outputDir>",
            "// ALL generalization parents are kept (C++ native multiple inheritance), so mixin attributes",
            "// remain reachable on the subclass - unlike the C# single-inheritance conversion.",
            "// Model-typed members are std::shared_ptr (object-graph semantics); collections are std::vector.",
            "// </auto-generated>",
            "#pragma once",
            "",
            "#include <memory>",
            "#include <optional>",
            "#include <string>",
            "#include <vector>",
            "",
            "// Forward declarations for every model class (std::shared_ptr members accept incomplete types).",
            "namespace dexpi2 {",
        };
        foreach (var package in _model.Packages.Where(p => p.Types.Count > 0))
        {
            var fsegs = package.FullPath.Split('.').Select(seg => seg.TrimStart('_').ToLowerInvariant()).ToArray();
            foreach (var seg in fsegs) lines.Add("namespace " + seg + " {");
            foreach (var t in package.Types.Where(t => t is DexpiClass)) lines.Add("    class " + t.CSharpName + ";");
            for (int i = fsegs.Length - 1; i >= 0; i--) lines.Add("} // namespace " + fsegs[i]);
        }
        lines.Add("} // namespace dexpi2");
        lines.Add("");

        var topo = TopoOrderClasses();

        lines.Add("namespace dexpi2 {");

        // 1) All enumerations first: they have no dependencies, but classes of any package may
        //    reference enums of another package, so every enum must be defined before any class.
        foreach (var package in _model.Packages.Where(p => p.Types.OfType<DexpiEnum>().Any()))
        {
            var segs = CppSegments(package);
            OpenNamespaces(lines, segs);
            foreach (var t in package.Types.OfType<DexpiEnum>()) EmitEnum(t, lines);
            CloseNamespaces(lines, segs);
        }

        // 2) Classes in global topological order (parents first). Each class lives in its own
        //    namespace block; adjacent classes of the same package share one block. This avoids
        //    package-level cycles (e.g. plant::piping VentLine inherits plant::processequipment::Vent
        //    while plant::processequipment::Nozzle inherits plant::piping mixins).
        DexpiPackage? current = null;
        foreach (var cls in topo)
        {
            if (!ReferenceEquals(cls.Package, current))
            {
                if (current != null) CloseNamespaces(lines, CppSegments(current));
                current = cls.Package;
                OpenNamespaces(lines, CppSegments(current));
            }
            EmitClass(cls, lines);
        }
        if (current != null) CloseNamespaces(lines, CppSegments(current));

        lines.Add("");
        lines.Add("} // namespace dexpi2");
        return string.Join("\r\n", lines) + "\r\n";
    }

    private static string[] CppSegments(DexpiPackage p) =>
        p.FullPath.Split('.').Select(seg => seg.TrimStart('_').ToLowerInvariant()).ToArray();

    private static void OpenNamespaces(List<string> lines, string[] segs)
    {
        lines.Add("");
        foreach (var seg in segs) lines.Add("namespace " + seg + " {");
    }

    private static void CloseNamespaces(List<string> lines, string[] segs)
    {
        for (int i = segs.Length - 1; i >= 0; i--) lines.Add("} // namespace " + segs[i]);
    }

    /// <summary>Global topological order (parents first), stable w.r.t. document order.</summary>
    private List<DexpiClass> TopoOrderClasses()
    {
        var classes = _model.TypesById.Values.OfType<DexpiClass>().ToList();
        var byName = classes.ToDictionary(c => c.CSharpName, StringComparer.Ordinal);
        var indeg = classes.ToDictionary(c => c.CSharpName, _ => 0, StringComparer.Ordinal);
        var children = classes.ToDictionary(c => c.CSharpName, _ => new List<DexpiClass>(), StringComparer.Ordinal);
        var pos = classes.Select((c, i) => (c, i)).ToDictionary(x => x.c.CSharpName, x => x.i, StringComparer.Ordinal);

        foreach (var c in classes)
        {
            foreach (var gid in c.GeneralizationIds)
            {
                if (!_model.TypesById.TryGetValue(gid, out var g) || g is not DexpiClass gc) continue;
                if (!byName.ContainsKey(gc.CSharpName) || gc.CSharpName == c.CSharpName) continue;
                indeg[c.CSharpName]++;
                children[gc.CSharpName].Add(c);
            }
        }

        var result = new List<DexpiClass>();
        var ready = classes.Where(c => indeg[c.CSharpName] == 0).OrderBy(c => pos[c.CSharpName]).ToList();
        while (ready.Count > 0)
        {
            var c = ready[0];
            ready.RemoveAt(0);
            result.Add(c);
            foreach (var child in children[c.CSharpName].OrderBy(x => pos[x.CSharpName]))
            {
                indeg[child.CSharpName]--;
                if (indeg[child.CSharpName] == 0) ready.Add(child);
            }
        }
        if (result.Count != classes.Count)
        {
            _report.AddWarning($"Inheritance cycle detected: {classes.Count - result.Count} class(es) emitted after cycle members.");
            var emitted = result.Select(c => c.CSharpName).ToHashSet(StringComparer.Ordinal);
            foreach (var c in classes.Where(c => !emitted.Contains(c.CSharpName))) result.Add(c);
        }
        return result;
    }

    private void EmitEnum(DexpiEnum e, List<string> lines)
    {
        lines.Add("");
        lines.Add("    // DEXPI 2.0 enumeration " + e.Name + " (XMI id " + e.Id + ")");
        lines.Add("    enum class " + e.CSharpName);
        lines.Add("    {");
        var used = new HashSet<string>(StringComparer.Ordinal);
        foreach (var lit in e.Literals)
        {
            var raw = CppEscape(lit);
            var name = raw;
            var i = 1;
            while (!used.Add(name))
            {
                name = raw + "_" + i.ToString(CultureInfo.InvariantCulture);
                i++;
            }
            lines.Add("        " + name + ",  // " + lit);
        }
        lines.Add("    };");
    }

    private void EmitClass(DexpiClass c, List<string> lines)
    {
        lines.Add("");
        lines.Add("    // DEXPI 2.0 model class " + c.Name + " (XMI id " + c.Id + ")" + (c.IsAbstract ? " [abstract in DEXPI]" : ""));
        var bases = ResolveBases(c);
        if (c.GeneralizationIds.Count > 1)
        {
            MixinClassCount++;
            MixinTotal += c.GeneralizationIds.Count;
        }
        var decl = "    class " + c.CSharpName;
        if (bases.Count > 0) decl += " : " + string.Join(", ", bases);
        lines.Add(decl);
        lines.Add("    {");
        lines.Add("    public:");
        lines.Add("        virtual ~" + c.CSharpName + "() = default;");

        var used = new HashSet<string>(StringComparer.Ordinal);
        foreach (var p in c.Properties)
        {
            if (string.IsNullOrEmpty(p.Name))
            {
                _report.AddSkipped($"Class {c.CSharpName}: unnamed property '{p.Id}' skipped.");
                continue;
            }
            var raw = CppEscape(p.Name);
            if (raw == c.CSharpName)
            {
                raw += "_";
                _report.AddNote($"Property '{p.Name}' of {c.CSharpName} renamed to '{raw}' (member cannot share its enclosing class name, C7539).");
            }
            var name = raw;
            var i = 1;
            while (!used.Add(name))
            {
                name = raw + "_" + i.ToString(CultureInfo.InvariantCulture);
                i++;
            }
            if (name != raw)
                _report.AddNote($"Property name collision in {c.CSharpName}: '{p.Name}' -> '{name}'.");
            lines.Add("        " + EmitProperty(p, c.Package, name) + ";");
        }

        lines.Add("");
        if (c.IsAbstract)
        {
            lines.Add("    protected:");
            lines.Add("        " + c.CSharpName + "() = default;  // abstract in DEXPI: not directly instantiable");
        }
        else
        {
            lines.Add("        " + c.CSharpName + "() = default;");
        }
        lines.Add("    };");
    }

    private List<string> ResolveBases(DexpiClass c)
    {
        var bases = new List<string>();
        foreach (var gid in c.GeneralizationIds)
        {
            if (!_model.TypesById.TryGetValue(gid, out var g))
            {
                _report.AddWarning($"Class {c.CSharpName}: unresolved generalization '{gid}' skipped.");
                continue;
            }
            switch (g)
            {
                case DexpiClass gc:
                    if (gc.CSharpName == c.CSharpName)
                    {
                        _report.AddSkipped($"Class {c.CSharpName}: self-referencing generalization skipped.");
                        continue;
                    }
                    bases.Add("public " + Qualify(gc, c.Package));
                    break;
                case DexpiEnum ge:
                    _report.AddSkipped($"Class {c.CSharpName}: generalization to enum {ge.CSharpName} not representable; skipped.");
                    break;
                case DexpiBuiltin gb:
                    _report.AddSkipped($"Class {c.CSharpName}: generalization to builtin '{gb.Name}' not representable; skipped.");
                    break;
            }
        }
        return bases;
    }

    private string EmitProperty(DexpiProperty p, DexpiPackage pkg, string name)
    {
        var t = _model.TypesById.TryGetValue(p.TypeId ?? "", out var pt) ? pt : null;
        var (kind, elem) = ElementCore(t, p.TypeId, pkg);

        string element = kind == "class" ? "std::shared_ptr<" + elem + ">" : elem;

        if (p.IsCollection)
            return "std::vector<" + element + "> " + name;
        if (p.IsOptional && kind != "class")
            return "std::optional<" + element + "> " + name;
        // class members are shared_ptr (empty = absent); builtins get deterministic defaults
        if (kind == "primitive" && elem == "int") return element + " " + name + " = 0";
        if (kind == "primitive" && elem == "double") return element + " " + name + " = 0.0";
        if (kind == "primitive" && elem == "bool") return element + " " + name + " = false";
        if (kind == "enum") return element + " " + name + "{}";
        return element + " " + name;
    }

    private static string ComputeSha256(string path)
    {
        using var sha = SHA256.Create();
        var bytes = File.ReadAllBytes(path);
        return Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant();
    }
}
