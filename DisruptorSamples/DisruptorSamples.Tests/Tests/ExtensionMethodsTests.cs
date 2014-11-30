using System;
using System.Linq;
using NUnit.Framework;

namespace DisruptorSamples.Tests.Tests
{
    [TestFixture]
    public class ExtensionMethodsTests
    {
        [Test]
        public void PowerOfTwo()
        {
            int x;
            x = 1;
            Assert.IsFalse(x.IsPowerOfTwo(), "Inexpected result for value {0}.", x);
            x = 2;
            Assert.IsTrue(x.IsPowerOfTwo(), "Inexpected result for value {0}.", x);
            x = 3;
            Assert.IsFalse(x.IsPowerOfTwo(), "Inexpected result for value {0}.", x);
            x = 4;
            Assert.IsTrue(x.IsPowerOfTwo(), "Inexpected result for value {0}.", x);
        }


        [Test]
        public void NextPowerOfTwo()
        {
            int x;
            x = 1;
            Assert.AreEqual(2, x.NextPowerOfTwo(), "Inexpected result for value {0}.", x);
            x = 2;
            Assert.AreEqual(2, x.NextPowerOfTwo(), "Inexpected result for value {0}.", x);
            x = 3;
            Assert.AreEqual(4, x.NextPowerOfTwo(), "Inexpected result for value {0}.", x);
            x = 4;
            Assert.AreEqual(4, x.NextPowerOfTwo(), "Inexpected result for value {0}.", x);
        }
    }
}
