namespace DEXPI2

/// <summary>Equipment 表的一行：一个设备（tag）一条记录，主键 Equipment。</summary>
type EquipmentRow =
    {
        /// 设备 tag（主键）
        Equipment: string
        /// 介质，默认 ""（未设置）
        mutable Fluid: string // = ""
        /// 压力等级 kPa，默认 0（未设置）
        mutable PN: int // = 0
        /// 公称直径 mm，默认 0（未设置）
        mutable DN: int // = 0
    }

// 注：所有非key字段可变，其实就是fluid,pn,dn三个字段，其他字段都是key字段，不能变

/// Equipment 表建表 SQL（每个 PID 完全重建数据库，无需 IF NOT EXISTS；值列 NOT NULL DEFAULT 0 / ''）
/// Id：自动递增整数主键，保持插入顺序；Equipment 为业务唯一键（UNIQUE）。
module EquipmentTable =
    let schemaSql =
        """
    CREATE TABLE Equipment (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Equipment TEXT NOT NULL UNIQUE,
        Fluid TEXT NOT NULL DEFAULT '',
        PN INTEGER NOT NULL DEFAULT 0,
        DN INTEGER NOT NULL DEFAULT 0
    );
    """

    /// 从 PlantModel 提取 Equipment 行（每设备一行，值列默认）。纯函数，不碰数据库。
    let from (model: PlantModel) : EquipmentRow list =
        model.ProcessEquipments
        |> List.map(fun eq ->
            let tag, _ = ProcessEquipmentCategory.equipmentTagNozzles eq
            {
                Equipment = tag
                Fluid = ""
                PN = 0
                DN = 0
            }
        )

    /// 行 → (Fluid, PN, DN) 三元组（按影响范围从大到小）
    let toTuple (r: EquipmentRow) : string * int * int = r.Fluid, r.PN, r.DN

    /// 在行列表中按主键（Equipment tag）查找行；不存在返回 None。
    /// 输入行列表（如实体 e 的 e.Equipments），不引用 ModelEntities 类型，避免循环引用。
    let tryGet (rows: EquipmentRow list) (equipment: string) : EquipmentRow option =
        rows
        |> List.tryFind (fun r -> r.Equipment = equipment)

    /// 在行列表中按主键（Equipment tag）设置 DN（mm）；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.Equipments），不引用 ModelEntities 类型，避免循环引用。
    let setDN (rows: EquipmentRow list) (dn: int) (equipment: string) : unit =
        match tryGet rows equipment with
        | Some r -> r.DN <- dn
        | None ->
            invalidOp (
                sprintf
                    "Equipment '%s' does not exist; call createFromModel first."
                    equipment
            )

    /// 在行列表中按主键（Equipment tag）设置 PN（kPa）；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.Equipments），不引用 ModelEntities 类型，避免循环引用。
    let setPN (rows: EquipmentRow list) (pn: int) (equipment: string) : unit =
        match tryGet rows equipment with
        | Some r -> r.PN <- pn
        | None ->
            invalidOp (
                sprintf
                    "Equipment '%s' does not exist; call createFromModel first."
                    equipment
            )

    /// 在行列表中按主键（Equipment tag）设置流体介质；行不存在抛异常。
    /// 输入行列表（如实体 e 的 e.Equipments），不引用 ModelEntities 类型，避免循环引用。
    let setFluid (rows: EquipmentRow list) (fluid: string) (equipment: string) : unit =
        match tryGet rows equipment with
        | Some r -> r.Fluid <- fluid
        | None ->
            invalidOp (
                sprintf
                    "Equipment '%s' does not exist; call createFromModel first."
                    equipment
            )
