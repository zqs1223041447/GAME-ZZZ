using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Phase 5 Art Trial R4（工作令 S3-P5-ART-R4-SECOND-ENEMY-VISUAL）：第二敌人视觉（Stinger→FireLion）
    /// 与通用映射表（EnemyVisualCatalog）契约。只验证「映射纯函数 + 正式视觉预制体安全可挂载」，
    /// 不评美术观感；R3 既有测试不动、必须继续 PASS。
    /// </summary>
    public sealed class EnemyVisualCatalogContractTests
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
        public void Catalog_Mapping_Deterministic()
        {
            Assert.AreEqual(RuntimeResourcePaths.EnemyTrollVisual, EnemyVisualCatalog.VisualResourcePath(EnemyKind.Brute),
                "Brute 必须仍映射 TrollWarriorVisual（R3 契约不变）");
            Assert.AreEqual(RuntimeResourcePaths.EnemyFireLionVisual, EnemyVisualCatalog.VisualResourcePath(EnemyKind.Stinger),
                "Stinger 必须映射 FireLionVisual（R4 第二视觉）");
            Assert.AreEqual(RuntimeResourcePaths.EnemyBruceVisual, EnemyVisualCatalog.VisualResourcePath(EnemyKind.Warden),
                "Warden 必须映射 BruceVisual（S3-P5-ART-R6 第三正式视觉）");
            Assert.IsNull(EnemyVisualCatalog.VisualResourcePath(EnemyKind.Ashling),
                "Ashling 保持 placeholder——R8 候选 GargoyleVisual 被 Formal Art Performance Gate 阻断（ASSET ACCEPTED, INTEGRATION BLOCKED）");
            Assert.IsNull(EnemyVisualCatalog.VisualResourcePath(EnemyKind.Dummy), "Dummy（Harness 路径）不接入");
            Assert.IsNull(EnemyVisualCatalog.VisualResourcePath(EnemyKind.Dummy), "Dummy（Harness 路径）不接入");
        }

        [Test]
        public void FireLionVisual_LoadsAtContractPath()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyFireLionVisual);
            Assert.IsNotNull(prefab, "Resources 缺失 " + RuntimeResourcePaths.EnemyFireLionVisual + "（R4 契约 REQUIRED）");
            Assert.AreEqual("FireLionVisual", prefab.name);
        }

        [Test]
        public void FireLionVisual_ComponentAudit_VisualOnly()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyFireLionVisual);
            Assert.IsNotNull(prefab);

            var behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            Assert.AreEqual(0, behaviours.Length, "正式视觉预制体不得含任何 MonoBehaviour（vendor 脚本=0）");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Collider>(true), "视觉预制体零 Collider");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Rigidbody>(true), "视觉预制体零 Rigidbody");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Camera>(true), "视觉预制体零 Camera");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Light>(true), "视觉预制体零 Light");

            var skins = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Assert.GreaterOrEqual(skins.Length, 1, "Lion 至少 1 个 SkinnedMeshRenderer");
            for (int i = 0; i < skins.Length; i++)
            {
                Assert.IsNotNull(skins[i].sharedMesh, "SMR 网格必须绑定：" + skins[i].name);
                var materials = skins[i].sharedMaterials;
                for (int m = 0; m < materials.Length; m++)
                    Assert.IsNotNull(materials[m], "SMR 材质槽必须已挂：" + skins[i].name);
            }
        }

        [Test]
        public void FireLionVisual_AnimatorStates_FiveStates_NoWalkFallback()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyFireLionVisual);
            Assert.IsNotNull(prefab);
            var parent = new GameObject("parent");
            _created.Add(parent);
            var presenter = EnemyVisualPresenter.Mount(prefab, parent.transform);
            _created.Add(presenter.Root);
            Assert.IsNotNull(presenter);

            var animator = presenter.Root.GetComponentInChildren<Animator>();
            Assert.IsNotNull(animator, "视觉预制体必须含 Animator");
            Assert.IsNotNull(animator.runtimeAnimatorController, "Animator 必须挂 controller");
            // R4：vendor 无 walk 类 clip（如实记录），表现层 Move=Run/Idle 覆盖，不伪造 Walk
            Assert.IsTrue(animator.HasState(0, Animator.StringToHash("Idle")), "缺 Idle 状态");
            Assert.IsTrue(animator.HasState(0, Animator.StringToHash("Run")), "缺 Run 状态");
            Assert.IsTrue(animator.HasState(0, Animator.StringToHash("Attack")), "缺 Attack 状态");
            Assert.IsTrue(animator.HasState(0, Animator.StringToHash("Hit")), "缺 Hit 状态");
            Assert.IsTrue(animator.HasState(0, Animator.StringToHash("Death")), "缺 Death 状态");
        }

        [Test]
        public void FireLionVisual_Mount_DoesNotTouchGameplayRoot()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyFireLionVisual);
            Assert.IsNotNull(prefab);
            var parent = new GameObject("gameplay-root");
            parent.transform.localScale = new Vector3(1.1f, 1f, 1.1f);
            _created.Add(parent);
            var presenter = EnemyVisualPresenter.Mount(prefab, parent.transform);
            _created.Add(presenter.Root);
            Assert.AreEqual(new Vector3(1.1f, 1f, 1.1f), parent.transform.localScale, "挂载不得改 gameplay root scale");
            Assert.AreEqual(Vector3.zero, presenter.Root.transform.localPosition);
            float s = presenter.Root.transform.localScale.x;
            Assert.IsTrue(float.IsFinite(s) && s > 0f && s < 0.05f, $"挂载必须保留预制体根 scale（实测 {s}）");
            Assert.AreEqual(prefab.transform.localScale, presenter.Root.transform.localScale, "挂载不得覆写预制体根 scale（≈0.00648，世界高≈1.40m）");
        }

        [Test]
        public void FireLionVisual_UrpMaterial_NoErrorShader()
        {
            var prefab = Resources.Load<GameObject>(RuntimeResourcePaths.EnemyFireLionVisual);
            Assert.IsNotNull(prefab);
            var skins = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Shader urp = Shader.Find("Universal Render Pipeline/Lit");
            Assert.IsNotNull(urp);
            foreach (var s in skins)
                foreach (var m in s.sharedMaterials)
                {
                    Assert.IsNotNull(m);
                    Assert.AreNotEqual("Error", m.shader.name, "Error shader：" + m.name);
                    Assert.AreEqual(urp, m.shader, "第二视觉沿用 URP/Lit（§28 不换 shader system）");
                }
        }

        [Test]
        public void GameplayConstants_UnchangedByR4Integration()
        {
            Assert.AreEqual(300, CombatRules.DummyPoolSize);
            Assert.AreEqual(0.40f, CombatRules.DeathRecycle);
            Assert.AreEqual(0.20f, CombatRules.HitFlash);
        }
    }
}
