using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Build Settings 场景契约（S3-M5）：锁定「Bootstrap=index 0 / Arena=index 1」。
    /// 隐藏依赖：ArenaPerfHarness 从 Bootstrap 自动进入 Arena 使用 SceneManager.LoadScene(1)——
    /// 场景顺序漂移会造成「Edit/Play 全绿、Player 也能 build，但性能 Harness 启动错场景」。
    /// 用 EditorBuildSettings API 校验（不解析 YAML）。本轮只锁现状，不改 Harness 的 index 契约；
    /// 未来正式改变场景顺序时必须同步处理 ArenaPerfHarness（另行工作令）。
    /// </summary>
    public sealed class BuildSettingsContractTests
    {
        const string BootstrapScene = "Assets/Scenes/Bootstrap.unity";
        const string ArenaScene = "Assets/Scenes/Arena.unity";

        [Test]
        public void BuildScenes_Index0IsBootstrap_Index1IsArena_Enabled_AndAssetsExist()
        {
            var scenes = EditorBuildSettings.scenes;
            Assert.GreaterOrEqual(scenes.Length, 2, "Build Settings 必须至少含 Bootstrap 与 Arena 两个场景");
            Assert.IsTrue(scenes[0].enabled, "Index 0 必须启用");
            Assert.AreEqual(BootstrapScene, scenes[0].path,
                "Index 0 必须=Bootstrap（ArenaPerfHarness 的 LoadScene(1) 依赖 Arena=index 1）");
            Assert.IsTrue(scenes[1].enabled, "Index 1 必须启用");
            Assert.AreEqual(ArenaScene, scenes[1].path,
                "Index 1 必须=Arena（ArenaPerfHarness SceneManager.LoadScene(1) 契约）");
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScene),
                "场景资产缺失: " + BootstrapScene);
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>(ArenaScene),
                "场景资产缺失: " + ArenaScene);
        }

        [Test]
        public void BuildScenes_EnabledNoDuplicates_NoEnabledMissingAsset_BootstrapArenaEachOnce()
        {
            var scenes = EditorBuildSettings.scenes;
            var seen = new HashSet<string>();
            int bootstrap = 0, arena = 0;
            foreach (var s in scenes)
            {
                if (!s.enabled)
                    continue;
                Assert.IsTrue(seen.Add(s.path), "启用场景路径重复: " + s.path);
                if (s.path == BootstrapScene)
                    bootstrap++;
                if (s.path == ArenaScene)
                    arena++;
                Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>(s.path),
                    "启用场景但资产缺失: " + s.path);
            }
            Assert.AreEqual(1, bootstrap, "Bootstrap 在启用列表内只能出现一次");
            Assert.AreEqual(1, arena, "Arena 在启用列表内只能出现一次");
        }
    }
}
