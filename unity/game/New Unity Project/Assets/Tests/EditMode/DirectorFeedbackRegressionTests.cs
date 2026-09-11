using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// 导演 2026-09-11 反馈回归门禁（三条）：
    ///   ① 背包改快捷键开关式 → 关闭时底栏必须能用整幅宽度，且面板不再吞世界点击（几何可证）；
    ///   ② 天赋树「看不出是线连起来的」→ 连接线必须落在节点真实坐标上；簇几何必须由成员节点推导
    ///      （canonical 的 group.x/y 与节点坐标不同系，旧实现据此按 orbit 公式画弧，1457/2787 条连线悬空）；
    ///   ③ 怪物「移动/攻击没有动画」→ 敌人循环片段（Idle/Run/Walk 一类）必须开着 Loop Time。
    /// </summary>
    public sealed class DirectorFeedbackRegressionTests
    {
        static readonly Vector2[] Spaces =
        {
            new Vector2(1920f, 1080f),
            new Vector2(2560f, 1440f),
            new Vector2(1280f, 720f)
        };

        // ---------- ① 背包快捷键开关 ----------

        [Test]
        public void BagToggle_Closed_GivesCombatBarFullWidth()
        {
            foreach (Vector2 space in Spaces)
            {
                var open = SliceHud.CombatBarRects(space.x, space.y, true);
                var closed = SliceHud.CombatBarRects(space.x, space.y, false);
                Assert.LessOrEqual(open.Bar.xMax, space.x - SliceDrawerLayout.PanelW + 0.01f,
                    "背包开启时底栏不得伸进面板（既有契约保持）");
                Assert.GreaterOrEqual(closed.Bar.width, open.Bar.width,
                    "背包关闭后底栏不得变窄");
                // 关掉面板后可用宽度变大：窄设计空间下必须真的变宽，宽空间下至少把整条栏右移到新中心
                bool narrow = space.x - SliceDrawerLayout.PanelW < 1028f;
                if (narrow)
                    Assert.Greater(closed.Bar.width, open.Bar.width,
                        "窄设计空间（" + space.x + "）关掉背包后底栏必须变宽");
                Assert.Greater(closed.Bar.center.x, open.Bar.center.x,
                    "关掉背包后底栏必须在更宽的可用区里重新居中（右移）");
                Assert.GreaterOrEqual(closed.Frame.x, -0.01f, "关闭态外框不得出左界");
                Assert.LessOrEqual(closed.Frame.xMax, space.x + 0.01f, "关闭态外框不得出右界");
            }
        }

        [Test]
        public void BagToggle_DefaultIsOpen()
        {
            var s = new SliceSession();
            Assert.IsTrue(s.BagOpen, "默认开启：保持既有布局与既有玩家体验（新开关只增加关闭能力）");
            s.BagOpen = false;
            s.BagOpen = true;
            Assert.IsTrue(s.BagOpen, "开关必须可来回切换（快捷键 I 的语义载体）");
        }

        [Test]
        public void BagToggle_ClosedPanel_ReleasesWorldClicks()
        {
            const float dw = 1920f, dh = 1080f;
            Rect shell = SliceDrawerLayout.Shell(dw, dh);
            Vector2 insidePanel = new Vector2(shell.x + 40f, 60f);
            var topBar = new Rect(12, 10, 620, 84);
            var nav = new Rect(660, 10, 354, 40);
            var skillHud = SliceHud.CombatBarRects(dw, dh, true).Frame;

            Assert.IsTrue(SliceHud.BlocksWorldInput(false, false, topBar, nav, skillHud, default, shell, insidePanel),
                "背包开启时面板内点必须吞掉世界点击");
            Assert.IsFalse(SliceHud.BlocksWorldInput(false, false, topBar, nav, skillHud, default, default, insidePanel),
                "背包关闭后（壳矩形归零）面板区域必须放行世界点击");
        }

        // ---------- ② 天赋树连线与簇几何 ----------

        [Test]
        public void TreeLinks_AreDrawnFromRealNodeCoordinates()
        {
            // 源码级钉死：连线渲染必须用节点自身坐标，且不得再有按 orbit 公式重算端点的弧线路径
            string hud = ReadSource("Assets/Runtime/Core/Gameplay/SliceHud.cs");
            StringAssert.Contains("PoeTreeView.ScreenOf(new Vector2(n.x, n.y), _treePan, _treeZoom)", hud,
                "连线必须由节点真实坐标投影");
            Assert.IsFalse(hud.Contains("ArcPoint"), "旧弧线端点重算路径必须已移除（它画出的线不碰节点）");
            Assert.IsFalse(hud.Contains("DrawArc("), "旧弧线绘制路径必须已移除");
        }

        [Test]
        public void TreeLinks_EveryLinkHasTwoDistinctFiniteEndpoints()
        {
            PoeNode[] nodes = PoeTree.Nodes;
            Assert.Greater(nodes.Length, 0, "天赋树数据必须可加载");
            int links = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                int[] ls = nodes[i].links;
                if (ls == null)
                    continue;
                for (int k = 0; k < ls.Length; k++)
                {
                    int j = ls[k];
                    Assert.GreaterOrEqual(j, 0);
                    Assert.Less(j, nodes.Length);
                    var a = new Vector2(nodes[i].x, nodes[i].y);
                    var b = new Vector2(nodes[j].x, nodes[j].y);
                    Assert.IsTrue(Finite(a) && Finite(b), "节点坐标必须有限");
                    Assert.Greater((b - a).magnitude, 0.01f, "连线两端不得重合");
                    links++;
                }
            }
            Assert.Greater(links, 2000, "真实主树应有数千条有向连线");
        }

        [Test]
        public void TreeGroupGeometry_IsDerivedFromMemberNodes()
        {
            PoeNode[] nodes = PoeTree.Nodes;
            PoeGroup[] groups = PoeTree.Groups;
            int checkedGroups = 0;
            for (int g = 0; g < groups.Length; g++)
            {
                int members = 0;
                float maxDist = 0f;
                Vector2 centre = PoeTreeView.GroupCentre(g);
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i].group != g)
                        continue;
                    members++;
                    float d = Vector2.Distance(new Vector2(nodes[i].x, nodes[i].y), centre);
                    if (d > maxDist) maxDist = d;
                }
                if (members == 0)
                    continue;
                checkedGroups++;
                Assert.AreEqual(maxDist, PoeTreeView.GroupRadius(g), 0.01f,
                    "簇半径必须 = 成员节点最大环半径（簇 " + g + "）");
                Rect r = PoeTreeView.GroupRect(g, 100f, 100f, Vector2.zero, 1f);
                Assert.GreaterOrEqual(r.width, maxDist * 2f, "底衬宽度必须覆盖整个簇：" + g);
            }
            Assert.Greater(checkedGroups, 500, "真实天赋树应有数百个含节点的簇");
        }

        [Test]
        public void TreeDefaultZoom_IsReadable()
        {
            Assert.GreaterOrEqual(SliceHud.DefaultTreeZoom, 0.30f,
                "默认缩放必须能看清节点与连线（整树 FitZoom 在 1080p 下约 0.044，节点只剩约 2px）");
            Assert.LessOrEqual(SliceHud.DefaultTreeZoom, PoeTreeView.MaxZoom);
        }

        // ---------- ③ 敌人动画 ----------

        [Test]
        public void EnemyCyclicClips_AreLooping_SoMovementAnimates()
        {
            string[] prefabs =
            {
                RuntimeResourcePaths.EnemyTrollVisual,
                RuntimeResourcePaths.EnemyFireLionVisual,
                RuntimeResourcePaths.EnemyBruceVisual,
                RuntimeResourcePaths.EnemyGargoyleVisual
            };
            string[] cyclicNames = { "Idle", "Idle2", "Run", "Walk", "Walk2", "Standby", "combat_mode" };
            int checkedClips = 0;
            for (int p = 0; p < prefabs.Length; p++)
            {
                GameObject prefab = Resources.Load<GameObject>(prefabs[p]);
                Assert.IsNotNull(prefab, "敌人视觉预制体缺失：" + prefabs[p]);
                Animator[] animators = prefab.GetComponentsInChildren<Animator>(true);
                Assert.Greater(animators.Length, 0, "敌人视觉必须带 Animator：" + prefabs[p]);
                for (int a = 0; a < animators.Length; a++)
                {
                    RuntimeAnimatorController ctrl = animators[a].runtimeAnimatorController;
                    if (ctrl == null || ctrl.animationClips == null)
                        continue;
                    for (int c = 0; c < ctrl.animationClips.Length; c++)
                    {
                        AnimationClip clip = ctrl.animationClips[c];
                        if (clip == null)
                            continue;
                        bool cyclic = false;
                        for (int n = 0; n < cyclicNames.Length; n++)
                            if (clip.name == cyclicNames[n])
                                cyclic = true;
                        if (!cyclic)
                            continue;
                        checkedClips++;
                        Assert.IsTrue(clip.isLooping,
                            "循环片段必须开 Loop Time，否则播完停在末帧 —— 表现即「怪物没有移动动画」："
                            + prefabs[p] + " / " + clip.name);
                    }
                }
            }
            Assert.GreaterOrEqual(checkedClips, 4, "至少应覆盖各敌人的 Idle/Run 循环片段");
        }

        static bool Finite(Vector2 v)
        {
            return !float.IsNaN(v.x) && !float.IsInfinity(v.x) && !float.IsNaN(v.y) && !float.IsInfinity(v.y);
        }

        static string ReadSource(string relative)
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));
            Assert.IsTrue(File.Exists(path), "源码缺失：" + path);
            return File.ReadAllText(path);
        }
    }
}
