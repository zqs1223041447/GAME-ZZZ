using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Game.Runtime.Core
{
    public struct PerfRow
    {
        public int DummyCount;
        public int AliveCount;
        public int Frames;
        public float MainMsAvg;
        public float MainMsP95;
        public float MainMsMax;
        public float GcBytesAvg;
    }

    public sealed class PerfSampler
    {
        readonly List<float> _ms = new List<float>(128);
        readonly List<long> _gc = new List<long>(128);
        long _gcLast;
        bool _gcHasLast;

        public static string DefaultPath
        {
            get
            {
                return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "s1-perf-arena.txt"));
            }
        }

        public void Begin()
        {
            _ms.Clear();
            _gc.Clear();
            _gcLast = GC.GetAllocatedBytesForCurrentThread();
            _gcHasLast = true;
        }

        public void Sample(float unscaledDeltaTime)
        {
            _ms.Add(unscaledDeltaTime * 1000f);
            long now = GC.GetAllocatedBytesForCurrentThread();
            if (_gcHasLast)
            {
                long delta = now - _gcLast;
                if (delta < 0)
                    delta = 0;
                _gc.Add(delta);
            }

            _gcLast = now;
            _gcHasLast = true;
        }

        public PerfRow End(int dummyCount, int aliveCount)
        {
            PerfRow row;
            row.DummyCount = dummyCount;
            row.AliveCount = aliveCount;
            row.Frames = _ms.Count;
            row.MainMsAvg = Average(_ms);
            row.MainMsP95 = Percentile(_ms, 0.95f);
            row.MainMsMax = Max(_ms);
            row.GcBytesAvg = Average(_gc);
            return row;
        }

        public static void Write(string path, IReadOnlyList<PerfRow> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# S1 PerformanceArena");
            sb.AppendLine("dummy_count,alive,frames,main_ms_avg,main_ms_p95,main_ms_max,gc_alloc_bytes_avg");
            for (int i = 0; i < rows.Count; i++)
            {
                PerfRow r = rows[i];
                sb.Append(r.DummyCount.ToString(CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(r.AliveCount.ToString(CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(r.Frames.ToString(CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(r.MainMsAvg.ToString("F3", CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(r.MainMsP95.ToString("F3", CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(r.MainMsMax.ToString("F3", CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(r.GcBytesAvg.ToString("F1", CultureInfo.InvariantCulture));
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("# top_bottlenecks");
            sb.AppendLine("1. Per-dummy MeshRenderer + Transform sync (300 GO, no instancing)");
            sb.AppendLine("2. Near-band dummy steer + move every frame");
            sb.AppendLine("3. URP forward + directional light (dummy shadows off)");

            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, sb.ToString());
            GameLog.Info("Perf", "wrote " + path);
        }

        static float Average(List<float> values)
        {
            if (values.Count == 0)
                return 0f;
            double sum = 0;
            for (int i = 0; i < values.Count; i++)
                sum += values[i];
            return (float)(sum / values.Count);
        }

        static float Average(List<long> values)
        {
            if (values.Count == 0)
                return 0f;
            double sum = 0;
            for (int i = 0; i < values.Count; i++)
                sum += values[i];
            return (float)(sum / values.Count);
        }

        static float Max(List<float> values)
        {
            float m = 0f;
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] > m)
                    m = values[i];
            }

            return m;
        }

        public static float Percentile(List<float> values, float p)
        {
            if (values == null || values.Count == 0)
                return 0f;
            float[] copy = values.ToArray();
            Array.Sort(copy);
            float idx = p * (copy.Length - 1);
            int lo = (int)idx;
            int hi = lo + 1;
            if (hi >= copy.Length)
                return copy[copy.Length - 1];
            float t = idx - lo;
            return copy[lo] + (copy[hi] - copy[lo]) * t;
        }
    }
}
