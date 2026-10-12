-- ============================================================
-- PlantModel 拓扑数据库 schema
--
-- 通用约定：
--   * 每张表第一列 id INTEGER PRIMARY KEY 为自增 rowid，记录插入顺序；
--     业务键（tag / line_number / 复合键）降级为 UNIQUE 约束保留，
--     外键仍指向这些业务列。
--   * 外键需在连接后执行 PRAGMA foreign_keys = ON; 才生效。
--   * meta 表为 key-content，本库身份信息写入后，多库查找时
--     只需遍历各 .db 的 meta 表即可定位，无需全量加载业务表。
-- ============================================================

-- 设备表：一行一个 ProcessEquipment。
-- tag 即设备位号，全模型唯一。
CREATE TABLE Equipment (
    id INTEGER PRIMARY KEY,
    tag  TEXT NOT NULL UNIQUE,
    kind TEXT NOT NULL
);

CREATE TABLE Nozzle (
    id INTEGER PRIMARY KEY,
    equipment_tag TEXT NOT NULL,
    tag        TEXT NOT NULL,
    position      INTEGER NOT NULL,
    UNIQUE (equipment_tag, tag),
    FOREIGN KEY (equipment_tag) REFERENCES Equipment(tag) ON DELETE CASCADE
);

-- 管网系统：一行一个 PipingNetworkSystem。
CREATE TABLE PipingNetworkSystem (
    id INTEGER PRIMARY KEY,
    line_number TEXT NOT NULL UNIQUE
);

-- 管段表：Segment 与 HubSegment 共用一张表。
--   kind = 'Segment'     常规段，有 Items（见 SegmentItem）
--   kind = 'HubSegment' 枢纽段，只有段号，被其他段以 SegmentRef 引用
-- 同一系统内 segmentNumber 唯一；不允许跨系统引用段。
CREATE TABLE Segment (
    id INTEGER PRIMARY KEY,
    line_number    TEXT NOT NULL,
    segment_number TEXT NOT NULL,
    kind           TEXT NOT NULL CHECK (kind IN ('Segment', 'HubSegment')),
    UNIQUE (line_number, segment_number),
    FOREIGN KEY (line_number) REFERENCES PipingNetworkSystem(line_number) ON DELETE CASCADE
);

CREATE TABLE SegmentItem (
    id INTEGER PRIMARY KEY,
    line_number     TEXT NOT NULL,
    segment_number  TEXT NOT NULL,
    position        INTEGER NOT NULL,
    node_kind       TEXT NOT NULL,
    tag             TEXT,
    UNIQUE (line_number, segment_number, position),
    FOREIGN KEY (line_number, segment_number) REFERENCES Segment(line_number, segment_number) ON DELETE CASCADE
);

-- 元数据表：key-content 结构，记录本库身份与定位信息。
CREATE TABLE meta (
    title      TEXT PRIMARY KEY,
    content    TEXT NOT NULL,
    updated_at TEXT NOT NULL DEFAULT (datetime('now', 'localtime'))
);

-- 常用索引：按 tag 查管件；meta.title 已由 UNIQUE 自动建索引。
CREATE INDEX ix_segmentitem_tag ON SegmentItem(tag);

-- 预置 schema 版本；其他业务标识由写入侧补充。
INSERT INTO meta (title, content) VALUES ('namespace', 'xp44mm/FSharp.DEXPI2/DEXPI2');
INSERT INTO meta (title, content) VALUES ('schema_version', '1.0');

