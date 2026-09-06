using System.Collections.Generic;
using NUnit.Framework;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    public sealed class PerfSamplerTests
    {
        [Test]
        public void Percentile_P95OnTenSamples()
        {
            var values = new List<float> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            float p95 = PerfSampler.Percentile(values, 0.95f);
            Assert.AreEqual(9.55f, p95, 0.001f);
        }

        [Test]
        public void Sampler_AveragesFrameMsAndGc()
        {
            var sampler = new PerfSampler();
            sampler.Begin();
            sampler.Sample(0.010f);
            sampler.Sample(0.020f);
            sampler.Sample(0.030f);
            PerfRow row = sampler.End(100, 100);
            Assert.AreEqual(100, row.DummyCount);
            Assert.AreEqual(3, row.Frames);
            Assert.AreEqual(20f, row.MainMsAvg, 0.01f);
            Assert.GreaterOrEqual(row.GcBytesAvg, 0f);
        }
    }
}
