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
        public static bool GateEnvironment { get; private set; }
        public static bool ArtVisualsRequested { get; private set; }
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
                else if (args[i] == "-arenaPerfGate")
                    GateEnvironment = true;
                else if (args[i] == "-arenaArtVisuals")
                    ArtVisualsRequested = true;
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
            // S3-M7：-arenaPerfGate 仅固定测量环境（解除帧率上限），不改游戏工作量/质量/分辨率
            if (GateEnvironment)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
                GameLog.Info("Perf", "gate env pinned: vSync=" + QualitySettings.vSyncCount +
                    " targetFrameRate=" + Application.targetFrameRate);
            }
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
            // S3-P5-ART-R7：formal art stress 层——canonical Dummy gameplay workload 不变，只叠加正式 presentation。
            // 预载/池化必须在 warmup 之前完成（§15/§16/§17）；失败时证据标记 fallback（§34：不能仍 PASS）。
            bool artMode = ArtVisualsRequested;
            if (artMode && !BuildArtVisuals())
            {
                artMode = false;
                _artLoadFailed = true;
            }
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
                    if (artMode) SyncArtVisuals(sim, count);
                    yield return null;
                }

                sampler.Begin();
                for (int i = 0; i < SampleFrames; i++)
                {
                    TickHarness(sim, count, ref castAt, ref pick);
                    if (artMode) SyncArtVisuals(sim, count);
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
            // 标题不声明分辨率（真实分辨率以 resolution= 实测行为准；S3-M6 证据中性化）
            sb.AppendLine("# ArenaPerfHarness, density=" + count);
            if (ArtVisualsRequested)
            {
                // S3-P5-ART-R7：formal art stress 证据元数据（observation，不设独立硬门槛；机器可读供 Gate 校验）
                sb.AppendLine("# art_profile=" + ArtProfileId);
                sb.AppendLine("# formal_visuals=" + (_artLoadFailed ? "false(load-failed-primitive-fallback)" : "true"));
                sb.AppendLine("# visual_instances=" + count.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("# visual_type_count=" + _artResolved.Count.ToString(CultureInfo.InvariantCulture));
                var mix = new StringBuilder();
                for (int t = 0; t < _artResolved.Count; t++)
                {
                    if (t > 0) mix.Append(';');
                    mix.Append(_artResolved[t]).Append('=').Append(CountMix(count, t));
                }
                sb.AppendLine("# visual_mix=" + mix.ToString());
                sb.AppendLine("# resolved_visuals=" + string.Join(";", _artResolved.ToArray()));
                int renderers, skinned, slots, verts, tris;
                CountArtRenderers(count, out renderers, out skinned, out slots, out verts, out tris);
                sb.AppendLine("# renderer_instances=" + renderers.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("# skinned_renderer_instances=" + skinned.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("# material_slots=" + slots.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("# approx_vertices=" + verts.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("# approx_triangles=" + tris.ToString(CultureInfo.InvariantCulture));
            }
            sb.AppendLine("# resolution=" + Screen.width + "x" + Screen.height +
                " fullscreen=" + Screen.fullScreen +
                " currentRes=" + Screen.currentResolution.width + "x" + Screen.currentResolution.height +
                " editor=" + Application.isEditor +
                " dx=" + SystemInfo.graphicsDeviceType);
            // S3-M7 只读硬件与环境证据（真实值来自 Runtime API / 既有常量，不打印预期值冒充实际值）
            sb.AppendLine("# hardware_cpu=" + SystemInfo.processorType);
            sb.AppendLine("# hardware_gpu=" + SystemInfo.graphicsDeviceName);
            sb.AppendLine("# perf_env quality=" + QualitySettings.names[QualitySettings.GetQualityLevel()] +
                " vsync=" + QualitySettings.vSyncCount +
                " targetFps=" + Application.targetFrameRate +
                " warmup=" + WarmupFrames + " sample=" + SampleFrames +
                " castInterval=" + CastInterval.ToString(CultureInfo.InvariantCulture));
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

        // ------------------------------------------------------------------
        // S3-P5-ART-R7：formal art stress 层（benchmark-only）
        // gameplay 实体仍为 EnemyKind.Dummy（canonical workload 不变）；presentation
        // 经 EnemyVisualPresenter + EnemyVisualCatalog 正式映射 round-robin 叠加。
        // 禁止把 gameplay Kind 改成 Brute/Stinger/Warden 凑视觉（§9）。
        // ------------------------------------------------------------------
        const string ArtProfileId = "formal-enemy-visual-stress-v1";
        const string ArtSelectionMode = "DistinctMappedFormalVisuals";
        const string ArtAssignmentMode = "RoundRobinByEnemyKind";

        static GameObject _artRoot;
        static readonly System.Collections.Generic.List<GameObject> _artPrefabs = new System.Collections.Generic.List<GameObject>();
        static readonly System.Collections.Generic.List<string> _artResolved = new System.Collections.Generic.List<string>();
        static readonly System.Collections.Generic.List<EnemyVisualPresenter> _artPresenters = new System.Collections.Generic.List<EnemyVisualPresenter>();
        static readonly System.Collections.Generic.List<int> _artVisualIndexOfSlot = new System.Collections.Generic.List<int>();
        static bool _artLoadFailed;

        /// <summary>art benchmark 视觉槽数（最高密度档一次建满，密度切换只 active/inactive，§16）。</summary>
        public static int ArtSlotCapacity
        {
            get { return Densities[Densities.Length - 1]; }
        }

        /// <summary>art 正式视觉层是否已接管 Dummy 表现（加载成功才算接管；失败回退基元，canonical 原样）。</summary>
        public static bool ArtVisualsActive
        {
            get { return RequestedFromArgs && ArtVisualsRequested && !_artLoadFailed && _artRoot != null; }
        }

        static int CountMix(int density, int visualIndex)
        {
            int n = 0;
            for (int i = 0; i < density; i++)
                if (_artVisualIndexOfSlot[i] == visualIndex)
                    n++;
            return n;
        }

        static void CountArtRenderers(int density, out int renderers, out int skinned, out int materialSlots, out int verts, out int tris)
        {
            renderers = 0; skinned = 0; materialSlots = 0; verts = 0; tris = 0;
            for (int i = 0; i < density && i < _artPresenters.Count; i++)
            {
                var root = _artPresenters[i].Root;
                if (root == null || !root.activeSelf)
                    continue;
                var rs = root.GetComponentsInChildren<Renderer>(true);
                for (int r = 0; r < rs.Length; r++)
                {
                    if (!rs[r].enabled)
                        continue; // §57：renderer disabled 不得计入 visual_instances 口径
                    renderers++;
                    if (rs[r] is SkinnedMeshRenderer) skinned++;
                    var mats = rs[r].sharedMaterials;
                    if (mats != null) materialSlots += mats.Length;
                }
                var smr = root.GetComponentInChildren<SkinnedMeshRenderer>();
                if (smr != null && smr.sharedMesh != null)
                {
                    verts += smr.sharedMesh.vertexCount;
                    tris += smr.sharedMesh.triangles.Length / 3;
                }
            }
        }

        static bool BuildArtVisuals()
        {
            _artLoadFailed = false;
            // 解析：EnemyVisualCatalog 当前全部非 null 正式映射（EnemyKind 确定性顺序），不复制路径 truth（§8/§26）
            var kinds = new[] { EnemyKind.Brute, EnemyKind.Stinger, EnemyKind.Ashling, EnemyKind.Warden };
            for (int k = 0; k < kinds.Length; k++)
            {
                string path = EnemyVisualCatalog.VisualResourcePath(kinds[k]);
                if (string.IsNullOrEmpty(path))
                    continue;
                GameObject prefab = Resources.Load<GameObject>(path);
                if (prefab == null)
                {
                    GameLog.Error("Perf", "art visual load failed: " + path);
                    return false; // §34：正式 visual 缺失不得仍 PASS（fallback 标记由证据层校验）
                }
                _artPrefabs.Add(prefab);
                _artResolved.Add(prefab.name);
            }
            if (_artResolved.Count == 0)
                return false;

            _artRoot = new GameObject("ArenaArtVisualPool");
            UnityEngine.Object.DontDestroyOnLoad(_artRoot);
            int capacity = ArtSlotCapacity;
            for (int i = 0; i < capacity; i++)
            {
                int vi = i % _artPrefabs.Count; // RoundRobinByEnemyKind
                var host = new GameObject("art_slot_" + i.ToString(CultureInfo.InvariantCulture));
                host.transform.SetParent(_artRoot.transform, false);
                var presenter = EnemyVisualPresenter.Mount(_artPrefabs[vi], host.transform);
                if (presenter == null)
                    return false;
                presenter.Hide();
                _artPresenters.Add(presenter);
                _artVisualIndexOfSlot.Add(vi);
            }
            GameLog.Info("Perf", "art visual pool ready: visuals=" + string.Join(";", _artResolved.ToArray()) +
                " slots=" + capacity.ToString(CultureInfo.InvariantCulture));
            return true;
        }

        static void SyncArtVisuals(ArenaSim sim, int density)
        {
            // art 槽 j 与 Dummy 池槽 j 固定 1:1（视觉身份随池位不走），j%visualCount=round-robin；
            // occupied（存活+死亡回收窗口内）→显示并同步 canonical 位置/姿态；否则隐藏。
            var items = sim.Dummies.Items;
            int n = density < _artPresenters.Count ? density : _artPresenters.Count;
            for (int j = 0; j < n; j++)
            {
                var presenter = _artPresenters[j];
                var d = items[j];
                if (!d.Occupied)
                {
                    if (presenter.Root != null && presenter.Root.activeSelf)
                        presenter.Hide();
                    continue;
                }
                if (presenter.Root != null && !presenter.Root.activeSelf)
                    presenter.Show();
                var t = presenter.Root.transform;
                t.position = new Vector3(d.X, 0f, d.Z);
                t.rotation = Quaternion.Euler(0f, d.YawDeg, 0f);
                presenter.Present(d.Alive, d.Anim, d.HitFlash, d.AttackExecutions, Time.time);
                presenter.ApplyFeedback(d.Alive, d.HitFlash, d.IgniteRemain > 0f);
            }
            for (int j = n; j < _artPresenters.Count; j++)
            {
                var presenter = _artPresenters[j];
                if (presenter.Root != null && presenter.Root.activeSelf)
                    presenter.Hide();
            }
        }
    }
}
