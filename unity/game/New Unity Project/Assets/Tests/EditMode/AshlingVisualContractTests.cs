using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Phase 5 Art Trial R8（工作令 S3-P5-ART-R8-ASHLING-FORMAL-VISUAL）：Ashling 候选（GargoyleVisual）
    /// 资产健康验证——**ASSET ACCEPTED, RUNTIME INTEGRATION BLOCKED BY ART PERFORMANCE**：
    /// Formal Art Performance Gate 实测 200/300 密度超 8.33ms 预算（R8 FAIL），按工作令恢复
    /// Ashling→placeholder、不做降质修绿；资产保留于 Assets/Art/Enemies/Gargoyle/Prefabs/（非 Resources 入口），
    /// 供未来 Art Performance 优化轮直接复用。本套测试锁定「保留资产仍健康 + Catalog 不接线」。
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
        public void Catalog_AshlingStaysPlaceholder()
        {
            Assert.IsNull(EnemyVisualCatalog.VisualResourcePath(EnemyKind.Ashling),
                "R8 Art Gate 阻断后 Ashling 必须回退 placeholder（不降质修绿）");
            Assert.IsNull(EnemyVisualCatalog.VisualResourcePath(EnemyKind.Dummy), "Dummy（Harness 路径）不接入");
            Assert.AreEqual(RuntimeResourcePaths.EnemyBruceVisual, EnemyVisualCatalog.VisualResourcePath(EnemyKind.Warden), "Warden→Bruce 契约不变");
        }

        [Test]
        public void RetainedPrefab_NotAResourcesEntry()
        {
            Assert.IsNull(Resources.Load<GameObject>("Enemies/GargoyleVisual"),
                "被阻断的候选不得保留 Resources Runtime 入口");
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(RetainedPrefabPath);
            Assert.IsNotNull(prefab, "保留资产必须存在于 " + RetainedPrefabPath);
            Assert.AreEqual("GargoyleVisual", prefab.name);
        }

        [Test]
        public void RetainedPrefab_ComponentAudit_VisualOnly()
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(RetainedPrefabPath);
            Assert.IsNotNull(prefab);
            Assert.IsEmpty(prefab.GetComponentsInChildren<MonoBehaviour>(true), "零 MonoBehaviour");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Collider>(true), "零 Collider");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Rigidbody>(true), "零 Rigidbody");
            Assert.IsEmpty(prefab.GetComponentsInChildren<CharacterController>(true), "零 CharacterController");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Camera>(true), "零 Camera");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Light>(true), "零 Light");
            var skins = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Assert.GreaterOrEqual(skins.Length, 1, "至少 1 个 SkinnedMeshRenderer");
            foreach (var s in skins)
                foreach (var m in s.sharedMaterials)
                    Assert.IsNotNull(m, "SMR 材质槽不得为空");
        }

        [Test]
        public void RetainedPrefab_Animator_FiveStatesAndNoRootMotion()
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(RetainedPrefabPath);
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
        public void RetainedPrefab_WrapperScale_FinitePositive_MidThreat()
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(RetainedPrefabPath);
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
            Assert.AreEqual(1.7f, b.size.y, 0.2f, $"世界高应≈1.70m（实测 {b.size.y:0.00}）");
            Assert.AreEqual(0f, b.min.y, 0.05f, "必须贴地（worldMinY≈0）");
        }

        [Test]
        public void RetainedPrefab_MountAndFeedback_StillHealthy()
        {
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(RetainedPrefabPath);
            Assert.IsNotNull(prefab);
            var parent = new GameObject("gameplay-root");
            _created.Add(parent);
            var presenter = EnemyVisualPresenter.Mount(prefab, parent.transform);
            _created.Add(presenter.Root);
            Assert.AreEqual(prefab.transform.localScale, presenter.Root.transform.localScale,
                "Mount 必须保留预制体根 scale（R4 根因不回归——保留资产同样适用）");
            Assert.AreEqual(EnemyFeedbackState.Normal, presenter.LastFeedbackState, "挂载初始=Normal");
            presenter.ApplyFeedback(true, 0f, true);
            Assert.AreEqual(EnemyFeedbackState.Ignite, presenter.LastFeedbackState, "保留资产反馈健康：Ignite 可用");
            presenter.ApplyFeedback(true, 0f, false);
            Assert.AreEqual(EnemyFeedbackState.Normal, presenter.LastFeedbackState, "恢复精确 Normal");
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
