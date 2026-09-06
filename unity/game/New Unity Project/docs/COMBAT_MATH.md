# COMBAT_MATH

S2 已实现子集。每条规则都有 Input / Expected / Source。禁止凭记忆改公式；改公式必须改黄金向量。

切片范围：Physical、Fire、Hit、Crit、Resistance、Armour、Accuracy/Evasion、1 个 DoT（Ignite）、1 条 Conversion（Physical → Fire）。

后置（未实现）：ES、Block、Suppression、复杂 Leech、高级 Ailment、Reflect、Damage Taken As、多阶段 Conversion。

实现：`Assets/Runtime/Core/Gameplay/CombatMath.cs`。测试：`Assets/Tests/EditMode/CombatMathTests.cs`。

内部单位：抗性 / 转化 / 暴击率为 0–1。伤害在结算末尾 `RoundToInt`，成功 Hit 至少 1。

---

## STAT — Increased / More

```
stat = (base + flat) * max(0, 1 + increased) * moreProduct
moreProduct = Π (1 + more_i)
```

| ID | Input | Expected | Source |
|---|---|---|---|
| GV-STAT-001 | base+flat=120, inc=50%, more=20% | 216 | https://pathofexile.fandom.com/wiki/Stat |
| GV-STAT-002 | 100, inc=-200%, more=1 | 0 | 同上；increased 乘区下限 0 |

---

## HIT — Accuracy / Evasion

攻击才检命中。法术（E）跳过，命中率 = 100%。

```
if evasion <= 0: chance = 1
if accuracy <= 0: chance = 0.05
uncapped = 1.25 * Acc / (Acc + (Eva / 5) ^ 0.9)
chance = clamp(uncapped, 0.05, 1.0)
Hit if HitRoll < chance
```

| ID | Input | Expected | Source |
|---|---|---|---|
| GV-HIT-001 | Acc=10, Eva=0 | 1.0 | https://pathofexile.fandom.com/wiki/Accuracy |
| GV-HIT-002 | Acc=0, Eva=1000 | 0.05 | 同上，下限 5% |
| GV-HIT-003 | Acc=1000, Eva=5000 | 0.832674 | 同上；`(5000/5)^0.9 = 1000^0.9 ≈ 501.18723`，`1250/(1000+501.18723)` |

S2 不做 Entropy 伪随机串。Roll 来自 `SeededRng.NextFloat01()`。

---

## CRIT

```
CritChance = (Base + Additional) * (1 + Increased)
CritMultiplier = max(1.0, 1.50 + AdditionalMultiplier)
若 CritRoll < CritChance：PhysPre 与 FirePre 同乘 CritMultiplier
```

基础暴击倍率 150%。DoT tick 不暴击。

| ID | Input | Expected | Source |
|---|---|---|---|
| GV-CRIT-001 | base=5%, add=0, inc=100% | 10% | https://pathofexile.fandom.com/wiki/Critical_strike |
| GV-CRIT-002 | damage=100, addMulti=0 | 150；addMulti=+50% → 倍率 2.0 | 同上 |

---

## ARMOUR

只减 Physical Hit。DoT 不吃护甲。

```
DR = Armour / (Armour + 5 * rawPhys)
DR = min(DR, 0.90)
taken = rawPhys * (1 - DR)
```

| ID | Input | Expected | Source |
|---|---|---|---|
| GV-ARMOUR-001 | A=5000, D=1000 | DR=50%, taken=500 | PoEDB Armour https://poedb.tw/us/Armour ；公式快照 JP wiki `A/(A+5D)`（与 Path of Building `armourReductionF` 一致） |
| GV-ARMOUR-002 | A=45000, D=1000 | DR=90% cap, taken=100 | 同上，上限 90% |
| GV-ARMOUR-003 | A=0, D=100 | taken=100 | 退化 |

POE2 `A/(A+10D)` 不采用。S2 锁 POE1 的 5。

---

## RESISTANCE

只对 Fire（Hit 与 Ignite tick）。

```
effective = min(resist, min(maxResist, 0.90))
taken = damage * (1 - effective)
```

默认玩家抗性上限 75%。允许负抗。

| ID | Input | Expected | Source |
|---|---|---|---|
| GV-RES-001 | 100 fire, 75% res, max 75% | 25 | https://pathofexile.fandom.com/wiki/Resistance |
| GV-RES-002 | 100 fire, -20% | 120 | 同上 |
| GV-RES-003 | 100 fire, 120% res, max 90% | 10 | 硬顶 90% |

---

## CONVERSION — 一条 Physical → Fire

转化发生在 flat 之后、increased/more 之前。转化部分同时吃来源与目标的 increased（加法）和 more（乘法）。总转化钳在 100%。不做 Gain as Extra、不做多跳。

```
converted = physFlat * convert
physLeft = physFlat * (1 - convert)
physPre = Combine(physLeft, incDmg+incPhys, moreDmg*morePhys)
fireNative = Combine(fireFlat, incDmg+incFire, moreDmg*moreFire)
fireConv = Combine(converted, incDmg+incPhys+incFire, moreDmg*morePhys*moreFire)
firePre = fireNative + fireConv
```

| ID | Input | Expected | Source |
|---|---|---|---|
| GV-CONV-001 | 100 phys, 50% conv, 无 inc | 50 phys + 50 fire | PoEDB Damage conversion https://poedb.tw/us/Damage_conversion ；markdown_raw：转化在 increased/more 前，Converted to 替换来源 |
| GV-CONV-002 | 100 phys, 50% conv, +100% inc phys, +100% inc fire | 100 phys + 150 fire = 250 | 同上；转化伤害同时吃两种 increased 且加法合并 |
| GV-CONV-003 | 80 phys, 100% conv | 0 phys + 80 fire | 上限 100% |

方向快照：Physical 可以直接转到 Fire（允许跳过中间元素）。S2 只有这一条。

---

## DOT — Ignite

```
IgniteDps = 0.50 * FirePreMit    # 暴击后、抗性前的火伤
Duration = 4s
Tick 吃火抗，不吃护甲，不命中，不暴击
同一目标只保留更高 DPS 的 Ignite
```

| ID | Input | Expected | Source |
|---|---|---|---|
| GV-IGNITE-001 | FirePreMit=100 | 50 dps，持续 4s | PoEDB Damage over time 表：Ignite 50% of Fire / 4s（markdown_raw snapshot） |

S2 不实现 3.16+ 玩家 90%/125% 点燃，也不实现 ailment threshold。有火伤且 IgniteChance roll 过才上。

---

## 完整命中顺序（已实现）

```
1. flat phys / fire
2. Convert Physical → Fire
3. increased / more（转化部分吃双边）
4. Accuracy vs Evasion（攻击）
5. Crit
6. Armour → remaining Physical
7. Fire Resistance → Fire
8. RoundToInt；Hit 成功至少 1
9. Ignite 来自 FirePreMit
```

| ID | Input | Expected | Source |
|---|---|---|---|
| GV-FULL-001 | 1000 phys, armour 5000, 必中无暴 | taken 500 | ARMOUR-001 接入 ResolveHit |
| GV-FULL-002 | Acc=0, Eva=1000, HitRoll=0.5 | miss, taken 0 | HIT-002 |
| GV-FULL-003 | 100 fire, crit（5%×2=10%, roll 0.01）, 50% fire res, 必点燃 | FirePre 150, taken 75, IgniteDps 75 | CRIT + RES + IGNITE |

---

## 未做

Energy Shield、Block、Suppression、Leech、Bleed/Poison/Shock、Reflect、Damage Taken As、Gain as Extra、多阶段 Conversion、穿透、抗性上限堆叠以外的 Exposure 系统。
