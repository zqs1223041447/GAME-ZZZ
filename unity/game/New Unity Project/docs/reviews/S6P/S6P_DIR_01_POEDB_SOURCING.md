# S6P-DIR-01 · poedb.tw 美术摄取台账

> 导演 2026-09-11 补充要求 4/5：「装备图标现在是纯2D，我需你从POEDB.TW上扒取装备图标来用」、
> 「技能特效顺别也从POEDB.TW上扒取，制作冰茅和火球术」。
>
> 本台账是**逐条来源真值**：每个落盘资源 = 一条 poedb 页面 → CDN 绝对路径 → 本工程内相对 key。
> 摄取脚本 = `tools/poedb/fetch_art.py`（清单与运行时 `SlicePoeArt.DeclaredKeys` 逐条对应，
> 由 `S6PDir01Tests.PoeArt_DeclaredKeys_*` / `PoeArt_Folders_HaveNoUndeclaredTextures` 锁定）。

## 0. 摄取方式与限制（先说清楚，避免误解）

| 项 | 事实 |
|---|---|
| 站点 | `https://poedb.tw/us/<页面>`（PoE1 数据），图片 CDN = `https://cdn.poedb.tw/image/<内部路径>` |
| 可用格式 | **仅 `.webp`**（同路径 `.png` 一律 HTTP 403；`.webp` HTTP 200 —— 实测记录见下） |
| 处理 | `fetch_art.py` GET → Pillow 解码 → 转 RGBA → 写 `.png` + Unity `TextureImporter` `.meta`（默认 Texture2D、sRGB、alphaIsTransparency、无 mipmap） |
| 未做 | 不整包下载、不抓 HTML 页面正文入工程、不改任何 gameplay 数据 |
| **poedb 不发布粒子系统** | CDN 上的是**美术图**（宝石图 / 物品图 / 特效 MTX 图标），不是技能特效的粒子资产。本工程把它们作为**投射物广告牌贴图**与**槽位图标**使用；飞行、拖尾、命中爆炸的粒子行为仍由引擎自绘（`ArenaDirector.BuildProjectileArt` / `SliceHudIcons`）。这一区分写进报告，不冒称「扒到了粒子特效」。 |

实测（`.png` vs `.webp` 同路径）：

```
ERR 403 https://cdn.poedb.tw/image/Art/2DArt/SkillIcons/IceSpear.png
OK  200  1792 https://cdn.poedb.tw/image/Art/2DArt/SkillIcons/IceSpear.webp
```

## 1. 装备图标（`Assets/Resources/UI/PoE/Items/`）

普通/稀有各一档；导演原话「装备图标现在是纯2D」→ 本批替换掉程序化槽位符形。

| 目标 key | poedb 页面 | CDN 内部路径 | 尺寸 | 选取理由 |
|---|---|---|---|---|
| `Items/Weapon_Ordinary` | `Rusted_Sword` | `Art/2DItems/Weapons/OneHandWeapons/OneHandSwords/OneHandSword1.webp` | 78×234 | 单手剑 1 阶基底（=游戏内「铁刃」的最低阶同型） |
| `Items/Weapon_Rare` | `Rusted_Sword` | `…/OneHandSwords/OneHandSword5.webp` | 156×234 | 同族 5 阶：稀有度更高一档的观感 |
| `Items/Body_Ordinary` | `Plate_Vest`（族） | `Art/2DItems/Armours/BodyArmours/BodyDex1A.webp` | 156×234 | 皮质/闪避系胸甲基底 |
| `Items/Body_Rare` | `Plate_Vest`（族） | `Art/2DItems/Armours/BodyArmours/BodyStr2A.webp` | 156×234 | 力量系较重的一档 |
| `Items/Helmet_Ordinary` | `Rusted_Coif` | `Art/2DItems/Armours/Helmets/HelmetStrInt1.webp` | 156×156 | 锈铁头巾——「铁盔」的字面同型 |
| `Items/Helmet_Rare` | — | `Art/2DItems/Armours/Helmets/HelmetStr4.webp` | 156×156 | 4 阶金属盔（更高稀有度观感） |
| `Items/Gloves_Ordinary` | `Wool_Gloves` | `Art/2DItems/Armours/Gloves/GlovesInt1.webp` | 156×156 | 布/智系手套——「布手」同型 |
| `Items/Gloves_Rare` | — | `Art/2DItems/Armours/Gloves/GlovesStr5.webp` | 156×156 | 5 阶力量系护手 |
| `Items/Boots_Ordinary` | `Iron_Greaves` | `Art/2DItems/Armours/Boots/BootsStr1.webp` | 156×156 | 铁护胫——「旧靴」的最低阶同型 |
| `Items/Boots_Rare` | — | `Art/2DItems/Armours/Boots/BootsInt1.webp` | 156×156 | 智系软靴（稀有档差异） |
| `Items/Belt_Ordinary` | `Rustic_Sash` | `Art/2DItems/Belts/Belt1.webp` | 156×78 | 粗布腰带——「皮带」同型 |
| `Items/Belt_Rare` | `Leather_Belt` | `Art/2DItems/Belts/Belt7.webp` | 156×78 | 7 阶腰带（稀有档差异） |

> 物品图是**基底美术**（同基底不同词缀在 PoE 里本就同图），本工程按 `EquipSlot × Rarity` 取图，
> 不做「词缀决定图标」的虚构映射——那会造出游戏里不存在的规则。

## 2. 主动技能宝石图（`Assets/Resources/UI/PoE/Skills/`）

| 目标 key | poedb 页面 | CDN 内部路径 | 说明 |
|---|---|---|---|
| `Skills/Melee` | `Heavy_Strike` | `Art/2DItems/Gems/HeavyStrike.webp` | 近战（Q）的 PoE 近战宝石代表图 |
| `Skills/Projectile` | `Split_Arrow` | `Art/2DItems/Gems/SplitArrow.webp` | 弹道（W）——分裂箭是「多枚弹道」的代表 |
| `Skills/Area` | `Firestorm` | `Art/2DItems/Gems/Firestorm.webp` | 范围（E）——范围法术代表 |
| `Skills/IceSpear` | `Ice_Spear` | `Art/2DItems/Gems/IceSpear.webp` | 冰矛（R）本体 |
| `Skills/Fireball` | `Fireball` | `Art/2DItems/Gems/Fireball.webp` | 火球术（T）本体 |

> 前三条是**呈现层映射**（本工程的「近战/弹道/范围」是抽象技能，非 PoE 具体宝石）；
> 逐条选型理由如上，属人工决策，不宣称等同。

## 3. 辅助宝石图（`Assets/Resources/UI/PoE/Supports/`）

来源页 = `Support_Gems` 列表页逐条相邻 `<img>`（按内部名核对），或该辅助自身页面。

| 目标 key | 支援的 SupportId | CDN 内部路径 | 说明 |
|---|---|---|---|
| `Supports/AddedFire` | `AddedFire`（燃烧） | `Support/AddedFireDamage.webp` | 同名 |
| `Supports/Brutal` | `Brutal`（残暴） | `Support/Brutality.webp` | 残暴 = Brutality |
| `Supports/Concentrated` | `Concentrated`（集中） | `Support/ConcentratedAOE.webp` | 集中效应（内部名 ConcentratedAOE） |
| `Supports/Faster` | `Faster`（迅捷） | `Support/FasterAttacks.webp` | 迅捷攻击 |
| `Supports/Combustion` | `Combustion`（燃尽） | `Support/ChancetoIgnite.webp` | **poedb 对 Combustion Support 复用 Chance to Ignite 美术**（列表页实测），照抄不臆造 |
| `Supports/Fork` | `Fork`（分裂） | `Support/Fork.webp` | 同名 |
| `Supports/FireConversion` | `FireConversion`（火焰转化） | `Support/ColdtoFire.webp` | 元素转伤系的代表图；本工程该 Support 的 Stat 来源=Avatar of Fire 词条（`+x% of Physical, Cold and Lightning Damage Converted to Fire`），故取元素转伤图而非不存在的「物转火」图 |
| `Supports/ReturningProjectiles` | `ReturningProjectiles` | `Support/ReturnProjectiles.webp` | **PoE1 确有 Returning Projectiles Support**（页面 `Returning_Projectiles_Support`），取本体图 |
| `Supports/SnipersMark` | `SnipersMark` | `Art/2DItems/Gems/ProjectileWeakness.webp` | 狙击印记 = Sniper's Mark，内部名 `ProjectileWeakness`（旧名 Projectile Weakness），取本体图 |

## 4. 技能特效图（`Assets/Resources/UI/PoE/Vfx/`）

| 目标 key | CDN 内部路径 | 处理 | 用途 |
|---|---|---|---|
| `Vfx/IceSpear` | `Art/2DArt/SkillIcons/IceSpear.webp` | **抠背景** | 冰矛弹体贴图（冷青细长） |
| `Vfx/Fireball` | `Art/2DArt/SkillIcons/iconfireball.webp` | **抠背景** | 火球术弹体贴图（炽橙火球） |
| `Vfx/IceSpearMtx` | `Art/2DItems/Effects/VoidEmperorIceSpearEffect.webp` | 原图（自带 alpha） | 登记未消费（特效档位切换备用） |
| `Vfx/IceSpearMtxAlt` | `Art/2DItems/Effects/AuspiciousIceSpearEffect.webp` | 原图（自带 alpha） | 登记未消费 |

### 4.1 为什么这两条要抠背景（唯一派生化处理）

poedb 的施法图标（`Art/2DArt/SkillIcons/*`）是**不透明方图**：实测 alpha 恒 255，背景是深色但**不是近黑**
（火球图标角落 luma≈61、冰矛图标角落 luma≈27 且右上角存在亮像素）。因此：

- 不处理 → 贴图球体/广告牌上会带一块深色底板或方晕；
- 纯 luma 阈值抠除 → 残留方晕（实测）；
- 加色混合 → 饱和方片（实测）。

最终处理 = **亮度软阈值抠除**（`keyout_background`：luma ≤ 26 → alpha 0；26–56 线性过渡；≥ 56 保留原 alpha），
并且**弹体形态改为一律不透明贴图球体（Unlit 着色）**——贴图球体没有平面边界，残余暗边不会形成「方片」观感。
抠除只作用于 `Vfx/IceSpear`、`Vfx/Fireball` 两项，其余素材为原图。

> 这是**本工程对来源图做的唯一派生化处理**。若导演/规划 AI 要求「不得改造来源图」，撤掉 `keyout_background`
> 即可（`fetch_art.py` 单点），届时弹体退回宝石图（`Skills/*`，自带 alpha 但画的是镶金宝石）。

### 4.2 弹体形态的四轮实测取舍（留档，避免重复踩坑）

| 形态 | 结果 |
|---|---|
| 平面广告牌 + 不抠图 | 深色方块跟着飞（`03_*` 早期取证） |
| 平面广告牌 + 亮度抠图 | 方形残余边缘（浅色地面上形成菱形晕） |
| 平面广告牌 + 加色混合 | 整块饱和方片（青/白） |
| **贴图球体（Unlit）** | **无方晕、美术本色**（冷青冰矛 / 炽橙火球）→ 采用 |

> 也试过用**宝石图**（自带 alpha）做弹体——alpha 干净但画的是「镶金宝石」，作为冰矛/火球弹体辨识不足，故仅作回退。

## 5. 来源页清单（可复核）

```
https://poedb.tw/us/Ice_Spear
https://poedb.tw/us/Fireball
https://poedb.tw/us/Snipers_Mark
https://poedb.tw/us/Returning_Projectiles_Support
https://poedb.tw/us/Support_Gems
https://poedb.tw/us/Heavy_Strike
https://poedb.tw/us/Split_Arrow
https://poedb.tw/us/Firestorm
https://poedb.tw/us/Rusted_Sword
https://poedb.tw/us/Plate_Vest
https://poedb.tw/us/Rusted_Coif
https://poedb.tw/us/Wool_Gloves
https://poedb.tw/us/Iron_Greaves
https://poedb.tw/us/Leather_Belt
https://poedb.tw/us/Rustic_Sash
```

## 6. 复现

```pwsh
python tools/poedb/fetch_art.py            # 幂等：缺什么补什么
python tools/poedb/fetch_art.py --force    # 全部重下
```

脚本只写 `Assets/Resources/UI/PoE/{Items,Skills,Supports,Vfx}` 下的 `.png` 与 `.meta`；
既有 `.meta` 的 guid 会被保留（重下不换 GUID，避免引用断裂）。

## 7. 资产清单（路径 + SHA-256 + 用途 + 授权状态）

> 规划 AI（Channel A）2026-09-11 裁定要求：外部素材必须有 **URL/来源 + checksum + Unity 路径 + 用途**，
> 且在**没有明确产品发行授权**的情况下，状态必须写 **PROTOTYPE / REFERENCE ONLY / NOT PRODUCTION-ADMITTED**，
> 不得因为「能下载」就视为版权许可。本表照此执行。

| 目录 | 文件 | 字节 | SHA-256 | 用途 |
|---|---|---|---|---|
| Items | `Belt_Ordinary.png` | 22178 | `fc2b77b8764f799f95b1d431dba8d55c6c38912313b6da16f9a966b243c919f4` | 装备/物品图标（背包格 + 装备卡 + tooltip 底图） |
| Items | `Belt_Rare.png` | 19015 | `365df2f35e96d07606eb0b60d8f66839310a3a3842d37f455a97906a874c7fb5` | 装备/物品图标（背包格 + 装备卡 + tooltip 底图） |
| Items | `Body_Ordinary.png` | 36672 | `3a090d12e6bd547a7e403f1bf8bbbb668705a60766a4079b11563fa445cc5312` | 装备/物品图标（背包格 + 装备卡 + tooltip 底图） |
| Items | `Body_Rare.png` | 42153 | `04e482975cf802cfaee123d83de6b700025b5cab9cf9d62ff1a70b55fe77882b` | 装备/物品图标（背包格 + 装备卡 + tooltip 底图） |
| Items | `Boots_Ordinary.png` | 23594 | `ab9ffa72cf4070b1fc1dd67e3d77876e9a7fad7ccad200100510bb6f9857b2ec` | 装备/物品图标（背包格 + 装备卡 + tooltip 底图） |
| Items | `Boots_Rare.png` | 25469 | `18b0a5331a1807f1da5ee359ef8eb1c803e92348b5c23f0dfb517f9f52f88493` | 装备/物品图标（背包格 + 装备卡 + tooltip 底图） |
| Items | `Gloves_Ordinary.png` | 20656 | `0e27da12af6b10c9d15640fa0f9c92294607771c70d6f6236303f00cad3c6790` | 装备/物品图标（背包格 + 装备卡 + tooltip 底图） |
| Items | `Gloves_Rare.png` | 40066 | `f3b2f2ff38a58cf19360a4d5344acb34cbb0eb532139ec943bc44723034c9f3d` | 装备/物品图标（背包格 + 装备卡 + tooltip 底图） |
| Items | `Helmet_Ordinary.png` | 33619 | `f09aa4d6132f5c0d71d7d0ec0a32aeb27e01dbe1a51742bfc5fbb21ab0a38a4a` | 装备/物品图标（背包格 + 装备卡 + tooltip 底图） |
| Items | `Helmet_Rare.png` | 29139 | `be464bc97f99e1088faf279501607cfe8b69560ec889780d30776f80cf4e05d5` | 装备/物品图标（背包格 + 装备卡 + tooltip 底图） |
| Items | `Weapon_Ordinary.png` | 11091 | `767b82b3e99ba73832b47a95a761570ba8805e5cbd74fabbf5408c771bb9cddc` | 装备/物品图标（背包格 + 装备卡 + tooltip 底图） |
| Items | `Weapon_Rare.png` | 19017 | `b2bf647ea388d2621149cf5e79d1b0b6f603d075367d2d5954cf8cc10aee0e6e` | 装备/物品图标（背包格 + 装备卡 + tooltip 底图） |
| Skills | `Area.png` | 13256 | `0e62bb72ac1c6edc3462669e744c41d435a41165185aabcbe8e44a9286821fcd` | 主动技能宝石图（技能槽图标；4 条既有技能亦是弹体回退贴图） |
| Skills | `Fireball.png` | 10847 | `cfa2c746dc2b72b3aa87c2d3bc385d10dc59995d8d1f61b86231c7e397d82e37` | 主动技能宝石图（技能槽图标；4 条既有技能亦是弹体回退贴图） |
| Skills | `IceSpear.png` | 10074 | `0801e6ff062992b2bb041171662d0143207977667023fa232a7b6c929772a4d2` | 主动技能宝石图（技能槽图标；4 条既有技能亦是弹体回退贴图） |
| Skills | `Melee.png` | 10098 | `5e16e9508fab65c4dcca5098a1e8b3cfa71b0b1299ed78bf87573d06a47c9ffb` | 主动技能宝石图（技能槽图标；4 条既有技能亦是弹体回退贴图） |
| Skills | `Projectile.png` | 10475 | `59c68d061c75e69f7c4a0714cd0440322ec9143dc8d3296e5df3008d2e7d576a` | 主动技能宝石图（技能槽图标；4 条既有技能亦是弹体回退贴图） |
| Supports | `AddedFire.png` | 8267 | `620d046e9ccc0e95676d11bf369aa88309febf32c577b35bc3d16c72247222f6` | 辅助宝石图（辅助托盘格） |
| Supports | `Brutal.png` | 10978 | `a7262375c4b8be8478d53b52c83052d5bfbf6ed23949113e62c93238450d6c17` | 辅助宝石图（辅助托盘格） |
| Supports | `Combustion.png` | 12589 | `636edee2bf599fb8d6fc485d4d1bb95f2f0a21c6afb3368f97887f526aed9ea5` | 辅助宝石图（辅助托盘格） |
| Supports | `Concentrated.png` | 13059 | `c4d0da169f54ddb118fbf409aac1a635fdf01628b72afa95417acf81f88e22c4` | 辅助宝石图（辅助托盘格） |
| Supports | `Faster.png` | 11325 | `fad80ca5636bd4e8797ff6d059dd499b99e26575d48003c31f56a6d71586a5d4` | 辅助宝石图（辅助托盘格） |
| Supports | `FireConversion.png` | 12549 | `a10a6f3bf24fbd50bceb5a60394e51d562d770f3c339063f7f346ca686205030` | 辅助宝石图（辅助托盘格） |
| Supports | `Fork.png` | 8962 | `12a624d753e06a9784b8ce679a7fabc85e226199d28c2cfdeb9641efd11f4495` | 辅助宝石图（辅助托盘格） |
| Supports | `ReturningProjectiles.png` | 10920 | `bf9afe6775edd3c71a550fd763f178e5ee38aee090568c04ded43cfb3dad7e37` | 辅助宝石图（辅助托盘格） |
| Supports | `SnipersMark.png` | 9354 | `8fd6f257282d50ee25b65e49e8fad27d9b76d93dcbb94e0d30e120d466a70245` | 辅助宝石图（辅助托盘格） |
| Vfx | `Fireball.png` | 11608 | `3fac3e5c2a6ddf1f15e6d7dc050ae04890700e97337d3013a748800896e63504` | 冰矛/火球弹体贴图（抠背景）；Mtx 两条为登记未消费 |
| Vfx | `IceSpear.png` | 12526 | `d54ea1ae894a19c899285461a759796671e47b498c0d01488f8996a0ec4dde2a` | 冰矛/火球弹体贴图（抠背景）；Mtx 两条为登记未消费 |
| Vfx | `IceSpearMtx.png` | 35133 | `0917a468a841de9b3287cd6de1853da93da67919fa48696bff4b4070300c69a8` | 冰矛/火球弹体贴图（抠背景）；Mtx 两条为登记未消费 |
| Vfx | `IceSpearMtxAlt.png` | 27019 | `2b7499d801b7445c387f1bffdb3b9875543733527787eb83873e395902d72ae7` | 冰矛/火球弹体贴图（抠背景）；Mtx 两条为登记未消费 |

### 7.1 授权状态（全表统一）

| 项 | 值 |
|---|---|
| 状态 | **PROTOTYPE / REFERENCE ONLY / NOT PRODUCTION-ADMITTED** |
| 来源 | poedb.tw 公开 CDN（`https://cdn.poedb.tw/image/...`）；逐条原始路径见本文件 §1–§4 |
| 依据 | 本轮收到的是导演口述指令「从 POEDB.TW 上扒取」；**未收到可用于本项目产品发行的授权文件**。可下载 ≠ 已授权。 |
| 解除条件 | 导演/法务给出明确的商用授权或替换方案；此前这批素材只能用于原型与内部评审，不得进入发行包 |
| 派生处理 | 仅 `Vfx/IceSpear`、`Vfx/Fireball` 两项做过亮度软阈值抠背景（`tools/poedb/fetch_art.py: keyout_background`），单点可撤 |

### 7.2 复核方式

```pwsh
# 重新核对本表（缺什么补什么；--force 全量重下）
python tools/poedb/fetch_art.py
# 逐文件校验 SHA-256
Get-ChildItem 'unity/game/New Unity Project/Assets/Resources/UI/PoE/*/*.png' | ForEach-Object { (Get-FileHash $_.FullName -Algorithm SHA256).Hash + '  ' + $_.FullName }
```

共 30 个文件（Items 12 / Skills 5 / Supports 9 / Vfx 4）。
