# S4R_REFERENCE_BUILD_CANDIDATES — Reference Build 候选信封（CANDIDATE / NON-AUTHORITATIVE / NOT LOCKED）

**Work Order**: S4R-WO-02 任务 D。**性质**：documentation-only 候选集，用于未来 build coverage 与产品方向判断的输入。**本文件不锁定正式 Reference Build Targets、不构成产品 Gate、不要求 balance/DPS/通关 target**（WO-02 明文；锁定 Targets 属未来经导演方向授权的独立工作令）。
**约束自查（AC-07）**：候选只使用 S4 当前 **Supported** mechanics；每候选「Currently unsupported mechanics required = **NONE**」；Support 组合逐一对 `SupportCompatGolden` oracle 合法（3×7=21 组合，16 兼容/5 不兼容：Concentrated 仅 Area、Fork 仅 Projectile、FireConversion 拒 Area）；Partial 机制仅作 coverage observation 不作成立必需；Unsupported/Director-gated 机制未进入任何必需组成。

## 候选 1 — RC-M「烈刃转火」（Core Skill: Melee）

| 字段 | 内容 |
|---|---|
| Candidate Build ID | S4R-RBC-M |
| Core skill | Melee（Range 2.4 锥形 60°，武器 3S：1 Active + 2 Support） |
| Intended mechanical identity | 近战物理→火转化 + 点燃 DoT：把近战白值持续转成火伤并以 Ignite 叠加消耗 |
| Support combination | **FireConversion + Brutal**（golden 合法 ✓：FireConversion∈{Melee,Projectile}） |
| Passive/mechanic families exercised | Cinder Heart（40% 物转火，与 Support 0.50 经 StatBag 聚合 0.90）；Brutal Strikes（近战进攻向）；Start 免费节点；出图免费重置 |
| Equipment/affix families exercised | 6 槽全闭环；词缀轴=PhysicalDamage / FireDamage / IgniteChance / Life / Armour（均 declared stats）；含 Gloves/Belt 槽（可吃 4 条槽位专属词缀，单一 applicability truth 决定） |
| Damage/defense mechanics exercised | Conversion 单轴（上限 100%）；Ignite（50%/4s，单实例取高 DPS）；Crit（可选轴）；Armour（Physical 减伤）；Fire Resistance；Accuracy/Evasion 攻击检定 |
| Expected coverage purpose | 证明「转火聚合 + DoT + 6 槽装备聚合」整链对近战技能成立；Reference Build Coverage Gap 的近战基线 |
| Currently unsupported mechanics required | **NONE** |
| Status | **CANDIDATE / NOT LOCKED** |

## 候选 2 — RC-P「分裂弹幕」（Core Skill: Projectile）

| 字段 | 内容 |
|---|---|
| Candidate Build ID | S4R-RBC-P |
| Core skill | Projectile（Range 12，弹道池；机制 Support 唯一宿主） |
| Intended mechanical identity | 弹道分裂弹幕：Fork 分裂 + 攻速/暴击轴的高频多段输出 |
| Support combination | **Fork + Faster**（golden 合法 ✓：Fork 仅 Projectile；同 Support 不得装两条 Link） |
| Passive/mechanic families exercised | Brutal Strikes（物理/进攻向）；Start；机制族=弹道分裂（MechanicSkill=弹道，命中后分裂 2 发，同池取弹） |
| Equipment/affix families exercised | 词缀轴=CritChanceIncreased / CritChanceAdded / AttackSpeed / Accuracy / AddedPhysical / MorePhysical；Gloves/Belt 专属词缀可入 |
| Damage/defense mechanics exercised | Crit（chance/multi，base 150%）；Accuracy/Evasion；Physical 直伤轴；（可选）Life/Armour 防御轴 |
| Expected coverage purpose | 证明「机制 Support（Fork）+ 暴击/攻速装备轴」对弹道技能成立；机制 Support 兼容门与 golden parity 的 build 侧验收 |
| Currently unsupported mechanics required | **NONE** |
| Status | **CANDIDATE / NOT LOCKED** |

## 候选 3 — RC-A「灰烬领域」（Core Skill: Area）

| 字段 | 内容 |
|---|---|
| Candidate Build ID | S4R-RBC-A |
| Core skill | Area（Range 7 / Radius 3.2，一次遍历结算；领域帽 2S：1 Support） |
| Intended mechanical identity | 范围火法：集中放大单圈 + 燃烧/火伤轴；被动层 Cinder Heart 转化对全技能生效 |
| Support combination | **Concentrated + Combustion**（ Concentrated=Area 专属 ✓；Area 仅 2S 装 1 Support，二选一以 canonical 数据为准——两 Support 均 Area 合法，最终取舍在正式锁定时定） |
| Passive/mechanic families exercised | Pyre（火系 Notable）；Cinder Heart（跨技能转化聚合——展示 Passive 轴不绑单技能）；Start |
| Equipment/affix families exercised | 词缀轴=AreaRadiusMore / AreaDamageMore / FireDamage / Life / FireResistance / Intelligence；Gloves/Belt 可入 |
| Damage/defense mechanics exercised | Area 一次结算；Fire 直伤；（FireConversion 支持位=Area 被拒——本候选经 **Passive** Cinder Heart 走转化轴，展示 Support/Passive 双来源边界）；Fire Resistance 防御 |
| Expected coverage purpose | 证明「范围技能 + Area 专属 Support 边界（Concentrated 仅 Area）+ 跨技能被动聚合」成立；同时验证 FireConversion×Area 拒绝边界在 build 侧的语义 |
| Currently unsupported mechanics required | **NONE** |
| Status | **CANDIDATE / NOT LOCKED** |

## Candidate Coverage Mapping（与正式 Locked Targets 分开；AC-08 非权威）

| Canonical Skill | Candidate | 主覆盖族 | 辅覆盖族 |
|---|---|---|---|
| Melee | S4R-RBC-M | Conversion 轴 / DoT(Ignite) / Armour | 6 槽聚合 / 槽位专属词缀 |
| Projectile | S4R-RBC-P | Fork 机制 / Crit / Accuracy-Evasion | 攻速 / 物理直伤 |
| Area | S4R-RBC-A | 范围结算 / Area 专属 Support / Fire 抗 | 跨技能 Passive 转化聚合 |

- 本映射写入 Mechanic Matrix 时仅作为 **Candidate Coverage Mapping** 分区，正式 Reference Build Coverage 字段保持 N/A（Targets 未锁定，不构成 Gap 判断）——AC-08。
- 候选间互斥组合不预判（如 Fork 与 FireConversion 同为 Projectile 可同装 2 槽位，属合法组合空间，留待正式 Targets 阶段决策）。
- 与 Backlog 的关系：候选不依赖任何 BL 项；BL-023/024（全量导入/pipeline）等扩张成立后，候选可被更广机制集替换——替换不追溯本文件。

## S5-WO-05 Validation Records（Phase 4 验证记录；2026-09-09；**状态保持 CANDIDATE / NOT LOCKED，不晋升**）

**性质**：以下为 S5-WO-05 联合验证的如实记录——证明「当前已批准机制可以连贯组合并执行」，**不等于**「canonical product build / balanced / optimal / certified」。测试载体=`S5BuildInteractionTests`（EditMode，Runtime Product Delta=NONE）。

### RC-M（S4R-RBC-M）烈刃转火 — 已验证

| 字段 | 验证记录 |
|---|---|
| Exact scenario exercised | `MeaningfulChoice_ThreeDistinguishableValidLoadouts` 场景 1（legacy 单连接） |
| Supports actually used | Q[0]=火焰转化（ConvertPhysToFire Flat 0.50）+ Q[1]=残暴（MorePhysical More 0.40）——golden 合法（FireConversion×Melee ✓ / Brutal×Melee ✓） |
| Affixes actually used | 坚韧（Tenacity，手套，Strength/Flat 12） |
| Link mode | legacy 单连接（LinkSkill1=None，组0 双 Support 位） |
| Observed mechanic identity | 转化 0.5 进入近战聚合 + MorePhys 1.4 + Strength 32（词缀经玩家属性轴与连接正交） |
| Result | **PASS**（全部断言绿） |
| Unsupported dependencies | **NONE** |
| Status | **CANDIDATE / NOT LOCKED**（验证通过不构成锁定/晋升） |

### RC-P（S4R-RBC-P）分裂弹幕 — 已验证

| 字段 | 验证记录 |
|---|---|
| Exact scenario exercised | `V4_MechanicSupportCrossPosition_RCP_Fork_WithAffixes`（组0 与组1 双位，同 seed 9u 真实 ArenaSim 施放） |
| Supports actually used | 组0 位=Fork+Faster（legacy 容 2）；组1 位=恰 Fork（组 1 容 1=容量真相） |
| Affixes actually used | 迅疾（SwiftBreeze，武器，AttackSpeed/Increased 0.16） |
| Link mode | 双位验证：legacy 组0 + 武器改挂弹道（组1）各一次 |
| Observed mechanic identity | Fork 真实分裂（ForkSpawns==2 双位等价）——真实 runtime 行为非数据模型断言 |
| Result | **PASS** |
| Unsupported dependencies | **NONE** |
| Status | **CANDIDATE / NOT LOCKED** |

### RC-A（S4R-RBC-A）灰烬领域 — 已验证（部分：Concentrated 变体）

| 字段 | 验证记录 |
|---|---|
| Exact scenario exercised | `V6_CrossGroupIsolation_UnderAffixes`（Area legacy 头盔位 + Concentrated；另 `V2_TwoLink_ConfigB` 验证 Combustion 变体经组1） |
| Supports actually used | V6=Concentrated（Area 专属 ✓）；V2B=Combustion（Area ✓，经武器组1）——候选描述的「Concentrated/Combustion 二选一」两变体均得到独立验证，取舍仍留正式锁定阶段 |
| Affixes actually used | V6=坚韧（武器）+迅疾（身体）；V2B=无词缀（隔离观测） |
| Link mode | V6=武器改挂弹道（双连接在场）+ Area legacy 头盔位；V2B=Area 经武器组1 |
| Observed mechanic identity | AreaDamageMore 0.4 仅进 Area 聚合（RawMore=1.4 含中性 1）；燃尽点燃/火伤轴经组1 生效；跨组零泄漏 |
| Result | **PASS** |
| Unsupported dependencies | **NONE**（FireConversion×Area 拒绝边界=golden 既有事实，未在本场景强制） |
| Status | **CANDIDATE / NOT LOCKED** |

### 池假设审计记录（WO-05 §8/AC-13；audit=test hygiene，非权重调整授权）

- **stale current-pool assumptions found: 0**（grep 全测试目录：无任何测试断言「当前目录/池==17」）。
- **number intentionally retained with rationale**：`ContentAuditS2Tests.S2BaselineAffixCount=10`（S2 历史基线语义）；`S5AffixBreadthTests` 内 17 = stable-ID 常量断言（SwiftBreeze ID=17）与「Before 17 + 4 = 21」计数语义、旧 ID 稳定域 0-16 位置扫描——均非当前池基数断言；`AshlingVisualContractTests`「17k 顶点」等=无关数值。
- **test-only corrections**：无（WO-04 已把两处护栏更新为 21；本轮无需再修）。
- 随机/概率行为断言均不依赖池基数（sweep=「至少出现一次」的存在性断言）。
