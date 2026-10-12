namespace DEXPI2

/// <summary>
/// 展平的属性变更：一条 = 一个实体（某 Row 类型，由其 key 定位）+ 一个参数（fluid/pn/dn）的新值。
/// 标签为各 Row 类型与 fluid/pn/dn 的笛卡尔组合；数据为各实体的 key 和对应的新值。
/// PlantModelProperty 列表即 ModelEntities 参数状态的展平（flatten）结果。
/// </summary>
type PlantModelProperty =
    // EquipmentRow × fluid/pn/dn，key = Equipment
    | EquipmentFluid of equipment: string * fluid: string
    | EquipmentPN of equipment: string * pn: int
    // NozzleRow × fluid/pn/dn，key = (Equipment, Nozzle)
    | NozzleFluid of equipment: string * nozzle: string * fluid: string
    | NozzlePN of equipment: string * nozzle: string * pn: int
    | NozzleDN of equipment: string * nozzle: string * dn: int
    // PipingNetworkSystemRow × fluid/pn/dn，key = LineNumber
    | NetworkFluid of lineNumber: string * fluid: string
    | NetworkPN of lineNumber: string * pn: int
    | NetworkDN of lineNumber: string * dn: int
    // SegmentRow × fluid/pn/dn，key = (LineNumber, SegmentNumber)
    | SegmentFluid of lineNumber: string * segmentNumber: string * fluid: string
    | SegmentPN of lineNumber: string * segmentNumber: string * pn: int
    | SegmentDN of lineNumber: string * segmentNumber: string * dn: int
    // HubSegmentRow × fluid/pn/dn，key = (LineNumber, SegmentNumber)；dn 为端点列表（多值）
    | HubSegmentFluid of lineNumber: string * segmentNumber: string * fluid: string
    | HubSegmentPN of lineNumber: string * segmentNumber: string * pn: int
    //| HubSegmentDN of lineNumber: string * segmentNumber: string * dns: int list
    // SegmentItemRow × fluid/pn/dn，key = (LineNumber, SegmentNumber, Tag)
    | SegmentItemFluid of lineNumber: string * segmentNumber: string * tag: string * fluid: string
    | SegmentItemPN of lineNumber: string * segmentNumber: string * tag: string * pn: int
    | SegmentItemDN of lineNumber: string * segmentNumber: string * tag: string * dn: int

    member this.update() =
        match this with
        | EquipmentFluid (equipment, fluid) -> 
            fun  (plantModel: PlantModel)  (e: ModelEntities) ->       
            ModelEntities.cascadeEquipmentFluid plantModel e equipment fluid
            
        | EquipmentPN (equipment, pn) ->
            fun (plantModel: PlantModel) (e: ModelEntities) ->
            ModelEntities.cascadeEquipmentPN plantModel e equipment pn
        | NozzleFluid (equipment, nozzle, fluid) ->
            fun (plantModel: PlantModel) (e: ModelEntities) ->
            ModelEntities.cascadeNozzleFluid plantModel e equipment nozzle fluid
        | NozzlePN (equipment, nozzle, pn) ->
            fun (plantModel: PlantModel) (e: ModelEntities) ->
            ModelEntities.cascadeNozzlePN plantModel e equipment nozzle pn
        | NozzleDN (equipment, nozzle, dn) ->
            fun (plantModel: PlantModel) (e: ModelEntities) ->
            ModelEntities.cascadeNozzleDN plantModel e equipment nozzle dn
        | NetworkFluid (lineNumber, fluid) ->
            fun (plantModel: PlantModel) (e: ModelEntities) ->
            ModelEntities.cascadeNetworkFluid plantModel e lineNumber fluid
        | NetworkPN (lineNumber, pn) ->
            fun (plantModel: PlantModel) (e: ModelEntities) ->
            ModelEntities.cascadeNetworkPN plantModel e lineNumber pn
        | NetworkDN (lineNumber, dn) ->
            fun (plantModel: PlantModel) (e: ModelEntities) ->
            ModelEntities.cascadeNetworkDN plantModel e lineNumber dn
        | SegmentFluid (lineNumber, segmentNumber, fluid) ->
            fun (plantModel: PlantModel) (e: ModelEntities) ->
            ModelEntities.cascadeSegmentFluid plantModel e lineNumber segmentNumber fluid
        | SegmentPN (lineNumber, segmentNumber, pn) ->
            fun (plantModel: PlantModel) (e: ModelEntities) ->
            ModelEntities.cascadeSegmentPN plantModel e lineNumber segmentNumber pn
        | SegmentDN (lineNumber, segmentNumber, dn) ->
            fun (plantModel: PlantModel) (e: ModelEntities) ->
            ModelEntities.cascadeSegmentDN plantModel e lineNumber segmentNumber dn
        | HubSegmentFluid (lineNumber, segmentNumber, fluid) ->
            fun (plantModel: PlantModel) (e: ModelEntities) ->
            ModelEntities.cascadeSegmentFluid plantModel e lineNumber segmentNumber fluid
        | HubSegmentPN (lineNumber, segmentNumber, pn) ->
            fun (plantModel: PlantModel) (e: ModelEntities) ->
            ModelEntities.cascadeSegmentPN plantModel e lineNumber segmentNumber pn
        //| HubSegmentDN (lineNumber, segmentNumber, dns) -> sprintf "HubSegment %s/%s: set dns = [%A]" lineNumber segmentNumber dns
        | SegmentItemFluid (lineNumber, segmentNumber, tag, fluid) ->
            fun (_: PlantModel) (e: ModelEntities) ->
            SegmentItemTable.setFluid e.SegmentItems fluid lineNumber segmentNumber tag
        | SegmentItemPN (lineNumber, segmentNumber, tag, pn) ->
            fun (_: PlantModel) (e: ModelEntities) ->
            SegmentItemTable.setPN e.SegmentItems pn lineNumber segmentNumber tag
        | SegmentItemDN (lineNumber, segmentNumber, tag, dn) ->
            fun (_: PlantModel) (e: ModelEntities) ->
            SegmentItemTable.setDN e.SegmentItems dn lineNumber segmentNumber tag
        
module PlantModelProperty =
    let getEntities (plantModel: PlantModel) (actions: PlantModelProperty list) =
        let ent = ModelEntities.from plantModel
        actions
        |> List.sort
        |> List.map(fun action -> action.update())
        |> List.iter(fun update ->
            update plantModel ent
        )
        ent

