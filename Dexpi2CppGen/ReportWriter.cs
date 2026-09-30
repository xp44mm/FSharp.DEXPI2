using System.Text;

namespace Dexpi2CppGen;

/// <summary>Writes the deterministic generation report (markdown, UTF-8 BOM, CRLF).</summary>
internal static class ReportWriter
{
    public static void Write(string path, CppEmitter emitter, DexpiModel model, GenerationReport report)
    {
        var lines = new List<string>
        {
            "# DEXPI 2.0 UML -> C++ - Generation Report",
            "",
            "- Input: " + emitter.SourceFileName + " (SHA-256: " + emitter.SourceSha256 + ")",
            "- Rule version: " + CppEmitter.RulesVersion,
            "- Primitive types in model: " + model.PrimitiveTypeCount,
            "",
            "## Counts",
            "",
            "- Packages: " + model.Packages.Count(p => p.Types.Count > 0),
            "- Classes: " + report.ClassCount + " (abstract: " + report.AbstractCount + ")",
            "- Enumerations: " + report.EnumCount,
            "- Properties: " + report.PropertyCount,
            "- Associations: " + report.AssociationCount,
            "- Classes with multiple inheritance: " + emitter.MixinClassCount + " (mixin references total: " + emitter.MixinTotal + ")",
            "",
            "## Output",
            "",
            "- dexpi2.hpp (single header, all namespaces)",
            "",
        };

        if (report.Notes.Count > 0)
        {
            lines.Add("## Notes");
            lines.Add("");
            foreach (var n in report.Notes) lines.Add("- " + n);
            lines.Add("");
        }
        if (report.Skipped.Count > 0)
        {
            lines.Add("## Skipped");
            lines.Add("");
            foreach (var s in report.Skipped) lines.Add("- " + s);
            lines.Add("");
        }
        if (report.Warnings.Count > 0)
        {
            lines.Add("## Warnings");
            lines.Add("");
            foreach (var w in report.Warnings) lines.Add("- " + w);
            lines.Add("");
        }

        lines.Add("## Mapping rules (deterministic)");
        lines.Add("");
        lines.Add("- Special-named DataTypes map to C++ builtins: \"Undefined | String\"->std::string, \"Undefined | Integer\"->int, \"Undefined | Double\"->double, \"Undefined | Boolean\"->bool, \"Undefined | DateTime\"->std::string (ISO text in XML interchange), \"Undefined | AnyURI\"->std::string, UnsignedByte->int.");
        lines.Add("- \"PhysicalQuantity...\" / \"PhysicalQuantityVector...\" special types map to the materialized classes PhysicalQuantity / PhysicalQuantityVector.");
        lines.Add("- \"Undefined | X\" maps to type X (enum or class) resolved by name; first occurrence wins when ambiguous.");
        lines.Add("- Plain-named DataTypes are materialized as classes.");
        lines.Add("- ALL generalization parents become base classes (C++ native multiple inheritance); generalizations to enums/builtins are skipped.");
        lines.Add("- \"QualifiedValue with Type=(T)\" binding variants become classes named QualifiedValueOf<T>.");
        lines.Add("- Model-typed members are std::shared_ptr<T> (object-graph semantics, empty = absent); enum/builtin members are values; collections are std::vector<T>; 0..1 enum/builtin members are std::optional<T>.");
        lines.Add("- Abstract classes get a protected default constructor and a virtual destructor (not directly instantiable).");
        lines.Add("- Named reverse association ends become properties on the class at the opposite end; unnamed reverse ends are skipped.");
        lines.Add("- One header file dexpi2.hpp: namespaces mirror packages (dexpi2::&lt;pkg&gt;, lower-case); type names are globally unique; cross-package references are fully qualified.");
        lines.Add("- Identifiers: invalid characters become '_'; C++ keywords get a trailing '_'; name collisions get a deterministic numeric suffix.");
        lines.Add("- Output encoding: UTF-8 with BOM, CRLF line endings, trailing CRLF. No timestamps: output depends only on input bytes and rule version.");

        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        File.WriteAllText(path, string.Join("\r\n", lines) + "\r\n", new UTF8Encoding(true));
    }
}
