namespace DEXPI2

open System

/// <summary>PlantModel → 数据库实体 的转换结果：五张表实体行 + 段 → 父级(设备,管嘴) 映射。</summary>
/// 值列均为默认值（Fluid=""、PN=0、DN=0）；SegmentParents 即 plant 级传递结果。
/// 本类型是参数设置的权威内存态：设置（set*）与覆盖求值（get*Final）都在此层次完成，不碰数据库；
/// 数据库（Setter）只是把本实体投影到五张表的持久化层。
type ModelEntities =
    {
        /// Equipment 表实体行（每设备一行）
        Equipments: EquipmentRow list
        /// Nozzle 表实体行（每管嘴一行）
        Nozzles: NozzleRow list
        /// 拓扑网络（PlantModel.PipingNetworkSystems 原样保留，供级联传播按拓扑定位）
        Networks: PipingNetworkSystemRow list

        /// Segment 表实体行（每常规段一行）
        Segments: SegmentRow list
        /// HubSegment 表实体行（每枢纽段一行，DN 为端点列表，默认空）
        HubSegments: HubSegmentRow list
        /// SegmentItem 表实体行（每段项一行，Tag＝tag 管件真 tag / 无名管件 ItemIndex.ToString()）
        SegmentItems: SegmentItemRow list
    }

    static member from(plantModel: PlantModel) : ModelEntities =
        let equipments = EquipmentTable.from(plantModel)
        let nozzles = NozzleTable.from(plantModel)
        let segments = SegmentTable.from(plantModel)
        let hubSegments = HubSegmentTable.from(plantModel)
        let segmentItems = SegmentItemTable.from(plantModel)
        let networks = PipingNetworkSystemTable.from(plantModel)
        {
            Equipments = equipments
            Nozzles = nozzles
            Networks = networks
            Segments = segments
            HubSegments = hubSegments
            SegmentItems = segmentItems
        }

/// <summary>
/// ModelEntities 同名操作模块：设置 dn/pn/fluid 与 CSS 覆盖求值。纯内存，不碰数据库。
/// 参数数值传递方向：equipment -> nozzle -> segment -> segmentItem。
/// 求最终值类似于 css 算法，后来的、具体的、局部的覆盖先来的、父容器的、全局的参数。
/// pn: 压力等级，单位 kPa；dn: 公称直径，单位 mm；fluid: 介质，string，如 "Water","Steam","Air","Oil","Gas" 等。
/// 覆盖链：equipment（最全局）→ nozzle → segment → segmentItem（最局部），沿链取第一个非默认值
/// （Fluid≠""、PN≠0、DN≠0），局部覆盖父级。段 → 父级(设备,管嘴) 由 modelToEntities 建立（plant 级传递）。
/// </summary>
module ModelEntities =

    /// 段值传播到引用管嘴的规则（物化写模式）：nozzle 为默认值则设为段值；
    /// 非默认值则核对与段值一致（一致静默，不一致抛异常）。管嘴行不存在抛异常。
    let cascadeNozzleValue
        (e: ModelEntities)
        (equipment: string)
        (nozzle: string)
        (getValue: NozzleRow -> 'T)
        (setValue: NozzleRow -> 'T -> unit)
        (defaultValue: 'T)
        (segmentValue: 'T)
        (valueName: string)
        : unit
        =
        match
            e.Nozzles
            |> List.tryFind(fun r -> r.Equipment = equipment && r.Nozzle = nozzle)
        with
        | Some r ->
            if getValue r = defaultValue then
                setValue r segmentValue
            elif getValue r <> segmentValue then
                invalidOp(
                    sprintf
                        "Nozzle '%s/%s' %s '%A' conflicts with segment value '%A'."
                        equipment
                        nozzle
                        valueName
                        (getValue r)
                        segmentValue
                )
        | None ->
            invalidOp(
                sprintf
                    "Nozzle '%s/%s' not found in entities; call createFromModel first."
                    equipment
                    nozzle
            )

    /// 段（lineNumber, segment）fluid 级联传播（物化写模式，递归）：
    /// 段行自身（常规段 SegmentRow / 枢纽段 HubSegmentRow）与段内所有段项设为 fluid；
    /// 段项中引用 Nozzle 的按 cascadeNozzleValue 规则（默认值则设、非默认核对一致、冲突抛异常）；
    /// 引用 Segment 的递归本函数（被引用段也设 fluid）。
    /// plantModel 提供拓扑，e 为被更新的权威内存态；段项写用 SegmentItemTable.setFluid，
    /// 枢纽段自身写用 HubSegmentTable.setFluid。
    let rec cascadeSegmentFluid
        (plantModel: PlantModel)
        (e: ModelEntities)
        (lineNumber: string)
        (segment: string)
        (fluid: string)
        : unit
        =
        let sys =
            plantModel.PipingNetworkSystems
            |> List.tryFind(fun s -> s.LineNumber = lineNumber)
            |> Option.defaultWith(fun () ->
                invalidOp(sprintf "System '%s' not found in plantModel." lineNumber)
            )

        match
            sys.Segments
            |> List.tryFind(
                function
                | Segment(n, _) -> n = segment
                | HubSegment n -> n = segment
            )
        with
        | Some(Segment(_, items)) ->
            // 段行自身（常规段）
            match SegmentTable.tryGet e.Segments lineNumber segment with
            | Some r -> r.Fluid <- fluid
            | None ->
                invalidOp(
                    sprintf "Segment '%s/%s' not found in entities." lineNumber segment
                )
            // 段内项：引用 Nozzle 按 cascadeNozzleValue 规则；引用 Segment 递归本函数；其余管件写段项行
            items
            |> List.iteri(fun i item ->
                match item with
                | Nozzle(eq, nz) ->
                    // 段值传播到引用管嘴：默认值则设、非默认核对一致、冲突抛异常
                    cascadeNozzleValue
                        e
                        eq
                        nz
                        (fun r -> r.Fluid)
                        (fun r v -> r.Fluid <- v)
                        ""
                        fluid
                        "Fluid"
                | SegmentRef sn -> cascadeSegmentFluid plantModel e lineNumber sn fluid
                | _ ->
                    // 段项 tag：tag 管件真 tag；无名管件用其在 items 中的索引（与 SegmentItemTable.from 一致）
                    let tag =
                        match PipingNodeOwnerCategory.tagItemOf item with
                        | Some(_, tag) -> tag
                        | None ->
                            match PipingNodeOwnerCategory.unnamedTypeOf item with
                            | Some _ -> string i
                            | None ->
                                invalidOp(
                                    sprintf
                                        "Unexpected node in segment '%s/%s'."
                                        lineNumber
                                        segment
                                )
                    SegmentItemTable.setFluid e.SegmentItems fluid lineNumber segment tag
            )
        | Some(HubSegment _) ->
            // 枢纽段自身（无段项）
            HubSegmentTable.setFluid e.HubSegments fluid lineNumber segment
        | None ->
            invalidOp(
                sprintf "Segment '%s/%s' not found in plantModel." lineNumber segment
            )

    /// 段（lineNumber, segment）pn 级联传播（物化写模式，递归）：与 cascadeSegmentFluid 同一逻辑。
    /// 段行自身（常规段 SegmentRow / 枢纽段 HubSegmentRow）与段内所有段项设为 pn；
    /// 段项中引用 Nozzle 的按 cascadeNozzleValue 规则（默认值则设、非默认核对一致、冲突抛异常）；
    /// 引用 Segment 的递归本函数（被引用段也设 pn）。
    /// plantModel 提供拓扑，e 为被更新的权威内存态；段项写用 SegmentItemTable.setPN，
    /// 枢纽段自身写用 HubSegmentTable.setPN。
    let rec cascadeSegmentPN
        (plantModel: PlantModel)
        (e: ModelEntities)
        (lineNumber: string)
        (segment: string)
        (pn: int)
        : unit
        =
        let sys =
            plantModel.PipingNetworkSystems
            |> List.tryFind(fun s -> s.LineNumber = lineNumber)
            |> Option.defaultWith(fun () ->
                invalidOp(sprintf "System '%s' not found in plantModel." lineNumber)
            )

        match
            sys.Segments
            |> List.tryFind(
                function
                | Segment(n, _) -> n = segment
                | HubSegment n -> n = segment
            )
        with
        | Some(Segment(_, items)) ->
            // 段行自身（常规段）
            match SegmentTable.tryGet e.Segments lineNumber segment with
            | Some r -> r.PN <- pn
            | None ->
                invalidOp(
                    sprintf "Segment '%s/%s' not found in entities." lineNumber segment
                )
            // 段内项：引用 Nozzle 按 cascadeNozzleValue 规则；引用 Segment 递归本函数；其余管件写段项行
            items
            |> List.iteri(fun i item ->
                match item with
                | Nozzle(eq, nz) ->
                    // 段值传播到引用管嘴：默认值则设、非默认核对一致、冲突抛异常
                    cascadeNozzleValue
                        e
                        eq
                        nz
                        (fun r -> r.PN)
                        (fun r v -> r.PN <- v)
                        0
                        pn
                        "PN"
                | SegmentRef sn -> cascadeSegmentPN plantModel e lineNumber sn pn
                | _ ->
                    // 段项 tag：tag 管件真 tag；无名管件用其在 items 中的索引（与 SegmentItemTable.from 一致）
                    let tag =
                        match PipingNodeOwnerCategory.tagItemOf item with
                        | Some(_, tag) -> tag
                        | None ->
                            match PipingNodeOwnerCategory.unnamedTypeOf item with
                            | Some _ -> string i
                            | None ->
                                invalidOp(
                                    sprintf
                                        "Unexpected node in segment '%s/%s'."
                                        lineNumber
                                        segment
                                )
                    SegmentItemTable.setPN e.SegmentItems pn lineNumber segment tag
            )
        | Some(HubSegment _) ->
            // 枢纽段自身（无段项）
            HubSegmentTable.setPN e.HubSegments pn lineNumber segment
        | None ->
            invalidOp(
                sprintf "Segment '%s/%s' not found in plantModel." lineNumber segment
            )

    /// 段（lineNumber, segment）dn 级联传播（物化写模式，递归）：与 cascadeSegmentPN 同一逻辑，
    /// 差异仅在枢纽段——hub 的 DN 是端点列表，给列表追加一个 dn（不替换）。
    /// 段行自身（常规段 SegmentRow / 枢纽段 HubSegmentRow）与段内所有段项设为 dn；
    /// 段项中引用 Nozzle 的按 cascadeNozzleValue 规则（默认值则设、非默认核对一致、冲突抛异常）；
    /// 引用 Segment 的递归本函数（被引用段也设 dn）。
    /// plantModel 提供拓扑，e 为被更新的权威内存态；段项写用 SegmentItemTable.setDN。
    let rec cascadeSegmentDN
        (plantModel: PlantModel)
        (e: ModelEntities)
        (lineNumber: string)
        (segment: string)
        (dn: int)
        : unit
        =
        let sys =
            plantModel.PipingNetworkSystems
            |> List.tryFind(fun s -> s.LineNumber = lineNumber)
            |> Option.defaultWith(fun () ->
                invalidOp(sprintf "System '%s' not found in plantModel." lineNumber)
            )

        match
            sys.Segments
            |> List.tryFind(
                function
                | Segment(n, _) -> n = segment
                | HubSegment n -> n = segment
            )
        with
        | Some(Segment(_, items)) ->
            // 段行自身（常规段）
            match SegmentTable.tryGet e.Segments lineNumber segment with
            | Some r -> r.DN <- dn
            | None ->
                invalidOp(
                    sprintf "Segment '%s/%s' not found in entities." lineNumber segment
                )
            // 段内项：引用 Nozzle 按 cascadeNozzleValue 规则；引用 Segment 递归本函数；其余管件写段项行
            items
            |> List.iteri(fun i item ->
                match item with
                | Nozzle(eq, nz) ->
                    // 段值传播到引用管嘴：默认值则设、非默认核对一致、冲突抛异常
                    cascadeNozzleValue
                        e
                        eq
                        nz
                        (fun r -> r.DN)
                        (fun r v -> r.DN <- v)
                        0
                        dn
                        "DN"
                | SegmentRef sn -> cascadeSegmentDN plantModel e lineNumber sn dn
                | _ ->
                    // 段项 tag：tag 管件真 tag；无名管件用其在 items 中的索引（与 SegmentItemTable.from 一致）
                    let tag =
                        match PipingNodeOwnerCategory.tagItemOf item with
                        | Some(_, tag) -> tag
                        | None ->
                            match PipingNodeOwnerCategory.unnamedTypeOf item with
                            | Some _ -> string i
                            | None ->
                                invalidOp(
                                    sprintf
                                        "Unexpected node in segment '%s/%s'."
                                        lineNumber
                                        segment
                                )
                    SegmentItemTable.setDN e.SegmentItems dn lineNumber segment tag
            )
        | Some(HubSegment _) ->
            // 枢纽段自身：DN 是端点列表，追加一个 dn（不替换，同 cascadeNozzleDN 语义）
            match HubSegmentTable.tryGet e.HubSegments lineNumber segment with
            | Some h -> h.DNs <- dn :: h.DNs
            | None ->
                invalidOp(
                    sprintf "HubSegment '%s/%s' not found in entities." lineNumber segment
                )
        | None ->
            invalidOp(
                sprintf "Segment '%s/%s' not found in plantModel." lineNumber segment
            )

    /// 显式值写入系统（Network）内所有行（物化写模式）：系统内所有常规段、枢纽段、
    /// 段项的 Fluid 设为指定值。lineNumber 定位系统；与同系统必然同 fluid 规则一致。
    let cascadeNetworkFluid
        (plantModel: PlantModel)
        (e: ModelEntities)
        (lineNumber: string)
        (fluid: string)
        : unit
        =

        let sys =
            plantModel.PipingNetworkSystems
            |> List.tryFind(fun s -> s.LineNumber = lineNumber)
            |> Option.defaultWith(fun () ->
                invalidOp(sprintf "System '%s' not found in plantModel." lineNumber)
            )
        // 系统内所有常规段、枢纽段、段项设为 fluid；
        // 段项中引用 Nozzle 的跳过（引用项不入库，SegmentItems 表中本无 Nozzle 引用项）；
        // 不会引用其他的网络系统的 Segment，所以不需要递归调用 cascadeSegmentFluid
        e.Segments
        |> List.filter(fun r -> r.LineNumber = lineNumber)
        |> List.iter(fun r -> r.Fluid <- fluid)
        e.HubSegments
        |> List.filter(fun r -> r.LineNumber = lineNumber)
        |> List.iter(fun r -> r.Fluid <- fluid)
        e.SegmentItems
        |> List.filter(fun r -> r.LineNumber = lineNumber)
        |> List.iter(fun r -> r.Fluid <- fluid)

    /// 显式值写入系统（Network）内所有行（物化写模式）：系统内所有常规段、枢纽段、
    /// 段项的 PN 设为指定值。lineNumber 定位系统。
    let cascadeNetworkPN
        (plantModel: PlantModel)
        (e: ModelEntities)
        (lineNumber: string)
        (pn: int)
        : unit
        =
        let sys =
            plantModel.PipingNetworkSystems
            |> List.tryFind(fun s -> s.LineNumber = lineNumber)
            |> Option.defaultWith(fun () ->
                invalidOp(sprintf "System '%s' not found in plantModel." lineNumber)
            )
        // 系统内所有常规段、枢纽段、段项设为 pn；
        // 段项中引用 Nozzle 的跳过（引用项不入库，SegmentItems 表中本无 Nozzle 引用项）；
        // 不会引用其他的网络系统的 Segment，所以不需要递归调用 cascadeSegmentPN
        e.Segments
        |> List.filter(fun r -> r.LineNumber = lineNumber)
        |> List.iter(fun r -> r.PN <- pn)
        e.HubSegments
        |> List.filter(fun r -> r.LineNumber = lineNumber)
        |> List.iter(fun r -> r.PN <- pn)
        e.SegmentItems
        |> List.filter(fun r -> r.LineNumber = lineNumber)
        |> List.iter(fun r -> r.PN <- pn)

    /// 显式值写入系统（Network）内所有行（物化写模式）：系统内所有常规段、段项的
    /// DN 设为指定值。枢纽段跳过——其 DN 为端点列表（多端点），单值不适用，
    /// 端点 DN 请用 cascadeNozzleDN 沿段传播追加。lineNumber 定位系统。
    let cascadeNetworkDN
        (plantModel: PlantModel)
        (e: ModelEntities)
        (lineNumber: string)
        (dn: int)
        : unit
        =
        let sys =
            plantModel.PipingNetworkSystems
            |> List.tryFind(fun s -> s.LineNumber = lineNumber)
            |> Option.defaultWith(fun () ->
                invalidOp(sprintf "System '%s' not found in plantModel." lineNumber)
            )
        // 系统内所有常规段、段项设为 dn；
        // 段项中引用 Nozzle 的跳过（引用项不入库，SegmentItems 表中本无 Nozzle 引用项）；
        // 枢纽段：DN 是端点列表，单值不适用；按系统内被引用次数生成 dns = [dn; ...]，
        // 元素个数是被引用的数量（reducer/tee 各端点共用同一 dn）
        // 不会引用其他的网络系统的 Segment，所以不需要递归调用 cascadeSegmentDN
        e.Segments
        |> List.filter(fun r -> r.LineNumber = lineNumber)
        |> List.iter(fun r -> r.DN <- dn)
        e.SegmentItems
        |> List.filter(fun r -> r.LineNumber = lineNumber)
        |> List.iter(fun r -> r.DN <- dn)
        e.HubSegments
        |> List.filter(fun r -> r.LineNumber = lineNumber)
        |> List.iter(fun h ->
            // 统计系统内引用本枢纽段（SegmentRef）的次数
            let count =
                sys.Segments
                |> List.sumBy(fun seg ->
                    match seg with
                    | Segment(_, items) ->
                        items
                        |> List.sumBy(
                            function
                            | SegmentRef sn when sn = h.SegmentNumber -> 1
                            | _ -> 0
                        )
                    | HubSegment _ -> 0
                )
            h.DNs <- List.replicate count dn
        )

    /// 从管嘴出发的 fluid 级联传播（物化写模式），模式同 cascadeEquipmentFluid，只是起点为管嘴：
    /// 管嘴行 Fluid 为源值，传播到引用该管嘴的系统内所有段（含枢纽段）与所有段项。
    let cascadeNozzleFluid
        (plantModel: PlantModel)
        (e: ModelEntities)
        (equipment: string)
        (nozzle: string)
        (fluid: string)
        : unit
        =
        //本管嘴，和引用本管嘴的网络系统是同一个 fluid，沿系统内所有段（含枢纽段）与段项设为本管嘴的 fluid
        // 注意：一个管嘴只能被引用一次

        // 1. 设备行（e 中）：Fluid 源值；不存在抛异常
        let equip =
            match EquipmentTable.tryGet e.Equipments equipment with
            | Some row -> row
            | None ->
                invalidOp
                    $"Equipment '{equipment}' does not exist in entities; call createFromModel first."

        // 1. 管嘴行（e 中）：用输入参数设置 Fluid；不存在抛异常
        match
            e.Nozzles
            |> List.filter(fun n -> n.Equipment = equip.Equipment)
            |> List.tryFind(fun r -> r.Nozzle = nozzle)
        with
        | Some r -> r.Fluid <- fluid
        | None ->
            invalidOp(
                sprintf
                    "Nozzle '%s/%s' does not exist in entities; call createFromModel first."
                    equipment
                    nozzle
                )

        // 2. 引用该管嘴的系统（段 Items 中有 Nozzle(equipment, nozzle)）；一个管嘴只能被引用一次：
        //    0 次引用正常（无事可做），1 次引用正常执行，2 次及以上引用抛异常
        let systems =
            plantModel.PipingNetworkSystems
            |> List.filter(fun sys ->
                sys.Segments
                |> List.exists(
                    function
                    | Segment(_, items) ->
                        items
                        |> List.exists(
                            function
                            | Nozzle(eq, nz) -> eq = equipment && nz = nozzle
                            | _ -> false
                        )
                    | HubSegment _ -> false
                )
            )

        match systems with
        | [] -> () // 0 次引用：无事可做
        | [ sys ] ->
            // 3. 沿该系统内所有段（含枢纽段）与段项设为本管嘴的 fluid
            cascadeNetworkFluid plantModel e sys.LineNumber fluid
        | _ ->
            invalidOp(
                sprintf
                    "Nozzle '%s/%s' is referenced by %d systems; a nozzle can only be referenced once."
                    equipment
                    nozzle
                    systems.Length
            )

    /// 从管嘴出发的 PN 级联传播（物化写模式），模式同 cascadeEquipmentPN，只是起点为管嘴：
    /// 管嘴行 PN 为源值，传播到引用该管嘴的系统内所有段（含枢纽段）与所有段项。
    let cascadeNozzlePN
        (plantModel: PlantModel)
        (e: ModelEntities)
        (equipment: string)
        (nozzle: string)
        (pn: int)
        : unit
        =
        let equip =
            match EquipmentTable.tryGet e.Equipments equipment with
            | Some row -> row
            | None ->
                invalidOp
                    $"Equipment '{equipment}' does not exist in entities; call createFromModel first."

        // 1. 管嘴行（e 中）：用输入参数设置 PN；不存在抛异常
        match
            e.Nozzles
            |> List.filter(fun n -> n.Equipment = equip.Equipment)
            |> List.tryFind(fun r -> r.Nozzle = nozzle)
        with
        | Some r -> r.PN <- pn
        | None ->
            invalidOp(
                sprintf
                    "Nozzle '%s/%s' does not exist in entities; call createFromModel first."
                    equipment
                    nozzle
                )

        // 2. 引用该管嘴的系统（段 Items 中有 Nozzle(equipment, nozzle)）；一个管嘴只能被引用一次：
        //    0 次引用正常（无事可做），1 次引用正常执行，2 次及以上引用抛异常
        let systems =
            plantModel.PipingNetworkSystems
            |> List.filter(fun sys ->
                sys.Segments
                |> List.exists(
                    function
                    | Segment(_, items) ->
                        items
                        |> List.exists(
                            function
                            | Nozzle(eq, nz) -> eq = equipment && nz = nozzle
                            | _ -> false
                        )
                    | HubSegment _ -> false
                )
            )

        match systems with
        | [] -> () // 0 次引用：无事可做
        | [ sys ] ->
            // 3. 沿该系统内所有段（含枢纽段）与段项设为本管嘴的 pn
            cascadeNetworkPN plantModel e sys.LineNumber pn
        | _ ->
            invalidOp(
                sprintf
                    "Nozzle '%s/%s' is referenced by %d systems; a nozzle can only be referenced once."
                    equipment
                    nozzle
                    systems.Length
            )

    /// 从管嘴出发的 DN 级联传播（物化写模式）：管嘴行 DN 设为输入参数 dn；
    /// 引用该管嘴的段整段级联传播——复用 cascadeSegmentDN（段行自身与段内所有段项
    /// 设同 DN；引用 Nozzle 跳过不入库；引用 Segment 递归；枢纽段 DNs 列表追加一个 dn）。
    let cascadeNozzleDN
        (plantModel: PlantModel)
        (e: ModelEntities)
        (equipment: string)
        (nozzle: string)
        (dn: int)
        : unit
        =

        let equip =
            match EquipmentTable.tryGet e.Equipments equipment with
            | Some row -> row
            | None ->
                invalidOp
                    $"Equipment '{equipment}' does not exist in entities; call createFromModel first."

        match
            e.Nozzles
            |> List.filter(fun n -> n.Equipment = equip.Equipment)
            |> List.tryFind(fun r -> r.Nozzle = nozzle)
        with
        | Some r -> r.DN <- dn
        | None ->
            invalidOp(
                sprintf
                    "Nozzle '%s/%s' does not exist in entities; call createFromModel first."
                    equipment
                    nozzle
            )

        // 2. 找引用该管嘴的段（段 Items 中有 Nozzle(equipment, nozzle)）
        match
            plantModel.PipingNetworkSystems
            |> List.tryPick(fun sys ->
                sys.Segments
                |> List.tryPick(
                    function
                    | Segment(segmentNumber, items) when
                        items
                        |> List.exists(
                            function
                            | Nozzle(eq, nz) -> eq = equipment && nz = nozzle
                            | _ -> false
                        )
                        ->
                        Some(sys.LineNumber, segmentNumber, items)
                    | _ -> None
                )
            )
        with
        | None -> () // 无段引用该管嘴：无事可做
        | Some(lineNumber, segmentNumber, _) ->
            // 3. 整段级联传播：复用 cascadeSegmentDN（段行自身与段内所有段项设 dn；
            //    引用 Nozzle 跳过不入库；引用 Segment 递归；枢纽段 DNs 列表追加一个 dn）
            cascadeSegmentDN plantModel e lineNumber segmentNumber dn

    /// 从设备出发的 fluid 级联传播（物化写模式）：设备行 Fluid 设为输入参数 fluid，
    /// 并级联调用管嘴规则（cascadeNozzleFluid）——该设备每个管嘴行设 fluid，
    /// 沿引用该管嘴的系统传播到段（含枢纽段）与段项。plantModel 提供拓扑，e 为被更新的权威内存态。
    let cascadeEquipmentFluid
        (plantModel: PlantModel)
        (e: ModelEntities)
        (equipment: string)
        (fluid: string)
        : unit
        =
        // 1. 设备行（e 中）：用输入参数设置 Fluid；不存在抛异常
        match EquipmentTable.tryGet e.Equipments equipment with
        | Some row -> row.Fluid <- fluid
        | None ->
            invalidOp
                $"Equipment '{equipment}' does not exist in entities; call createFromModel first."

        // 2. 该设备的所有管嘴（plantModel 拓扑），级联调用管嘴规则
        plantModel.ProcessEquipments
        |> List.tryPick(fun eq ->
            let tag, ns = ProcessEquipmentCategory.equipmentTagNozzles eq
            if tag = equipment then
                Some ns
            else
                None
        )
        |> Option.defaultValue []
        |> List.iter(fun nz -> cascadeNozzleFluid plantModel e equipment nz fluid)

    /// 从设备出发的 PN 级联传播（物化写模式）：设备行 PN 设为输入参数 pn，
    /// 并级联调用管嘴规则（cascadeNozzlePN）——该设备每个管嘴行设 pn，
    /// 沿引用该管嘴的系统传播到段（含枢纽段）与段项。plantModel 提供拓扑，e 为被更新的权威内存态。
    let cascadeEquipmentPN
        (plantModel: PlantModel)
        (e: ModelEntities)
        (equipment: string)
        (pn: int)
        : unit
        =
        // 1. 设备行（e 中）：用输入参数设置 PN；不存在抛异常
        match
            e.Equipments
            |> List.tryFind(fun r -> r.Equipment = equipment)
        with
        | Some r -> r.PN <- pn
        | None ->
            invalidOp(
                sprintf
                    "Equipment '%s' does not exist in entities; call createFromModel first."
                    equipment
            )

        // 2. 该设备的所有管嘴（plantModel 拓扑），级联调用管嘴规则
        plantModel.ProcessEquipments
        |> List.tryPick(fun eq ->
            let tag, ns = ProcessEquipmentCategory.equipmentTagNozzles eq
            if tag = equipment then
                Some ns
            else
                None
        )
        |> Option.defaultValue []
        |> List.iter(fun nz -> cascadeNozzlePN plantModel e equipment nz pn)
