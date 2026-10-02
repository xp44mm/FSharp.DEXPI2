// Dexpi2CppFlatten - deterministic C++ flattening transformer.
//
// Input : the single generated header Dexpi2.Cpp/Generated/dexpi2.hpp (Dexpi2CppGen rule 1.0).
// Output: a flat project where EVERY base class (abstract or concrete) is fully inlined into
//         each derived class, so the class hierarchy disappears and every class is self-contained.
//         Abstract classes are kept as reference types (shared_ptr members may point at them),
//         still marked [abstract in DEXPI] with a protected default constructor.
//         One header per top-level namespace (auxiliaries / core / plant / process) plus an
//         aggregate dexpi2.hpp.
//
// Deterministic: output depends only on input bytes and rule version. UTF-8 BOM, CRLF,
// trailing CRLF, no timestamps.

module Dexpi2CppFlatten

open System
open System.Collections.Generic
open System.IO
open System.Security.Cryptography
open System.Text
open System.Text.RegularExpressions

let RuleVersion = "1.0"

// ---------------------------------------------------------------------------
// Model
// ---------------------------------------------------------------------------

type EnumDecl =
    { CppName: string
      ModelName: string
      XmiId: string
      Literals: (string * string) list
      NsPath: string list } // segments below dexpi2, e.g. ["core"; "datatypes"]

type ClassDecl =
    { CppName: string
      ModelName: string
      XmiId: string
      IsAbstract: bool
      Bases: string list // base CppNames (last ::-segment, resolved)
      Members: string list // data member declarations, no trailing ';'
      NsPath: string list }

type Model =
    { Classes: ClassDecl list
      Enums: EnumDecl list
      ClassesByName: Map<string, ClassDecl>
      TypeNs: Map<string, string list> } // C++ type name -> namespace path (classes + enums)

// ---------------------------------------------------------------------------
// Parser (line based; the generated file has a strict, verified layout)
// ---------------------------------------------------------------------------

let parse (lines: string array) : Model =
    let mutable nsStack: string list = []
    let mutable classes: ClassDecl list = []
    let mutable enums: EnumDecl list = []
    let mutable curClass: ClassDecl option = None
    let mutable curEnum: EnumDecl option = None
    let mutable pendingClass: (string * string * bool) option = None
    let mutable pendingEnum: (string * string) option = None

    let reClassComment =
        Regex(@"^    // DEXPI 2.0 model class (.+?) \(XMI id ([A-Za-z0-9]+)\)( \[abstract in DEXPI\])?$")

    let reEnumComment =
        Regex(@"^    // DEXPI 2.0 enumeration (.+?) \(XMI id ([A-Za-z0-9]+)\)$")

    let reNsOpen = Regex(@"^namespace (\w+) \{$")
    let reNsClose = Regex(@"^\} // namespace (\w+)$")
    let reClassDef = Regex(@"^    class ([A-Za-z_]\w*)(.*)$")
    let reEnumDef = Regex(@"^    enum class ([A-Za-z_]\w*)$")
    let reMember = Regex(@"^        (.+);\s*$")
    let reLiteral = Regex(@"^        ([A-Za-z_]\w*),  // (.*)$")

    let parseBases (rest: string) : string list =
        let t = rest.TrimStart()
        if not (t.StartsWith(":")) then []
        else
            t.Substring(1).Split(',')
            |> Array.toList
            |> List.map (fun s ->
                let s = s.Trim()
                let s = if s.StartsWith("public ") then s.Substring("public ".Length).Trim() else s
                if s.Contains("::") then s.Substring(s.LastIndexOf("::") + 2) else s)

    for raw in lines do
        let l = raw.TrimEnd('\r')
        match curClass with
        | Some c ->
            if l = "    };" then
                classes <- c :: classes
                curClass <- None
            else
                let mm = reMember.Match(l)
                if mm.Success then
                    let decl = mm.Groups.[1].Value.Trim()
                    // data members never contain '(' in this model; dtor/ctor do
                    if not (decl.Contains("(")) && not (decl.StartsWith("~")) then
                        curClass <- Some { c with Members = c.Members @ [ decl ] }
        | None ->
            match curEnum with
            | Some e ->
                if l = "    };" then
                    enums <- e :: enums
                    curEnum <- None
                else
                    let ml = reLiteral.Match(l)
                    if ml.Success then
                        curEnum <-
                            Some
                                { e with Literals = e.Literals @ [ (ml.Groups.[1].Value, ml.Groups.[2].Value) ] }
            | None ->
                let mc = reClassComment.Match(l)
                if mc.Success then
                    pendingClass <-
                        Some(mc.Groups.[1].Value, mc.Groups.[2].Value, mc.Groups.[3].Success)
                else
                    let me = reEnumComment.Match(l)
                    if me.Success then
                        pendingEnum <- Some(me.Groups.[1].Value, me.Groups.[2].Value)
                    else
                        let no = reNsOpen.Match(l)
                        if no.Success then nsStack <- no.Groups.[1].Value :: nsStack
                        else
                            let nc = reNsClose.Match(l)
                            if nc.Success then
                                if nc.Groups.[1].Value <> nsStack.Head then
                                    failwithf "namespace mismatch: closing %s but top is %s" nc.Groups.[1].Value nsStack.Head
                                nsStack <- List.tail nsStack
                            else
                                let ne = reEnumDef.Match(l)
                                if ne.Success then
                                    let (mn, xid) = pendingEnum |> Option.defaultValue ("", "")
                                    curEnum <-
                                        Some
                                            { CppName = ne.Groups.[1].Value
                                              ModelName = mn
                                              XmiId = xid
                                              Literals = []
                                              NsPath = List.rev nsStack |> List.tail }
                                    pendingEnum <- None
                                else
                                    let nd = reClassDef.Match(l)
                                    if nd.Success && nd.Groups.[2].Value.Trim() <> ";" then
                                        let (mn, xid, abs) = pendingClass |> Option.defaultValue ("", "", false)
                                        curClass <-
                                            Some
                                                { CppName = nd.Groups.[1].Value
                                                  ModelName = mn
                                                  XmiId = xid
                                                  IsAbstract = abs
                                                  Bases = parseBases nd.Groups.[2].Value
                                                  Members = []
                                                  NsPath = List.rev nsStack |> List.tail }
                                        pendingClass <- None

    if not (List.isEmpty nsStack) then
        failwithf "unbalanced namespaces, leftover: %A" nsStack
    if curClass.IsSome then failwith "unterminated class body"
    if curEnum.IsSome then failwith "unterminated enum body"

    let classes = List.rev classes
    let enums = List.rev enums
    let classesByName = classes |> List.map (fun c -> c.CppName, c) |> Map.ofList
    let typeNs =
        (classes |> List.map (fun c -> c.CppName, c.NsPath))
        @ (enums |> List.map (fun e -> e.CppName, e.NsPath))
        |> Map.ofList
    { Classes = classes; Enums = enums; ClassesByName = classesByName; TypeNs = typeNs }

// ---------------------------------------------------------------------------
// Member helpers
// ---------------------------------------------------------------------------

/// Split "type Name[ = init]" into (typePart, name, initPart).
let memberInfo (decl: string) : string * string * string =
    let initMatch = Regex.Match(decl, @"\s*=\s*(.*)$")
    let (baseDecl, initPart) =
        if initMatch.Success then
            (decl.Substring(0, initMatch.Index), " = " + initMatch.Groups.[1].Value)
        else
            (decl, "")
    let trimmed = baseDecl.TrimEnd()
    let idx = trimmed.LastIndexOf(' ')
    if idx < 0 then (trimmed, "", initPart)
    else (trimmed.Substring(0, idx).Trim(), trimmed.Substring(idx + 1), initPart)

let memberNameOf (decl: string) : string =
    let (_, name, _) = memberInfo decl
    name

/// Re-qualify every standalone model-type identifier in a member declaration's TYPE part
/// against its true namespace. Needed when a base member from another namespace is inlined
/// into a derived class (unqualified names would otherwise resolve in the derived scope).
/// Type names are globally unique, so qualification is unambiguous.
let qualifyMember (typeNs: Map<string, string list>) (fromNs: string list) (toNs: string list) (decl: string) : string =
    if fromNs = toNs then decl
    else
        let (typePart, name, initPart) = memberInfo decl
        let mutable result = typePart
        for KeyValue (tname, tns) in typeNs do
            let re = Regex("(?<![A-Za-z0-9_:])" + Regex.Escape(tname) + "(?![A-Za-z0-9_])")
            result <- re.Replace(result, "dexpi2::" + String.Join("::", tns) + "::" + tname)
        result + " " + name + initPart

// ---------------------------------------------------------------------------
// Flattening
// ---------------------------------------------------------------------------

/// Flattened inherited members of class c, grouped by the declaring base class.
/// Returns (keptInheritedGroups, ownMembers, shadowedDropped, dedupedSameType) where
/// shadowedDropped lists inherited members whose name is shadowed by the class's own member
/// (C++ shadowing semantics: the most-derived member wins; both cannot exist in one flat class),
/// and dedupedSameType counts inherited duplicates removed because the same name was already
/// inherited (diamond inheritance / repeated mixins).
let flattenClass (m: Model) (c: ClassDecl) : (ClassDecl * string list) list * string list * (string * string) list * int =
    let visited = HashSet<string>()
    let rec collect (name: string) : (ClassDecl * string) list =
        let cls = m.ClassesByName.[name]
        let fromBases =
            [ for b in cls.Bases do
                if visited.Add(b) then yield! collect b ]
        let own = [ for mem in cls.Members do yield (cls, mem) ]
        fromBases @ own

    let all = collect c.CppName
    let seenNames = HashSet<string>()
    let groups = ResizeArray<ClassDecl * string list>()
    let groupIndex = Dictionary<string, int>()
    let mutable dedupedSameType = 0
    for (src, mem) in all do
        if src.CppName <> c.CppName then
            if seenNames.Add(memberNameOf mem) then
                match groupIndex.TryGetValue(src.CppName) with
                | true, i -> groups.[i] <- (src, (snd groups.[i]) @ [ mem ])
                | false, _ ->
                    groupIndex.[src.CppName] <- groups.Count
                    groups.Add(src, [ mem ])
            else
                dedupedSameType <- dedupedSameType + 1

    let ownNames = c.Members |> List.map memberNameOf |> HashSet
    let dropped =
        [ for (src, mems) in groups do
            for mem in mems do
                if ownNames.Contains(memberNameOf mem) then yield (src.CppName, mem) ]
    let keptGroups =
        [ for (src, mems) in groups do
            yield (src, List.filter (fun mem -> not (ownNames.Contains(memberNameOf mem))) mems) ]
    (keptGroups, c.Members, dropped, dedupedSameType)

// ---------------------------------------------------------------------------
// Emission helpers
// ---------------------------------------------------------------------------

let topOf (nsPath: string list) : string =
    match nsPath with
    | top :: _ -> top
    | [] -> "dexpi2"

let emitEnum (e: EnumDecl) : string list =
    [ "    // DEXPI 2.0 enumeration " + e.ModelName + " (XMI id " + e.XmiId + ")"
      "    enum class " + e.CppName
      "    {" ]
    @ (e.Literals
       |> List.map (fun (lit, orig) -> "        " + lit + ",  // " + orig))
    @ [ "    };" ]

let emitClass (m: Model) (c: ClassDecl) : string list =
    let (groups, own, _, _) = flattenClass m c
    let absMark = if c.IsAbstract then " [abstract in DEXPI]" else ""
    let flatMark =
        if c.Bases.IsEmpty then ""
        else " [flattened; bases inlined: " + String.Join(", ", c.Bases) + "]"

    let inheritedLines =
        groups
        |> List.collect (fun (src, mems) ->
            [ "        // inherited from " + src.CppName + " (XMI id " + src.XmiId + ")" ]
            @ List.map (fun mem -> "        " + qualifyMember m.TypeNs src.NsPath c.NsPath mem + ";") mems)

    let ownLines = List.map (fun mem -> "        " + mem + ";") own

    let ctorLines =
        if c.IsAbstract then
            [ "    protected:"
              "        " + c.CppName + "() = default;  // abstract in DEXPI: not directly instantiable" ]
        else
            [ "        " + c.CppName + "() = default;" ]

    [ "    // DEXPI 2.0 model class " + c.ModelName + " (XMI id " + c.XmiId + ")" + absMark + flatMark
      "    class " + c.CppName
      "    {"
      "    public:"
      "        virtual ~" + c.CppName + "() = default;" ]
    @ inheritedLines
    @ ownLines
    @ [ "" ]
    @ ctorLines
    @ [ "    };" ]

/// Open namespace blocks for a path (below dexpi2): starts with a blank separator line.
let openNs (nsPath: string list) : string list =
    [ "" ] @ (nsPath |> List.map (fun s -> "namespace " + s + " {"))

let closeNs (nsPath: string list) : string list =
    nsPath |> List.rev |> List.map (fun s -> "} // namespace " + s)

/// Group items by their namespace path, preserving first-appearance order of the path.
let groupByNs (items: ('a * string list) list) : (string list * 'a list) list =
    let order = ResizeArray<string list>()
    let acc = Dictionary<string list, ResizeArray<'a>>()
    for (item, p) in items do
        if not (acc.ContainsKey(p)) then
            order.Add(p)
            acc.[p] <- ResizeArray()
        acc.[p].Add(item)
    [ for p in order -> p, List.ofSeq acc.[p] ]

// ---------------------------------------------------------------------------
// Stats
// ---------------------------------------------------------------------------

type FlattenStats =
    { TotalClasses: int
      AbstractClasses: int
      TotalEnums: int
      InlinedInstances: int
      ShadowedDropped: int
      DedupedSameType: int
      Requalified: int }

let emptyStats (m: Model) : FlattenStats =
    { TotalClasses = m.Classes.Length
      AbstractClasses = m.Classes |> List.filter (fun c -> c.IsAbstract) |> List.length
      TotalEnums = m.Enums.Length
      InlinedInstances = 0
      ShadowedDropped = 0
      DedupedSameType = 0
      Requalified = 0 }

// ---------------------------------------------------------------------------
// File emission
// ---------------------------------------------------------------------------

/// All top-level namespaces referenced by classes of `top` via fully qualified member types.
let depsOf (m: Model) (top: string) : string list =
    let tops = HashSet<string>()
    for c in m.Classes |> List.filter (fun c -> topOf c.NsPath = top) do
        let (groups, own, _, _) = flattenClass m c
        let mems = own @ (groups |> List.collect (fun (_, ms) -> ms))
        for mem in mems do
            for mm in Regex.Matches(mem, "dexpi2::([a-z_]+)::") do
                let t = mm.Groups.[1].Value
                if t <> top then tops.Add(t) |> ignore
    tops |> Seq.toList |> List.sort

/// Emit the body (namespaces, enums, classes) of one top-level-namespace file.
let buildFileBody (m: Model) (top: string) (stats: FlattenStats ref) : string list =
    let classes = m.Classes |> List.filter (fun c -> topOf c.NsPath = top)
    let enums = m.Enums |> List.filter (fun e -> topOf e.NsPath = top)

    // --- forward declarations, grouped by namespace path, in document order ---
    let fwdLines =
        [ "// Forward declarations for every model class in this file (std::shared_ptr members accept incomplete types)."
          "namespace dexpi2 {" ]
        @ (classes
           |> List.map (fun c -> c, c.NsPath)
           |> groupByNs
           |> List.collect (fun (p, cs) ->
               (openNs p |> List.tail) // drop the leading blank separator inside the outer block
               @ (cs |> List.map (fun c -> "    class " + c.CppName + ";"))
               @ closeNs p))
        @ [ "} // namespace dexpi2"; "" ]

    // --- enums first (classes may reference enums of the same file by value) ---
    let enumLines =
        enums
        |> List.map (fun e -> e, e.NsPath)
        |> groupByNs
        |> List.collect (fun (p, es) -> (openNs p) @ (es |> List.collect emitEnum) @ closeNs p)

    // --- classes, in document order ---
    let classLines =
        classes
        |> List.map (fun c -> c, c.NsPath)
        |> groupByNs
        |> List.collect (fun (p, cs) ->
            (openNs p)
            @ (cs
               |> List.collect (fun c ->
                   let (groups, _, dropped, deduped) = flattenClass m c
                   stats.Value <-
                       { stats.Value with
                           InlinedInstances =
                               stats.Value.InlinedInstances + (groups |> List.sumBy (fun (_, ms) -> ms.Length))
                           ShadowedDropped = stats.Value.ShadowedDropped + dropped.Length
                           DedupedSameType = stats.Value.DedupedSameType + deduped
                           Requalified =
                               stats.Value.Requalified
                               + (groups
                                  |> List.sumBy (fun (src, ms) -> if src.NsPath = c.NsPath then 0 else ms.Length)) }
                   emitClass m c))
            @ closeNs p)

    fwdLines
    @ [ "namespace dexpi2 {" ]
    @ enumLines
    @ classLines
    @ [ "} // namespace dexpi2" ]

let sha256 (path: string) : string =
    use sha = SHA256.Create()
    let bytes = File.ReadAllBytes(path)
    Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant()

let writeText (path: string) (text: string) =
    let dir =
        match Path.GetDirectoryName(path) with
        | null -> ""
        | d -> d
    if not (String.IsNullOrEmpty(dir)) then
        Directory.CreateDirectory(dir) |> ignore
    File.WriteAllText(path, text, UTF8Encoding(true))

// ---------------------------------------------------------------------------
// Verification
// ---------------------------------------------------------------------------

/// Verify the model invariant that the flattened output preserves exactly the
/// reachable member set of every class (own + all ancestors, most-derived wins).
let verifyModel (m: Model) : (string * string) list =
    let problems = ResizeArray<string * string>()

    let rec ancestorNames (name: string) (visited: HashSet<string>) : string list =
        let cls = m.ClassesByName.[name]
        let fromBases =
            [ for b in cls.Bases do
                if visited.Add(b) then yield! ancestorNames b visited ]
        fromBases @ (cls.Members |> List.map memberNameOf)

    for c in m.Classes do
        let visited = HashSet<string>()
        let names = ancestorNames c.CppName visited
        let ownNames = c.Members |> List.map memberNameOf |> HashSet
        // most-derived wins: drop inherited names shadowed by own
        let expected =
            (names |> List.filter (fun n -> not (ownNames.Contains(n))) |> Set.ofList)
            + (ownNames |> Set.ofSeq)

        let (groups, own, _, _) = flattenClass m c
        let actual =
            ((groups |> List.collect (fun (_, ms) -> ms |> List.map memberNameOf))
             @ (own |> List.map memberNameOf))
            |> Set.ofList

        if actual <> expected then
            problems.Add(
                c.CppName,
                sprintf
                    "flattened member set mismatch:\n  expected = %A\n  actual   = %A"
                    (expected |> Set.toList)
                    (actual |> Set.toList)
            )

        // duplicate member names within one flat class?
        let dupes =
            (groups |> List.collect (fun (_, ms) -> ms |> List.map memberNameOf))
            @ (own |> List.map memberNameOf)
            |> List.groupBy id
            |> List.filter (fun (_, xs) -> xs.Length > 1)
        if not (List.isEmpty dupes) then
            problems.Add(c.CppName, sprintf "duplicate member names after flattening: %A" (dupes |> List.map fst))

    List.ofSeq problems

/// Re-parse each emitted file and verify it reproduces the expected per-file model:
/// class/enum counts, zero bases, unique member names per class.
let verifyOutputs (m: Model) (written: ResizeArray<string>) : (string * string) list =
    let problems = ResizeArray<string * string>()
    for path in written do
        let fileName = Path.GetFileName(path)
        let top =
            if fileName.StartsWith("dexpi2_") && fileName.EndsWith(".hpp") then
                fileName.Substring("dexpi2_".Length, fileName.Length - "dexpi2_".Length - 4)
            else
                ""
        let text = File.ReadAllText(path, Encoding.UTF8)
        try
            let m2 = parse (text.Split([| "\r\n" |], StringSplitOptions.None))
            let expectedClasses = m.Classes |> List.filter (fun c -> topOf c.NsPath = top) |> List.length
            let expectedEnums = m.Enums |> List.filter (fun e -> topOf e.NsPath = top) |> List.length
            if m2.Classes.Length <> expectedClasses then
                problems.Add(fileName, sprintf "class count %d != expected %d" m2.Classes.Length expectedClasses)
            if m2.Enums.Length <> expectedEnums then
                problems.Add(fileName, sprintf "enum count %d != expected %d" m2.Enums.Length expectedEnums)
            for c in m2.Classes do
                if not (List.isEmpty c.Bases) then
                    problems.Add(fileName, sprintf "class %s still has bases: %A" c.CppName c.Bases)
                let dupes =
                    c.Members
                    |> List.groupBy memberNameOf
                    |> List.filter (fun (_, xs) -> xs.Length > 1)
                if not (List.isEmpty dupes) then
                    problems.Add(fileName, sprintf "class %s duplicate members: %A" c.CppName (dupes |> List.map fst))
        with ex ->
            problems.Add(fileName, sprintf "re-parse failed: %s" ex.Message)
    List.ofSeq problems

// ---------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------

let topNamespaces = [ "core"; "auxiliaries"; "plant"; "process" ]

[<EntryPoint>]
let main argv =
    if argv.Length < 2 then
        eprintfn "usage: Dexpi2CppFlatten <input.hpp> <outputDir>"
        2
    else
        let inputPath = Path.GetFullPath(argv.[0])
        let outputDir = Path.GetFullPath(argv.[1])
        if not (File.Exists(inputPath)) then
            eprintfn "input not found: %s" inputPath
            2
        else
            let lines = File.ReadAllLines(inputPath, Encoding.UTF8)
            let m = parse lines
            let sha = sha256 inputPath
            let sourceName = Path.GetFileName(inputPath)

            printfn "parsed: %d classes (%d abstract), %d enums" m.Classes.Length
                (m.Classes |> List.filter (fun c -> c.IsAbstract) |> List.length) m.Enums.Length

            // model-level verification
            let problems = verifyModel m
            if not (List.isEmpty problems) then
                for (name, msg) in problems do
                    eprintfn "VERIFY FAIL %s: %s" name msg
                eprintfn "aborting: model verification failed"
                2
            else
                printfn "model verification OK (member sets match for all %d classes)" m.Classes.Length

                Directory.CreateDirectory(outputDir) |> ignore

                let stats = ref (emptyStats m)

                // topological order of aggregate include (deps first)
                let rec topoOrder (remaining: string list) (acc: string list) : string list =
                    if List.isEmpty remaining then List.rev acc
                    else
                        let ready =
                            remaining
                            |> List.filter (fun t ->
                                let deps = depsOf m t
                                deps |> List.forall (fun d -> not (List.contains d remaining)))
                        if List.isEmpty ready then
                            failwithf "dependency cycle among top-level namespaces: %A" remaining
                        let t = List.head ready
                        topoOrder (remaining |> List.filter (fun x -> x <> t)) (t :: acc)

                let topo = topoOrder topNamespaces []

                let written = ResizeArray<string>()
                for t in topo do
                    let depFilesForT =
                        depsOf m t
                        |> List.map (fun d -> "dexpi2_" + d + ".hpp")
                    let fileName = "dexpi2_" + t + ".hpp"
                    let path = Path.Combine(outputDir, fileName)
                    let header =
                        [ "// <auto-generated>"
                          "// DEXPI 2.0 UML model -> C++ - flattened conversion by Dexpi2CppFlatten (rule version " + RuleVersion + ")."
                          "// Source: " + sourceName + " (SHA-256: " + sha + ")"
                          "// Abstract/concrete base classes are FULLY INLINED into every derived class (flat hierarchy):"
                          "//   - no class inherits from anything; every class is self-contained"
                          "//   - abstract classes remain as reference types, still marked [abstract in DEXPI]"
                          "//   - C++ shadowing: a derived member with the same name as an inherited one wins"
                          "// Top-level namespace: dexpi2::" + t
                          "// </auto-generated>"
                          "#pragma once"
                          ""
                          "#include <memory>"
                          "#include <optional>"
                          "#include <string>"
                          "#include <vector>"
                          "" ]
                        @ (depFilesForT |> List.map (fun f -> "#include \"" + f + "\""))
                        @ [ "" ]
                        |> String.concat "\r\n"

                    let body = buildFileBody m t stats |> String.concat "\r\n"
                    writeText path (header + "\r\n" + body + "\r\n")
                    written.Add(path)
                    printfn "wrote %s (%d classes, %d enums, includes %s)" fileName
                        (m.Classes |> List.filter (fun c -> topOf c.NsPath = t) |> List.length)
                        (m.Enums |> List.filter (fun e -> topOf e.NsPath = t) |> List.length)
                        (if depFilesForT.IsEmpty then "(none)" else String.Join(", ", depFilesForT))

                // aggregate header
                let aggPath = Path.Combine(outputDir, "dexpi2.hpp")
                let aggLines =
                    [ "// <auto-generated>"
                      "// DEXPI 2.0 UML model -> C++ - flattened conversion by Dexpi2CppFlatten (rule version " + RuleVersion + ")."
                      "// Source: " + sourceName + " (SHA-256: " + sha + ")"
                      "// Aggregate header: includes every top-level namespace file in dependency order."
                      "// Include this file to get the whole flat DEXPI 2.0 model."
                      "// </auto-generated>"
                      "#pragma once" ]
                    @ (topo |> List.map (fun t -> "#include \"dexpi2_" + t + ".hpp\""))
                    @ [ "" ]
                writeText aggPath (String.concat "\r\n" aggLines + "\r\n")
                printfn "wrote dexpi2.hpp (aggregate)"
                written.Add(aggPath)

                // report
                let reportPath = Path.Combine(outputDir, "flattening-report.md")
                let report =
                    [ "# DEXPI 2.0 C++ -> Flattened Project - Report"
                      ""
                      "- Input: " + sourceName + " (SHA-256: " + sha + ")"
                      "- Tool: Dexpi2CppFlatten (rule version " + RuleVersion + ")"
                      "- Output directory: " + outputDir
                      ""
                      "## Counts (unchanged by flattening)"
                      ""
                      "- Classes: " + string stats.Value.TotalClasses + " (abstract: " + string stats.Value.AbstractClasses + ")"
                      "- Enumerations: " + string stats.Value.TotalEnums
                      ""
                      "## Flattening effect"
                      ""
                      "- Inlined inherited member instances: " + string stats.Value.InlinedInstances
                      "- Inherited duplicates removed (diamond / repeated mixins, same name): " + string stats.Value.DedupedSameType
                      "- Inherited members dropped because the derived class declares the same name (C++ shadowing wins): " + string stats.Value.ShadowedDropped
                      "- Members re-qualified with fully qualified type names (cross-namespace inlining): " + string stats.Value.Requalified
                      ""
                      "## Files"
                      ""
                      "| File | Top-level namespace |"
                      "| --- | --- |"
                      "| dexpi2.hpp | aggregate (includes all below in dependency order) |" ]
                    @ (topNamespaces
                       |> List.map (fun t -> "| dexpi2_" + t + ".hpp | dexpi2::" + t + " |"))
                    @ [ ""
                        "## Shadowing resolutions (derived member wins)"
                        "" ]
                    @ (m.Classes
                       |> List.collect (fun c ->
                           let (_, _, dropped, _) = flattenClass m c
                           dropped
                           |> List.map (fun (src, mem) ->
                               "- " + c.CppName + ": inherited '" + mem + "' from " + src + " dropped (own member with same name wins)")))
                    @ [ ""
                        "## Verification"
                        ""
                        "- Model invariant checked: for every class, the flattened member-name set equals own names + all ancestor names (most-derived wins)."
                        "- No class in the output declares any base (\" : public\" absent)."
                        "- Each file is self-sufficient: it includes exactly the top-level files it references." ]
                writeText reportPath (String.concat "\r\n" report + "\r\n")
                printfn "wrote flattening-report.md"

                // verify no bases remain in generated text
                let ok =
                    written
                    |> Seq.forall (fun p ->
                        let text = File.ReadAllText(p, Encoding.UTF8)
                        not (Regex.IsMatch(text, @"class \w+ *: *public")))
                if not ok then
                    eprintfn "VERIFY FAIL: some emitted class still declares bases"
                    2
                else
                    printfn "verification: no \"class X : public ...\" remains in output"
                    // round-trip: re-parse every emitted file
                    let outProblems = verifyOutputs m written
                    if not (List.isEmpty outProblems) then
                        for (fn, msg) in outProblems do
                            eprintfn "VERIFY FAIL %s: %s" fn msg
                        eprintfn "aborting: output round-trip verification failed"
                        2
                    else
                        printfn "verification: outputs re-parse with expected class/enum counts, zero bases, unique members"
                        printfn "done."
                        0
