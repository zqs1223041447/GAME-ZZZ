using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Phase 5 Art Trial R8/R9（工作令 S3-P5-ART-R8-ASHLING-FORMAL-VISUAL → R9-GARGOYLE-PERFORMANCE-OPTIMIZATION）：
    /// Ashling→GargoyleVisual 第四正式视觉契约。R8 曾被 Formal Art Performance Gate 阻断；
    /// R9 因果隔离（结构审计：17k 顶点/5650 曲线/110 骨骼双 SMR）+ Optimize Game Objects 等优化后重试接入。
    /// 若 R9 双 Art Gate 仍 FAIL，按工作令回退 placeholder（本套断言同步回退）。
    /// </summary>
    public sealed class AshlingVisualContractTests
    {
        const string RetainedPrefabPath = "Assets/Art/Enemies/Gargoyle/Prefabs/GargoyleVisual.prefab";

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
        public void Catalog_AshlingMapsGargoyleVisual()
        {
            Assert.AreEqual(RuntimeResourcePaths.EnemyGargoyleVisual, EnemyVisualCatalog.VisualResourcePath(EnemyKind.Ashling),
                "Ashling 必须映射 GargoyleVisual（R9 优化后重试接入）");
            Assert.IsNull(EnemyVisualCatalog.VisualResourcePath(EnemyKind.Dummy), "Dummy（Harness 路径）不接入");
            Assert.AreEqual(RuntimeResourcePaths.EnemyBruceVisual, EnemyVisualCatalog.VisualResourcePath(EnemyKind.Warden), "Warden→Bruce 契约不变");
        }

        [Test]
        public void GargoyleVisual_LoadsAtContractPath()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyGargoyleVisual);
            Assert.IsNotNull(prefab, "Resources 缺失 " + RuntimeResourcePaths.EnemyGargoyleVisual + "（R9 契约 REQUIRED）");
            Assert.AreEqual("GargoyleVisual", prefab.name);
        }

        [Test]
        public void GargoyleVisual_ComponentAudit_VisualOnly()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyGargoyleVisual);
            Assert.IsNotNull(prefab);
            Assert.IsEmpty(prefab.GetComponentsInChildren<MonoBehaviour>(true), "正式视觉预制体零 MonoBehaviour");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Collider>(true), "零 Collider");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Rigidbody>(true), "零 Rigidbody");
            Assert.IsEmpty(prefab.GetComponentsInChildren<CharacterController>(true), "零 CharacterController");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Camera>(true), "零 Camera");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Light>(true), "零 Light");
            var skins = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Assert.GreaterOrEqual(skins.Length, 1, "至少 1 个 SkinnedMeshRenderer");
            foreach (var s in skins)
            {
                Assert.IsNotNull(s.sharedMesh, "SMR 必须绑定网格");
                foreach (var m in s.sharedMaterials)
                    Assert.IsNotNull(m, "SMR 材质槽不得为空");
            }
        }

        [Test]
        public void GargoyleVisual_Animator_FiveStatesAndNoRootMotion()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyGargoyleVisual);
            Assert.IsNotNull(prefab);
            var animator = prefab.GetComponentInChildren<Animator>(true);
            Assert.IsNotNull(animator, "必须含 Animator");
            Assert.IsFalse(animator.applyRootMotion, "root motion 必须 off");
            var ctrl = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
            Assert.IsNotNull(ctrl, "控制器应为 AnimatorController");
            var names = new System.Collections.Generic.HashSet<string>();
            foreach (var s in ctrl.layers[0].stateMachine.states)
                names.Add(s.state.name);
            foreach (string required in new[] { "Idle", "Run", "Attack", "Hit", "Death" })
                Assert.IsTrue(names.Contains(required), $"缺 {required} 状态（实有 {string.Join(",", names)}）");
        }

        [Test]
        public void GargoyleVisual_WrapperScale_FinitePositive_MidThreat()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyGargoyleVisual);
            Assert.IsNotNull(prefab);
            float s = prefab.transform.localScale.x;
            Assert.IsTrue(float.IsFinite(s) && s > 0f && s < 1f, $"wrapper scale 必须有限为正且小量（={s}，R8 目标 0.537）");
            Assert.AreEqual(s, prefab.transform.localScale.y, 1e-6f, "等比缩放");
            Assert.AreEqual(s, prefab.transform.localScale.z, 1e-6f, "等比缩放");
            var inst = (GameObject)Object.Instantiate(prefab);
            _created.Add(inst);
            var rs = inst.GetComponentsInChildren<Renderer>();
            Assert.Greater(rs.Length, 0);
            var b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            Assert.AreEqual(1.7f, b.size.y, 0.2f, $"世界高应≈1.70m（实测 {b.size.y:0.00}）；Ashling=中型威胁");
            Assert.AreEqual(0f, b.min.y, 0.05f, "必须贴地（worldMinY≈0）");
        }

        [Test]
        public void Presenter_Mount_PreservesPrefabScale_AndGameplayRootUnchanged()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyGargoyleVisual);
            Assert.IsNotNull(prefab);
            var parent = new GameObject("gameplay-root");
            parent.transform.localScale = new Vector3(1.4f, 1f, 1.4f);
            _created.Add(parent);
            var presenter = EnemyVisualPresenter.Mount(prefab, parent.transform);
            _created.Add(presenter.Root);
            Assert.AreEqual(prefab.transform.localScale, presenter.Root.transform.localScale,
                "Mount 必须保留 GargoyleVisual 预制体根 scale（≈0.537，世界高≈1.70m）——R4 根因不回归");
            Assert.AreEqual(Vector3.zero, presenter.Root.transform.localPosition, "挂载位置归零");
            Assert.AreEqual(Quaternion.identity, presenter.Root.transform.localRotation, "挂载朝向归零");
            Assert.AreEqual(new Vector3(1.4f, 1f, 1.4f), parent.transform.localScale, "gameplay root scale 零改动");
        }

        [Test]
        public void Presenter_Feedback_SupportsGargoyleMaterials_AndRestoresExactly()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyGargoyleVisual);
            Assert.IsNotNull(prefab);
            var parent = new GameObject("gameplay-root");
            _created.Add(parent);
            var presenter = EnemyVisualPresenter.Mount(prefab, parent.transform);
            _created.Add(presenter.Root);
            Assert.AreEqual(EnemyFeedbackState.Normal, presenter.LastFeedbackState, "挂载初始=Normal");

            var snapshot = new System.Collections.Generic.List<Material>();
            foreach (var r in presenter.Root.GetComponentsInChildren<Renderer>())
                foreach (var m in r.sharedMaterials)
                    snapshot.Add(m);
            Assert.Greater(snapshot.Count, 0, "至少一个材质槽");

            presenter.ApplyFeedback(true, 0f, true);
            Assert.AreEqual(EnemyFeedbackState.Ignite, presenter.LastFeedbackState, "必须进入 Ignite");
            var renderers = presenter.Root.GetComponentsInChildren<Renderer>();
            var block = new MaterialPropertyBlock();
            renderers[0].GetPropertyBlock(block, 0);
            Assert.IsTrue(block.HasProperty("_BaseColor"), "Ignite tint 必须写到 property block");

            presenter.ApplyFeedback(true, 0.1f, true);
            Assert.AreEqual(EnemyFeedbackState.Hit, presenter.LastFeedbackState, "Hit 优先于 Ignite");
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
