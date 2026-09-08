using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Phase 5 Art Trial R2（工作令 S3-P5-ART-R2-PBR-STYLE-ANCHOR 三十五）：Troll_2 候选集成契约——
    /// 只验证「能作为视觉资产被项目安全引用」，不评判美术观感：prefab 加载/缺脚本 0/渲染器>0/
    /// 缩放有限为正/零 gameplay 组件/材质走 URP Lit 且贴图挂接齐全（BaseMap+Normal）/Animator 有效/
    /// Humanoid rig 与 23 take 动画稳定/贴图导入形态正确/无 Resources 泄漏与 vendor 垃圾。
    /// </summary>
    public class TrollWarriorArtTrialTests
    {
        const string PrefabPath = "Assets/Art/Enemies/TrollWarrior/Prefabs/TrollWarriorTrial.prefab";
        const string FbxPath = "Assets/Art/Enemies/TrollWarrior/Source/Troll_2_L.FBX";
        const string AnimsPath = "Assets/Art/Enemies/TrollWarrior/Source/Troll_2_L_anims_split.fbx";

        static GameObject LoadPrefab()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        [Test]
        public void TrollWarriorTrial_PrefabLoads()
        {
            var prefab = LoadPrefab();
            Assert.IsNotNull(prefab, "TrollWarriorTrial.prefab 必须能加载");
            Assert.AreEqual("TrollWarriorTrial", prefab.name);
        }

        [Test]
        public void TrollWarriorTrial_NoMissingScripts()
        {
            var prefab = LoadPrefab();
            Assert.IsNotNull(prefab);
            foreach (var t in prefab.GetComponentsInChildren<Transform>(true))
                foreach (var c in t.GetComponents<Component>())
                    Assert.IsNotNull(c, $"{t.name} 存在缺失脚本（missing script）");
        }

        [Test]
        public void TrollWarriorTrial_RequiredRenderers_Present()
        {
            var prefab = LoadPrefab();
            Assert.IsNotNull(prefab);
            var smr = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var mr = prefab.GetComponentsInChildren<MeshRenderer>(true);
            Assert.AreEqual(3, smr.Length, "body+weapon_left+weapon_right=3 个 SkinnedMeshRenderer");
            Assert.AreEqual(0, mr.Length, "无静态 MeshRenderer");
            var body = smr.FirstOrDefault(r => r.name == "body");
            Assert.IsNotNull(body, "必须有 body 渲染器");
            Assert.IsTrue(body.sharedMesh != null, "body SMR 必须绑定网格");
            foreach (var r in smr)
                Assert.IsFalse(r.sharedMaterials.Any(m => m == null), $"{r.name} 存在空材质槽");
        }

        [Test]
        public void TrollWarriorTrial_WrapperScale_FinitePositive_HeavyHeight()
        {
            var prefab = LoadPrefab();
            Assert.IsNotNull(prefab);
            float s = prefab.transform.localScale.x;
            Assert.IsTrue(float.IsFinite(s) && s > 0.5f && s < 20f, $"wrapper scale 必须有限为正且合理（={s}）");
            var smr = prefab.GetComponentInChildren<SkinnedMeshRenderer>(true);
            // 高度=body 网格本地 bounds × wrapper 缩放（bind 口径；实测动画渲染口径≈2.35≈bind×1.09）
            // 角色定位：重量级精英≈2.0×DarkKnight 1.19，低于 Bruce Boss 2.60
            float h = smr.sharedMesh.bounds.size.y * s;
            Assert.AreEqual(2.15f, h, 0.15f, $"bind 口径高度应≈2.15m（实测 {h:0.00}，动画口径≈{h*1.09f:0.00}）");
            Assert.AreEqual(s, prefab.transform.localScale.y, 1e-4f, "等比缩放");
            Assert.AreEqual(s, prefab.transform.localScale.z, 1e-4f, "等比缩放");
        }

        [Test]
        public void TrollWarriorTrial_NoGameplayComponents_NoVendorScripts()
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
        public void TrollWarriorTrial_Materials_UrpLit_PbrMapsAssigned()
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
                    var bump = m.GetTexture("_BumpMap");
                    Assert.IsNotNull(bump, $"{m.name} 必须挂 Normal（十八：PBR 贴图组正确挂接）");
                    // 项目自有 URP 转换材质（不得残留 FBX 内嵌 vendor 默认材质 Material_#0/_7___Default）
                    Assert.IsTrue(m.name == "Troll_2_D" || m.name == "Sword_D",
                        $"{r.name} 应使用项目材质 Troll_2_D/Sword_D（实测 {m.name}）");
                }
        }

        [Test]
        public void TrollWarriorTrial_AnimatorReferences_Valid()
        {
            var prefab = LoadPrefab();
            Assert.IsNotNull(prefab);
            var anim = prefab.GetComponent<Animator>();
            Assert.IsNotNull(anim, "wrapper 根必须带 Animator");
            Assert.IsNotNull(anim.avatar, "Humanoid avatar 必须有效");
            Assert.IsTrue(anim.avatar.isHuman, "36 骨 Biped 人形=Humanoid rig（工作令 十五：按真实结构判断）");
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
        public void TrollWarrior_SourceAssets_HumanoidRig_StableClips()
        {
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            Assert.IsNotNull(fbx, "Troll_2_L.FBX 必须能加载");
            var importer = AssetImporter.GetAtPath(FbxPath) as ModelImporter;
            Assert.IsNotNull(importer);
            Assert.AreEqual((ModelImporterAnimationType)3, importer.animationType,
                "36 骨标准 Biped 人形=Humanoid（isHuman 已验证；非强行转换）");
            var animsImporter = AssetImporter.GetAtPath(AnimsPath) as ModelImporter;
            Assert.IsNotNull(animsImporter);
            Assert.AreEqual((ModelImporterAnimationType)3, animsImporter.animationType, "动画源 FBX 同为 Humanoid");
            var clips = AssetDatabase.LoadAllAssetsAtPath(AnimsPath).OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToList();
            Assert.GreaterOrEqual(clips.Count, 20, $"厂商已切好的 take 应≥20（实测 {clips.Count}）");
            // 五类代表 clip 必须在（controller 映射源）
            foreach (var need in new[] { "Standby", "Walk", "Run", "Attack1", "Beaten", "Death1" })
                Assert.IsTrue(clips.Any(c => c.name == need), $"缺动画 clip {need}");
            var smr = fbx.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(r => r.name == "body");
            Assert.IsNotNull(smr);
            Assert.IsNotNull(smr.sharedMesh);
            Assert.Greater(smr.bones.Length, 20, "骨骼数应>20（实测 36 骨 Biped）");
        }

        [Test]
        public void TrollWarrior_Textures_PbrMapTypes()
        {
            var d = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Enemies/TrollWarrior/Textures/Troll_2_D.tga");
            Assert.IsNotNull(d, "Troll_2_D 必须导入");
            Assert.AreEqual(2048, d.width, "身体 D=2048²");
            Assert.AreEqual(2048, d.height);
            var n = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Enemies/TrollWarrior/Textures/Troll_2_N.tga");
            Assert.IsNotNull(n, "Troll_2_N 必须导入");
            var nImp = AssetImporter.GetAtPath("Assets/Art/Enemies/TrollWarrior/Textures/Troll_2_N.tga") as TextureImporter;
            Assert.AreEqual(TextureImporterType.NormalMap, nImp.textureType, "法线图必须 NormalMap 类型");
            var s = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Enemies/TrollWarrior/Textures/Troll_2_S.tga");
            Assert.IsNotNull(s, "Troll_2_S（spec）必须导入");
            var sd = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Enemies/TrollWarrior/Textures/Sword_D.tga");
            Assert.IsNotNull(sd, "Sword_D 必须导入");
            Assert.AreEqual(512, sd.width, "武器 D=512×1024");
            Assert.AreEqual(1024, sd.height);
            var sn = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Enemies/TrollWarrior/Textures/Sword_N.tga");
            Assert.IsNotNull(sn, "Sword_N 必须导入");
            var snImp = AssetImporter.GetAtPath("Assets/Art/Enemies/TrollWarrior/Textures/Sword_N.tga") as TextureImporter;
            Assert.AreEqual(TextureImporterType.NormalMap, snImp.textureType, "武器法线图必须 NormalMap 类型");
        }

        [Test]
        public void TrollWarrior_NoResourcesFolder_NoVendorJunk()
        {
            Assert.IsFalse(System.IO.Directory.Exists(
                "Assets/Art/Enemies/TrollWarrior/Resources"), "Art 目录不得塞 Resources/");
            var dir = "Assets/Art/Enemies/TrollWarrior";
            Assert.IsFalse(System.IO.Directory.GetFiles(dir, "*.unity", System.IO.SearchOption.AllDirectories).Any(),
                "不得拷入 vendor 演示场景");
            var prefabs = System.IO.Directory.GetFiles(dir, "*.prefab", System.IO.SearchOption.AllDirectories);
            Assert.AreEqual(1, prefabs.Length, $"只允许 TrollWarriorTrial.prefab（实测 {string.Join("|", prefabs)}）");
        }
    }
}
