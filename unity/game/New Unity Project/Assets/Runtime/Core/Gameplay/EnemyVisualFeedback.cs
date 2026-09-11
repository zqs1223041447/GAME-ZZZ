using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>正式敌人视觉的战斗反馈状态（纯 presentation）。优先级：Hit &gt; Ignite &gt; Normal。</summary>
    public enum EnemyFeedbackState : byte
    {
        Normal = 0,
        Ignite = 1,
        Hit = 2,
        // S6P-WO-05：狙击印记标记态（最低优先级——被印记是持续态，不应盖过命中/点燃这两个瞬时反馈）
        Mark = 3
    }

    /// <summary>
    /// 正式敌人 PBR 视觉状态反馈（S3-P5-ART-R5-VISUAL-FEEDBACK-PARITY）：只消费既有 canonical gameplay truth
    /// （调用方从 HitFlash / IgniteRemain 派生输入，本类不派生、不倒计时、不写回 gameplay）。
    /// 实现=URP/Lit _BaseColor 的 MaterialPropertyBlock tint：挂载时缓存每 Renderer×材质槽的
    /// canonical shared material 原色，仅状态变化时写 property block（无每帧分配），Normal 精确恢复原色。
    /// 禁止 renderer.material 实例化路径；shader 无 _BaseColor 时安全跳过。
    /// </summary>
    public sealed class EnemyVisualFeedback
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        // presentation-only tint 常量：Ignite=持续暖灼 tint（与基元 placeholder AshColor 同族同强度，保留 texture/PBR 可读性）；
        // Hit=短促更强白闪（与 placeholder HitColor/强度对齐），不做高频刺眼纯白闪屏。
        static readonly Color IgniteTint = new Color(0.95f, 0.42f, 0.12f);
        static readonly Color HitTint = new Color(1f, 0.97f, 0.92f);
        // S6P-WO-05：印记 tint = 冷白偏青（与 Life紫/火橙/命中白都可区分），强度弱于命中闪
        static readonly Color MarkTint = new Color(0.55f, 0.95f, 1f);
        const float IgniteBlend = 0.55f;
        const float HitBlend = 0.95f;
        const float MarkBlend = 0.40f;

        struct RendererSlots
        {
            public Renderer Renderer;
            public Color[] Original; // 每材质 index 的原 _BaseColor（缓存自 sharedMaterial，不假定白色）
            public MaterialPropertyBlock[] Blocks; // 每 index 一块，复用不重建
            public bool[] HasBaseColor;
        }

        readonly RendererSlots[] _slots;
        EnemyFeedbackState _last = EnemyFeedbackState.Normal;
        bool _written;

        public EnemyVisualFeedback(GameObject root)
        {
            if (root == null)
                return;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            int count = 0;
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i] != null && renderers[i].sharedMaterials != null && renderers[i].sharedMaterials.Length > 0)
                    count++;
            _slots = new RendererSlots[count];
            int w = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null)
                    continue;
                Material[] mats = r.sharedMaterials;
                if (mats == null || mats.Length == 0)
                    continue;
                var slot = new RendererSlots();
                slot.Renderer = r;
                slot.Original = new Color[mats.Length];
                slot.Blocks = new MaterialPropertyBlock[mats.Length];
                slot.HasBaseColor = new bool[mats.Length];
                for (int m = 0; m < mats.Length; m++)
                {
                    // 只从 canonical shared material 读原色；缺失/空槽安全跳过，不建实例
                    if (mats[m] != null && mats[m].HasProperty(BaseColorId))
                    {
                        slot.Original[m] = mats[m].GetColor(BaseColorId);
                        slot.HasBaseColor[m] = true;
                        slot.Blocks[m] = new MaterialPropertyBlock();
                    }
                }
                _slots[w++] = slot;
            }
        }

        /// <summary>状态映射纯函数（确定性）：死亡→Normal；HitFlash 窗口内→Hit；否则 Ignited→Ignite；否则 Marked→Mark；否则 Normal。</summary>
        public static EnemyFeedbackState Compute(bool alive, float hitFlash, bool ignited)
        {
            return Compute(alive, hitFlash, ignited, false);
        }

        /// <summary>S6P-WO-05：加上印记态的完整映射（3 参重载=marked:false，既有行为逐位不变）。</summary>
        public static EnemyFeedbackState Compute(bool alive, float hitFlash, bool ignited, bool marked)
        {
            if (!alive)
                return EnemyFeedbackState.Normal;
            if (hitFlash > 0.02f)
                return EnemyFeedbackState.Hit;
            if (ignited)
                return EnemyFeedbackState.Ignite;
            if (marked)
                return EnemyFeedbackState.Mark;
            return EnemyFeedbackState.Normal;
        }

        /// <summary>每帧调用：状态未变化时不写任何 renderer（§18）；变化时全量覆盖所有有效材质槽。</summary>
        public void Apply(bool alive, float hitFlash, bool ignited)
        {
            Apply(alive, hitFlash, ignited, false);
        }

        /// <summary>S6P-WO-05：含印记态的写入路径（3 参重载=marked:false）。</summary>
        public void Apply(bool alive, float hitFlash, bool ignited, bool marked)
        {
            if (_slots == null)
                return;
            EnemyFeedbackState state = Compute(alive, hitFlash, ignited, marked);
            if (_written && state == _last)
                return;
            _written = true;
            _last = state;
            for (int s = 0; s < _slots.Length; s++)
            {
                RendererSlots slot = _slots[s];
                if (slot.Renderer == null)
                    continue;
                for (int m = 0; m < slot.Original.Length; m++)
                {
                    if (!slot.HasBaseColor[m])
                        continue;
                    MaterialPropertyBlock block = slot.Blocks[m];
                    if (state == EnemyFeedbackState.Normal)
                        block.SetColor(BaseColorId, slot.Original[m]); // 精确恢复缓存原色
                    else if (state == EnemyFeedbackState.Hit)
                        block.SetColor(BaseColorId, Color.Lerp(slot.Original[m], HitTint, HitBlend));
                    else if (state == EnemyFeedbackState.Mark)
                        block.SetColor(BaseColorId, Color.Lerp(slot.Original[m], MarkTint, MarkBlend));
                    else
                        block.SetColor(BaseColorId, Color.Lerp(slot.Original[m], IgniteTint, IgniteBlend));
                    slot.Renderer.SetPropertyBlock(block, m);
                }
            }
        }

        /// <summary>只读观察：最近一次应用的反馈状态（测试/QA 验证钩子）。</summary>
        public EnemyFeedbackState LastState
        {
            get { return _last; }
        }
    }
}
