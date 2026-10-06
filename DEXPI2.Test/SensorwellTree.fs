module DEXPI2.SensorwellTree

/// 工厂模型入口（两罐在位内联，段 S1 中部挂传感器套管 SW-101 作压力测点 PT）
let plantModel: PlantModel =
    {
        ProcessEquipments =
            [
                Tank(tag = "V-101", nozzles = [ "N1" ])
                Tank(tag = "V-102", nozzles = [ "N2" ])
            ]
        PipingNetworkSystems =
            [
                {
                    LineNumber = "PL-401"

                    Segments =
                        [
                            {
                                SegmentNumber = "S1"
                                // 罐 V-101（左）→ 传感器套管 SW-101（管道中部测点 PT）→ 罐 V-102（右）
                                Items =
                                    [
                                        PipingNodeOwner.Nozzle(equipment = "V-101", nozzle = "N1")
                                        PipingNodeOwner.Sensorwell(typeRepresentation = "PT")
                                        PipingNodeOwner.Nozzle(equipment = "V-102", nozzle = "N2")
                                    ]
                            }
                        ]
                }
            ]
    }
