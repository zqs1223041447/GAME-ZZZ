using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Phase 5 Art Trial R1（工作令 S3-P5-ART-R1-BRUCE-TRIAL 三十六）：Bruce 集成契约——
    /// 只验证「能作为视觉资产被项目安全引用」，不评判美术观感：prefab 加载/缺脚本 0/渲染器>0/
    /// 缩放有限为正/零 gameplay 组件/材质走 URP Lit/Animator 引用有效/源层级稳定/贴图导入/无 Resources 泄漏。
    /// </summary>
    public class BruceArtTrialTests
    {
        const string PrefabPath = "Assets/Art/Enemies/DragonWarlordBruce/Prefabs/BruceTrial.prefab";
        const string FbxPath = "Assets/Art/Enemies/DragonWarlordBruce/Source/bruce.FBX";

        static GameObject LoadPrefab()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        [Test]
        public void BruceTrial_PrefabLoads()
        {
            var prefab = LoadPrefab();
            Assert.IsNotNull(prefab, "BruceTrial.prefab 必须能加载");
            Assert.AreEqual("BruceTrial", prefab.name);
        }

        [Test]
        public void BruceTrial_NoMissingScripts()
        {
            var prefab = LoadPrefab();
            Assert.IsNotNull(prefab);
            foreach (var t in prefab.GetComponentsInChildren<Transform>(true))
                foreach (var c in t.GetComponents<Component>())
                    Assert.IsNotNull(c, $"{t.name} 存在缺失脚本（missing script）");
        }

        [Test]
        public void BruceTrial_RequiredRenderers_Present()
        {
            var prefab = LoadPrefab();
            Assert.IsNotNull(prefab);
            var smr = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var mr = prefab.GetComponentsInChildren<MeshRenderer>(true);
            Assert.AreEqual(1, smr.Length, "主体=1 个 SkinnedMeshRenderer");
            Assert.AreEqual(4, mr.Length, "装备/肩甲静态件=4 个 MeshRenderer");
            Assert.IsTrue(smr[0].sharedMesh != null, "SMR 必须绑定网格");
            foreach (var r in smr.Concat<Renderer>(mr))
                Assert.IsFalse(r.sharedMaterials.Any(m => m == null), $"{r.name} 存在空材质槽");
        }

        [Test]
        public void BruceTrial_WrapperScale_FinitePositive_BossHeight()
        {
            var prefab = LoadPrefab();
            Assert.IsNotNull(prefab);
            float s = prefab.transform.localScale.x;
            Assert.IsTrue(float.IsFinite(s) && s > 0f && s < 0.01f, $"wrapper scale 必须有限为正且小量（={s}）");
            var smr = prefab.GetComponentInChildren<SkinnedMeshRenderer>(true);
            // 高度=网格本地 bounds × wrapper 缩放（不依赖皮肤缓存 bounds）；Boss 目标≈2.6m（≈DarkKnight 1.19 的 2.2 倍）
            float h = smr.sharedMesh.bounds.size.y * s;
            Assert.AreEqual(2.6f, h, 0.2f, $"可见高度应≈2.6m（实测 {h:0.00}）");
            Assert.AreEqual(s, prefab.transform.localScale.y, 1e-6f, "等比缩放");
            Assert.AreEqual(s, prefab.transform.localScale.z, 1e-6f, "等比缩放");
        }

        [Test]
        public void BruceTrial_NoGameplayComponents_NoVendorScripts()
        {
            var prefab = LoadPrefab();
            Assert.IsNotNull(prefab);
            foreach (var t in prefab.GetComponentsInChildren<Transform>(true))
            {
                foreach (var c in t.GetComponents<Component>())
                {
                    var type = c.GetType();
                    Assert.IsFalse(typeof(Collider).IsAssignableFrom(type), $"{t.name} 不得带 gameplay collider（{type.Name}）");
                    Assert.IsFalse(typeof(Rigidbody).IsAssignableFrom(type), $"{t.name} 不得带 Rigidbody");
                    Assert.IsFalse(type == typeof(Camera) || type == typeof(Light), $"{t.name} 不得带相机/光源（vendor demo 残留）");
                    Assert.IsFalse(typeof(MonoBehaviour).IsAssignableFrom(type), $"{t.name} 不得带任何 MonoBehaviour（vendor/demo 脚本目标=0）");
                }
            }
        }

        [Test]
        public void BruceTrial_Materials_UrpLit_NoErrorShader()
        {
            var prefab = LoadPrefab();
            Assert.IsNotNull(prefab);
            foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
                foreach (var m in r.sharedMaterials)
                {
                    Assert.IsNotNull(m, "材质槽不得为空");
                    Assert.AreEqual("Universal Render Pipeline/Lit", m.shader.name,
                        $"{r.name} 材质 {m.name} 必须走 URP/Lit（实测 {m.shader.name}）");
                    Assert.IsFalse(m.shader.name.Contains("Error"), "不得使用错误着色器");
                    var map = m.GetTexture("_BaseMap");
                    Assert.IsNotNull(map, $"{m.name} 必须挂 BaseMap");
                }
        }

        [Test]
        public void BruceTrial_AnimatorReferences_Valid()
        {
            var prefab = LoadPrefab();
            Assert.IsNotNull(prefab);
            var anim = prefab.GetComponent<Animator>();
            Assert.IsNotNull(anim, "wrapper 根必须带 Animator");
            Assert.IsNotNull(anim.avatar, "Generic avatar 必须有效");
            Assert.IsFalse(anim.applyRootMotion, "root motion 不接管 gameplay 移动（工作令 二十九）");
            Assert.IsNotNull(anim.runtimeAnimatorController, "controller 引用必须有效");
            var ctrl = anim.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
            Assert.IsNotNull(ctrl);
            var names = ctrl.layers[0].stateMachine.states.Select(s => s.state.name).ToArray();
            // 动画五类全覆盖（工作令 九）：Idle/Move(walk,run)/Attack/Hit/Death
            foreach (var need in new[] { "Idle", "Walk", "Run", "Attack", "Hit", "Death" })
                Assert.Contains(need, names, $"controller 缺状态 {need}");
            Assert.AreEqual("Idle", ctrl.layers[0].stateMachine.defaultState.name);
        }

        [Test]
        public void Bruce_SourceHierarchy_StableClips()
        {
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            Assert.IsNotNull(fbx, "bruce.FBX 必须能加载");
            var importer = AssetImporter.GetAtPath(FbxPath) as ModelImporter;
            Assert.IsNotNull(importer);
            Assert.AreEqual(ModelImporterAnimationType.Generic, importer.animationType,
                "龙形特殊骨架=Generic（工作令 八：不得强行 Humanoid）");
            var clips = AssetDatabase.LoadAllAssetsAtPath(FbxPath).OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToList();
            Assert.GreaterOrEqual(clips.Count, 20, $"应从 855 帧时间线切出 20 个 clip（实测 {clips.Count}）");
            // 五类代表 clip 必须在（canonical 名，见 Bruce_trial 记录）
            foreach (var need in new[] { "combat_mode", "walk", "run", "left_straight_blow", "taking_hit", "dying" })
                Assert.IsTrue(clips.Any(c => c.name == need), $"缺动画 clip {need}");
            var smr = fbx.GetComponentInChildren<SkinnedMeshRenderer>(true);
            Assert.IsNotNull(smr);
            Assert.IsNotNull(smr.sharedMesh);
            Assert.Greater(smr.bones.Length, 40, "骨骼数应>40（实测 biped 骨架）");
        }

        [Test]
        public void Bruce_Textures_Default2D_512()
        {
            foreach (var p in new[] {
                "Assets/Art/Enemies/DragonWarlordBruce/Textures/warlord_complete_map.tif",
                "Assets/Art/Enemies/DragonWarlordBruce/Textures/warlord_spear_complete.tif" })
            {
                var t = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                Assert.IsNotNull(t, $"{p} 必须导入为 Texture2D（不得 Cubemap）");
                Assert.AreEqual(512, t.width, "vendor 说明=512²");
                Assert.AreEqual(512, t.height);
                var imp = AssetImporter.GetAtPath(p) as TextureImporter;
                Assert.AreEqual(TextureImporterShape.Texture2D, imp.textureShape);
            }
        }

        [Test]
        public void Bruce_NoResourcesFolder_NoVendorPrefab()
        {
            Assert.IsFalse(System.IO.Directory.Exists(
                "Assets/Art/Enemies/DragonWarlordBruce/Resources"), "Art 目录不得塞 Resources/");
            Assert.IsFalse(System.IO.File.Exists(
                "Assets/Art/Enemies/DragonWarlordBruce/Prefabs/Bruce_Prefab.prefab"),
                "vendor demo prefab（含 Light）应被剔除（原件留导演 RAR）");
        }
    }
}
