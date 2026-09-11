using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// poedb.tw 美术资源层（S6P-WO-05，导演 2026-09-11 指令「从 POEDB.TW 扒取装备图标 / 技能特效」）。
    /// 单一职责：Resources/UI/PoE 下 PoE 原始美术的懒加载 + 静态缓存 + 缺失回退（返回 null，
    /// 调用方退回 SliceAria 通用图形、再退回 SliceHudIcons 程序化合成——回退路径永不失效）。
    /// 摄取脚本=tools/poedb/fetch_art.py（清单即真值，DeclaredKeys 与清单逐条对应）；
    /// 逐条来源页 / 原始内部名 / 选择理由见 docs/reviews/S6P/S6P_WO_05_POEDB_SOURCING.md。
    /// 本轮摄取的是 poedb 发布的美术资源（宝石图 / 词缀无关的基底物品图 / 特效 MTX 图标），
    /// 不是粒子系统——投射物粒子行为仍由引擎自绘，美术只作贴图。
    /// </summary>
    public static class SlicePoeArt
    {
        public const string Root = "UI/PoE/";

        static readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();

        /// <summary>加载失败返回 null（不抛异常），调用方必须允许回退。</summary>
        public static Texture2D Get(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;
            Texture2D t;
            if (_cache.TryGetValue(relativePath, out t) && t != null)
                return t;
            t = Resources.Load<Texture2D>(Root + relativePath);
            _cache[relativePath] = t;
            return t;
        }

        /// <summary>声明的素材全集（单一真相；资源契约测试据此锁定「全部可加载」且「无未声明文件」）。</summary>
        public static readonly string[] DeclaredKeys =
        {
            "Items/Weapon_Ordinary", "Items/Weapon_Rare",
            "Items/Body_Ordinary", "Items/Body_Rare",
            "Items/Helmet_Ordinary", "Items/Helmet_Rare",
            "Items/Gloves_Ordinary", "Items/Gloves_Rare",
            "Items/Boots_Ordinary", "Items/Boots_Rare",
            "Items/Belt_Ordinary", "Items/Belt_Rare",
            "Skills/Melee", "Skills/Projectile", "Skills/Area", "Skills/IceSpear", "Skills/Fireball",
            "Supports/AddedFire", "Supports/Brutal", "Supports/Concentrated", "Supports/Faster",
            "Supports/Combustion", "Supports/Fork", "Supports/FireConversion",
            "Supports/ReturningProjectiles", "Supports/SnipersMark",
            "Vfx/IceSpear", "Vfx/Fireball", "Vfx/IceSpearMtx", "Vfx/IceSpearMtxAlt"
        };

        // ================= 装备/物品图标（按槽位 + 稀有度） =================

        static readonly string[] SlotKeys =
        {
            "Weapon", "Body", "Helmet", "Gloves", "Boots", "Belt"
        };

        /// <summary>物品美术（普通/稀有各一档；缺失返回 null）。</summary>
        public static Texture2D ItemArt(EquipSlot slot, Rarity rarity)
        {
            int i = (int)slot;
            if (i < 0 || i >= SlotKeys.Length)
                return null;
            return Get("Items/" + SlotKeys[i] + (rarity == Rarity.Rare ? "_Rare" : "_Ordinary"));
        }

        // ================= 主动技能宝石图 =================

        static readonly string[] SkillKeys =
        {
            null, "Melee", "Projectile", "Area", "IceSpear", "Fireball"
        };

        public static Texture2D SkillArt(SkillId id)
        {
            int i = (int)id;
            if (i <= 0 || i >= SkillKeys.Length || SkillKeys[i] == null)
                return null;
            return Get("Skills/" + SkillKeys[i]);
        }

        // ================= 辅助宝石图 =================

        static readonly string[] SupportKeys =
        {
            null, "AddedFire", "Brutal", "Concentrated", "Faster", "Combustion", "Fork",
            "FireConversion", "ReturningProjectiles", "SnipersMark"
        };

        public static Texture2D SupportArt(SupportId id)
        {
            int i = (int)id;
            if (i <= 0 || i >= SupportKeys.Length || SupportKeys[i] == null)
                return null;
            return Get("Supports/" + SupportKeys[i]);
        }

        // ================= 技能特效图（投射物广告牌） =================
        // poedb 的 `Art/2DArt/SkillIcons/*` 是**不透明方图**（实测 alpha 恒 255、背景为深色而非近黑），
        // 直接做广告牌会带一块深色底板，故摄取时按亮度软阈值抠除背景（唯一派生化处理，见 sourcing §0）。
        // 宝石图（Skills/*）虽然 alpha 干净，但画的是「镶金宝石」，作为冰矛/火球弹体辨识不足；
        // 因此两条新技能优先用抠好的技能图标，其余投射物回退宝石图。

        /// <summary>冰矛投射物特效（抠背景的技能图标）。</summary>
        public static Texture2D IceSpearVfx { get { return Get("Vfx/IceSpear"); } }

        /// <summary>火球术投射物特效（抠背景的技能图标）。</summary>
        public static Texture2D FireballVfx { get { return Get("Vfx/Fireball"); } }

        /// <summary>冰矛特效 MTX 美术（登记未消费，供后续特效档位切换）。</summary>
        public static Texture2D IceSpearMtx { get { return Get("Vfx/IceSpearMtx"); } }

        /// <summary>冰矛特效 MTX 美术·备档（登记未消费）。</summary>
        public static Texture2D IceSpearMtxAlt { get { return Get("Vfx/IceSpearMtxAlt"); } }

        /// <summary>投射物广告牌贴图（缺失返回 null → 调用方退回球体程序化视觉）。</summary>
        public static Texture2D ProjectileArt(SkillId id)
        {
            if (id == SkillId.IceSpear && IceSpearVfx != null)
                return IceSpearVfx;
            if (id == SkillId.Fireball && FireballVfx != null)
                return FireballVfx;
            return SkillArt(id);
        }
    }
}
