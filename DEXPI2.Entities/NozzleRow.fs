namespace DEXPI2

/// <summary>Nozzle 表的一行：一个设备管嘴一条记录，主键 (Equipment, Nozzle)。</summary>
type NozzleRow =
    {
        /// 所属设备 tag（主键）
        Equipment: string
        /// 管嘴名（主键）
        Nozzle: string
        /// 介质，默认 ""（未设置）
        mutable Fluid: string // = ""
        /// 压力等级 kPa，默认 0（未设置）
        mutable PN: int // = 0
        /// 公称直径 mm，默认 0（未设置）
        mutable DN: int // = 0
    }

/// Nozzle 表建表 SQL（每个 PID 完全重建数据库，无需 IF NOT EXISTS；值列 NOT NULL DEFAULT 0 / ''）
/// Id：自动递增整数主键，保持插入顺序；(Equipment, Nozzle) 为业务唯一键（UNIQUE）。
module NozzleTable =
    let schemaSql =
        """
    CREATE TABLE Nozzle (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Equipment TEXT NOT NULL,
        Nozzle TEXT NOT NULL,
        Fluid TEXT NOT NULL DEFAULT '',
        PN INTEGER NOT NULL DEFAULT 0,
        DN INTEGER NOT NULL DEFAULT 0,
        UNIQUE (Equipment, Nozzle)
    );
    """

    /// 从 PlantModel 提取 Nozzle 行（每设备管嘴一行，值列默认）。纯函数，不碰数据库。
    let from (model: PlantModel) : NozzleRow list =
        model.ProcessEquipments
        |> List.collect(fun eq ->
            let tag, nozzles = ProcessEquipmentCategory.equipmentTagNozzles eq
            nozzles
            |> List.map(fun nozzle ->
                {
                    Equipment = tag
                    Nozzle = nozzle
                    Fluid = ""
                    PN = 0
                    DN = 0
                }
            )
        )

    /// 行 → (Fluid, PN, DN) 三元组（按影响范围从大到小）
    let toTuple (r: NozzleRow) : string * int * int = r.Fluid, r.PN, r.DN

    /// 在行列表中按主键 (Equipment, Nozzle) 查找行；不存在返回 None。
    /// 输入行列表（如实体 e 的 e.Nozzles），不引用 ModelEntities 类型，避免循环引用。
    let tryGet
        (rows: NozzleRow list)
        (equipment: string)
        (nozzle: string)
        : NozzleRow option
        =
        rows
        |> List.tryFind(fun r ->
            r.Equipment = equipment
            && r.Nozzle = nozzle
        )

    /// 在行列表中按主键 (Equipment, Nozzle) 设置 PN（kPa）；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.Nozzles），不引用 ModelEntities 类型，避免循环引用。
    let setPN
        (rows: NozzleRow list)
        (pn: int)
        (equipment: string)
        (nozzle: string)
        : unit
        =
        match tryGet rows equipment nozzle with
        | Some r -> r.PN <- pn
        | None ->
            invalidOp (
                sprintf
                    "Nozzle '%s/%s' does not exist; call createFromModel first."
                    equipment
                    nozzle
            )

    /// 在行列表中按主键 (Equipment, Nozzle) 设置 DN（mm）；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.Nozzles），不引用 ModelEntities 类型，避免循环引用。
    let setDN
        (rows: NozzleRow list)
        (dn: int)
        (equipment: string)
        (nozzle: string)
        : unit
        =
        match tryGet rows equipment nozzle with
        | Some r -> r.DN <- dn
        | None ->
            invalidOp (
                sprintf
                    "Nozzle '%s/%s' does not exist; call createFromModel first."
                    equipment
                    nozzle
            )

    /// 在行列表中按主键 (Equipment, Nozzle) 设置流体介质；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.Nozzles），不引用 ModelEntities 类型，避免循环引用。
    let setFluid
        (rows: NozzleRow list)
        (fluid: string)
        (equipment: string)
        (nozzle: string)
        : unit
        =
        match tryGet rows equipment nozzle with
        | Some r -> r.Fluid <- fluid
        | None ->
            invalidOp (
                sprintf
                    "Nozzle '%s/%s' does not exist; call createFromModel first."
                    equipment
                    nozzle
            )
