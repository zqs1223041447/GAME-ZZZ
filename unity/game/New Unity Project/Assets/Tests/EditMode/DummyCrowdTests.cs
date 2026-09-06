using System;
using NUnit.Framework;
using UnityEngine;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    public sealed class DummyCrowdTests
    {
        [Test]
        public void Spawn_100_200_300_Counts()
        {
            var crowd = new DummyCrowd();
            var rng = new SeededRng(CombatRules.ArenaSeed);
            crowd.SpawnRing(100, rng, 0f, 0f);
            Assert.AreEqual(100, crowd.OccupiedCount);
            Assert.AreEqual(100, crowd.AliveCount);

            rng = new SeededRng(CombatRules.ArenaSeed);
            crowd.SpawnRing(200, rng, 0f, 0f);
            Assert.AreEqual(200, crowd.AliveCount);

            rng = new SeededRng(CombatRules.ArenaSeed);
            crowd.SpawnRing(300, rng, 0f, 0f);
            Assert.AreEqual(300, crowd.AliveCount);
            Assert.AreEqual(300, crowd.OccupiedCount);
        }

        [Test]
        public void SpawnLayout_SameSeedReproducible()
        {
            var a = new DummyCrowd();
            var b = new DummyCrowd();
            a.SpawnRing(100, new SeededRng(CombatRules.ArenaSeed), 0f, 0f);
            b.SpawnRing(100, new SeededRng(CombatRules.ArenaSeed), 0f, 0f);
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(a.Items[i].X, b.Items[i].X, 0.0001f, "x " + i);
                Assert.AreEqual(a.Items[i].Z, b.Items[i].Z, 0.0001f, "z " + i);
            }
        }

        [Test]
        public void Lod_FarStopsTurning()
        {
            var crowd = new DummyCrowd();
            int id = crowd.SpawnAt(0f, 0f);
            for (int i = 0; i < 30; i++)
                crowd.TickAi(0.02f, 80f, 0f);
            Assert.AreEqual(2, crowd.Items[id].LodBand);
            Assert.AreEqual(0f, crowd.Items[id].YawDeg, 0.01f);
        }

        [Test]
        public void Lod_NearTurnsTowardPlayer()
        {
            var crowd = new DummyCrowd();
            int id = crowd.SpawnAt(0f, 0f);
            for (int i = 0; i < 20; i++)
                crowd.TickAi(0.02f, 4f, 0f);
            Assert.AreEqual(0, crowd.Items[id].LodBand);
            Assert.Greater(crowd.Items[id].YawDeg, 10f);
        }

        [Test]
        public void Tick300_NoBurstAllocAfterWarmup()
        {
            var sim = new ArenaSim();
            sim.Reset();
            sim.SpawnDummies(300, CombatRules.ArenaSeed);
            for (int i = 0; i < 10; i++)
                sim.Tick(1f / 60f, PlayerCommand.None());

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 60; i++)
                sim.Tick(1f / 60f, PlayerCommand.None());
            long after = GC.GetAllocatedBytesForCurrentThread();
            long delta = after - before;
            Assert.Less(delta, 2048, "hot tick allocated " + delta + " bytes");
        }

        [Test]
        public void EveFit_ScalesTinyMeshToCapsuleHeight()
        {
            var root = new GameObject("EveFitRoot");
            var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesh.transform.SetParent(root.transform, false);
            mesh.transform.localScale = new Vector3(0.01f, 0.017f, 0.01f);
            EveView.FitToCapsule(root, EveView.TargetHeight);
            Bounds b = mesh.GetComponent<Renderer>().bounds;
            Assert.AreEqual(EveView.TargetHeight, b.size.y, 0.05f);
            Assert.AreEqual(0f, b.min.y, 0.05f);
            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void CollisionLayers_IgnorePlayerAndMonsterPush()
        {
            CollisionLayers.Apply();
            Assert.IsTrue(Physics.GetIgnoreLayerCollision(CollisionLayers.Player, CollisionLayers.Monster));
            Assert.IsTrue(Physics.GetIgnoreLayerCollision(CollisionLayers.Monster, CollisionLayers.Monster));
        }

        [Test]
        public void Separate_PushesStackedDummiesApart()
        {
            var crowd = new DummyCrowd();
            int a = crowd.SpawnAt(0f, 0f);
            int b = crowd.SpawnAt(0.05f, 0f);
            crowd.Separate(CombatRules.DummyMinSeparation);
            float dist = CombatMathUtil.Dist(
                crowd.Items[a].X, crowd.Items[a].Z,
                crowd.Items[b].X, crowd.Items[b].Z);
            Assert.GreaterOrEqual(dist, CombatRules.DummyMinSeparation - 0.02f);
        }

        [Test]
        public void AiTowardPlayer_DoesNotStayStacked()
        {
            var sim = new ArenaSim();
            sim.Reset();
            int a = sim.Dummies.SpawnAt(4f, 0f);
            int b = sim.Dummies.SpawnAt(4.04f, 0.02f);
            for (int i = 0; i < 200; i++)
                sim.Tick(1f / 60f, PlayerCommand.None());

            float dist = CombatMathUtil.Dist(
                sim.Dummies.Items[a].X, sim.Dummies.Items[a].Z,
                sim.Dummies.Items[b].X, sim.Dummies.Items[b].Z);
            Assert.GreaterOrEqual(dist, CombatRules.DummyMinSeparation - 0.05f);
            Assert.IsTrue(sim.Dummies.Items[a].Alive);
            Assert.IsTrue(sim.Dummies.Items[b].Alive);
        }

        [Test]
        public void SimOnly_CanMeasure300()
        {
            var sim = new ArenaSim();
            sim.Reset();
            sim.SpawnDummies(300, CombatRules.ArenaSeed);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 120; i++)
                sim.Tick(1f / 60f, PlayerCommand.None());
            sw.Stop();
            Assert.AreEqual(300, sim.AliveDummyCount);
            Assert.Greater(sw.Elapsed.TotalMilliseconds, 0d);
            GameLog.Info("Perf", "sim-only 300x120 ticks ms=" + sw.Elapsed.TotalMilliseconds.ToString("F2"));
        }
    }
}
