using System.Xml;

namespace Dexpi2CppGen;

/// <summary>Parses the DEXPI 2.0 standard XMI 2.1 export (Dexpi.xmi) into a DexpiModel.</summary>
public static class XmiParser
{
    // XMI 2.1 elements in this export (packagedElement, ownedAttribute, generalization, ...)
    // carry NO namespace prefix; xmi:type / xmi:id attributes are in the XMI namespace;
    // "uml:Class" etc. are xmi:type attribute values.
    private const string XmiNs = "http://schema.omg.org/spec/XMI/2.1";

    public static DexpiModel Parse(string path)
    {
        var doc = new XmlDocument();
        doc.Load(path);
        var root = doc.DocumentElement ?? throw new InvalidOperationException("Empty XMI document.");
        var model = new DexpiModel();

        foreach (XmlNode node in root.ChildNodes)
        {
            if (!IsElement(node, "packagedElement", "")) continue;
            var xmiType = GetXmiType(node);
            if (xmiType == "uml:Model" || xmiType == "uml:Package")
                WalkPackage(node, Attr(node, "name"), model);
            else
                AddType(node, xmiType, Attr(node, "name"), new DexpiPackage(Attr(node, "name")), model);
        }

        return model;
    }

    private static void WalkPackage(XmlNode node, string packagePath, DexpiModel model)
    {
        var package = new DexpiPackage(packagePath);
        model.Packages.Add(package);

        foreach (XmlNode child in node.ChildNodes)
        {
            if (!IsElement(child, "packagedElement", "")) continue;
            var xmiType = GetXmiType(child);
            var name = Attr(child, "name");
            if (xmiType == "uml:Model" || xmiType == "uml:Package")
                WalkPackage(child, package.FullPath + "." + name, model);
            else
                AddType(child, xmiType, name, package, model);
        }
    }

    private static void AddType(XmlNode node, string xmiType, string name, DexpiPackage package, DexpiModel model)
    {
        var id = GetXmiId(node);
        switch (xmiType)
        {
            case "uml:Class":
                AddClass(node, id, name, package, model);
                break;
            case "uml:Enumeration":
            {
                var en = new DexpiEnum(id, name, package);
                foreach (XmlNode lit in node.ChildNodes)
                    if (IsElement(lit, "ownedLiteral", ""))
                        en.Literals.Add(Attr(lit, "name"));
                model.TypesById[id] = en;
                package.Types.Add(en);
                break;
            }
            case "uml:DataType":
                // Special-named DataTypes ("Undefined | X", "PhysicalQuantity with UnitType=...") are
                // builtin mappings; plain-named DataTypes are materialized as classes.
                if (IsSpecialDataTypeName(name))
                    model.TypesById[id] = new DexpiBuiltin(id, name, package);
                else
                    AddClass(node, id, name, package, model);
                break;
            case "uml:PrimitiveType":
                model.PrimitiveTypeCount++;
                model.TypesById[id] = new DexpiBuiltin(id, name, package);
                break;
            case "uml:Association":
            {
                var assoc = new DexpiAssociation { Id = id };
                foreach (var endId in (Attr(node, "memberEnd") ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    assoc.Ends.Add(FindOwnedEnd(node, endId) ?? new DexpiEnd { Id = endId });
                model.Associations.Add(assoc);
                break;
            }
        }
    }

    private static void AddClass(XmlNode node, string id, string name, DexpiPackage package, DexpiModel model)
    {
        var cls = new DexpiClass(id, name, package, Attr(node, "isAbstract") == "true");
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType != XmlNodeType.Element || child.NamespaceURI != "") continue;
            switch (child.LocalName)
            {
                case "generalization":
                    cls.GeneralizationIds.Add(Attr(child, "general"));
                    break;
                case "ownedAttribute":
                    cls.Properties.Add(ParseProperty(child));
                    break;
            }
        }
        model.TypesById[id] = cls;
        package.Types.Add(cls);
    }

    private static DexpiProperty ParseProperty(XmlNode node)
    {
        var p = new DexpiProperty
        {
            Id = GetXmiId(node),
            Name = Attr(node, "name"),
            TypeId = Attr(node, "type"),
            IsComposite = Attr(node, "aggregation") == "composite",
            RedefinedPropertyId = Attr(node, "redefinedProperty"),
        };
        foreach (XmlNode c in node.ChildNodes)
        {
            if (c.NodeType != XmlNodeType.Element || c.NamespaceURI != "") continue;
            var value = Attr(c, "value");
            switch (c.LocalName)
            {
                case "lowerValue":
                    p.IsOptional = value.Length == 0 || value == "0";
                    break;
                case "upperValue":
                    if (value == "*") p.IsCollection = true;
                    else if (int.TryParse(value, out var n)) p.IsCollection = n > 1;
                    break;
            }
        }
        return p;
    }

    private static DexpiEnd? FindOwnedEnd(XmlNode association, string endId)
    {
        foreach (XmlNode child in association.ChildNodes)
        {
            if (!IsElement(child, "ownedEnd", "")) continue;
            if (GetXmiId(child) != endId) continue;
            var e = new DexpiEnd
            {
                Id = endId,
                IsOwnedEnd = true,
                Name = Attr(child, "name"),
                TypeId = Attr(child, "type"),
            };
            foreach (XmlNode c in child.ChildNodes)
            {
                if (c.NodeType != XmlNodeType.Element || c.NamespaceURI != "") continue;
                var value = Attr(c, "value");
                switch (c.LocalName)
                {
                    case "lowerValue":
                        e.IsOptional = value.Length == 0 || value == "0";
                        break;
                    case "upperValue":
                        if (value == "*") e.IsCollection = true;
                        else if (int.TryParse(value, out var n)) e.IsCollection = n > 1;
                        break;
                }
            }
            return e;
        }
        return null;
    }

    private static bool IsSpecialDataTypeName(string name) =>
        name.Contains(" | ", StringComparison.Ordinal) || name.Contains(" with ", StringComparison.Ordinal);

    private static bool IsElement(XmlNode n, string localName, string ns) =>
        n.NodeType == XmlNodeType.Element && n.LocalName == localName && n.NamespaceURI == ns;

    private static string GetXmiType(XmlNode n) => GetAttr(n, "type", XmiNs);
    private static string GetXmiId(XmlNode n) => GetAttr(n, "id", XmiNs);

    private static string GetAttr(XmlNode n, string localName, string ns) =>
        n is XmlElement e ? e.GetAttribute(localName, ns) : "";

    private static string Attr(XmlNode n, string name) =>
        n is XmlElement e ? e.GetAttribute(name) : "";
}
