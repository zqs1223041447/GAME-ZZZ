using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

    public struct ResourceContract
    {
        public string Logical;
        public string Key;
        public string TypeName;
        public ResourceClass Class;
        public string GateReason;
        public string Fallback;
    }

    public struct ResourceVerifyResult
    {
        public bool Loaded;
        public string AssetPath;
        public string TypeName;
        public string Error;
    }

    /// <summary>
    /// 资源契约 golden（S3-P12-AUDIT-CLOSEOUT）：当前 Runtime 全部 Resources 入口的清单与分类。
    /// 来源=对 Runtime 代码的实际扫描（DarkKnightView / AudioEvents / VoiceCues；VFX 无声明引用）。
    /// 验证语义与 Runtime 完全同源：同 key、同类型、同拼接方式真实执行 Resources.Load。
    /// </summary>
    public static class ContentResourceAuditContracts
    {
        public static readonly ResourceContract[] All =
        {
            new ResourceContract
            {
                Logical = "玩家模型（DarkKnight 预制体）",
                Key = "Player/DarkKnight",
                TypeName = "GameObject",
                Class = ResourceClass.Required,
                GateReason = null,
                // 运行时缺失会回退胶囊（DarkKnightView.TryMount），但那是降级模式：正式玩家模型缺失即内容债
                Fallback = "Runtime 缺失回退胶囊（降级）；契约仍 REQUIRED"
            },
            new ResourceContract { Logical = "战斗 SFX Cast", Key = "Audio/Cast", TypeName = "AudioClip", Class = ResourceClass.Required, GateReason = null, Fallback = "Runtime 缺失=静音+限频日志（降级）；契约仍 REQUIRED" },
            new ResourceContract { Logical = "战斗 SFX Impact", Key = "Audio/Impact", TypeName = "AudioClip", Class = ResourceClass.Required, GateReason = null, Fallback = "Runtime 缺失=静音+限频日志（降级）；契约仍 REQUIRED" },
            new ResourceContract { Logical = "战斗 SFX Hit", Key = "Audio/Hit", TypeName = "AudioClip", Class = ResourceClass.Required, GateReason = null, Fallback = "Runtime 缺失=静音+限频日志（降级）；契约仍 REQUIRED" },
            new ResourceContract { Logical = "战斗 SFX Death", Key = "Audio/Death", TypeName = "AudioClip", Class = ResourceClass.Required, GateReason = null, Fallback = "Runtime 缺失=静音+限频日志（降级）；契约仍 REQUIRED" },
            new ResourceContract { Logical = "战斗 SFX Loot", Key = "Audio/Loot", TypeName = "AudioClip", Class = ResourceClass.Required, GateReason = null, Fallback = "Runtime 缺失=静音+限频日志（降级）；契约仍 REQUIRED" },
            new ResourceContract { Logical = "人声 Cast", Key = "Audio/Voice/Cast", TypeName = "AudioClip", Class = ResourceClass.Gated, GateReason = "语音映射待导演试听指认（20 候选），未指认=静音", Fallback = "缺失=静音" },
            new ResourceContract { Logical = "人声 Hit", Key = "Audio/Voice/Hit", TypeName = "AudioClip", Class = ResourceClass.Gated, GateReason = "语音映射待导演试听指认（20 候选），未指认=静音", Fallback = "缺失=静音" },
            new ResourceContract { Logical = "人声 Death", Key = "Audio/Voice/Death", TypeName = "AudioClip", Class = ResourceClass.Gated, GateReason = "语音映射待导演试听指认（20 候选），未指认=静音", Fallback = "缺失=静音" }
        };

        /// <summary>按 Runtime 同语义真实加载一次并取真实资产路径（与代码同 key、同类型、同拼接）。</summary>
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
