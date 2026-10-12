namespace DEXPI2

type PlantModel = {
    ProcessEquipments: list<ProcessEquipment>
    PipingNetworkSystems: list<PipingNetworkSystem>
}

module PlantModel =
    /// PlantModel 是聚合根，跨 Equipment / Nozzle / PipingNetworkSystem / Segment / SegmentItem 五张表。
    /// 单表 toSqlite/fromSqlite 见各子类型模块；此处不提供单 tuple 转换，
    /// 由上层按表拆分加载后重新组装。
    let toSqlite (_: PlantModel) : unit = ()
    let fromSqlite (_: unit) : PlantModel =
        failwith "PlantModel.fromSqlite: 聚合根请按表拆分加载后组装"
