using NUnit.Framework;
using Game.Runtime.Core;

namespace Game.Tests.EditMode
{
    public sealed class SeededRngTests
    {
        [Test]
        public void SameSeed_SixteenNextUInt_Match()
        {
            var a = new SeededRng(0xC0FFEE);
            var b = new SeededRng(0xC0FFEE);
            for (int i = 0; i < 16; i++)
                Assert.AreEqual(a.NextUInt(), b.NextUInt(), "index " + i);
        }

        [Test]
        public void DifferentSeeds_FirstValueDiffers()
        {
            uint firstA = new SeededRng(1u).NextUInt();
            uint firstB = new SeededRng(2u).NextUInt();
            Assert.AreNotEqual(firstA, firstB);
        }

        [Test]
        public void SeedZero_DoesNotEmitZeroAsFirstValue()
        {
            uint first = new SeededRng(0u).NextUInt();
            Assert.AreNotEqual(0u, first);
        }

        [Test]
        public void NextFloat01_IsHalfOpenUnitInterval()
        {
            var rng = new SeededRng(0xC0FFEE);
            for (int i = 0; i < 64; i++)
            {
                float v = rng.NextFloat01();
                Assert.GreaterOrEqual(v, 0f);
                Assert.Less(v, 1f);
            }
        }
    }
}
