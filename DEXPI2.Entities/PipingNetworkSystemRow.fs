namespace DEXPI2

/// <summary>PipingNetworkSystem 表实体行：一个系统一条记录，主键 LineNumber。</summary>
type PipingNetworkSystemRow =
    {
        /// 系统线号（主键）
        LineNumber: string
        /// 介质，默认 ""（未设置）
        mutable Fluid: string // = ""
        /// 压力等级 kPa，默认 0（未设置）
        mutable PN: int // = 0
        /// 公称直径 mm，默认 0（未设置）
        mutable DN: int // = 0
    }

/// <summary>PipingNetworkSystem 表实体模块（内存态，不落数据库表）：从 PlantModel 提取系统行。</summary>
module PipingNetworkSystemTable =
    /// 从 PlantModel 提取 PipingNetworkSystem 行（每系统一行，值列默认）。纯函数，不碰数据库。
    let from (model: PlantModel) : PipingNetworkSystemRow list =
        model.PipingNetworkSystems
        |> List.map (fun sys ->
            {
                LineNumber = sys.LineNumber
                Fluid = ""
                PN = 0
                DN = 0
            }
        )
