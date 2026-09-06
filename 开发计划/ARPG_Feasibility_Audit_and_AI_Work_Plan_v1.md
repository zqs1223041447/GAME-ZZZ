# ARPG 项目可行性审计与 AI 工作计划

**版本：** Feasibility & Execution Plan v1.0  
**日期：** 2026-09-05  
**审计范围：** 已沟通并锁定的 Q1–Q65，以及“Combat Math 默认以 POEDB 为基线”的上位约束  
**项目状态建议：** **GO — 条件可行（Conditional Go）**

---

# 1. 总结结论

## 1.1 项目是否可行？

**可行。**

但这里的“可行”必须严格理解为：

> 使用 Unity 制作一款以 POE 为主要机制参考、3D、单机、长期通过 End Game 扩张的高构筑自由 ARPG，是可行的。

不应理解为：

> 一开始就直接完成与当前 POE 同等内容规模、技能规模、装备规模、Boss 数量和机制密度。

当前设计最适合的研发方式是：

```text
先证明架构
↓
再证明手感
↓
再证明完整微循环
↓
再证明 300 怪 / 120 FPS
↓
再扩大内容
↓
最后扩大 End Game
```

AI 团队的存在会显著降低以下成本：

```text
代码
数据配置
自动测试
工具开发
模拟器
批量内容生成
平衡初筛
文档
重构
```

但不会自动消除以下成本：

```text
3D 美术统一性
角色动画质量
技能视觉辨识度
战斗手感
Sound Design
Boss 设计
最终平衡判断
```

因此本项目真正合理的组织结构不是：

```text
AI 自动把游戏做完
```

而是：

```text
人类 Game Director
决定体验与审美

AI Production Team
负责实现、验证、生产、模拟和迭代
```

---

# 2. 当前设计整体一致性检查

## 2.1 高度一致的设计组合

以下决定互相强化，没有明显矛盾：

```text
单机
+
Windows Only
+
Unity
+
URP
+
强风格化
+
Kitbash Hybrid
```

这组决定明显降低了项目制作难度。

---

以下决定也高度一致：

```text
免费 Respec
+
地图内锁 Build
+
Map Stability
+
失败后可以重新构筑再挑战
```

这使玩家可以自由实验，但一次地图挑战仍然需要承担 Build Commitment。

---

以下决定同样一致：

```text
Active Skill + Support
+
装备 Socket / Link
+
无 Socket 颜色
+
装备制作导向
```

保留技能链接深度，同时减少不必要的孔色 RNG 摩擦。

---

以下决定形成了清晰装备哲学：

```text
无白装
+
Ordinary 至少 2 Affix
+
Rare 作为 Craft 主场
+
Unique 提供固定机制
+
Unique 允许有限制作
+
最终制作允许不可逆风险
```

这是可行且清晰的装备生命周期。

---

以下决定形成了清晰角色哲学：

```text
职业只是起点
+
共享大型天赋树
+
不同职业不同起点
+
免费 Respec
```

角色身份通过：

```text
路径成本
装备
技能
天赋
珠宝
制作
```

形成，而不是通过职业硬锁形成。

---

# 3. 已发现的设计张力

这些不是必须推翻的决定，但必须在实现中加护栏。

---

## 3.1 Traditional Unity vs 150–300 怪 vs 1440p / 120 FPS

### 状态

**高技术风险，但可行。**

组合：

```text
Traditional Unity
+
150～300 战斗实体
+
高密度 Projectile / Hit / Trigger
+
1440p
+
120 FPS
```

是当前项目最大的工程风险。

120 FPS 对应约：

```text
8.33 ms / frame
```

Unity 官方明确提供 Profiler / GPU Profiling、URP 性能配置、Job System 和 Burst 等工具；Burst 可以用于优化 CPU-bound 代码，Job System 可以利用多核心。项目不需要因此改成 Full ECS，但必须保留局部优化出口。

### 必须附加的工程约束

Traditional Unity 在本项目中定义为：

```text
GameObject / MonoBehaviour
作为主要 Authoring 和 View 方式
```

而不是：

```text
每个对象随意 Update
每个 Buff 一个 GameObject
每个 Projectile 一个 Rigidbody
每个怪都完整 NavMeshAgent
```

必须禁止：

```text
300 × 多 Component Update()
大量战斗热路径 GC
每次 Hit 扫描整套装备和天赋
每个 Buff / Modifier 创建 GameObject
每个 Projectile 默认 Rigidbody
所有怪物每帧完整寻路
所有怪物 Animator 永远 Full Update
```

允许在局部热点采用：

```text
Central Tick Scheduler
Object Pool
Array / NativeArray
Jobs
Burst
Batch Spatial Query
Custom Projectile Simulation
AI LOD
Animation LOD
```

### 结论

**不修改 Q15A，但必须把 Performance Gate 作为硬门槛。**

---

## 3.2 POEDB 级 Combat Math vs 首个版本范围

### 状态

**架构可行，内容范围高风险。**

POEDB 已记录诸如 Damage Conversion、Armour、Evasion、Leech 等机制数据和说明，因此可以作为第一版 Combat Math 设计参考。

但禁止：

```text
“既然最终按 POE 算法做，
所以 Vertical Slice 一次实现所有 POE 防御和伤害机制。”
```

正确方式：

```text
完整 Spec
+
分阶段实现
```

即：

```text
CombatMathSpec
可以较完整

Vertical Slice Runtime
只实现代表性子集
```

第一阶段只需要代表性证明：

```text
Physical
Fire
Hit
Crit
Resistance
Armour
Accuracy / Evasion
1 个 DoT
1 条 Conversion
```

其余：

```text
Energy Shield
Block
Suppression
复杂 Leech
高级 Ailment
Reflect
Damage Taken As
多阶段 Conversion
```

后置。

### 结论

**POEDB Baseline 可行，但必须 Snapshot + Spec + Golden Tests，不允许模型凭记忆写公式。**

---

## 3.3 Q28C vs Q62D

当前已选择：

```text
Q28C
敌方危险技能拥有视觉优先级

Q62D
高难内容允许大量极弱提示 / 近乎无提示攻击
```

### 状态

**存在体验张力。**

两项并非完全冲突。

可以定义为：

```text
正常内容：
危险机制强调可读

最高难内容：
部分机制允许降低 Telegraph
```

但必须增加一条底线：

> “弱 Telegraph”不能等于“没有任何可学习信息的随机秒杀”。

即使最高难度，也至少应存在一种：

```text
怪物姿态
动画起手
声音
位置规律
怪物类型知识
地图词缀预告
```

让玩家在学习后能够预判。

否则高难会从：

```text
Build / Knowledge Check
```

退化成：

```text
不可解释死亡
```

### 结论

**可行，但属于高风险 Game Feel 决定。**

---

## 3.4 免费 Respec + 无 Loadout

### 状态

**可行，有 UX 成本。**

玩家可以完全免费重构，但没有一键 Loadout。

这意味着：

```text
自由度非常高
+
频繁改 Build 操作较繁琐
```

这是一个合法设计选择。

必须保证：

```text
天赋搜索
批量退点
Support Compatibility Highlight
快速拆装技能
装备比较
Tooltip 清晰
```

否则“没有 Loadout”会从角色身份设计变成单纯 UI 折磨。

---

## 3.5 无章节 / End Game Is The Game

### 状态

**完全可行，但目前缺少 Progression Spine。**

没有章节并不代表没有进程。

项目后续仍然必须设计：

```text
玩家如何从 Level 1
逐渐进入真正最高难地图？
```

必须至少存在一种：

```text
Map Tier
区域等级
地图池解锁
Boss Gate
Modifier Capacity
Risk Rating
地图节点 / Atlas
```

否则：

```text
End Game Is The Game
```

可能变成：

```text
从第一分钟开始重复同一张随机地图
```

### 结论

**属于尚未设计的核心系统，不属于当前矛盾。**

---

# 4. 可行性矩阵

| 系统 | 可行性 | 风险等级 | 结论 |
|---|---|---:|---|
| Unity + Windows | 很高 | 低 | 直接执行 |
| URP + 强风格化 | 很高 | 低 | 非常适合 |
| Click-to-Move | 很高 | 中 | 需要重点做手感 |
| Traditional Unity | 高 | 中 | 必须有限制 |
| 150～300 敌人 | 高 | 高 | 必须 Performance Gate |
| 1440p / 120 FPS | 中高 | 高 | 必须锁定测试硬件 |
| POEDB Combat Math | 高 | 高 | Spec 完整、实现分阶段 |
| 巨型 Passive Tree | 高 | 中高 | 必须工具化生成 / 验证 |
| Active + Support | 很高 | 中 | Tag / Resolver 是核心 |
| Equipment Socket / Link | 很高 | 中 | 无孔色明显降低复杂度 |
| Crafting | 高 | 高 | 后续最复杂内容系统之一 |
| Unique | 很高 | 中 | 核心机制固定即可 |
| Map Modifier | 高 | 高 | 需要 Rule Engine |
| Map Risk / Reward | 高 | 高 | 需要模拟器 |
| Map Stability | 很高 | 低 | 简单可靠 |
| Build Lock in Map | 很高 | 低 | 与免费 Respec 配合良好 |
| Trigger Framework | 高 | 高 | 必须严格防递归 |
| Flask + 自动 Craft | 很高 | 中 | 可以后置 |
| Strong Stylized Art | 高 | 中 | Kitbash Hybrid 合理 |
| 大量独立 3D 资产 | 中 | 高 | 不应成为前期目标 |
| 高质量角色动画 | 中高 | 高 | 必须复用 Rig / Retarget |
| VFX 可读性 | 高 | 高 | 高密度下是长期任务 |
| Punch Audio | 高 | 中 | 需要 Voice Limiting / 聚合 |
| Minimal Music | 很高 | 低 | 降低音乐生产压力 |
| AI-only Production | 高 | 高 | 必须有严格流程和集成 Agent |

---

# 5. 最重要的可行性修订

以下不是重新询问 Game Director，而是工程执行规则。

---

## R1 — 游戏规模只能通过“内容扩展”增长，不能通过“核心架构数量”增长

后期新增：

```text
100 个技能
1000 个 Affix
500 个天赋
50 个 Map Mod
```

应该主要是：

```text
数据
+
已有 Effect / Modifier / Trigger 组合
```

而不是：

```text
100 个新系统
1000 个新 MonoBehaviour
```

---

## R2 — 机制必须先抽象，再批量生产内容

例如不能：

```text
先让 AI 写 50 个技能
↓
之后发现每个技能架构都不同
```

必须：

```text
实现 Projectile
实现 AoE
实现 Hit
实现 DoT
实现 Trigger
实现 Status
实现 Conversion
↓
再用它们组合技能
```

---

## R3 — Performance Gate 失败时，停止扩内容

性能失败不能处理为：

```text
以后优化
```

必须处理为：

```text
停止 Content Expansion
↓
找到瓶颈
↓
解决
↓
重新 Gate
```

---

## R4 — 美术和手感不能等系统完成后再做

已选 Q20D。

因此每个 Vertical Slice 必须包含：

```text
Animation
VFX
Audio
Camera
Hit Feel
UI
```

至少一轮完整 Polish。

---

## R5 — 所有长期规则必须进入仓库，不允许存在 AI“记忆”中

核心文档建议：

```text
/docs
    PRODUCT.md
    GAME_PILLARS.md
    DECISIONS.md
    ROADMAP.md

/docs/architecture
    RUNTIME.md
    MODIFIER_ENGINE.md
    SKILL_SYSTEM.md
    EVENT_TRIGGER.md
    SAVE_SYSTEM.md
    CONTENT_PIPELINE.md

/docs/combat-math
    SOURCE_POLICY.md
    DAMAGE.md
    DEFENCE.md
    ORDER_OF_OPERATIONS.md
    TEST_VECTORS.md

/docs/art
    ART_BIBLE.md
    VFX_BIBLE.md
    ANIMATION_RULES.md

/docs/audio
    AUDIO_BIBLE.md

/docs/qa
    PERFORMANCE_BUDGET.md
    TEST_STRATEGY.md
```

---

# 6. AI 团队组织结构

推荐不要按“多少个 AI”组织，而按**责任域**组织。

---

## A0 — Game Director（人类）

负责人：

```text
好不好玩
好不好看
爽不爽
是否符合项目方向
哪些机制保留
哪些机制删除
```

人类必须亲自验收：

```text
Combat Feel
Art Direction
Audio Feel
Boss Fairness
Build Fun
```

AI 不得用自动测试代替这些判断。

---

## A1 — Lead Architect AI

唯一责任：

```text
维护架构边界
审查模块依赖
维护 ADR
阻止重复系统
阻止架构漂移
```

禁止成为“大量写 Feature 的 AI”。

它是守门人。

---

## A2 — Unity Core AI

负责：

```text
Unity Project
GameObject 生命周期
Scene Bootstrap
Input
Save
Pooling
Tick Scheduler
Asset Loading
Camera
基础工具
```

---

## A3 — Combat Math AI

负责：

```text
POEDB Snapshot
CombatMathSpec
Damage Pipeline
Defence Pipeline
Formula Golden Tests
Calculation Trace
```

任何 Combat Formula 修改都必须先修改 Spec 与测试。

---

## A4 — Skill / Modifier AI

负责：

```text
Tags
Stats
Modifiers
Conditions
Effects
Supports
Skill Resolution
Trigger Framework
Aura
Curse
Status Runtime
```

这是项目最核心的 Gameplay Agent。

---

## A5 — Item / Craft AI

负责：

```text
Item Base
Affix
Rarity
Socket / Link
Unique
Craft Operation
Loot Roll
Drop Table
```

---

## A6 — Map / Endgame AI

负责：

```text
Map Definition
Monster Package
Map Modifier
Map Stability
Build Snapshot Lock
Risk Score
Reward Multiplier
Map Progression
```

---

## A7 — Monster / AI AI

负责：

```text
Monster Archetype
Steering
Targeting
AI LOD
Elite
Boss
Pack Behavior
On-death
```

---

## A8 — Tooling / Content Pipeline AI

负责：

```text
YAML / JSON Schema
Importer
Validator
Generated Assets
Passive Tree Tools
Affix Tools
Skill Compatibility
Content Reports
```

这个 Agent 的优先级非常高。

---

## A9 — Performance AI

负责：

```text
Profiler
PerformanceArena
Frame Time
GC
Animator
Navigation
Physics
Rendering
VFX Cost
Stress Test
```

拥有权力：

> Performance Gate 失败时阻止进入下一阶段。

---

## A10 — UI / UX AI

负责：

```text
HUD
Inventory
Tooltip
Passive Tree
Skill / Support UI
Craft UI
Map UI
Settings
Damage Number Options
```

---

## A11 — Technical Art / VFX AI

负责：

```text
URP
Shader
Material
VFX Graph / Particle System
VFX Priority
VFX Pool
Lighting
Asset Style Normalization
```

---

## A12 — Animation AI

负责：

```text
Humanoid Rig
Retarget
Animator
Blend
Attack Timing
Movement
Animation Events
High Attack Speed Presentation
```

---

## A13 — Audio AI

负责：

```text
Audio Event Layer
Punch Sound
Voice Priority
Voice Limiting
Hit Aggregation
Loot Cue
Monster Cue
Ambient Soundscape
```

---

## A14 — QA / Simulation AI

负责：

```text
Unit Test
Integration Test
Seed Reproduction
Build Fuzzer
Loot Simulator
Craft Simulator
Map Simulator
Regression
```

---

## A15 — Integration / Reviewer AI

这个 Agent **不开发新功能**。

只负责：

```text
检查提交
执行测试
检查架构违反
检查重复实现
检查文档
整合 Branch
```

对于多 AI 项目，它是必要角色。

---

# 7. AI 工作流

每个任务必须执行：

```text
Issue
↓
Design Spec
↓
Technical Spec
↓
Acceptance Criteria
↓
Implementation
↓
Automated Tests
↓
Performance Check
↓
Review
↓
Integration
↓
Human Feel Review（需要时）
```

---

# 8. AI Task 标准格式

任何 AI 任务禁止只写：

```text
“实现装备系统”
```

必须使用：

```text
TASK ID
Owner
Dependencies

Goal

In Scope

Out of Scope

Inputs

Outputs

Interfaces

Acceptance Criteria

Tests

Performance Constraints

Art / Audio Dependencies

Documentation Changes
```

---

# 9. 多 AI 并行规则

## Rule 1

同一时间：

> **一个文件 / 一个核心模块只有一个 Owner。**

---

## Rule 2

其他 Agent 不直接修改别人的模块。

需要变更：

```text
提交 Interface Change Request
```

---

## Rule 3

核心接口修改必须经过 Architect。

例如：

```text
Stat
Modifier
SkillSpec
Event
Item
MapRule
SaveData
```

---

## Rule 4

Content AI 不允许创造新 Runtime 概念。

如果内容需要：

```text
一个系统当前表达不了的新机制
```

必须先申请：

```text
Mechanic Feature
```

然后由系统 Agent 实现。

---

## Rule 5

AI 不允许“为了完成当前任务”复制一套类似代码。

必须优先复用：

```text
Tag
Modifier
Effect
Condition
Trigger
```

---

# 10. 代码所有权建议

```text
Runtime/Core          A1 + A2
Runtime/Combat        A3
Runtime/Skill         A4
Runtime/Items         A5
Runtime/Maps          A6
Runtime/AI            A7

Editor/Content        A8

Presentation/UI       A10
Presentation/VFX      A11
Presentation/Anim     A12
Presentation/Audio    A13

Tests                 对应 Owner + A14
Integration           A15
```

---

# 11. Phase 0 — 项目治理与骨架

## 目标

不做游戏内容。

先确保 AI 不会把项目写乱。

---

## 工作

### Architect AI

创建：

```text
DECISIONS.md
ARCHITECTURE.md
MODULE_OWNERS.md
CODING_STANDARD.md
TASK_TEMPLATE.md
```

---

### Unity Core AI

建立：

```text
Unity Project
URP
Folder Structure
Assembly Definitions
Bootstrap Scene
Test Scene
Input Layer
Logging
```

---

### Tooling AI

建立：

```text
Stable Content ID
Schema Version
Text Data Directory
Basic Importer
Validation CLI / Editor Tool
```

---

### QA AI

建立：

```text
EditMode Test
PlayMode Test
Seed Test
Build Validation Script
```

---

## Gate 0

满足：

```text
项目可无错误打开
所有测试可一次运行
文本数据可以 Import
错误数据会明确失败
固定 Seed 可重现
```

否则不进入下一阶段。

---

# 12. Phase 1 — Synthetic Performance Spike

**优先级高于复杂装备系统。**

---

## 内容

只做：

```text
玩家胶囊
300 个 Dummy Monster
简单移动
简单 Steering
基础 Animator
简单攻击
对象池 Hit VFX
简单 Projectile
```

没有：

```text
完整装备
完整天赋
Craft
Unique
完整 Combat Math
```

---

## 测试目标

验证：

```text
URP
300 个 GameObject
Animation
AI LOD
Navigation / Steering
Projectile
VFX
Pooling
```

是否有希望达到：

```text
1440p
120 FPS
```

---

## Performance AI 必须输出

```text
CPU Frame
Main Thread
Render Thread
GPU Frame
GC Alloc / Frame
Animator Cost
Navigation Cost
Physics Cost
VFX Cost
Draw Calls
Memory
```

---

## Gate 1

如果达不到目标：

优先修改：

```text
AI Tick Frequency
Animator LOD
Enemy Steering
Projectile Simulation
Shadow
Light
VFX
Pooling
```

仍然失败才允许局部：

```text
Jobs
Burst
Native Collections
```

**禁止第一反应是 Full DOTS 重写。**

---

# 13. Phase 2 — Core Rule Kernel

这一步是整个游戏真正的底座。

---

## 13.1 Stable ID / Tag

实现：

```text
Skill Tags
Damage Tags
Item Tags
Monster Tags
Map Tags
Effect Tags
```

---

## 13.2 Stat System

实现：

```text
Base
Flat
Increased
Reduced
More
Less
Minimum
Maximum
Override
```

---

## 13.3 Modifier System

实现：

```text
Source
Target
Scope
Tags
Conditions
Priority
```

---

## 13.4 Event System

实现：

```text
OnSkillUse
OnHit
OnCrit
OnKill
OnDamageTaken
OnBlock
OnResourceSpend
OnAilment
```

---

## 13.5 Trigger Safety

必须拥有：

```text
Recursion Guard
Trigger Depth
Cooldown
Chance
Tag Filter
Event ID
```

---

## Gate 2

使用纯测试证明：

```text
10000+ 随机 Modifier 组合
不会 Crash
不会 Infinite Loop
不会 NaN
不会 Invalid Stack
```

---

# 14. Phase 3 — Combat Math Spec

Combat Math AI 独立工作。

---

## 任务

冻结：

```text
POEDB Reference Snapshot
```

并建立：

```text
Damage Types
Hit
Crit
Accuracy
Evasion
Armour
Resistance
Conversion
DoT
Ailment
Leech
Defence Layers
Order of Operations
```

---

## 注意

此阶段：

```text
Spec 可以较完整
Runtime 不必全部实现
```

---

## Golden Tests

每条核心规则必须存在：

```text
Input
Expected Result
Source Reference
```

以后 AI 修改公式：

```text
Golden Test Fail
=
必须解释
```

---

# 15. Phase 4 — Combat Feel Slice

在完整 Loot Loop 之前先确认最基本手感。

---

## 实现

```text
Click-to-Move
Player Facing
Attack Range
Auto Approach
Skill Cast
Cancel Window
Input Buffer
3 种技能原型

Melee
Projectile
AoE
```

---

## 表现

必须同时有：

```text
Animation
VFX
Audio
Camera
Hit Reaction
Monster Death
```

---

## 人类 Gate

Game Director 必须亲自判断：

```text
移动是否舒服
点击是否准确
技能是否有重量
Hit 是否爽
声音是否有冲击
怪物死亡是否满足
```

这一步不能由自动测试代替。

---

# 16. Phase 5 — Micro Complete Loop（Vertical Slice #1）

严格按照已选 Q30C。

---

## Character

```text
1 个角色
基础等级
Life / Mana
基础三属性
```

---

## Skill

```text
3 Active Skills
6 Supports
```

至少：

```text
1 个 Support
真正改变技能机制
```

而不是全部：

```text
+20% Damage
```

---

## Item

```text
3～4 装备槽
Ordinary
Rare
至少 10 Affixes
Socket / Link
```

---

## Passive

```text
10～20 Nodes
1～2 Notable
1 Mechanic Node
```

---

## Monster

```text
3 普通怪
1 Elite
```

---

## Map

```text
1 Map
2～3 Map Mods
Map Stability
Reward Multiplier
Build Lock
```

---

## Craft

只做：

```text
1 个随机 Craft
1 个定向 Craft
```

---

## Audio / VFX / UI

全部必须达到：

```text
可判断是否好玩的最低质量
```

不能全部灰盒。

---

## Slice Loop

```text
进入地图
↓
杀怪
↓
掉装备 / 资源
↓
制作 / 换装 / 改 Link
↓
Build 发生变化
↓
选择更危险词缀
↓
重新进入地图
```

---

# 17. Phase 6 — Vertical Slice Performance Gate

这一次测试真实系统：

```text
Skill
Support
Modifier
Hit
Trigger
Loot
Map Mod
Audio
VFX
UI
```

在高密度下的性能。

---

## Gate 3

目标：

```text
系统复杂度增加以后
仍然具有接近最终性能目标的路径
```

如果失败：

> 暂停增加技能、装备和天赋数量。

先优化。

---

# 18. Phase 7 — Production Tooling

只有 Vertical Slice 成立以后，才值得投资“大规模内容工具”。

---

## Tool 1 — Skill Validator

自动检查：

```text
Tag
Support Compatibility
Missing VFX
Missing Audio
Invalid Trigger
```

---

## Tool 2 — Affix Generator / Validator

检查：

```text
Item Tag
Prefix / Suffix
Tier
Level
Weight
Conflict Group
```

---

## Tool 3 — Passive Tree Tool

支持：

```text
Graph
Connectivity
Auto-layout Helper
Search
Region
Path Cost
Disconnected Node Detection
```

---

## Tool 4 — Map Mod Validator

自动计算：

```text
风险
组合
冲突
潜在硬克制
```

---

## Tool 5 — Content Report

每天可以输出：

```text
技能总数
Support 总数
Affix 总数
Unique 总数
被引用 Stat
未使用 Tag
缺失 Asset
测试失败项
```

---

# 19. Phase 8 — 系统扩张

按价值逐个加入。

推荐顺序：

```text
完整装备槽
↓
更多 Affix
↓
更完整 Passive Tree
↓
Aura / Reservation
↓
Curse
↓
Flask
↓
更多 Defense
↓
更完整 Ailment
↓
高级 Trigger
↓
Unique
↓
Jewel
```

---

# 20. Phase 9 — Skill Gem 高级成长

已明确前期不做。

后期再加入：

```text
Quality
Corruption
Ascension
Alternate Form
Final Upgrade
```

这些必须在核心 SkillInstance 已稳定后添加。

---

# 21. Phase 10 — Crafting Deepening

这是后期核心内容。

逐步加入：

```text
Add Affix
Remove Affix
Reroll
Lock
Targeted Reroll
Upgrade Tier
Special Craft
Final Craft
Corruption
```

---

## Craft Simulator

AI 必须能执行：

```text
100000 次 Craft
```

分析：

```text
达到目标装备的平均成本
P50
P90
P99
```

否则制作系统无法平衡。

---

# 22. Phase 11 — End Game Expansion

此时才真正扩大地图系统。

---

## 必须建立

```text
Map Tier / Progression
Map Modifier Pool
Modifier Synergy
Map Stability
Reward Multiplier
Monster Package
Boss
Special Encounter
```

---

## Map Risk Simulator

输入：

```text
Map Mods
Monster Package
Boss
```

输出：

```text
Base Risk
Synergy Risk
Expected Reward
Potential Build Counters
```

---

# 23. Phase 12 — 内容工厂

当所有核心机制已经稳定后，AI 才开始大规模生产：

```text
技能
Support
Affix
Unique
Passive
Jewel
Monster
Map Mod
Boss Mod
```

原则：

```text
80% 内容
=
组合已有机制

20% 内容
=
提出新机制需求
```

如果比例反过来：

> 架构会失控。

---

# 24. 美术 AI 工作计划

---

## Step A — Art Bible

先固定：

```text
角色比例
材质语言
轮廓
颜色
地图明暗
危险色
友方技能色
敌方技能色
Rare / Unique Drop Visual
```

---

## Step B — Asset Intake Pipeline

所有外部 Asset 自动检查：

```text
Scale
Rig
Material
Shader
Texture
LOD
Collider
Naming
Pivot
```

---

## Step C — Style Normalization

使用：

```text
统一 URP Shader
统一 Lighting
统一 Palette
统一 Post Process
```

消除 Asset Pack 拼接感。

---

## Step D — Unique Asset Priority

独立制作资源优先投给：

```text
玩家角色
核心技能
Boss
代表性怪物
关键 Unique
关键地图地标
```

而不是：

```text
普通箱子
普通墙
普通石头
普通树
```

---

# 25. Animation AI 工作计划

建立：

```text
Shared Humanoid Rig
Retarget Pipeline
```

第一阶段只需要：

```text
Idle
Run
Turn
Basic Attack
Cast
Hit
Death
```

再逐步扩展。

---

## 高攻速规则

Gameplay 和 Animation 已决定解耦。

因此 Animation AI 必须提供：

```text
Normal Rate
Fast Rate
Extreme Rate
```

三个表现区间。

极端攻速依靠：

```text
Blend
Trail
FX
Sound Rhythm
```

而不是要求骨骼完整播放每一次逻辑 Hit。

---

# 26. VFX AI 工作计划

建立 VFX Priority：

```text
P0 Instant Danger
P1 Enemy Mechanic
P2 Elite / Boss State
P3 Player Core Skill
P4 Player Secondary
P5 Normal Hit
P6 Decoration
```

---

## 性能

高密度战斗必须支持：

```text
Pool
Emission Limit
Distance Cull
Screen Density Limit
Duplicate Effect Reduction
```

---

# 27. Audio AI 工作计划

已选择：

```text
Minimal Music
+
Heavy Punch Combat Audio
```

所以 Sound Design 优先级很高。

---

## Audio Event Layer

例如：

```text
Skill.Cast
Skill.Impact
Hit.Normal
Hit.Crit
Monster.Death
Elite.Death
Loot.Rare
Loot.Unique
Danger
```

---

## 高频限制

必须：

```text
Voice Limit
Priority
Cooldown
Aggregation
Pitch Variation
Distance Filtering
```

避免高攻速下每秒数百声音同时播放。

---

# 28. QA / Simulation 体系

必须拥有 6 种测试。

---

## 1. Unit Test

单个系统。

---

## 2. Integration Test

系统组合。

---

## 3. Golden Test

Combat Math 固定结果。

---

## 4. Seed Replay

Bug 可百分之百重现。

---

## 5. Fuzz Test

随机生成：

```text
Skill
Support
Modifier
Trigger
Map Mod
```

寻找异常。

---

## 6. Performance Test

固定场景自动跑：

```text
100
200
300
```

敌人。

输出帧时间。

---

# 29. AI 自动模拟器优先级

建议按以下顺序：

```text
1. Combat Simulator
2. Loot Simulator
3. Craft Simulator
4. Build Simulator
5. Map Risk Simulator
```

不要一开始就试图制作完整 PoB。

---

# 30. 当前禁止开发清单

首个 Vertical Slice 完成以前：

**禁止正式生产：**

```text
500+ 天赋节点
50+ 技能
大量 Unique
完整 Crafting
完整高级 Gem
完整 Atlas
大量 Boss
程序化地图大系统
完整所有 POE Combat Edge Case
Full DOTS 重写
FMOD / Wwise
多人游戏
Console
```

---

# 31. 当前必须优先完成的 12 个 AI 工作包

## WP-001 — Project Governance

Owner：

```text
Architect AI
```

输出：

```text
Architecture
ADR
Module Ownership
Task Format
```

---

## WP-002 — Unity Bootstrap

Owner：

```text
Unity Core AI
```

输出：

```text
URP Project
Bootstrap
Test Scenes
Input
Base Camera
```

---

## WP-003 — Content Pipeline v0

Owner：

```text
Tooling AI
```

输出：

```text
Text Schema
Importer
Validator
Stable IDs
```

---

## WP-004 — Deterministic Core

Owner：

```text
Unity Core + QA
```

输出：

```text
Seeded RNG
Replay IDs
Save Version
```

---

## WP-005 — PerformanceArena

Owner：

```text
Performance + Monster AI
```

输出：

```text
300 Dummy Stress Test
Profiler Report
```

---

## WP-006 — Stat / Modifier Kernel

Owner：

```text
Skill / Modifier AI
```

输出：

```text
Stat
Modifier
Tag
Condition
Resolver
```

---

## WP-007 — Event / Trigger Kernel

Owner：

```text
Skill / Modifier AI
```

输出：

```text
Events
Triggers
Recursion Guard
```

---

## WP-008 — CombatMathSpec v0.1

Owner：

```text
Combat Math AI
```

输出：

```text
POEDB Snapshot
Order of Operations
Golden Test Vectors
```

---

## WP-009 — Click-to-Move Feel Prototype

Owner：

```text
Unity Core + Animation + Audio + VFX
```

输出：

```text
Movement
Targeting
Attack
Cast
Hit
Death
```

---

## WP-010 — Micro Skill System

Owner：

```text
Skill AI
```

输出：

```text
3 Active
6 Supports
Socket / Link Resolver
```

---

## WP-011 — Micro Loot / Craft

Owner：

```text
Item AI
```

输出：

```text
Ordinary
Rare
10 Affix
2 Craft Actions
```

---

## WP-012 — Micro Map Loop

Owner：

```text
Map AI
```

输出：

```text
1 Map
2～3 Mod
Stability
Build Lock
Reward Multiplier
```

---

# 32. 第一阶段并行关系

可以并行：

```text
WP-001 Governance
WP-002 Unity Bootstrap
WP-003 Content Pipeline
WP-008 Combat Math Research
```

然后：

```text
WP-005 PerformanceArena
WP-006 Modifier Kernel
WP-009 Feel Prototype
```

再进入：

```text
WP-007 Trigger
WP-010 Skill
WP-011 Loot
WP-012 Map
```

最后：

```text
Micro Complete Loop
↓
Performance Gate
↓
Human Review
```

---

# 33. AI 工作优先级原则

每一个新功能都按照以下优先级判断：

```text
1. 是否支撑 Core Loop？
2. 是否是多个系统共享的基础能力？
3. 是否能被数据化复用？
4. 是否已经通过性能验证？
5. 是否需要现在做？
```

如果第 5 项答案是：

```text
“以后可能需要”
```

默认：

> **现在不做。**

---

# 34. Game Director 每个 Slice 只需要做什么

你的工作不应该是审查：

```text
类名
算法细节
文件结构
代码风格
```

这些由 AI 负责。

你每个 Slice 主要回答：

```text
1. 移动爽吗？
2. 技能爽吗？
3. 怪物杀起来爽吗？
4. Build 变化明显吗？
5. Loot 想捡吗？
6. Craft 有决策吗？
7. 高风险地图值得冒险吗？
8. 画面是否清楚？
9. 美术是否统一？
10. 声音是否有力量？
```

---

# 35. 下一批必须由人类决定但尚未讨论的问题

这些不是当前开发阻塞项，可以后续继续 ABCD 讨论。

---

## End Game Progression

```text
没有章节以后
玩家怎样逐步解锁更高等级地图？
```

---

## Map Generation

```text
固定布局
模块化拼接
程序化生成
混合
```

---

## Exact Target Hardware

当前只有：

```text
High-end PC
1440p
120 FPS
```

但没有固定：

```text
CPU
GPU
RAM
```

没有具体硬件，就无法形成真正客观 Performance Gate。

---

## Inventory

```text
格子
重量
无限列表
仓库
```

尚未锁定。

---

## Loot Filter

高密度 End Game 几乎必然需要。

---

## Jewel 具体结构

目前只确定“存在珠宝”，没有锁定：

```text
普通 Jewel
Cluster
特殊 Jewel
天赋树修改 Jewel
```

---

## Map Progression / Atlas-like Structure

目前仍为空白。

---

## Boss Philosophy

尚未锁定：

```text
Boss 密度
Boss 时长
阶段
机制
奖励
```

---

# 36. 最终可行性判断

## 技术

**可行。**

Unity 6 系列目前拥有 URP、AI Navigation、Job System、Burst、Entities 等可用能力；即使默认采用 Traditional Unity，也存在明确的局部性能优化路径。Unity 官方同时强调 URP 性能需要通过 Profiler / GPU Profiling 等方式分析，而不是仅凭配置推测。

---

## 玩法系统

**可行。**

POE 级 Combat Math、Skill / Support、Affix、Passive、Map Mod 都可以用：

```text
Tag
Modifier
Condition
Effect
Trigger
```

统一表达。

关键是不能把每个机制写成独立例外。

---

## 内容规模

**长期可行，短期不可一次完成。**

项目必须先变成：

```text
Content Factory
```

然后才扩大内容。

---

## 美术

**可行，但必须接受强风格化 + Kitbash Hybrid。**

如果以后目标重新变成：

```text
AAA 写实
+
大量独有角色
+
大量独有动画
```

可行性会显著下降。

---

## AI 团队

**可行，但只有严格治理时可行。**

最大风险不是 AI 不会写代码。

最大风险是：

> **多个 AI 同时产生各自合理、彼此不兼容的系统。**

因此 Architect、Integration、Tests 和 Repository-as-Memory 不是辅助工作。

它们本身就是核心生产系统。

---

# 37. 立刻执行顺序

当前不应该继续写大量 Game Design 内容。

下一步真正值得启动的工作顺序是：

```text
01 Governance
02 Unity Bootstrap
03 Text Content Pipeline
04 Deterministic RNG / IDs
05 300 Monster PerformanceArena
06 Stat / Tag / Modifier Kernel
07 CombatMathSpec
08 Click-to-Move Feel Prototype
09 Event / Trigger Kernel
10 Skill / Support Micro System
11 Item / Affix / Craft Micro System
12 Map / Stability Micro System
13 Micro Complete Loop
14 Performance Gate
15 Human Feel / Art / Audio Review
```

通过之后，才开始：

```text
扩大技能
扩大天赋
扩大词条
扩大装备
扩大地图
```

---

# 38. 结论状态

```text
PROJECT FEASIBILITY
GO — CONDITIONAL

ENGINE
GO

CORE ARCHITECTURE
GO

COMBAT MATH
GO — PHASED IMPLEMENTATION

300 ENEMIES
GO — PERFORMANCE GATED

1440P / 120 FPS
GO — HARDWARE BASELINE REQUIRED

ART
GO — STYLIZED + KITBASH

AUDIO
GO

AI PRODUCTION
GO — STRICT GOVERNANCE REQUIRED

FULL POE-LIKE CONTENT SCALE
NOT YET
MUST BE EARNED THROUGH TOOLING + VALIDATION
```

---

# 39. 当前推荐的唯一项目目标

在进入大规模内容生产之前，只追求一个目标：

> **做出一个 10～20 分钟可以重复游玩的 Micro ARPG Loop，让玩家能通过一次装备 / Support / 天赋变化明显改变技能，并用变化后的 Build 挑战一张更危险、收益更高的地图，同时在目标架构下证明高密度战斗性能存在可实现路径。**

一旦这一目标成立：

> 项目从“设计概念”正式进入“可扩张游戏”。

如果这一目标不成立：

> 继续增加技能、装备和天赋只会放大问题。

---

# 40. 技术核验来源

本审计涉及的当前 Unity / POEDB 状态参考：

- Unity 6 Releases & Support — Unity 6.3 LTS 支持政策：  
  https://unity.com/releases/unity-6/support

- Unity 6.3 LTS announcement：  
  https://unity.com/blog/unity-6-3-lts-is-now-available

- Unity Manual — URP performance：  
  https://docs.unity3d.com/6000.0/Documentation/Manual/urp/understand-performance-landing.html

- Unity Manual — Job System：  
  https://docs.unity3d.com/6000.0/Documentation/Manual/job-system-overview.html

- Unity Manual — Burst：  
  https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.burst.html

- Unity Manual — Entities：  
  https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.entities.html

- Unity Manual — AI Navigation：  
  https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.ai.navigation.html

- Unity Manual — UI Toolkit：  
  https://docs.unity3d.com/6000.0/Documentation/Manual/ui-systems/introduction-ui-toolkit.html

- Unity Manual — Addressables：  
  https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.addressables.html

- PoEDB — Damage Conversion：  
  https://poedb.tw/us/Damage_conversion

- PoEDB — Armour：  
  https://poedb.tw/us/Armour

- PoEDB — Evasion：  
  https://poedb.tw/us/Evasion

- PoEDB — Leech：  
  https://poedb.tw/us/Leech

---

**《ARPG 项目可行性审计与 AI 工作计划》结束。**
