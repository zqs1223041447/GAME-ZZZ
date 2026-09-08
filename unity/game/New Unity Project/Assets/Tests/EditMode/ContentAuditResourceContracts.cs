using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    public enum ResourceClass
    {
        /// <summary>当前 Slice 正常呈现所要求存在；缺失=内容债，审计必须失败。</summary>
        Required,
        /// <summary>Runtime 已有查找入口，但资产缺失由已登记导演门控决定（缺失不失败，如实报告）。</summary>
        Gated,
        /// <summary>Runtime 明确设计允许缺失且有合法 fallback（非降级债，需记录 fallback 行为）。</summary>
        Optional
    }

    /// <summary>资源路径域（对应 RuntimeResourcePaths 的拼接规则；共享「如何加载」，不共享「应有哪些 key」）。</summary>
    public enum ResourceDomain
    {
        Player,
        CombatSfx,
        Voice,
        Enemy
    }

    public struct ResourceContract
    {
        public string Logical;
        public ResourceDomain Domain;
        public string LogicalKey;
        public string TypeName;
        public ResourceClass Class;
        public string GateReason;
        public string Fallback;

        /// <summary>最终 Resource Path 由 Runtime 统一路径契约拼出（不再手写 prefix）。</summary>
        public string Key
        {
            get
            {
                if (Domain == ResourceDomain.Player)
                    return RuntimeResourcePaths.PlayerDarkKnight;
                if (Domain == ResourceDomain.Enemy)
                    return RuntimeResourcePaths.EnemyTrollVisual;
                if (Domain == ResourceDomain.Voice)
                    return RuntimeResourcePaths.Voice(LogicalKey);
                return RuntimeResourcePaths.CombatSfx(LogicalKey);
            }
        }
    }

    public struct ResourceVerifyResult
    {
        public bool Loaded;
        public string AssetPath;
        public string TypeName;
        public string Error;
    }

    /// <summary>
    /// 资源契约 golden（S3-P12 建立；S3-M2 改为共享路径拼接）：当前 Runtime 全部 Resources 入口的清单与分类。
    /// 独立 oracle 边界：期望有哪些逻辑 key、REQUIRED/GATED 分类、Gate 理由全部人工声明——
    /// 不得从 Runtime declared keys 自动生成（Runtime 未审批增删 key 由 declared-key parity 测试变红）。
    /// 验证语义与 Runtime 同源：真实执行 Resources.Load（同类型、同 key、AssetDatabase 真实路径）。
    /// </summary>
    public static class ContentResourceAuditContracts
    {
        const string VoiceGateReason = "语音映射待导演试听指认（20 候选），未指认=静音";
        const string SfxFallback = "Runtime 缺失=静音+限频日志（降级）；契约仍 REQUIRED";

        public static readonly ResourceContract[] All =
        {
            new ResourceContract
            {
                Logical = "玩家模型（DarkKnight 预制体）",
                Domain = ResourceDomain.Player,
                LogicalKey = "DarkKnight",
                TypeName = "GameObject",
                Class = ResourceClass.Required,
                GateReason = null,
                // 运行时缺失会回退胶囊（DarkKnightView.TryMount），但那是降级模式：正式玩家模型缺失即内容债
                Fallback = "Runtime 缺失回退胶囊（降级）；契约仍 REQUIRED"
            },
            new ResourceContract { Logical = "战斗 SFX Cast", Domain = ResourceDomain.CombatSfx, LogicalKey = "Cast", TypeName = "AudioClip", Class = ResourceClass.Required, GateReason = null, Fallback = SfxFallback },
            new ResourceContract { Logical = "战斗 SFX Impact", Domain = ResourceDomain.CombatSfx, LogicalKey = "Impact", TypeName = "AudioClip", Class = ResourceClass.Required, GateReason = null, Fallback = SfxFallback },
            new ResourceContract { Logical = "战斗 SFX Hit", Domain = ResourceDomain.CombatSfx, LogicalKey = "Hit", TypeName = "AudioClip", Class = ResourceClass.Required, GateReason = null, Fallback = SfxFallback },
            new ResourceContract { Logical = "战斗 SFX Death", Domain = ResourceDomain.CombatSfx, LogicalKey = "Death", TypeName = "AudioClip", Class = ResourceClass.Required, GateReason = null, Fallback = SfxFallback },
            new ResourceContract { Logical = "战斗 SFX Loot", Domain = ResourceDomain.CombatSfx, LogicalKey = "Loot", TypeName = "AudioClip", Class = ResourceClass.Required, GateReason = null, Fallback = SfxFallback },
            new ResourceContract { Logical = "敌人视觉预制体（TrollWarriorVisual，Brute 默认视觉）", Domain = ResourceDomain.Enemy, LogicalKey = "TrollWarriorVisual", TypeName = "GameObject", Class = ResourceClass.Required, GateReason = null, Fallback = "Runtime 缺失回退基元视觉（降级）；契约仍 REQUIRED（S3-P5-ART-R3）" },
            new ResourceContract { Logical = "人声 Cast", Domain = ResourceDomain.Voice, LogicalKey = "Cast", TypeName = "AudioClip", Class = ResourceClass.Gated, GateReason = VoiceGateReason, Fallback = "缺失=静音" },
            new ResourceContract { Logical = "人声 Hit", Domain = ResourceDomain.Voice, LogicalKey = "Hit", TypeName = "AudioClip", Class = ResourceClass.Gated, GateReason = VoiceGateReason, Fallback = "缺失=静音" },
            new ResourceContract { Logical = "人声 Death", Domain = ResourceDomain.Voice, LogicalKey = "Death", TypeName = "AudioClip", Class = ResourceClass.Gated, GateReason = VoiceGateReason, Fallback = "缺失=静音" }
        };

        /// <summary>按 Runtime 同语义真实加载一次并取真实资产路径（同类型、同 key、AssetDatabase 真实路径）。</summary>
        public static ResourceVerifyResult Verify(ResourceContract contract)
        {
            var r = new ResourceVerifyResult();
            if (contract.TypeName == "GameObject")
            {
                GameObject go = Resources.Load<GameObject>(contract.Key);
                r.Loaded = go != null;
                if (go != null)
                {
                    r.AssetPath = AssetDatabase.GetAssetPath(go);
                    r.TypeName = "GameObject";
                }
            }
            else
            {
                AudioClip clip = Resources.Load<AudioClip>(contract.Key);
                r.Loaded = clip != null;
                if (clip != null)
                {
                    r.AssetPath = AssetDatabase.GetAssetPath(clip);
                    r.TypeName = "AudioClip";
                }
            }
            if (!r.Loaded)
                r.Error = "Resources.Load 返回空（key=" + contract.Key + "）";
            return r;
        }

        public static string ResultWord(bool loaded, ResourceClass cls)
        {
            if (loaded)
                return "PASS";
            if (cls == ResourceClass.Required)
                return "MISSING";
            if (cls == ResourceClass.Gated)
                return "GATED-MISSING";
            return "OPTIONAL-MISSING";
        }
    }
}
