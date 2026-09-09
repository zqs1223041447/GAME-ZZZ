using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// S5U-WO-02（规划 AI 授权的 presentation-only 通用数值格式化器）：
    /// HUD 大数值压缩显示（999→999 / 12,450→12.5K / 9,999,999→10.0M）。
    /// 底层值零改动、零写回；通用阈值（不得对 PlayerBaseLife 或任意特定值写特殊分支）；
    /// 纯函数、零分配路径（string.Format 一次）。
    /// </summary>
    public static class SliceHudFormat
    {
        /// <summary>K 段起点（≥1000 进 K）。</summary>
        public const float KThreshold = 1000f;
        /// <summary>M 段起点（≥1,000,000 进 M）。</summary>
        public const float MThreshold = 1000000f;

        /// <summary>HUD 紧凑格式：&lt;K 原值整数；K/M 段保留 1 位小数（去尾零）；负数原样整数。</summary>
        public static string Compact(float value)
        {
            float v = Mathf.Abs(value);
            if (value < 0f || v < KThreshold)
                return Mathf.RoundToInt(value).ToString();
            if (v < MThreshold)
                return FormatScaled(value / KThreshold, "K");
            return FormatScaled(value / MThreshold, "M");
        }

        static string FormatScaled(float scaled, string suffix)
        {
            // 1 位小数；999,960/1000=999.96 → "1000.0K" 可接受（上限即 10.0M 量级，框内恒两段）
            return scaled.ToString("0.0", CultureInfoInvariant) + suffix;
        }

        /// <summary>恒定不变文化（小数点=‘.’，中文环境不漂移）。</summary>
        static System.Globalization.CultureInfo CultureInfoInvariant
        {
            get { return System.Globalization.CultureInfo.InvariantCulture; }
        }
    }
}
