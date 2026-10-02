// Dexpi2CSharpGenerator
//
// Regenerates the flattened C++ headers on the fly (Dexpi2CppFlatten module in
// Flatten.fs, restored from git history) from Dexpi2.Cpp/Generated/dexpi2.hpp
// and emits a flat (inheritance-free) C# class library, mirroring the C++
// flattening rules:
//   - no class inherits from anything; every class is self-contained
//   - abstract DEXPI classes are emitted as C# `abstract` classes
//   - cross-namespace references are fully qualified; same-namespace plain
//
// Usage: Dexpi2CSharpGenerator [inputHpp] [outDir]
//   inputHpp default: <repoRoot>/Dexpi2.Cpp/Generated/dexpi2.hpp
//   outDir   default: <repoRoot>/Dexpi2.CSharp

module Dexpi2CSharpGenerator

open System
open System.IO
open System.Text
open System.Text.RegularExpressions

// ---------------------------------------------------------------------------
// Model
// ---------------------------------------------------------------------------

type EnumDecl =
    { Ns: string list
      Name: string
      XmiId: string
      Members: string list }

type Member =
    { CppType: string
      Name: string
      Init: string option
      InheritedFrom: string option }

type ClassDecl =
    { Ns: string list
      Name: string
      XmiId: string
      Doc: string
      Abstract: bool
      Members: Member list }

type Decl =
    | Enum of EnumDecl
    | Class of ClassDecl

// ---------------------------------------------------------------------------
// Parsing
// ---------------------------------------------------------------------------

let private nsRe = Regex(@"^namespace\s+(\w+)\s*\{\s*$")
let private nsCloseRe = Regex(@"^\}\s*//\s*namespace\s+(\w+)\s*$")
let private enumRe =
    Regex(@"// DEXPI 2\.0 enumeration\s+(?<name>\w+)\s+\(XMI id (?<xmi>ID\d+)\)\s*\n\s*enum class\s+\w+\s*\{\s*(?<body>.*?)\s*\};",
          RegexOptions.Singleline)
let private classRe =
    // note: the UML comment name (doc) is NOT always the class name
    // (e.g. "QualifiedValue with Type=(...)" in dexpi2_auxiliaries.hpp).
    Regex(@"// DEXPI 2\.0 model class\s+(?<doc>.+?)\s*\(XMI id (?<xmi>ID\d+)\)(?<rest>[^\n]*)\n\s*class\s+(?<name>\w+)\s*\{\s*(?<body>.*?)\s*\};",
          RegexOptions.Singleline)
let private enumMemberRe = Regex(@"^\s*(\w+)\s*,?\s*(//.*)?\s*$")
let private inheritedRe = Regex(@"^\s*//\s*inherited from\s+(.+?)\s*$")
let private ownMembersRe = Regex(@"^\s*//\s*own members\s*$")
let private memberRe = Regex(@"^\s*(\S+)\s+([A-Za-z_]\w*)\s*(?:=\s*([^;]+))?\s*;\s*$")

/// Parse one flattened hpp file into a declaration list (in source order).
let parseFile (path: string) : Decl list =
    let text = File.ReadAllText(path) // BOM auto-detected
    let textN = text.Replace("\r\n", "\n").Replace('\r', '\n')
    let lines = textN.Split('\n')
    let n = lines.Length

    // namespace path for every line index
    let nsAtLine = Array.create n []
    let mutable stack: string list = []
    for li in 0 .. n - 1 do
        nsAtLine.[li] <- List.rev stack // stack holds [innermost; ...; dexpi2]
        let line = lines.[li]
        let m = nsRe.Match(line)
        if m.Success then stack <- m.Groups.[1].Value :: stack
        else
            let m2 = nsCloseRe.Match(line)
            if m2.Success && not stack.IsEmpty then stack <- stack.Tail

    let startLine (m: Match) =
        textN.Substring(0, m.Index).Split('\n').Length - 1

    let decls = ResizeArray<Decl>()

    for m in enumRe.Matches(textN) do
        let body = m.Groups.["body"].Value
        let members =
            body.Split('\n')
            |> Array.choose (fun l ->
                let mm = enumMemberRe.Match(l)
                if mm.Success then Some mm.Groups.[1].Value else None)
            |> Array.toList
        decls.Add(
            Enum
                { Ns = nsAtLine.[startLine m]
                  Name = m.Groups.["name"].Value
                  XmiId = m.Groups.["xmi"].Value
                  Members = members })

    for m in classRe.Matches(textN) do
        let body = m.Groups.["body"].Value
        let abstract_ = m.Groups.["rest"].Value.Contains("[abstract in DEXPI]")
        let members = ResizeArray<Member>()
        let mutable lastInherited: string option = None
        for l in body.Split('\n') do
            let im = inheritedRe.Match(l)
            if im.Success then lastInherited <- Some im.Groups.[1].Value
            else
                let om = ownMembersRe.Match(l)
                if om.Success then lastInherited <- None
                else
                    let mm = memberRe.Match(l)
                    if mm.Success && not (l.Contains("virtual ~")) && not (l.Contains("() = default")) then
                        let init =
                            if mm.Groups.[3].Success then Some (mm.Groups.[3].Value.Trim())
                            else None
                        members.Add(
                            { CppType = mm.Groups.[1].Value
                              Name = mm.Groups.[2].Value
                              Init = init
                              InheritedFrom = lastInherited })
        decls.Add(
            Class
                { Ns = nsAtLine.[startLine m]
                  Name = m.Groups.["name"].Value
                  XmiId = m.Groups.["xmi"].Value
                  Doc = m.Groups.["doc"].Value
                  Abstract = abstract_
                  Members = List.ofSeq members })

    List.ofSeq decls

// ---------------------------------------------------------------------------
// C# type mapping
// ---------------------------------------------------------------------------

let capitalize (s: string) =
    if s.Length = 0 then s else string (Char.ToUpperInvariant s.[0]) + s.Substring(1)

let csNamespace (path: string list) =
    let segments = path |> List.filter (fun s -> s <> "dexpi2") |> List.map capitalize
    if segments.IsEmpty then "Dexpi2" else "Dexpi2." + String.Join(".", segments)

/// Translate a C++ type token to C# (nullable annotations enabled).
let rec mapType (currentNs: string list) (cppType: string) : string =
    let qualifiedToCs (s: string) =
        let parts = s.Split([| "::" |], StringSplitOptions.RemoveEmptyEntries) |> Array.toList
        let name = List.last parts
        let ns = parts |> List.tail |> List.rev |> List.tail |> List.rev // drop "dexpi2" and Name
        let csNs = csNamespace ns
        if csNs = csNamespace currentNs then name else csNs + "." + name

    let innerOf (prefix: string) (t: string) =
        t.Substring(prefix.Length, t.Length - prefix.Length - 1) // strip "prefix<...>"

    if cppType = "std::string" then "string"
    elif cppType = "std::vector<std::string>" then "List<string>"
    elif cppType = "std::vector<double>" then "List<double>"
    elif cppType = "std::optional<std::string>" then "string?"
    elif cppType = "std::optional<int>" then "int?"
    elif cppType.StartsWith("std::shared_ptr<") && cppType.EndsWith(">") then
        mapType currentNs (innerOf "std::shared_ptr<" cppType) + "?"
    elif cppType.StartsWith("std::optional<") && cppType.EndsWith(">") then
        mapType currentNs (innerOf "std::optional<" cppType) + "?"
    elif cppType.StartsWith("std::vector<") && cppType.EndsWith(">") then
        let inner = innerOf "std::vector<" cppType
        // A vector element wrapped in shared_ptr is a mandatory member of the
        // collection. In UML, 0..* multiplicity is expressed by an empty list,
        // never by a null element, so the element type must not be nullable.
        let elementType =
            if inner.StartsWith("std::shared_ptr<") && inner.EndsWith(">") then
                mapType currentNs (innerOf "std::shared_ptr<" inner)
            else
                mapType currentNs inner
        "List<" + elementType + ">"
    elif cppType.Contains("::") then qualifiedToCs cppType
    else cppType

/// Default initializer for the C# property, mirroring the C++ member defaults.
let defaultInit (csType: string) : string =
    if csType = "string" then "\"\""
    elif csType = "int" then "0"
    elif csType = "double" then "0.0"
    elif csType = "bool" then "false"
    elif csType.StartsWith("List<") then "new()"
    elif csType.EndsWith("?") then "" // nullable: no initializer (null default)
    else "default" // enum

// ---------------------------------------------------------------------------
// Emission
// ---------------------------------------------------------------------------

let emitEnum (b: StringBuilder) (e: EnumDecl) =
    let append (s: string) = b.Append(s).Append("\r\n") |> ignore
    append (sprintf "    /// <summary>DEXPI 2.0 enumeration %s (XMI id %s)</summary>" e.Name e.XmiId)
    append (sprintf "    public enum %s" e.Name)
    append "    {"
    for m in e.Members do
        append (sprintf "        %s," m)
    append "    }"
    append ""

let emitClass (b: StringBuilder) (c: ClassDecl) =
    let append (s: string) = b.Append(s).Append("\r\n") |> ignore
    let abstractMark = if c.Abstract then "abstract " else ""
    let absDoc = if c.Abstract then " [abstract in DEXPI]" else ""
    let docExtra = if c.Doc = c.Name then "" else sprintf "; source comment: %s" c.Doc
    append (sprintf "    /// <summary>DEXPI 2.0 model class %s (XMI id %s)%s%s</summary>" c.Name c.XmiId absDoc docExtra)
    append (sprintf "    public %sclass %s" abstractMark c.Name)
    append "    {"
    let mutable lastInherited = ""
    for m in c.Members do
        let inh = defaultArg m.InheritedFrom ""
        if inh <> lastInherited && inh <> "" then
            append (sprintf "        // inherited from %s" inh)
            lastInherited <- inh
        let csType = mapType c.Ns m.CppType
        let init = defaultInit csType
        let initPart = if init = "" then "" else " = " + init + ";"
        append (sprintf "        public %s %s { get; set; }%s" csType m.Name initPart)
    append "    }"
    append ""

let emitFile (b: StringBuilder) (sourceHeader: string) (decls: Decl list) =
    let append (s: string) = b.Append(s).Append("\r\n") |> ignore
    append "// <auto-generated>"
    append "// DEXPI 2.0 UML model -> C# (flat, inheritance-free) conversion by Dexpi2CSharpGenerator."
    append (sprintf "// Source: %s (flattened C++ headers, snapshot under Dexpi2.CSharp.Generator/input)." sourceHeader)
    append "// Flat hierarchy: no class inherits from anything; every class is self-contained."
    append "// Abstract DEXPI classes are emitted as C# 'abstract' classes (not directly instantiable)."
    append "// Regenerate with: dotnet run --project Dexpi2.CSharp.Generator"
    append "// </auto-generated>"
    append ""
    append "using System;"
    append "using System.Collections.Generic;"
    append ""
    append "#nullable enable"
    append ""
    let groups = decls |> List.groupBy (fun d -> match d with Enum e -> e.Ns | Class c -> c.Ns)
    for (ns, ds) in groups do
        append (sprintf "namespace %s" (csNamespace ns))
        append "{"
        for d in ds do
            match d with
            | Enum e -> emitEnum b e
            | Class c -> emitClass b c
        append "}"
        append ""

// ---------------------------------------------------------------------------
// Validation helpers
// ---------------------------------------------------------------------------

let csharpKeywords =
    set [ "abstract"; "as"; "base"; "bool"; "break"; "byte"; "case"; "catch"; "char"; "checked";
          "class"; "const"; "continue"; "decimal"; "default"; "delegate"; "do"; "double"; "else";
          "enum"; "event"; "explicit"; "extern"; "false"; "finally"; "fixed"; "float"; "for";
          "foreach"; "goto"; "if"; "implicit"; "in"; "int"; "interface"; "internal"; "is"; "lock";
          "long"; "namespace"; "new"; "null"; "object"; "operator"; "out"; "override"; "params";
          "private"; "protected"; "public"; "readonly"; "ref"; "return"; "sbyte"; "sealed"; "short";
          "sizeof"; "stackalloc"; "static"; "string"; "struct"; "switch"; "this"; "throw"; "true";
          "try"; "typeof"; "uint"; "ulong"; "unchecked"; "unsafe"; "ushort"; "using"; "virtual";
          "void"; "volatile"; "while" ]

let validate (decls: Decl list) : string list =
    let issues = ResizeArray<string>()
    let classCount = ref 0
    let abstractCount = ref 0
    let enumCount = ref 0

    for d in decls do
        match d with
        | Enum e ->
            incr enumCount
            let seen = System.Collections.Generic.HashSet<string>()
            for m in e.Members do
                if not (seen.Add m) then
                    issues.Add(sprintf "enum %s: duplicate member '%s'" e.Name m)
                if csharpKeywords.Contains m then
                    issues.Add(sprintf "enum %s: member '%s' is a C# keyword" e.Name m)
        | Class c ->
            incr classCount
            if c.Abstract then incr abstractCount
            if csharpKeywords.Contains c.Name then
                issues.Add(sprintf "class '%s': name is a C# keyword" c.Name)
            let seen = System.Collections.Generic.HashSet<string>()
            for m in c.Members do
                if not (seen.Add m.Name) then
                    issues.Add(sprintf "class %s: duplicate member '%s'" c.Name m.Name)
                if m.Name = c.Name then
                    issues.Add(sprintf "class %s: member name equals class name (CS0542)" c.Name)
                if csharpKeywords.Contains m.Name then
                    issues.Add(sprintf "class %s: member '%s' is a C# keyword" c.Name m.Name)

    // duplicate class names within one namespace
    let nsClassNames =
        decls
        |> List.choose (fun d -> match d with Class c -> Some (c.Ns, c.Name) | _ -> None)
        |> List.groupBy id
    for ((ns, name), g) in nsClassNames do
        if g.Length > 1 then
            issues.Add(sprintf "class %s appears %d times in namespace %s" name g.Length (csNamespace ns))

    printfn "Parsed: %d classes (%d abstract), %d enums" !classCount !abstractCount !enumCount
    List.ofSeq issues

// ---------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------

let writeUtf8BomCrlf (path: string) (content: string) =
    let dir = Path.GetDirectoryName(path)
    if not (Directory.Exists dir) then Directory.CreateDirectory(dir) |> ignore
    File.WriteAllText(path, content, UTF8Encoding(true))

[<EntryPoint>]
let main argv =
    let repoRoot =
        let di = DirectoryInfo(__SOURCE_DIRECTORY__)
        di.Parent.FullName

    let inputHpp =
        if argv.Length > 0 then argv.[0]
        else Path.Combine(repoRoot, "Dexpi2.Cpp", "Generated", "dexpi2.hpp")

    let outDir =
        if argv.Length > 1 then argv.[1]
        else Path.Combine(repoRoot, "Dexpi2.CSharp")

    // Regenerate the flattened C++ headers from dexpi2.hpp (no input/ snapshot).
    let flattenDir = Path.Combine(__SOURCE_DIRECTORY__, "obj", "flatten-hpp")
    if Directory.Exists flattenDir then Directory.Delete(flattenDir, true)
    let flattenCode = Dexpi2CppFlatten.flattenAll inputHpp flattenDir
    if flattenCode <> 0 then failwithf "Dexpi2CppFlatten failed with exit code %d" flattenCode
    let inDir = flattenDir

    let headers =
        [ "dexpi2_core.hpp", "Dexpi2.Core.cs"
          "dexpi2_auxiliaries.hpp", "Dexpi2.Auxiliaries.cs"
          "dexpi2_plant.hpp", "Dexpi2.Plant.cs"
          "dexpi2_process.hpp", "Dexpi2.Process.cs" ]

    printfn "Input : %s" inDir
    printfn "Output: %s" outDir

    let allIssues = ResizeArray<string>()
    for (hpp, cs) in headers do
        let src = Path.Combine(inDir, hpp)
        if not (File.Exists src) then failwithf "missing input: %s" src
        printfn "Parsing %s ..." hpp
        let decls = parseFile src
        let issues = validate decls
        for i in issues do allIssues.Add(sprintf "%s: %s" hpp i)
        let b = StringBuilder()
        emitFile b hpp decls
        writeUtf8BomCrlf (Path.Combine(outDir, cs)) (b.ToString())
        printfn "  wrote %s" cs

    // csproj
    let csproj =
        """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <RootNamespace>Dexpi2</RootNamespace>
    <AssemblyName>Dexpi2.CSharp</AssemblyName>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);CS1591</NoWarn>
  </PropertyGroup>
</Project>
"""
    writeUtf8BomCrlf (Path.Combine(outDir, "Dexpi2.CSharp.csproj")) (csproj.Replace("\r\n", "\n").Replace("\n", "\r\n"))
    printfn "  wrote Dexpi2.CSharp.csproj"

    if allIssues.Count = 0 then
        printfn "Validation: OK (no collisions)"
    else
        printfn "Validation issues:"
        for i in allIssues do printfn "  - %s" i
    0
