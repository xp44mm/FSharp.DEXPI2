#!/usr/bin/env python3
"""DEXPI 2.0 UML (XMI 2.1) -> Python dataclass deterministic generator.

Port of the C# rules (Dexpi2CSharpGen) with one deliberate difference:
ALL generalization parents are kept -> Python native multiple inheritance
expresses the XMI mixin structure (ProcessEquipment is simultaneously
ChamberOwner + NozzleOwner + TaggedPlantItem + TransmissionDriver).

Output: a single module with every model class as a dataclass, every
enumeration as an Enum. All fields default to None / empty list so any
inheritance combination is constructible (POCO style; a deserializer fills
the values from DEXPI XML).

Deterministic: no timestamps; output depends only on input bytes and the
rule version. File encoding: UTF-8 BOM, CRLF, trailing CRLF.

Usage:
    python dexpi2/gen.py <input.xmi> <output.py> [report.md]
"""

from __future__ import annotations

import argparse
import hashlib
import sys
import xml.etree.ElementTree as ET

XMI_NS = "http://schema.omg.org/spec/XMI/2.1"
RULES_VERSION = "1.0"

# Python 3 reserved words; identifiers equal to one of these get a trailing '_'.
PY_KEYWORDS = frozenset(
    """False None True and as assert async await break class continue def del elif
    else except finally for from global if import in is lambda nonlocal not or pass
    raise return try while with yield""".split()
)

# ---------------------------------------------------------------------------
# Intermediate model (mirrors Dexpi2CSharpGen/Model.cs)
# ---------------------------------------------------------------------------


class DexpiPackage:
    def __init__(self, full_path: str):
        self.full_path = full_path  # e.g. "Core.DataTypes"; "_Auxiliaries" keeps its underscore
        self.types = []  # generated types in document order

    @property
    def namespace(self) -> str:
        # "Core.DataTypes" -> "Dexpi.Core.DataTypes" (C# namespace; kept for the report)
        return "Dexpi." + ".".join(p.lstrip("_") for p in self.full_path.split("."))


class DexpiType:
    def __init__(self, type_id: str, name: str, package: DexpiPackage):
        self.id = type_id
        self.name = name
        self.package = package
        self.py_name = ""  # resolved unique Python identifier
        self.is_generated = False


class DexpiClass(DexpiType):
    def __init__(self, type_id: str, name: str, package: DexpiPackage, is_abstract: bool):
        super().__init__(type_id, name, package)
        self.is_abstract = is_abstract
        self.generalization_ids = []  # ALL parents (multiple inheritance)
        self.properties = []


class DexpiEnum(DexpiType):
    def __init__(self, type_id: str, name: str, package: DexpiPackage):
        super().__init__(type_id, name, package)
        self.literals = []


class DexpiBuiltin(DexpiType):
    pass


class DexpiProperty:
    def __init__(self):
        self.id = ""
        self.name = ""
        self.type_id = None
        self.is_collection = False
        self.is_optional = False
        self.is_composite = False
        self.redefined_property_id = None


class DexpiEnd:
    def __init__(self):
        self.id = ""
        self.is_owned_end = False
        self.name = ""
        self.type_id = None
        self.is_optional = False
        self.is_collection = False


class DexpiAssociation:
    def __init__(self, assoc_id: str):
        self.id = assoc_id
        self.ends = []


class DexpiModel:
    def __init__(self):
        self.packages = []
        self.types_by_id = {}
        self.associations = []
        self.primitive_type_count = 0


class GenerationReport:
    def __init__(self):
        self.class_count = 0
        self.abstract_count = 0
        self.enum_count = 0
        self.property_count = 0
        self.association_count = 0
        self.mixin_class_count = 0
        self.mixin_total = 0
        self.notes = []
        self.skipped = []
        self.warnings = []

    def add_note(self, s):
        if s not in self.notes:
            self.notes.append(s)

    def add_skipped(self, s):
        if s not in self.skipped:
            self.skipped.append(s)

    def add_warning(self, s):
        if s not in self.warnings:
            self.warnings.append(s)


# ---------------------------------------------------------------------------
# XMI parsing (mirrors Dexpi2CSharpGen/XmiParser.cs)
# ---------------------------------------------------------------------------


def _xmi_attr(elem: ET.Element, local_name: str) -> str:
    return elem.get("{%s}%s" % (XMI_NS, local_name), "")


def _is_special_data_type_name(name: str) -> bool:
    return " | " in name or " with " in name


def _parse_property(node: ET.Element) -> DexpiProperty:
    p = DexpiProperty()
    p.id = _xmi_attr(node, "id")
    p.name = node.get("name", "")
    p.type_id = node.get("type") or None
    p.is_composite = node.get("aggregation", "") == "composite"
    p.redefined_property_id = node.get("redefinedProperty") or None
    for c in node:
        if c.tag != "lowerValue" and c.tag != "upperValue":
            continue
        value = c.get("value", "")
        if c.tag == "lowerValue":
            p.is_optional = (value == "" or value == "0")
        else:  # upperValue
            if value == "*":
                p.is_collection = True
            elif value.isdigit():
                p.is_collection = int(value) > 1
    return p


def _find_owned_end(association: ET.Element, end_id: str):
    for child in association:
        if child.tag != "ownedEnd":
            continue
        if _xmi_attr(child, "id") != end_id:
            continue
        e = DexpiEnd()
        e.id = end_id
        e.is_owned_end = True
        e.name = child.get("name", "")
        e.type_id = child.get("type") or None
        for c in child:
            if c.tag not in ("lowerValue", "upperValue"):
                continue
            value = c.get("value", "")
            if c.tag == "lowerValue":
                e.is_optional = (value == "" or value == "0")
            else:
                if value == "*":
                    e.is_collection = True
                elif value.isdigit():
                    e.is_collection = int(value) > 1
        return e
    return None


def _add_class(node: ET.Element, type_id: str, name: str, package: DexpiPackage, model: DexpiModel):
    cls = DexpiClass(type_id, name, package, node.get("isAbstract", "") == "true")
    for child in node:
        if child.tag == "generalization":
            cls.generalization_ids.append(child.get("general", ""))
        elif child.tag == "ownedAttribute":
            cls.properties.append(_parse_property(child))
    model.types_by_id[type_id] = cls
    package.types.append(cls)


def _add_type(node: ET.Element, xmi_type: str, name: str, package: DexpiPackage, model: DexpiModel):
    type_id = _xmi_attr(node, "id")
    if xmi_type == "uml:Class":
        _add_class(node, type_id, name, package, model)
    elif xmi_type == "uml:Enumeration":
        en = DexpiEnum(type_id, name, package)
        for lit in node:
            if lit.tag == "ownedLiteral":
                en.literals.append(lit.get("name", ""))
        model.types_by_id[type_id] = en
        package.types.append(en)
    elif xmi_type == "uml:DataType":
        # Special-named DataTypes are builtin mappings; plain-named ones are classes.
        if _is_special_data_type_name(name):
            model.types_by_id[type_id] = DexpiBuiltin(type_id, name, package)
        else:
            _add_class(node, type_id, name, package, model)
    elif xmi_type == "uml:PrimitiveType":
        model.primitive_type_count += 1
        model.types_by_id[type_id] = DexpiBuiltin(type_id, name, package)
    elif xmi_type == "uml:Association":
        assoc = DexpiAssociation(type_id)
        for end_id in (node.get("memberEnd", "") or "").split():
            found = _find_owned_end(node, end_id)
            assoc.ends.append(found if found is not None else _blank_end(end_id))
        model.associations.append(assoc)


def _blank_end(end_id: str) -> DexpiEnd:
    e = DexpiEnd()
    e.id = end_id
    return e


def _walk_package(node: ET.Element, package_path: str, model: DexpiModel):
    package = DexpiPackage(package_path)
    model.packages.append(package)
    for child in node:
        if child.tag != "packagedElement":
            continue
        xmi_type = _xmi_attr(child, "type")
        name = child.get("name", "")
        if xmi_type == "uml:Model" or xmi_type == "uml:Package":
            _walk_package(child, package.full_path + "." + name, model)
        else:
            _add_type(child, xmi_type, name, package, model)


def parse(path: str) -> DexpiModel:
    tree = ET.parse(path)
    root = tree.getroot()
    model = DexpiModel()
    for child in root:
        if child.tag != "packagedElement":
            continue
        xmi_type = _xmi_attr(child, "type")
        name = child.get("name", "")
        if xmi_type == "uml:Model" or xmi_type == "uml:Package":
            _walk_package(child, name, model)
        else:
            _add_type(child, xmi_type, name, DexpiPackage(name), model)
    return model


# ---------------------------------------------------------------------------
# Identifier rules (mirrors Dexpi2CSharpGen/Identifier.cs; Python keywords get '_')
# ---------------------------------------------------------------------------


def sanitize(name: str) -> str:
    s = "".join(c if (c.isalnum() or c == "_") else "_" for c in name)
    if not s:
        s = "_"
    if s[0].isdigit():
        s = "_" + s
    return s


def escape(name: str) -> str:
    s = sanitize(name)
    return s + "_" if s in PY_KEYWORDS else s


def variant_name(original: str) -> str:
    marker = "with Type=("
    idx = original.find(marker)
    head = original[:idx].strip() if idx > 0 else original
    type_part = original[idx + len(marker):] if idx > 0 else original
    if type_part.endswith(")"):
        type_part = type_part[:-1].strip()
    if type_part.startswith("Undefined | "):
        type_part = type_part[len("Undefined | "):]
    if type_part.startswith("(") and type_part.endswith(")"):
        type_part = type_part[1:-1].strip()
    clean = "".join(c for c in head if c.isalnum()) + "Of" + "".join(c for c in type_part if c.isalnum())
    if not clean or clean[0].isdigit():
        clean = "_" + clean
    return clean


# ---------------------------------------------------------------------------
# Type mapping (mirrors Dexpi2CSharpGen/TypeMapper.cs)
# ---------------------------------------------------------------------------


def _resolve_by_name(model: DexpiModel, name: str, origin: DexpiBuiltin, report: GenerationReport):
    matches = [t for t in model.types_by_id.values() if t.name == name and t is not origin]
    if not matches:
        report.add_warning("Cannot resolve type named '%s' (referenced by '%s') -> object." % (name, origin.name))
        return None
    if len(matches) > 1:
        report.add_note("Ambiguous name '%s' resolved to first occurrence '%s' (%d candidates)."
                        % (name, matches[0].py_name, len(matches)))
    m = matches[0]
    if isinstance(m, (DexpiEnum, DexpiClass)):
        return m
    return None


def map_builtin(model: DexpiModel, b: DexpiBuiltin, report: GenerationReport):
    """Returns a Python type-name string, or a DexpiEnum/DexpiClass to reference, or None."""
    name = b.name.strip()
    if name in ("Undefined | String", "String"):
        return "str"
    if name in ("Undefined | Integer", "Integer"):
        return "int"
    if name in ("Undefined | Double", "Double"):
        return "float"
    if name in ("Undefined | Boolean", "Boolean"):
        return "bool"
    if name in ("Undefined | DateTime", "DateTime"):
        return "datetime.datetime"
    if name in ("Undefined | AnyURI", "AnyURI"):
        return "str"
    if name == "UnsignedByte":
        return "int"
    if name.startswith("Undefined | (PhysicalQuantityVector") or name.startswith("PhysicalQuantityVector"):
        return _resolve_by_name(model, "PhysicalQuantityVector", b, report)
    if (name.startswith("Undefined | (PhysicalQuantity") or name.startswith("Undefined | PhysicalQuantity")
            or name.startswith("PhysicalQuantity")):
        return _resolve_by_name(model, "PhysicalQuantity", b, report)
    if name.startswith("Undefined | "):
        rest = name[len("Undefined | "):].strip()
        if rest.startswith("(") and rest.endswith(")"):
            rest = rest[1:-1].strip()
        return _resolve_by_name(model, rest, b, report)
    report.add_warning("Builtin type '%s' has no mapping -> object." % name)
    return None


# ---------------------------------------------------------------------------
# Name resolution + reverse properties (mirrors CSharpEmitter.ResolveNames/BuildReverseProperties)
# ---------------------------------------------------------------------------


def resolve_names(model: DexpiModel, report: GenerationReport):
    # Global uniqueness across packages (single output module): classes and enums share one pool.
    used = set()
    for package in model.packages:
        for t in package.types:
            if isinstance(t, DexpiClass) and "with Type=" in t.name:
                base_name = variant_name(t.name)
            else:
                base_name = sanitize(t.name)
            name = base_name
            i = 1
            while name in used:
                name = base_name + "_" + str(i)
                i += 1
            used.add(name)
            t.py_name = name
            t.is_generated = True
            if name != base_name:
                report.add_note("Type name collision (global): '%s' -> '%s'." % (t.name, name))

    report.class_count = sum(1 for t in model.types_by_id.values() if isinstance(t, DexpiClass))
    report.abstract_count = sum(1 for t in model.types_by_id.values()
                                if isinstance(t, DexpiClass) and t.is_abstract)
    report.enum_count = sum(1 for t in model.types_by_id.values() if isinstance(t, DexpiEnum))
    report.property_count = sum(len(c.properties) for c in model.types_by_id.values()
                                if isinstance(c, DexpiClass))
    report.association_count = len(model.associations)


def build_reverse_properties(model: DexpiModel, report: GenerationReport):
    """Named reverse association ends become properties on the class at the opposite end's type."""
    for assoc in model.associations:
        for end in assoc.ends:
            if not end.is_owned_end:
                continue
            if not end.name:
                report.add_skipped("Association %s: unnamed owned end '%s' skipped (no reverse property)."
                                   % (assoc.id, end.id))
                continue
            other = next((e for e in assoc.ends if not e.is_owned_end), None)
            if other is None or not other.type_id:
                report.add_skipped("Association %s: owned end '%s' has no opposite class end; skipped."
                                   % (assoc.id, end.name))
                continue
            ot = model.types_by_id.get(other.type_id)
            if isinstance(ot, DexpiClass):
                p = DexpiProperty()
                p.id = end.id
                p.name = end.name
                p.type_id = end.type_id
                p.is_collection = end.is_collection
                p.is_optional = end.is_optional
                ot.properties.append(p)
                report.add_note("Reverse property '%s.%s' added from association %s."
                                % (ot.py_name, end.name, assoc.id))
            else:
                report.add_skipped("Association %s: owned end '%s' - opposite end type is not a generated class; skipped."
                                   % (assoc.id, end.name))


# ---------------------------------------------------------------------------
# Emission
# ---------------------------------------------------------------------------


def _type_ref(model: DexpiModel, type_id, report: GenerationReport) -> str:
    t = model.types_by_id.get(type_id or "", None)
    if t is None:
        report.add_warning("Unresolved type reference: '%s' -> object." % type_id)
        return "object"
    if isinstance(t, (DexpiEnum, DexpiClass)):
        return t.py_name
    if isinstance(t, DexpiBuiltin):
        m = map_builtin(model, t, report)
        if m is None:
            return "object"
        if isinstance(m, str):
            return m
        return m.py_name
    return "object"


def _base_classes(model: DexpiModel, cls: DexpiClass, report: GenerationReport):
    """ALL generalization parents (Python multiple inheritance)."""
    bases = []
    for gid in cls.generalization_ids:
        g = model.types_by_id.get(gid)
        if g is None:
            report.add_warning("Class %s: unresolved generalization '%s' skipped." % (cls.py_name, gid))
            continue
        if isinstance(g, DexpiClass):
            if g.py_name == cls.py_name:
                report.add_skipped("Class %s: self-referencing generalization skipped." % cls.py_name)
                continue
            bases.append(g.py_name)
        elif isinstance(g, DexpiEnum):
            report.add_skipped("Class %s: generalization to enum %s not representable; skipped."
                               % (cls.py_name, g.name))
        elif isinstance(g, DexpiBuiltin):
            report.add_skipped("Class %s: generalization to builtin '%s' not representable; skipped."
                               % (cls.py_name, g.name))
    return bases


def _topo_order_classes(model: DexpiModel, report: GenerationReport):
    """Kahn topological sort, parents first; stable w.r.t. document order."""
    classes = [t for t in model.types_by_id.values() if isinstance(t, DexpiClass)]
    by_name = {c.py_name: c for c in classes}
    indeg = {}
    children = {}
    for c in classes:
        indeg[c.py_name] = 0
        children[c.py_name] = []
    for c in classes:
        for gid in c.generalization_ids:
            g = model.types_by_id.get(gid)
            if isinstance(g, DexpiClass) and g.py_name != c.py_name and g.py_name in by_name:
                indeg[c.py_name] += 1
                children[g.py_name].append(c)
            elif isinstance(g, DexpiClass) and g.py_name not in by_name:
                report.add_warning("Class %s: generalization to '%s' outside generated set; skipped."
                                   % (c.py_name, gid))
    result = []
    ready = [c for c in classes if indeg[c.py_name] == 0]
    pos = {c.py_name: i for i, c in enumerate(classes)}
    ready.sort(key=lambda c: pos[c.py_name])
    while ready:
        c = ready.pop(0)
        result.append(c)
        for child in sorted(children[c.py_name], key=lambda x: pos[x.py_name]):
            indeg[child.py_name] -= 1
            if indeg[child.py_name] == 0:
                ready.append(child)
    if len(result) != len(classes):
        report.add_warning("Inheritance cycle detected: %d of %d classes emitted (cycle members inherit object)."
                           % (len(result), len(classes)))
        emitted = {c.py_name for c in result}
        for c in classes:
            if c.py_name not in emitted:
                result.append(c)
    return result


def _emit_class(model: DexpiModel, cls: DexpiClass, report: GenerationReport, out: list):
    bases = _base_classes(model, cls, report)
    if len(cls.generalization_ids) > 1:
        report.mixin_class_count += 1
        report.mixin_total += len(cls.generalization_ids)
    decl = "@dataclass\nclass %s" % cls.py_name
    if bases:
        decl += "(" + ", ".join(bases) + ")"
    out.append("")
    abstract_mark = " (abstract in DEXPI)" if cls.is_abstract else ""
    out.append(decl + ":")
    out.append("    \"\"\"DEXPI 2.0 model class %s%s (XMI id %s).\"\"\"" % (cls.name, abstract_mark, cls.id))

    used = set()
    for p in cls.properties:
        if not p.name:
            report.add_skipped("Class %s: unnamed property '%s' skipped." % (cls.py_name, p.id))
            continue
        raw = escape(p.name)
        name = raw
        i = 1
        while name in used:
            name = raw + "_" + str(i)
            i += 1
        used.add(name)
        if name != raw:
            report.add_note("Property name collision in %s: '%s' -> '%s'." % (cls.py_name, p.name, name))
        if name in PY_KEYWORDS:
            report.add_warning("Property '%s' of %s escaped to '%s' (Python keyword)."
                               % (p.name, cls.py_name, name))

        type_name = _type_ref(model, p.type_id, report)
        if p.is_collection:
            out.append("    %s: List[%s] = field(default_factory=list)" % (name, type_name))
        else:
            out.append("    %s: Optional[%s] = None" % (name, type_name))


def emit(model: DexpiModel, source_file_name: str, sha256: str, report: GenerationReport) -> str:
    out = [
        "# <auto-generated>",
        "# DEXPI 2.0 UML model -> Python - deterministic conversion by dexpi2.gen (rule version %s)." % RULES_VERSION,
        "# Source: %s (SHA-256: %s)" % (source_file_name, sha256),
        "# Regenerate: python dexpi2/gen.py <input.xmi> <output.py>",
        "# Python keeps ALL generalization parents (multiple inheritance), unlike the C# single-",
        "# inheritance conversion; mixin attributes therefore remain reachable on the subclass.",
        "# </auto-generated>",
        "from __future__ import annotations",
        "",
        "import datetime",
        "from dataclasses import dataclass, field",
        "from enum import Enum",
        "from typing import List, Optional",
        "",
    ]

    enums = [t for t in model.types_by_id.values() if isinstance(t, DexpiEnum)]
    for en in enums:
        out.append("")
        out.append("class %s(Enum):" % en.py_name)
        out.append("    \"\"\"DEXPI 2.0 enumeration %s (XMI id %s).\"\"\"" % (en.name, en.id))
        used = set()
        for lit in en.literals:
            raw = escape(lit)
            name = raw
            i = 1
            while name in used:
                name = raw + "_" + str(i)
                i += 1
            used.add(name)
            out.append("    %s = %r" % (name, lit))
        out.append("    # value kept as the original literal name for XML round-tripping")

    for cls in _topo_order_classes(model, report):
        _emit_class(model, cls, report, out)

    return "\r\n".join(out) + "\r\n"


# ---------------------------------------------------------------------------
# Report + main
# ---------------------------------------------------------------------------


def write_report(path: str, report: GenerationReport, model: DexpiModel,
                 source_file_name: str, sha256: str, output_file: str):
    lines = [
        "# DEXPI 2.0 UML -> Python - Generation Report",
        "",
        "- Input: %s (SHA-256: %s)" % (source_file_name, sha256),
        "- Rule version: %s" % RULES_VERSION,
        "- Primitive types in model: %d" % model.primitive_type_count,
        "",
        "## Counts",
        "",
        "- Packages: %d" % sum(1 for p in model.packages if p.types),
        "- Classes: %d (abstract: %d)" % (report.class_count, report.abstract_count),
        "- Enumerations: %d" % report.enum_count,
        "- Properties: %d" % report.property_count,
        "- Associations: %d" % report.association_count,
        "- Classes with multiple inheritance: %d (mixin references total: %d)"
        % (report.mixin_class_count, report.mixin_total),
        "",
        "## Output",
        "",
        "- %s" % output_file,
        "",
    ]
    if report.notes:
        lines += ["## Notes", ""]
        lines += ["- " + n for n in report.notes]
        lines += [""]
    if report.skipped:
        lines += ["## Skipped", ""]
        lines += ["- " + s for s in report.skipped]
        lines += [""]
    if report.warnings:
        lines += ["## Warnings", ""]
        lines += ["- " + w for w in report.warnings]
        lines += [""]
    lines += [
        "## Mapping rules (deterministic)",
        "",
        "- Special-named DataTypes map to Python builtins: \"Undefined | String\"->str, \"Undefined | Integer\"->int, "
        "\"Undefined | Double\"->float, \"Undefined | Boolean\"->bool, \"Undefined | DateTime\"->datetime.datetime, "
        "\"Undefined | AnyURI\"->str, UnsignedByte->int.",
        "- \"PhysicalQuantity...\" special types map to the materialized classes PhysicalQuantity / PhysicalQuantityVector.",
        "- \"Undefined | X\" maps to type X (enum or class) resolved by name; first occurrence wins when ambiguous.",
        "- Plain-named DataTypes are materialized as classes.",
        "- ALL generalization parents become base classes (Python multiple inheritance); generalizations to "
        "enums/builtins are skipped.",
        "- \"QualifiedValue with Type=(T)\" binding variants become classes named QualifiedValueOf<T>.",
        "- Properties: every field defaults to None; collections default to an empty list "
        "(POCO style, DEXPI XML deserializer fills values).",
        "- Named reverse association ends become properties on the class at the opposite end; unnamed reverse ends are skipped.",
        "- Identifiers: invalid characters become '_'; Python keywords get a trailing '_'; name collisions get a "
        "deterministic numeric suffix; type names are globally unique across packages (single output module).",
        "- Output encoding: UTF-8 with BOM, CRLF line endings, trailing CRLF. No timestamps: output depends only on "
        "input bytes and rule version.",
    ]
    with open(path, "w", encoding="utf-8-sig", newline="") as f:
        f.write("\r\n".join(lines) + "\r\n")


def sha256_of(path: str) -> str:
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(65536), b""):
            h.update(chunk)
    return h.hexdigest()


def write_utf8bom(path: str, text: str):
    with open(path, "w", encoding="utf-8-sig", newline="") as f:
        f.write(text)


def main(argv=None):
    parser = argparse.ArgumentParser(description="DEXPI 2.0 XMI -> Python dataclass generator")
    parser.add_argument("input", help="Dexpi.xmi path")
    parser.add_argument("output", help="output .py path")
    parser.add_argument("report", nargs="?", default=None, help="optional markdown report path")
    args = parser.parse_args(argv)

    model = parse(args.input)
    report = GenerationReport()
    resolve_names(model, report)
    build_reverse_properties(model, report)
    sha = sha256_of(args.input)
    text = emit(model, args.input.replace("\\", "/").rsplit("/", 1)[-1], sha, report)
    write_utf8bom(args.output, text)
    report_path = args.report or (args.output.rsplit(".", 1)[0] + "-report.md")
    write_report(report_path, report, model, args.input.replace("\\", "/").rsplit("/", 1)[-1], sha, args.output)

    print("Input:      " + args.input)
    print("SHA-256:    " + sha)
    print("Classes:    %d (abstract %d)" % (report.class_count, report.abstract_count))
    print("Enums:      %d" % report.enum_count)
    print("Properties: %d" % report.property_count)
    print("Associations: %d" % report.association_count)
    print("Multiple inheritance classes: %d (mixin refs %d)" % (report.mixin_class_count, report.mixin_total))
    print("Warnings:   %d | Skipped: %d | Notes: %d" % (len(report.warnings), len(report.skipped), len(report.notes)))
    print("Output:     " + args.output)
    print("Report:     " + report_path)
    return 0


if __name__ == "__main__":
    sys.exit(main())
