using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Runtime.Core;

namespace Game.Tests.PlayMode
{
    /// <summary>S6P-WO-05：关树释放走 HUD NotifyPanel；截图为辅助证据。</summary>
    public sealed class S6PWo05PlayModeTests
    {
        [UnityTest]
        public IEnumerator LeavingBuild_ReleasesTreeVisualOwner()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;
            var s = director.Sim.Session;
            float ds = SliceHud.DesignScale(Screen.width, Screen.height);
            float z = PassiveTreeLod.ZoomForProjectedPx(24f, ds);
            var view = SliceHud.PoeTreeViewport(1920f, 1080f);
            Vector2 pan = PassiveTreeLod.FocusPan(view, z, 2172);
            var plan = PassiveTreeRenderPlan.Build(view, pan, z, ds);
            PassiveTreeTextures.ReleaseAll();
            PassiveTreeTextures.Sync(plan);
            Assert.Greater(PassiveTreeTextures.ResidentIconCount, 0);
            var hud = new SliceHud();
            s.Panel = SlicePanel.Build;
            hud.NotifyPanel(s);
            s.Panel = SlicePanel.None;
            hud.NotifyPanel(s);
            Assert.AreEqual(0, PassiveTreeTextures.ResidentIconCount);
            Assert.AreEqual(0, PassiveTreeTextures.ResidentChromeCount);
            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TreeOpen_CaptureIfGameViewPresent()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;
            var s = director.Sim.Session;
            s.Panel = SlicePanel.Build;
            float ds = SliceHud.DesignScale(Screen.width, Screen.height);
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs/qa/wo05"));
            Directory.CreateDirectory(dir);
            float[] px = { 6f, 12f, 24f };
            string[] names = { "lod0", "lod1", "lod2" };
            int got = 0;
            for (int i = 0; i < px.Length; i++)
            {
                float z = PassiveTreeLod.ZoomForProjectedPx(px[i], ds);
                SliceHud.DebugTreeAt(z, 2172);
                yield return null;
                string path = Path.Combine(dir, names[i] + "_" + Screen.width + ".png");
                if (File.Exists(path))
                    File.Delete(path);
                ScreenCapture.CaptureScreenshot(path, 1);
                for (int f = 0; f < 12; f++)
                    yield return null;
                if (File.Exists(path) && new FileInfo(path).Length > 1024)
                    got++;
            }
            SliceHud.DebugTreeOff();
            if (got == 0)
                Assert.Ignore("无 Game View，截图跳过");
            Assert.Greater(got, 0);
            Object.Destroy(go);
        }
    }
}
