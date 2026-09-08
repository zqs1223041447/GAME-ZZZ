using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Phase 5 Art Trial R3（工作令 S3-P5-ART-R3-TROLL-RUNTIME-INTEGRATION）：Troll 正式敌人视觉集成契约。
    /// 只验证「正式视觉预制体安全可挂载 + 表现层映射纯函数 + gameplay 常量未被本轮改动」，
    /// 不评美术观感；按组件类型断言，不硬编码 vendor 层级名。
    /// </summary>
    public sealed class TrollRuntimeVisualContractTests
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

        GameObject LoadVisualPrefab()
        {
            return Resources.Load<GameObject>(RuntimeResourcePaths.EnemyTrollVisual);
        }

        [Test]
        public void EnemyVisualPrefab_LoadsAtContractPath()
        {
            GameObject prefab = LoadVisualPrefab();
            Assert.IsNotNull(prefab, "Resources 缺失 " + RuntimeResourcePaths.EnemyTrollVisual + "（R3 契约 REQUIRED）");
            Assert.AreEqual("TrollWarriorVisual", prefab.name);
        }

        [Test]
        public void EnemyVisualPrefab_ComponentAudit_VisualOnly()
        {
            GameObject prefab = LoadVisualPrefab();
            Assert.IsNotNull(prefab);

            var behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            Assert.AreEqual(0, behaviours.Length, "正式视觉预制体不得含任何 MonoBehaviour（vendor 脚本=0）");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Collider>(true), "视觉预制体零 Collider");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Rigidbody>(true), "视觉预制体零 Rigidbody");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Camera>(true), "视觉预制体零 Camera");
            Assert.IsEmpty(prefab.GetComponentsInChildren<Light>(true), "视觉预制体零 Light");

            var skins = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Assert.GreaterOrEqual(skins.Length, 3, "Troll_2 视觉=body+双持武器 3 个 SkinnedMeshRenderer");
            for (int i = 0; i < skins.Length; i++)
            {
                Assert.IsNotNull(skins[i].sharedMesh, "SMR 网格必须绑定：" + skins[i].name);
                Assert.Greater(skins[i].sharedMesh.vertexCount, 0);
                var materials = skins[i].sharedMaterials;
                for (int m = 0; m < materials.Length; m++)
                    Assert.IsNotNull(materials[m], "SMR 材质槽必须已挂：" + skins[i].name);
            }
        }

        [Test]
        public void EnemyVisualPrefab_AnimatorStates_SixStates()
        {
            GameObject prefab = LoadVisualPrefab();
            Assert.IsNotNull(prefab);
            var parent = new GameObject("parent");
            _created.Add(parent);
            EnemyVisualPresenter presenter = EnemyVisualPresenter.Mount(prefab, parent.transform);
            _created.Add(presenter.Root);
            Assert.IsNotNull(presenter);

            Animator animator = presenter.Root.GetComponentInChildren<Animator>();
            Assert.IsNotNull(animator, "视觉预制体必须含 Animator");
            Assert.IsNotNull(animator.runtimeAnimatorController, "Animator 必须挂 controller");
            // R2 controller 的 6 状态最小集沿用：五类动画（Idle/Move/Attack/Hit/Death，Move=Walk+Run 两态）
            Assert.IsTrue(animator.HasState(0, Animator.StringToHash("Idle")), "缺 Idle 状态");
            Assert.IsTrue(animator.HasState(0, Animator.StringToHash("Walk")), "缺 Walk 状态");
            Assert.IsTrue(animator.HasState(0, Animator.StringToHash("Run")), "缺 Run 状态");
            Assert.IsTrue(animator.HasState(0, Animator.StringToHash("Attack")), "缺 Attack 状态");
            Assert.IsTrue(animator.HasState(0, Animator.StringToHash("Hit")), "缺 Hit 状态");
            Assert.IsTrue(animator.HasState(0, Animator.StringToHash("Death")), "缺 Death 状态");
        }

        [Test]
        public void Presenter_Mount_DoesNotTouchGameplayRoot()
        {
            GameObject prefab = LoadVisualPrefab();
            Assert.IsNotNull(prefab);
            var parent = new GameObject("gameplay-root");
            parent.transform.localPosition = new Vector3(3f, 0f, -2f);
            parent.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            parent.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
            _created.Add(parent);

            EnemyVisualPresenter presenter = EnemyVisualPresenter.Mount(prefab, parent.transform);
            _created.Add(presenter.Root);

            // gameplay root 零改动：美术 scale 不作用于 root
            Assert.AreEqual(new Vector3(1.2f, 1f, 1.2f), parent.transform.localScale, "挂载不得改 gameplay root scale");
            Assert.AreEqual(new Vector3(3f, 0f, -2f), parent.transform.localPosition);
            Assert.AreEqual(Quaternion.Euler(0f, 45f, 0f), parent.transform.localRotation);
            // 视觉子层级位置/朝向归零，scale 沿用预制体根节点自带值（挂载不得覆写美术 scale）
            Assert.AreEqual(Vector3.zero, presenter.Root.transform.localPosition);
            Assert.AreEqual(Quaternion.identity, presenter.Root.transform.localRotation);
            Assert.AreEqual(prefab.transform.localScale, presenter.Root.transform.localScale, "挂载必须保留预制体根 scale（≈2.5047，动画口径≈2.35m）");
            Assert.AreEqual("VisualRoot", presenter.Root.name);
        }

        [Test]
        public void Presenter_MoveStateMapping_Pure()
        {
            Assert.AreEqual("Run", EnemyVisualPresenter.MoveStateFor(AnimState.Run));
            Assert.AreEqual("Idle", EnemyVisualPresenter.MoveStateFor(AnimState.Idle));
            Assert.AreEqual("Idle", EnemyVisualPresenter.MoveStateFor(AnimState.Hit));
            Assert.AreEqual("Idle", EnemyVisualPresenter.MoveStateFor(AnimState.Death));
        }

        [Test]
        public void Presenter_IdleAndRun_FollowMovementState()
        {
            GameObject prefab = LoadVisualPrefab();
            Assert.IsNotNull(prefab);
            var parent = new GameObject("gameplay-root");
            _created.Add(parent);
            EnemyVisualPresenter presenter = EnemyVisualPresenter.Mount(prefab, parent.transform);
            _created.Add(presenter.Root);

            presenter.Present(true, AnimState.Idle, 0f, 0, 0f);
            Assert.AreEqual("Idle", presenter.LastRequestedState);
            presenter.Present(true, AnimState.Run, 0f, 0, 1f);
            Assert.AreEqual("Run", presenter.LastRequestedState);
            presenter.Present(true, AnimState.Idle, 0f, 0, 2f);
            Assert.AreEqual("Idle", presenter.LastRequestedState);
        }

        [Test]
        public void Presenter_AttackEdge_DrivenByExecutionSerial()
        {
            GameObject prefab = LoadVisualPrefab();
            Assert.IsNotNull(prefab);
            var parent = new GameObject("gameplay-root");
            _created.Add(parent);
            EnemyVisualPresenter presenter = EnemyVisualPresenter.Mount(prefab, parent.transform);
            _created.Add(presenter.Root);

            // 首次 Present 建立基线：不伪造攻击边沿
            presenter.Present(true, AnimState.Idle, 0f, 0, 0f);
            Assert.AreEqual("Idle", presenter.LastRequestedState);

            // 攻击执行序号 +1（唯一 canonical 信号）→ Attack
            presenter.Present(true, AnimState.Idle, 0f, 1, 1f);
            Assert.AreEqual("Attack", presenter.LastRequestedState);

            // 动作窗口内不被移动姿态覆盖（同一 serial 不重复触发）
            presenter.Present(true, AnimState.Run, 0f, 1, 1.2f);
            Assert.AreEqual("Attack", presenter.LastRequestedState);

            // 窗口外恢复移动姿态；serial 不变则不再触发
            presenter.Present(true, AnimState.Run, 0f, 1, 30f);
            Assert.AreEqual("Run", presenter.LastRequestedState);
        }

        [Test]
        public void Presenter_HitEdge_PlaysHitState()
        {
            GameObject prefab = LoadVisualPrefab();
            Assert.IsNotNull(prefab);
            var parent = new GameObject("gameplay-root");
            _created.Add(parent);
            EnemyVisualPresenter presenter = EnemyVisualPresenter.Mount(prefab, parent.transform);
            _created.Add(presenter.Root);

            presenter.Present(true, AnimState.Run, 0f, 0, 0f);
            Assert.AreEqual("Run", presenter.LastRequestedState);
            // 受击=已有 HitFlash+AnimState.Hit 信号
            presenter.Present(true, AnimState.Hit, 0.20f, 0, 0.5f);
            Assert.AreEqual("Hit", presenter.LastRequestedState);
            // Hit 窗口内不被覆盖
            presenter.Present(true, AnimState.Hit, 0.10f, 0, 0.7f);
            Assert.AreEqual("Hit", presenter.LastRequestedState);
            // 信号消退后恢复
            presenter.Present(true, AnimState.Run, 0f, 0, 30f);
            Assert.AreEqual("Run", presenter.LastRequestedState);
        }

        [Test]
        public void Presenter_Death_PlaysDeathState_AlignsWithRecycle()
        {
            GameObject prefab = LoadVisualPrefab();
            Assert.IsNotNull(prefab);
            var parent = new GameObject("gameplay-root");
            _created.Add(parent);
            EnemyVisualPresenter presenter = EnemyVisualPresenter.Mount(prefab, parent.transform);
            _created.Add(presenter.Root);

            presenter.Present(true, AnimState.Run, 0f, 0, 0f);
            presenter.Present(false, AnimState.Death, 0f, 0, 1f);
            Assert.AreEqual("Death", presenter.LastRequestedState);
            // 死亡保持（回收由 gameplay 侧 0.40s DeathRecycle 决定，表现层不改）
            presenter.Present(false, AnimState.Death, 0f, 0, 1.2f);
            Assert.AreEqual("Death", presenter.LastRequestedState);
        }

        [Test]
        public void GameplayConstants_UnchangedByR3Integration()
        {
            // 本轮只加视觉：pool/回收/受击闪白等 gameplay 常量必须原样（护栏）
            Assert.AreEqual(300, CombatRules.DummyPoolSize);
            Assert.AreEqual(0.40f, CombatRules.DeathRecycle);
            Assert.AreEqual(0.20f, CombatRules.HitFlash);
        }

        [Test]
        public void EnemyVisualPrefab_IsPbrUrpMaterials()
        {
            GameObject prefab = LoadVisualPrefab();
            Assert.IsNotNull(prefab);
            var skins = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Assert.Greater(skins.Length, 0);
            Shader urp = Shader.Find("Universal Render Pipeline/Lit");
            Assert.IsNotNull(urp);
            bool anyUrp = false;
            for (int i = 0; i < skins.Length; i++)
            {
                var materials = skins[i].sharedMaterials;
                for (int m = 0; m < materials.Length; m++)
                {
                    Assert.IsNotNull(materials[m]);
                    Assert.AreNotEqual("Error", materials[m].shader.name, "Error shader：" + materials[m].name);
                    if (materials[m].shader == urp)
                        anyUrp = true;
                }
            }
            Assert.IsTrue(anyUrp, "视觉预制体至少一个 URP/Lit 材质（R2 已验收迁移，本轮沿用）");
        }
    }
}
