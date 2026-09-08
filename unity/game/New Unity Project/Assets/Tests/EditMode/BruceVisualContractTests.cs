using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Phase 5 Art Trial R6（工作令 S3-P5-ART-R6-WARDEN-BRUCE-INTEGRATION）：第三正式敌人视觉（Warden→BruceVisual）
    /// 契约：契约路径加载/visual-only 组件审计/Mount 不覆写美术 scale（R4 根因不回归）/六态映射/
    /// R5 反馈支持 Bruce 多材质槽且精确恢复/sharedMaterial 引用不变。
    /// </summary>
    public sealed class BruceVisualContractTests
    {
        readonly System.Collections.Generic.List<Object> _created = new System.Collections.Generic.List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _created.Count; i++)
                if (_created[i] != null)
                    Object.DestroyImmediate(_created[i]);
            _created.Clear();
        }

        [Test]
        public void BruceVisual_LoadsAtContractPath()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyBruceVisual);
            Assert.IsNotNull(prefab, "Resources 缺失 " + RuntimeResourcePaths.EnemyBruceVisual + "（R6 契约 REQUIRED）");
            Assert.AreEqual("BruceVisual", prefab.name);
        }

        [Test]
        public void BruceVisual_ComponentAudit_VisualOnly()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyBruceVisual);
            Assert.IsNotNull(prefab);

            Assert.IsEmpty(prefab.GetComponentsInChildren<MonoBehaviour>(true), "正式视觉预制体零 MonoBehaviour（vendor 脚本=0）");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Collider>(true), "零 Collider");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Rigidbody>(true), "零 Rigidbody");
            Assert.IsEmpty(prefab.GetComponentsInChildren<CharacterController>(true), "零 CharacterController");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Camera>(true), "零 Camera");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Light>(true), "零 Light");

            var skins = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Assert.GreaterOrEqual(skins.Length, 1, "Bruce 至少 1 个 SkinnedMeshRenderer");
            foreach (var s in skins)
            {
                Assert.IsNotNull(s.sharedMesh, "SMR 必须绑定网格");
                foreach (var m in s.sharedMaterials)
                    Assert.IsNotNull(m, "SMR 材质槽不得为空");
            }
        }

        [Test]
        public void BruceVisual_Animator_SixStatesAndNoRootMotion()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyBruceVisual);
            Assert.IsNotNull(prefab);
            var animator = prefab.GetComponentInChildren<Animator>(true);
            Assert.IsNotNull(animator, "必须含 Animator");
            Assert.IsFalse(animator.applyRootMotion, "root motion 必须 off");
            Assert.IsNotNull(animator.runtimeAnimatorController, "必须有控制器");
            Assert.IsNotNull(animator.runtimeAnimatorController.animationClips);
            Assert.Greater(animator.runtimeAnimatorController.animationClips.Length, 0, "控制器必须有 clip");
            // prefab 未实例化时 Animator.HasState 不可靠，改按控制器层状态机枚举
            var ctrl = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
            Assert.IsNotNull(ctrl, "控制器应为 AnimatorController");
            Assert.AreEqual(1, ctrl.layers.Length, "单层状态机");
            var names = new System.Collections.Generic.HashSet<string>();
            foreach (var s in ctrl.layers[0].stateMachine.states)
                names.Add(s.state.name);
            foreach (string required in new[] { "Idle", "Run", "Attack", "Hit", "Death" })
                Assert.IsTrue(names.Contains(required), $"缺 {required} 状态（实有 {string.Join(",", names)}）");
        }

        [Test]
        public void BruceVisual_WrapperScale_FinitePositive_BossHeight()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyBruceVisual);
            Assert.IsNotNull(prefab);
            float s = prefab.transform.localScale.x;
            Assert.IsTrue(float.IsFinite(s) && s > 0f && s < 0.01f, $"wrapper scale 必须有限为正且小量（={s}，R1 验证值 0.0025669）");
            Assert.AreEqual(s, prefab.transform.localScale.y, 1e-6f, "等比缩放");
            Assert.AreEqual(s, prefab.transform.localScale.z, 1e-6f, "等比缩放");
            var smr = prefab.GetComponentInChildren<SkinnedMeshRenderer>(true);
            float h = smr.sharedMesh.bounds.size.y * s;
            Assert.AreEqual(2.6f, h, 0.2f, $"可见高度应≈2.6m（实测 {h:0.00}，R1 目标）");
        }

        [Test]
        public void Presenter_Mount_PreservesPrefabScale_AndGameplayRootUnchanged()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyBruceVisual);
            Assert.IsNotNull(prefab);
            var parent = new GameObject("gameplay-root");
            parent.transform.localScale = new Vector3(1.3f, 1f, 1.3f);
            _created.Add(parent);

            var presenter = EnemyVisualPresenter.Mount(prefab, parent.transform);
            _created.Add(presenter.Root);

            // R4 根因不回归：Mount 不得覆写 prefab 根美术 scale，也不得改 gameplay root
            Assert.AreEqual(prefab.transform.localScale, presenter.Root.transform.localScale,
                "Mount 必须保留 BruceVisual 预制体根 scale（≈0.0025669，世界高≈2.6m）");
            Assert.AreEqual(Vector3.zero, presenter.Root.transform.localPosition, "挂载位置归零");
            Assert.AreEqual(Quaternion.identity, presenter.Root.transform.localRotation, "挂载朝向归零");
            Assert.AreEqual(new Vector3(1.3f, 1f, 1.3f), parent.transform.localScale, "gameplay root scale 零改动");
        }

        [Test]
        public void Presenter_Feedback_SupportsBruceMaterials_AndRestoresExactly()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyBruceVisual);
            Assert.IsNotNull(prefab);
            var parent = new GameObject("gameplay-root");
            _created.Add(parent);
            var presenter = EnemyVisualPresenter.Mount(prefab, parent.transform);
            _created.Add(presenter.Root);

            Assert.AreEqual(EnemyFeedbackState.Normal, presenter.LastFeedbackState, "挂载初始=Normal");

            var renderers = presenter.Root.GetComponentsInChildren<Renderer>();
            Assert.Greater(renderers.Length, 0, "Bruce 必须有渲染器");
            var snapshot = new System.Collections.Generic.List<Material>();
            foreach (var r in renderers)
                foreach (var m in r.sharedMaterials)
                    snapshot.Add(m);

            presenter.ApplyFeedback(true, 0f, true);
            Assert.AreEqual(EnemyFeedbackState.Ignite, presenter.LastFeedbackState, "Bruce 必须进入 Ignite（R5 通用反馈自动生效，无 Bruce 特例分支）");
            int tinted = 0;
            foreach (var r in renderers)
            {
                if (r.sharedMaterials == null || r.sharedMaterials.Length == 0)
                    continue;
                for (int i = 0; i < r.sharedMaterials.Length; i++)
                {
                    var m = r.sharedMaterials[i];
                    if (m == null || !m.HasProperty("_BaseColor"))
                        continue;
                    var block = new MaterialPropertyBlock();
                    r.GetPropertyBlock(block, i);
                    if (block.HasProperty("_BaseColor"))
                    {
                        Assert.AreNotEqual(m.GetColor("_BaseColor"), block.GetColor("_BaseColor"), "Ignite tint 必须落到每个有效材质槽");
                        tinted++;
                    }
                }
            }
            Assert.Greater(tinted, 0, "至少一个材质槽被 tint（多材质槽全覆盖）");

            presenter.ApplyFeedback(true, 0.1f, true);
            Assert.AreEqual(EnemyFeedbackState.Hit, presenter.LastFeedbackState, "Hit 必须优先于 Ignite");

            presenter.ApplyFeedback(true, 0f, true);
            Assert.AreEqual(EnemyFeedbackState.Ignite, presenter.LastFeedbackState, "Hit 结束回 Ignite");

            presenter.ApplyFeedback(true, 0f, false);
            Assert.AreEqual(EnemyFeedbackState.Normal, presenter.LastFeedbackState, "Ignite 结束回 Normal");

            int idx = 0;
            foreach (var r in renderers)
                foreach (var m in r.sharedMaterials)
                {
                    Assert.AreSame(snapshot[idx], m, "反馈前后 sharedMaterials 引用不变（零 runtime clone）");
                    idx++;
                }
        }

        [Test]
        public void GameplayConstants_Guard()
        {
            Assert.AreEqual(300, CombatRules.DummyPoolSize, "DummyPoolSize 钉住");
            Assert.AreEqual(0.40f, CombatRules.DeathRecycle, "DeathRecycle 钉住（Death 截断为已知限制）");
            Assert.AreEqual(0.20f, CombatRules.HitFlash, "HitFlash 钉住");
        }
    }
}
