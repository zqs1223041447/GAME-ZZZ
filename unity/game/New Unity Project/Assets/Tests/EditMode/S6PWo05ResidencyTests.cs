using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>S6P-WO-05：纹理常驻、全树命中、G0–G3 几何产物。</summary>
    public sealed class S6PWo05ResidencyTests
    {
        static readonly Rect View = new Rect(0f, 0f, 1920f, 1080f);

        [Test]
        public void CentreProbes_All2429_WinnerDeterministic()
        {
            float ds = SliceHud.DesignScale(1920f, 1080f);
            float z = PassiveTreeLod.ZoomForProjectedPx(12f, ds);
            var dups = new Dictionary<string, List<int>>();
            int mismatch = 0;
            for (int i = 0; i < PoeTree.Count; i++)
            {
                PoeNode n = PoeTree.Get(i);
                string key = n.x.ToString("R") + "," + n.y.ToString("R");
                List<int> g;
                if (!dups.TryGetValue(key, out g))
                {
                    g = new List<int>();
                    dups[key] = g;
                }
                g.Add(i);
                Vector2 pan = PassiveTreeLod.FocusPan(View, z, i);
                Vector2 c = PoeTreeView.ScreenOf(new Vector2(n.x, n.y), pan, z);
                int hit = PassiveTreeLod.HitNodeId(c, pan, z, ds);
                if (g.Count == 1)
                {
                    if (hit != i)
                        mismatch++;
                }
                else
                {
                    int winner = g[0];
                    for (int k = 1; k < g.Count; k++)
                        if (g[k] < winner)
                            winner = g[k];
                    if (hit != winner)
                        mismatch++;
                }
            }
            int dupGroups = 0;
            foreach (var kv in dups)
                if (kv.Value.Count > 1)
                    dupGroups++;
            TestContext.WriteLine("duplicate-position groups=" + dupGroups + " mismatch=" + mismatch);
            Assert.AreEqual(0, mismatch);
        }

        [Test]
        public void AllocatedPath_2172_71_183_IdentityUnchangedAcrossLod()
        {
            var s = new SliceSession();
            s.ResetTown(7u);
            string err;
            Assert.IsTrue(s.TryAllocate(71, out err), err);
            Assert.IsTrue(s.TryAllocate(183, out err), err);
            var t71 = PassiveSupport.EvaluateTruth(71);
            var t183 = PassiveSupport.EvaluateTruth(183);
            float ds = 1f;
            float[] px = { 6f, 12f, 24f };
            for (int i = 0; i < px.Length; i++)
            {
                float z = PassiveTreeLod.ZoomForProjectedPx(px[i], ds);
                Vector2 pan = PassiveTreeLod.FocusPan(View, z, 2172);
                PassiveTreeRenderPlan.Build(View, pan, z, ds);
                Assert.IsTrue(s.Allocated[2172] && s.Allocated[71] && s.Allocated[183]);
                Assert.AreEqual(t71.Effect, PassiveSupport.EvaluateTruth(71).Effect);
                Assert.AreEqual(t71.Traversal, PassiveSupport.EvaluateTruth(71).Traversal);
                Assert.AreEqual(t183.Effect, PassiveSupport.EvaluateTruth(183).Effect);
                Assert.AreEqual(NodeUiState.Allocated, s.NodeState(71));
                Assert.AreEqual(NodeUiState.Allocated, s.NodeState(183));
            }
        }

        [Test]
        public void Residency_Lod0HasZeroIcons_Lod2ThenLod0Releases()
        {
            float ds = 1f;
            PassiveTreeTextures.ReleaseAll();
            float z0 = PassiveTreeLod.ZoomForProjectedPx(6f, ds);
            Vector2 pan = PassiveTreeLod.FocusPan(View, z0, 2172);
            var p0 = PassiveTreeRenderPlan.Build(View, pan, z0, ds);
            PassiveTreeTextures.Sync(p0);
            Assert.AreEqual(0, p0.RequiredIconCount);
            Assert.AreEqual(0, PassiveTreeTextures.ResidentIconCount);
            Assert.AreEqual(0, PassiveTreeTextures.ResidentChromeCount);

            float z2 = PassiveTreeLod.ZoomForProjectedPx(24f, ds);
            pan = PassiveTreeLod.FocusPan(View, z2, 2172);
            var p2 = PassiveTreeRenderPlan.Build(View, pan, z2, ds);
            PassiveTreeTextures.Sync(p2);
            Assert.Greater(p2.RequiredIconCount, 0);
            Assert.IsTrue(PassiveTreeTextures.ResidentIconsMatch(p2.RequiredIconStems));

            PassiveTreeTextures.Sync(p0);
            Assert.AreEqual(0, PassiveTreeTextures.ResidentIconCount);
            Assert.AreEqual(0, PassiveTreeTextures.ResidentChromeCount);
            PassiveTreeTextures.ReleaseAll();
        }

        [Test]
        public void Residency_NinePointSweep_NoHistoryAccumulation()
        {
            float ds = 1f;
            float z = PassiveTreeLod.ZoomForProjectedPx(24f, ds);
            Rect b = PoeTreeView.WorldBounds();
            float[] xs = { b.xMin, b.xMin + b.width * 0.5f, b.xMax };
            float[] ys = { b.yMin, b.yMin + b.height * 0.5f, b.yMax };
            PassiveTreeTextures.ReleaseAll();
            int lastRequired = -1;
            for (int iy = 0; iy < 3; iy++)
                for (int ix = 0; ix < 3; ix++)
                {
                    Vector2 pan = new Vector2(View.x + View.width * 0.5f - xs[ix] * z,
                        View.y + View.height * 0.5f - ys[iy] * z);
                    var plan = PassiveTreeRenderPlan.Build(View, pan, z, ds);
                    PassiveTreeTextures.Sync(plan);
                    Assert.IsTrue(PassiveTreeTextures.ResidentIconsMatch(plan.RequiredIconStems),
                        "station " + ix + "," + iy + " resident=" + PassiveTreeTextures.ResidentIconCount +
                        " required=" + plan.RequiredIconCount);
                    lastRequired = plan.RequiredIconCount;
                }
            Assert.GreaterOrEqual(lastRequired, 0);
            PassiveTreeTextures.ReleaseAll();
            Assert.AreEqual(0, PassiveTreeTextures.ResidentIconCount);
        }

        [Test]
        public void Stable120Syncs_NoAdditionalLoads()
        {
            float ds = 1f;
            float z = PassiveTreeLod.ZoomForProjectedPx(24f, ds);
            Vector2 pan = PassiveTreeLod.FocusPan(View, z, 2172);
            var plan = PassiveTreeRenderPlan.Build(View, pan, z, ds);
            PassiveTreeTextures.ReleaseAll();
            PassiveTreeTextures.Sync(plan);
            int loads = PassiveTreeTextures.ResourceLoadCalls;
            int unloads = PassiveTreeTextures.OwnerUnloadCalls;
            int n = PassiveTreeTextures.ResidentIconCount;
            var ids = new List<int>();
            foreach (string stem in plan.RequiredIconStems)
            {
                var t = PassiveTreeTextures.Icon(stem);
                if (t != null)
                    ids.Add(t.GetInstanceID());
            }
            for (int i = 0; i < 120; i++)
                PassiveTreeTextures.Sync(plan);
            Assert.AreEqual(loads, PassiveTreeTextures.ResourceLoadCalls, "120 次同 plan Sync 不得再 Resources.Load");
            Assert.AreEqual(unloads, PassiveTreeTextures.OwnerUnloadCalls, "120 次同 plan Sync 不得 owner unload");
            Assert.AreEqual(n, PassiveTreeTextures.ResidentIconCount);
            int k = 0;
            foreach (string stem in plan.RequiredIconStems)
            {
                var t = PassiveTreeTextures.Icon(stem);
                if (t != null)
                {
                    Assert.AreEqual(ids[k], t.GetInstanceID());
                    k++;
                }
            }
            PassiveTreeTextures.ReleaseAll();
        }

        [Test]
        public void MissingStem_LoadAttemptOncePerEpoch()
        {
            float ds = 1f;
            float z = PassiveTreeLod.ZoomForProjectedPx(6f, ds);
            Vector2 pan = PassiveTreeLod.FocusPan(View, z, 2172);
            var plan = PassiveTreeRenderPlan.Build(View, pan, z, ds);
            PassiveTreeTextures.ReleaseAll();
            plan.RequiredIconStems.Add("__wo05_missing_stem__");
            PassiveTreeTextures.Sync(plan);
            int loads = PassiveTreeTextures.ResourceLoadCalls;
            PassiveTreeTextures.Sync(plan);
            Assert.AreEqual(loads, PassiveTreeTextures.ResourceLoadCalls);
            PassiveTreeTextures.ReleaseAll();
            PassiveTreeTextures.Sync(plan);
            Assert.Greater(PassiveTreeTextures.ResourceLoadCalls, loads, "新 epoch 允许再试一次");
            PassiveTreeTextures.ReleaseAll();
        }

        [Test]
        public void LeavingBuildPanel_ReleasesTreeVisuals()
        {
            float ds = 1f;
            float z = PassiveTreeLod.ZoomForProjectedPx(24f, ds);
            Vector2 pan = PassiveTreeLod.FocusPan(View, z, 2172);
            var plan = PassiveTreeRenderPlan.Build(View, pan, z, ds);
            PassiveTreeTextures.ReleaseAll();
            PassiveTreeTextures.Sync(plan);
            Assert.Greater(PassiveTreeTextures.ResidentIconCount, 0);
            var hud = new SliceHud();
            var s = new SliceSession();
            s.ResetTown(7u);
            s.Panel = SlicePanel.Build;
            hud.NotifyPanel(s);
            s.Panel = SlicePanel.None;
            hud.NotifyPanel(s);
            Assert.AreEqual(0, PassiveTreeTextures.ResidentIconCount);
            Assert.AreEqual(0, PassiveTreeTextures.ResidentChromeCount);
            s.Panel = SlicePanel.Map;
            hud.NotifyPanel(s);
            Assert.AreEqual(0, PassiveTreeTextures.ResidentIconCount, "幂等：非 Build 再切不加载");
        }

        [Test]
        public void VramFourStates_WritesTable()
        {
            float ds = 1f;
            PassiveTreeTextures.ReleaseAll();
            int closedIcons = PassiveTreeTextures.ResidentIconCount;
            int closedChrome = PassiveTreeTextures.ResidentChromeCount;
            int closedBytes = PassiveTreeTextures.ResidentBytes;
            Assert.AreEqual(0, closedIcons);
            Assert.AreEqual(0, closedChrome);
            Assert.AreEqual(0, closedBytes);

            float z0 = PassiveTreeLod.ZoomForProjectedPx(6f, ds);
            Vector2 pan0 = PassiveTreeLod.FocusPan(View, z0, 2172);
            var p0 = PassiveTreeRenderPlan.Build(View, pan0, z0, ds);
            PassiveTreeTextures.Sync(p0);
            Assert.AreEqual(0, p0.RequiredIconCount);
            Assert.AreEqual(0, PassiveTreeTextures.ResidentIconCount);
            Assert.AreEqual(0, PassiveTreeTextures.ResidentChromeCount);
            int lod0Bytes = PassiveTreeTextures.ResidentBytes;
            Assert.AreEqual(0, lod0Bytes);

            float z1 = PassiveTreeLod.ZoomForProjectedPx(12f, ds);
            Vector2 pan1 = PassiveTreeLod.FocusPan(View, z1, 2172);
            var p1 = PassiveTreeRenderPlan.Build(View, pan1, z1, ds);
            PassiveTreeTextures.Sync(p1);
            foreach (string stem in p1.RequiredIconStems)
            {
                bool fromAllowed = false;
                for (int i = 0; i < PoeTree.Count; i++)
                {
                    if (!p1.VisibleNodes[i] || string.IsNullOrEmpty(PoeTree.Get(i).icon))
                        continue;
                    if (PoeTree.Get(i).icon != stem)
                        continue;
                    PoeNodeKind k = PoeTree.Get(i).Kind;
                    if (k == PoeNodeKind.Notable || k == PoeNodeKind.Keystone || k == PoeNodeKind.Mastery || k == PoeNodeKind.Start)
                        fromAllowed = true;
                }
                Assert.IsTrue(fromAllowed, "LOD1 stem 必须来自 Notable/Keystone/Mastery/Start：" + stem);
            }
            Assert.IsTrue(PassiveTreeTextures.ResidentIconsMatch(p1.RequiredIconStems));
            int lod1Icons = PassiveTreeTextures.ResidentIconCount;
            int lod1Chrome = PassiveTreeTextures.ResidentChromeCount;
            int lod1Bytes = PassiveTreeTextures.ResidentBytes;

            float z2 = PassiveTreeLod.ZoomForProjectedPx(24f, ds);
            Vector2 pan2 = PassiveTreeLod.FocusPan(View, z2, 2172);
            var p2 = PassiveTreeRenderPlan.Build(View, pan2, z2, ds);
            PassiveTreeTextures.Sync(p2);
            Assert.IsTrue(PassiveTreeTextures.ResidentIconsMatch(p2.RequiredIconStems));
            Assert.Greater(p2.RequiredIconCount, 0);
            int lod2Icons = PassiveTreeTextures.ResidentIconCount;
            int lod2Chrome = PassiveTreeTextures.ResidentChromeCount;
            int lod2Bytes = PassiveTreeTextures.ResidentBytes;
            Assert.Greater(lod2Bytes, lod0Bytes);

            PassiveTreeTextures.ReleaseAll();
            Assert.AreEqual(0, PassiveTreeTextures.ResidentBytes);

            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"CLOSED\": {\"icons\":").Append(closedIcons).Append(",\"chrome\":").Append(closedChrome).Append(",\"bytes\":").Append(closedBytes).Append("},\n");
            sb.Append("  \"LOD0\": {\"icons\":0,\"chrome\":0,\"bytes\":").Append(lod0Bytes).Append("},\n");
            sb.Append("  \"LOD1\": {\"icons\":").Append(lod1Icons).Append(",\"chrome\":").Append(lod1Chrome).Append(",\"bytes\":").Append(lod1Bytes).Append("},\n");
            sb.Append("  \"LOD2\": {\"icons\":").Append(lod2Icons).Append(",\"chrome\":").Append(lod2Chrome).Append(",\"bytes\":").Append(lod2Bytes).Append("}\n");
            sb.Append("}\n");
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs/qa/wo05/VRAM_FOUR_STATE.json"));
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            TestContext.WriteLine(sb.ToString());
        }

        [Test]
        public void WritesUnityGeomArtifacts_G0G3()
        {
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "docs/qa/wo05"));
            Directory.CreateDirectory(dir);
            WriteGeom(dir, "G0_1920", 1920f, 1080f, true, 0f);
            WriteGeom(dir, "G1_1920", 1920f, 1080f, false, 6f);
            WriteGeom(dir, "G2_1920", 1920f, 1080f, false, 12f);
            WriteGeom(dir, "G3_1920", 1920f, 1080f, false, 24f);
            WriteGeom(dir, "G0_2560", 2560f, 1440f, true, 0f);
            WriteGeom(dir, "G1_2560", 2560f, 1440f, false, 6f);
            WriteGeom(dir, "G2_2560", 2560f, 1440f, false, 12f);
            WriteGeom(dir, "G3_2560", 2560f, 1440f, false, 24f);
            Assert.IsTrue(File.Exists(Path.Combine(dir, "G0_1920.unity.json")));
            string g0 = File.ReadAllText(Path.Combine(dir, "G0_1920.unity.json"));
            StringAssert.Contains("\"lod\": 0", g0);
        }

        static void WriteGeom(string dir, string name, float screenW, float screenH, bool fit, float projected)
        {
            float ds = SliceHud.DesignScale(screenW, screenH);
            var view = new Rect(0f, 0f, 1920f, 1080f);
            float z;
            Vector2 pan;
            if (fit)
            {
                z = PoeTreeView.FitZoom(view, 40f);
                pan = PoeTreeView.FitPan(view, z);
            }
            else
            {
                z = PassiveTreeLod.ZoomForProjectedPx(projected, ds);
                pan = PassiveTreeLod.FocusPan(view, z, 2172);
            }
            var plan = PassiveTreeRenderPlan.Build(view, pan, z, ds);
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"name\": \"").Append(name).Append("\",\n");
            sb.Append("  \"screen\": [").Append(screenW.ToString("0")).Append(", ").Append(screenH.ToString("0")).Append("],\n");
            sb.Append("  \"designScale\": ").Append(ds.ToString("R")).Append(",\n");
            sb.Append("  \"zoom\": ").Append(z.ToString("R")).Append(",\n");
            sb.Append("  \"pan\": [").Append(pan.x.ToString("R")).Append(", ").Append(pan.y.ToString("R")).Append("],\n");
            sb.Append("  \"lod\": ").Append((int)plan.Lod).Append(",\n");
            sb.Append("  \"normalProjectedPx\": ").Append(PassiveTreeLod.NormalProjectedPx(z, ds).ToString("R")).Append(",\n");
            sb.Append("  \"visibleNodeCount\": ").Append(plan.VisibleNodeCount).Append(",\n");
            sb.Append("  \"visibleEdgeCount\": ").Append(plan.VisibleEdgeCount).Append(",\n");
            sb.Append("  \"requiredIconCount\": ").Append(plan.RequiredIconCount).Append(",\n");
            sb.Append("  \"nodes\": [\n");
            bool first = true;
            for (int i = 0; i < PoeTree.Count; i++)
            {
                if (!plan.VisibleNodes[i])
                    continue;
                PoeNode n = PoeTree.Get(i);
                Vector2 c = PoeTreeView.ScreenOf(new Vector2(n.x, n.y), pan, z);
                float px = c.x * ds, py = c.y * ds;
                if (!first) sb.Append(",\n");
                first = false;
                sb.Append("    {\"id\":").Append(i)
                  .Append(",\"skill\":").Append(n.skill)
                  .Append(",\"kind\":").Append(n.kind)
                  .Append(",\"x\":").Append(n.x.ToString("R"))
                  .Append(",\"y\":").Append(n.y.ToString("R"))
                  .Append(",\"sx\":").Append(px.ToString("R"))
                  .Append(",\"sy\":").Append(py.ToString("R")).Append("}");
            }
            sb.Append("\n  ],\n  \"edges\": [\n");
            first = true;
            for (int i = 0; i < PoeTree.Count; i++)
            {
                int[] links = PoeTree.Get(i).links;
                if (links == null)
                    continue;
                Vector2 a = PoeTreeView.ScreenOf(new Vector2(PoeTree.Get(i).x, PoeTree.Get(i).y), pan, z);
                for (int k = 0; k < links.Length; k++)
                {
                    int j = links[k];
                    if (j <= i)
                        continue;
                    Vector2 b = PoeTreeView.ScreenOf(new Vector2(PoeTree.Get(j).x, PoeTree.Get(j).y), pan, z);
                    if (!PassiveTreeRenderPlan.IsEdgeVisible(a, b, view, PassiveTreeRenderPlan.EdgeVisiblePad))
                        continue;
                    if (!first) sb.Append(",\n");
                    first = false;
                    sb.Append("    {\"a\":").Append(i).Append(",\"b\":").Append(j)
                      .Append(",\"ax\":").Append((a.x * ds).ToString("R"))
                      .Append(",\"ay\":").Append((a.y * ds).ToString("R"))
                      .Append(",\"bx\":").Append((b.x * ds).ToString("R"))
                      .Append(",\"by\":").Append((b.y * ds).ToString("R")).Append("}");
                }
            }
            sb.Append("\n  ]\n}\n");
            File.WriteAllText(Path.Combine(dir, name + ".unity.json"), sb.ToString(), new UTF8Encoding(false));
        }
    }
}
