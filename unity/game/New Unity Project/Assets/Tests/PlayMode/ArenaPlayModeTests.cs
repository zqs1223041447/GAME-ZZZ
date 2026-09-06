using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using Game.Runtime.Core;

namespace Game.Tests.PlayMode
{
    public sealed class ArenaPlayModeTests
    {
        [UnityTest]
        public IEnumerator ArenaDirector_BootsAndThreeSkillsFire()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;

            Assert.IsNotNull(director.Sim);
            SkillDef melee = SkillCatalog.Get(SkillId.Melee);
            SkillDef proj = SkillCatalog.Get(SkillId.Projectile);
            SkillDef area = SkillCatalog.Get(SkillId.Area);
            director.Sim.Tick(0.02f, PlayerCommand.CastAt(SkillId.Melee, -1, 0f, 2f));
            Step(director.Sim, melee.Windup + melee.Active + melee.Recovery);
            Assert.GreaterOrEqual(director.Sim.CastEvents, 1);

            director.Sim.Tick(0.02f, PlayerCommand.CastAt(SkillId.Projectile, -1, 0f, 6f));
            Step(director.Sim, proj.Windup + proj.Active + proj.Recovery);
            director.Sim.Tick(0.02f, PlayerCommand.CastAt(SkillId.Area, -1, 0f, 4f));
            Step(director.Sim, area.Windup);
            Assert.GreaterOrEqual(director.Sim.CastEvents, 3);

            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ProjectilesAndDummies_HaveNoRigidbodyOrNavMeshAgent()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;

            director.Sim.SpawnDummies(20, CombatRules.ArenaSeed);
            director.Sim.Tick(0.02f, PlayerCommand.CastAt(SkillId.Projectile, -1, 0f, 8f));
            Step(director.Sim, SkillCatalog.Get(SkillId.Projectile).Windup);
            yield return null;

            Rigidbody[] bodies = Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
            Assert.AreEqual(0, bodies.Length, "S1 禁止弹道/Dummy Rigidbody");

            NavMeshAgent[] agents = Object.FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None);
            Assert.AreEqual(0, agents.Length, "S1 禁止 Dummy NavMeshAgent");

            Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PerformanceArena_WritesCostRows()
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            director.EnablePlayerInput = false;
            yield return null;

            yield return director.SampleDensity(100, 8, 20);
            yield return director.SampleDensity(200, 8, 20);
            yield return director.SampleDensity(300, 8, 20);

            Assert.IsFalse(string.IsNullOrEmpty(director.LastPerfPath));
            Assert.IsTrue(File.Exists(director.LastPerfPath), director.LastPerfPath);
            string text = File.ReadAllText(director.LastPerfPath);
            StringAssert.Contains("100,", text);
            StringAssert.Contains("200,", text);
            StringAssert.Contains("300,", text);
            StringAssert.Contains("top_bottlenecks", text);

            Object.Destroy(go);
            yield return null;
        }

        static void Step(ArenaSim sim, float seconds)
        {
            const float dt = 0.02f;
            int n = Mathf.RoundToInt(seconds / dt);
            for (int i = 0; i < n; i++)
                sim.Tick(dt, PlayerCommand.None());
        }
    }
}
