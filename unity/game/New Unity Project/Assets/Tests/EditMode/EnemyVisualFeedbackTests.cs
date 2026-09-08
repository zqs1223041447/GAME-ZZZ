using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Phase 5 Art Trial R5（工作令 S3-P5-ART-R5-VISUAL-FEEDBACK-PARITY）：正式敌人 Hit/Ignite 视觉反馈契约。
    /// 覆盖：状态映射确定性（Hit&gt;Ignite&gt;Normal）、原色缓存与精确恢复、多 Renderer/多材质槽、
    /// sharedMaterial 引用不变（零 runtime clone）、Presenter 无 gameplay 写 API。
    /// </summary>
    public sealed class EnemyVisualFeedbackTests
    {
        readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _created.Count; i++)
                if (_created[i] != null)
                    Object.DestroyImmediate(_created[i]);
            _created.Clear();
        }

        GameObject NewVisualRoot(string name, int rendererCount, int materialPerRenderer, Color baseColor)
        {
            var root = new GameObject(name);
            _created.Add(root);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            Assert.IsNotNull(shader, "测试依赖 URP/Lit");
            for (int r = 0; r < rendererCount; r++)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "r" + r;
                cube.transform.SetParent(root.transform, false);
                var renderer = cube.GetComponent<MeshRenderer>();
                var mats = new Material[materialPerRenderer];
                for (int m = 0; m < materialPerRenderer; m++)
                {
                    mats[m] = new Material(shader);
                    mats[m].SetColor("_BaseColor", new Color(baseColor.r + 0.01f * r, baseColor.g + 0.02f * m, baseColor.b));
                    _created.Add(mats[m]);
                }
                renderer.sharedMaterials = mats;
            }
            return root;
        }

        static Material[] SharedSnapshot(EnemyVisualFeedback fb, GameObject root)
        {
            var list = new List<Material>();
            foreach (var r in root.GetComponentsInChildren<Renderer>())
                foreach (var m in r.sharedMaterials)
                    list.Add(m);
            return list.ToArray();
        }

        static Color TintedColor(EnemyVisualFeedback fb, Renderer renderer, int index)
        {
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block, index);
            return block.GetColor("_BaseColor");
        }

        static Color TintedColor(Renderer renderer, int index)
        {
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block, index);
            return block.GetColor("_BaseColor");
        }

        [Test]
        public void Compute_Deterministic_AndPriority()
        {
            Assert.AreEqual(EnemyFeedbackState.Normal, EnemyVisualFeedback.Compute(true, 0f, false));
            Assert.AreEqual(EnemyFeedbackState.Ignite, EnemyVisualFeedback.Compute(true, 0f, true));
            Assert.AreEqual(EnemyFeedbackState.Hit, EnemyVisualFeedback.Compute(true, 0.1f, false));
            Assert.AreEqual(EnemyFeedbackState.Hit, EnemyVisualFeedback.Compute(true, 0.1f, true), "Hit 必须优先于 Ignite");
            Assert.AreEqual(EnemyFeedbackState.Normal, EnemyVisualFeedback.Compute(false, 0.1f, true), "死亡回 Normal");
            // 相同输入=相同输出（确定性）
            Assert.AreEqual(EnemyVisualFeedback.Compute(true, 0.05f, true), EnemyVisualFeedback.Compute(true, 0.05f, true));
        }

        [Test]
        public void Apply_NormalRestoresExactOriginalColor_MultiRenderer_MultiIndex()
        {
            GameObject root = NewVisualRoot("fb", 2, 2, new Color(0.3f, 0.6f, 0.9f));
            var fb = new EnemyVisualFeedback(root);

            fb.Apply(true, 0f, true);
            var renderers = root.GetComponentsInChildren<Renderer>();
            Color ignited = TintedColor(fb, renderers[0], 0);
            Assert.AreNotEqual(ignited, new Color(0.3f, 0.6f, 0.9f), "Ignite 必须改变 tint");

            fb.Apply(true, 0f, false); // Ignite 结束 → 精确 Normal
            for (int r = 0; r < renderers.Length; r++)
            {
                var mats = renderers[r].sharedMaterials;
                for (int m = 0; m < mats.Length; m++)
                    Assert.AreEqual(mats[m].GetColor("_BaseColor"), TintedColor(fb, renderers[r], m),
                        $"Normal 必须精确恢复 renderer{r} index{m} 的原色（多 Renderer/多材质槽全覆盖）");
            }
        }

        [Test]
        public void Apply_IgniteAndHitDiffer_AndHitRevertsBackToIgnite()
        {
            GameObject root = NewVisualRoot("fb", 1, 1, new Color(0.5f, 0.5f, 0.5f));
            var fb = new EnemyVisualFeedback(root);
            var renderer = root.GetComponentInChildren<Renderer>();

            fb.Apply(true, 0f, true);
            Color ignited = TintedColor(fb, renderer, 0);
            fb.Apply(true, 0.1f, true); // Hit while Ignite
            Color hit = TintedColor(fb, renderer, 0);
            Assert.AreNotEqual(ignited, hit, "Hit tint 必须不同于 Ignite tint");
            Assert.AreNotEqual(hit, renderer.sharedMaterial.GetColor("_BaseColor"), "Hit 必须改变 tint");

            fb.Apply(true, 0f, true); // Hit 结束、Ignite 仍存在 → 回 Ignite
            Assert.AreEqual(ignited, TintedColor(fb, renderer, 0), "Hit 结束必须自动回到 Ignite");

            fb.Apply(true, 0f, false); // Ignite 结束 → Normal
            Assert.AreEqual(renderer.sharedMaterial.GetColor("_BaseColor"), TintedColor(fb, renderer, 0));
        }

        [Test]
        public void Apply_OriginalColorNotAssumedWhite()
        {
            GameObject root = NewVisualRoot("fb", 1, 1, new Color(0.2f, 0.1f, 0.8f)); // 非白基色
            var fb = new EnemyVisualFeedback(root);
            var renderer = root.GetComponentInChildren<Renderer>();
            Color original = renderer.sharedMaterial.GetColor("_BaseColor");

            fb.Apply(true, 0f, true);
            Color ignited = TintedColor(fb, renderer, 0);
            Assert.AreNotEqual(original, ignited, "非白原色也必须被 tint");
            // 恢复后的 tint 必须基于原色混合（而不是基于白色）
            Color expectedIgnited = Color.Lerp(original, new Color(0.95f, 0.42f, 0.12f), 0.55f);
            Assert.AreEqual(expectedIgnited, ignited, "Ignite tint 必须以缓存原色为基底");
        }

        [Test]
        public void Apply_SharedMaterialReferencesUnchanged_NoRuntimeClone()
        {
            GameObject root = NewVisualRoot("fb", 2, 2, new Color(0.4f, 0.4f, 0.4f));
            var fb = new EnemyVisualFeedback(root);
            Material[] before = SharedSnapshot(fb, root);

            fb.Apply(true, 0f, true);
            fb.Apply(true, 0.1f, true);
            fb.Apply(true, 0f, true);
            fb.Apply(true, 0f, false);

            Material[] after = SharedSnapshot(fb, root);
            Assert.AreEqual(before.Length, after.Length);
            for (int i = 0; i < before.Length; i++)
                Assert.AreSame(before[i], after[i], "反馈前后 sharedMaterials 引用不得被替换成 runtime clone");

            foreach (var r in root.GetComponentsInChildren<Renderer>())
                foreach (var m in r.sharedMaterials)
                    Assert.IsFalse(m != null && m.name.Contains("(Instance)"), "不得出现 renderer.material 实例化产物");
        }

        [Test]
        public void Apply_MissingBaseColorSlot_SkipsSafely()
        {
            var root = new GameObject("fb-skip");
            _created.Add(root);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(root.transform, false);
            var renderer = cube.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var noColor = new Material(shader); // URP/Lit 有 _BaseColor；再塞一个空材质槽模拟缺失
            _created.Add(noColor);
            renderer.sharedMaterials = new Material[] { noColor, null };

            var fb = new EnemyVisualFeedback(root);
            Assert.DoesNotThrow(() => fb.Apply(true, 0f, true), "无 _BaseColor/空槽必须安全跳过，不得 exception");
            Assert.DoesNotThrow(() => fb.Apply(true, 0f, false));
        }

        [Test]
        public void Presenter_AndFeedback_HaveNoGameplayMutationApi()
        {
            foreach (var type in new[] { typeof(EnemyVisualPresenter), typeof(EnemyVisualFeedback) })
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    foreach (string banned in new[] { "ApplyHit", "ApplyIgnite", "SetIgnite", "AddIgnite", "RemoveIgnite", "SetHitFlash", "ApplyDamage", "Kill" })
                        Assert.IsFalse(method.Name.Contains(banned), $"{type.Name} 不得出现 gameplay 写 API（发现 {method.Name}）");
            }
        }

        [Test]
        public void Presenter_Mount_BuildsFeedback_OnTrollAndLionPrefabs()
        {
            foreach (string path in new[] { RuntimeResourcePaths.EnemyTrollVisual, RuntimeResourcePaths.EnemyFireLionVisual })
            {
                var prefab = Resources.Load<GameObject>(path);
                Assert.IsNotNull(prefab, "Resources 缺失 " + path);
                var parent = new GameObject("gameplay-root");
                _created.Add(parent);
                var presenter = EnemyVisualPresenter.Mount(prefab, parent.transform);
                _created.Add(presenter.Root);
                Assert.AreEqual(EnemyFeedbackState.Normal, presenter.LastFeedbackState, path + " 挂载初始=Normal");

                presenter.ApplyFeedback(true, 0f, true);
                Assert.AreEqual(EnemyFeedbackState.Ignite, presenter.LastFeedbackState, path + " 必须进入 Ignite");
                foreach (var r in presenter.Root.GetComponentsInChildren<Renderer>())
                {
                    if (r.sharedMaterials == null || r.sharedMaterials.Length == 0)
                        continue;
                    if (r.sharedMaterials[0] != null && r.sharedMaterials[0].HasProperty("_BaseColor"))
                        Assert.AreNotEqual(r.sharedMaterials[0].GetColor("_BaseColor"), TintedColor(r, 0), path + " Ignite tint 必须落到渲染器");
                }

                presenter.ApplyFeedback(true, 0f, false);
                Assert.AreEqual(EnemyFeedbackState.Normal, presenter.LastFeedbackState, path + " Ignite 结束必须回 Normal");
            }
        }
    }
}
