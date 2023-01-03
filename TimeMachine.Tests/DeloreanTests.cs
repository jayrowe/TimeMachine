using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TimeMachine.Tests
{
    [TestClass]
    public class DeloreanTests
    {
        [TestMethod]
        public void UtcNow_NotFrozen()
        {
            var before = DateTime.UtcNow;
            var providedNow = TimeProvider.UtcNow;
            var after = DateTime.UtcNow;

            Assert.IsTrue(before <= providedNow);
            Assert.IsTrue(after >= providedNow);
        }

        [TestMethod]
        public void UtcNow_Frozen()
        {
            using (var delorean = new Delorean(true))
            {
                delorean.Freeze();

                Thread.Sleep(1);

                var before = DateTime.UtcNow;

                var firstNow = TimeProvider.UtcNow;
                Thread.Sleep(1);
                var secondNow = TimeProvider.UtcNow;

                var after = DateTime.UtcNow;

                Assert.IsTrue(before >= firstNow);
                Assert.IsTrue(after >= firstNow);
                Assert.AreEqual(firstNow, secondNow);
            }
        }

        [TestMethod]
        public void UtcNowOffset_NotFrozen()
        {
            var before = DateTimeOffset.UtcNow;
            var providedNow = TimeProvider.UtcNowOffset;
            var after = DateTimeOffset.UtcNow;

            Assert.IsTrue(before <= providedNow);
            Assert.IsTrue(after >= providedNow);
        }

        [TestMethod]
        public void UtcNowOffset_Frozen()
        {
            using (var delorean = new Delorean(true))
            {
                delorean.Freeze();

                Thread.Sleep(1);

                var before = DateTimeOffset.UtcNow;

                var firstNow = TimeProvider.UtcNowOffset;
                Thread.Sleep(1);
                var secondNow = TimeProvider.UtcNowOffset;

                var after = DateTimeOffset.UtcNow;

                Assert.IsTrue(before >= firstNow);
                Assert.IsTrue(after >= firstNow);
                Assert.AreEqual(firstNow, secondNow);
            }
        }

        [TestMethod]
        public void Advance_Int32_TimeIsNotFrozen_ThrowsException()
        {
            using (var delorean = new Delorean(false))
            {
                Assert.ThrowsException<InvalidOperationException>(
                    () => delorean.Advance(1));
            }
        }

        [TestMethod]
        public void Advance_TimeSpan_TimeIsNotFrozen_ThrowsException()
        {
            using (var delorean = new Delorean(false))
            {
                Assert.ThrowsException<InvalidOperationException>(
                    () => delorean.Advance(TimeSpan.FromMilliseconds(1)));
            }
        }

        [TestMethod]
        public void Advance_StepBySingleMillisecond()
        {
            using (var delorean = new Delorean(true))
            {
                var firstNow = TimeProvider.UtcNow;

                delorean.Advance(1);

                var secondNow = TimeProvider.UtcNow;

                Assert.AreEqual(TimeSpan.TicksPerMillisecond, (secondNow - firstNow).Ticks);
            }
        }

        [TestMethod]
        public void Advance_StepByTimeSpan()
        {
            using (var delorean = new Delorean(true))
            {
                var firstNow = TimeProvider.UtcNow;

                delorean.Advance(TimeSpan.FromSeconds(1.0));

                var secondNow = TimeProvider.UtcNow;

                Assert.AreEqual(TimeSpan.TicksPerSecond, (secondNow - firstNow).Ticks);
            }
        }

        [TestMethod]
        public void Thaw_ReturnsToNormalTime()
        {
            using (var delorean = new Delorean(true))
            {
                delorean.Advance(10);

                var firstNow = TimeProvider.UtcNow;

                delorean.Thaw();

                var secondNow = TimeProvider.UtcNow;

                Assert.IsTrue(firstNow > secondNow);
            }
        }
    }
}
