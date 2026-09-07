using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Game.Runtime.Core
{
    /// <summary>
    /// S2P 测量工具（默认关闭）。启动方式二选一：
    /// 1) 独立包命令行 -arenaPerf [-arenaPerfOut &lt;dir&gt;]（ArenaDirector.BuildIfNeeded 自动拉起，跑完自动退出）；
    /// 2) 编辑器内显式调用 StartManual(director, outDir)。
    /// 密度策略：击杀后立即补到额定（SpawnAt 单只补怪，维持名义存活数）。
    /// 施法：Q/W/E 轮换，间隔 0.12s（约 8.3 次/秒），目标最近存活实体。
    /// </summary>
    public static class ArenaPerfHarness
    {
        static readonly int[] Densities = { 100, 200, 300 };
        const int WarmupFrames = 60;
        const int SampleFrames = 600;
        const float CastInterval = 0.12f;

        static bool _running;
        static string _outDir;

        public static bool RequestedFromArgs { get; private set; }
        public static string OutDir { get; private set; }
        public static bool Finished { get; private set; }

        public static void InitFromArgs()
        {
            if (RequestedFromArgs)
                return;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-arenaPerf")
                    RequestedFromArgs = true;
                else if (args[i] == "-arenaPerfOut" && i + 1 < args.Length)
                {
                    OutDir = args[i + 1];
                    i++;
                }
            }
        }

        // 独立包从 Bootstrap 场景启动：AfterSceneLoad 挂常驻 Runner，负责切到 Arena 场景并拉起测量
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void BootFromArgs()
        {
            InitFromArgs();
            if (!RequestedFromArgs)
                return;
            var go = new GameObject("ArenaPerfHarnessRunner");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<Runner>();
        }

        static void Pump()
        {
            if (!RequestedFromArgs || _running)
                return;
            var d = UnityEngine.Object.FindFirstObjectByType<ArenaDirector>();
            if (d != null)
                TryStart(d);
        }

        sealed class Runner : MonoBehaviour
        {
            bool _sceneLoaded;
            void Update()
            {
                if (!_sceneLoaded && RequestedFromArgs && !_running)
                {
                    var d = UnityEngine.Object.FindFirstObjectByType<ArenaDirector>();
                    if (d == null)
                    {
                        // 首场景是 Bootstrap：切到构建列表中的 Arena（索引 1）
                        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
                        _sceneLoaded = true;
                        return;
                    }
                }
                Pump();
            }
        }

        public static void TryStart(ArenaDirector director)
        {
            if (!RequestedFromArgs || _running || director == null)
                return;
            StartManual(director, OutDir);
        }

        public static void StartManual(ArenaDirector director, string outDir)
        {
            if (_running || director == null)
                return;
            try
            {
                _outDir = string.IsNullOrEmpty(outDir)
                    ? Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs"))
                    : Path.GetFullPath(outDir);
                Directory.CreateDirectory(_outDir);
            }
            catch (Exception e)
            {
                // 输出目录不可用（参数被拆分等）时退回 Logs，保证游戏可玩
                GameLog.Error("Perf", "harness outDir failed: " + e.Message + " -> fallback Logs");
                _outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs"));
                Directory.CreateDirectory(_outDir);
            }
            _running = true;
            Finished = false;
            director.StartCoroutine(Run(director));
        }

        static IEnumerator Run(ArenaDirector director)
        {
            var sim = director.Sim;
            for (int d = 0; d < Densities.Length; d++)
            {
                int count = Densities[d];
                sim.SpawnDummies(count, CombatRules.ArenaSeed);
                float castAt = 0f;
                int pick = 0;

                var sampler = new PerfSampler();
                for (int i = 0; i < WarmupFrames; i++)
                {
                    TickHarness(sim, count, ref castAt, ref pick);
                    yield return null;
                }

                sampler.Begin();
                for (int i = 0; i < SampleFrames; i++)
                {
                    TickHarness(sim, count, ref castAt, ref pick);
                    sampler.Sample(Time.unscaledDeltaTime);
                    yield return null;
                }

                PerfRow row = sampler.End(count, sim.AliveDummyCount);
                WriteRow(count, row);
                GameLog.Info("Perf", "harness " + count + " alive=" + sim.AliveDummyCount +
                    " avg=" + row.MainMsAvg.ToString("F3", CultureInfo.InvariantCulture));
            }

            Finished = true;
            if (!Application.isEditor)
                Application.Quit();
        }

        static int _topUpTick;

        // 施法 + 密度补齐
        static void TickHarness(ArenaSim sim, int target, ref float castAt, ref int pick)
        {
            if (sim.AliveDummyCount < target)
            {
                // 补到额定：黄金角确定性散布（项目规则禁 UnityEngine.Random），外圈半径节奏与 SpawnRing 一致
                _topUpTick++;
                float ang = (_topUpTick % 1000) * 2.3999632f;
                float radius = 5f + (float)((_topUpTick * 7919) % 100) * 0.022f +
                    ((sim.AliveDummyCount / 26) % 40) * 2.2f;
                sim.Dummies.SpawnAt(sim.Player.X + Mathf.Cos(ang) * radius,
                    sim.Player.Z + Mathf.Sin(ang) * radius);
            }

            float now = Time.unscaledTime;
            if (now < castAt)
                return;
            castAt = now + CastInterval;

            var items = sim.Dummies.Items;
            int best = -1;
            float bd = float.MaxValue;
            for (int i = 0; i < items.Length; i++)
            {
                if (!items[i].Occupied || !items[i].Alive)
                    continue;
                float dx = items[i].X - sim.Player.X;
                float dz = items[i].Z - sim.Player.Z;
                float q = dx * dx + dz * dz;
                if (q < bd)
                {
                    bd = q;
                    best = i;
                }
            }

            PlayerCommand cmd = PlayerCommand.None();
            cmd.Kind = CommandKind.Cast;
            cmd.Skill = pick % 3 == 0 ? SkillId.Melee
                : (pick % 3 == 1 ? SkillId.Projectile : SkillId.Area);
            pick++;
            if (best >= 0)
            {
                cmd.TargetDummy = best;
                cmd.AimX = items[best].X;
                cmd.AimZ = items[best].Z;
            }
            else
            {
                cmd.TargetDummy = -1;
                cmd.AimX = sim.Player.X + 3f;
                cmd.AimZ = sim.Player.Z;
            }

            PlayerMotorState p = sim.Player;
            sim.Caster.TryIssue(cmd, ref p, sim.Dummies);
            sim.Player = p;
        }

        static void WriteRow(int count, PerfRow row)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# S2P 1440p harness, density=" + count);
            sb.AppendLine("# resolution=" + Screen.width + "x" + Screen.height +
                " fullscreen=" + Screen.fullScreen +
                " currentRes=" + Screen.currentResolution.width + "x" + Screen.currentResolution.height +
                " editor=" + Application.isEditor +
                " dx=" + SystemInfo.graphicsDeviceType +
                " enteredMap=" + (Application.isEditor ? "n/a" : "false"));
            sb.AppendLine("# density strategy: kill-then-refill (alive kept at nominal via SpawnAt top-up)");
            sb.AppendLine("dummy_count,alive,frames,main_ms_avg,main_ms_p95,main_ms_p99,main_ms_p999,main_ms_max,gc_alloc_bytes_avg,cpu_ms_avg,gpu_ms_avg,mem_total_mb,frame_timing_ok");
            sb.Append(row.DummyCount.ToString(CultureInfo.InvariantCulture)).Append(',');
            sb.Append(row.AliveCount.ToString(CultureInfo.InvariantCulture)).Append(',');
            sb.Append(row.Frames.ToString(CultureInfo.InvariantCulture)).Append(',');
            sb.Append(row.MainMsAvg.ToString("F3", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(row.MainMsP95.ToString("F3", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(row.MainMsP99.ToString("F3", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(row.MainMsP999.ToString("F3", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(row.MainMsMax.ToString("F3", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(row.GcBytesAvg.ToString("F1", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(row.FrameCpuMsAvg.ToString("F3", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(row.FrameGpuMsAvg.ToString("F3", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(row.MemTotalMb.ToString("F1", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(row.FrameTimingAvailable ? "true" : "false");
            sb.AppendLine();

            string path = Path.Combine(_outDir, count + ".txt");
            File.WriteAllText(path, sb.ToString());
            GameLog.Info("Perf", "harness wrote " + path);
        }
    }
}
