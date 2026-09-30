using System.Text;

namespace Dexpi2CppGen;

/// <summary>Deterministic C# identifier rules: sanitization, keyword escaping, variant naming.</summary>
public static class Identifier
{
    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
        "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
        "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
        "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
        "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
        "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
        "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    };

    /// <summary>Replace every non letter/digit/underscore character with '_'; ensure a valid start.</summary>
    public static string Sanitize(string name)
    {
        var sb = new StringBuilder(name.Length);
        foreach (var c in name)
            sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
        var s = sb.ToString();
        if (s.Length == 0) s = "_";
        if (char.IsDigit(s[0])) s = "_" + s;
        return s;
    }

    /// <summary>Sanitize, then escape C# keywords with '@'.</summary>
    public static string Escape(string name)
    {
        var s = Sanitize(name);
        return Keywords.Contains(s) ? "@" + s : s;
    }

    /// <summary>
    /// Deterministic class name for a "X with Type=(T)" binding variant.
    /// "QualifiedValue with Type=(Undefined | Double)" -> "QualifiedValueOfDouble".
    /// </summary>
    public static string VariantName(string original)
    {
        const string marker = "with Type=(";
        var idx = original.IndexOf(marker, StringComparison.Ordinal);
        var head = idx > 0 ? original.Substring(0, idx).Trim() : original;
        var typePart = idx > 0 ? original.Substring(idx + marker.Length) : original;
        if (typePart.EndsWith(")", StringComparison.Ordinal))
            typePart = typePart.Substring(0, typePart.Length - 1).Trim();
        if (typePart.StartsWith("Undefined | ", StringComparison.Ordinal))
            typePart = typePart.Substring("Undefined | ".Length);
        if (typePart.StartsWith("(", StringComparison.Ordinal) && typePart.EndsWith(")", StringComparison.Ordinal))
            typePart = typePart.Substring(1, typePart.Length - 2).Trim();

        var clean = new StringBuilder();
        foreach (var c in head)
            if (char.IsLetterOrDigit(c)) clean.Append(c);
        clean.Append("Of");
        foreach (var c in typePart)
            if (char.IsLetterOrDigit(c)) clean.Append(c);
        var result = clean.ToString();
        if (result.Length == 0 || char.IsDigit(result[0])) result = "_" + result;
        return result;
    }
}
