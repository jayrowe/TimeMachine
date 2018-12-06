using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TimeMachine.Tests
{
    [TestClass]
    public class FrozenDelayTests
    {
        [TestMethod]
        public async Task NotFrozen_Int32()
        {
            var watch = Stopwatch.StartNew();

            await TimeProvider.Delay(5);

            watch.Stop();

            Assert.IsTrue(watch.ElapsedMilliseconds >= 5);
        }

        [TestMethod]
        public void NotFrozen_Int32_CancellationToken()
        {
            var token = new CancellationTokenSource();
            token.Cancel();

            Assert.ThrowsExceptionAsync<TaskCanceledException>(
                () => TimeProvider.Delay(5, token.Token));
        }

        [TestMethod]
        public async Task NotFrozen_TimeSpan()
        {
            var watch = Stopwatch.StartNew();

            await TimeProvider.Delay(TimeSpan.FromMilliseconds(5));

            watch.Stop();

            Assert.IsTrue(watch.ElapsedMilliseconds >= 5);
        }

        [TestMethod]
        public void NotFrozen_TimeSpan_CancellationToken()
        {
            var token = new CancellationTokenSource();
            token.Cancel();

            Assert.ThrowsExceptionAsync<TaskCanceledException>(
                () => TimeProvider.Delay(TimeSpan.FromMilliseconds(5), token.Token));
        }

        [TestMethod]
        public async Task Frozen_IgnoresRealTime()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var watch = Stopwatch.StartNew();

                var controlledDelay = TimeProvider.Delay(5);
                var realtimeDelay = Task.Delay(10);

                Assert.AreSame(
                    realtimeDelay,
                    await Task.WhenAny(controlledDelay, realtimeDelay));

                watch.Stop();

                Assert.IsTrue(watch.ElapsedMilliseconds >= 10);
            }
        }

        [TestMethod]
        public async Task Frozen_Int32_FollowsTimeAdvances()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var watch = Stopwatch.StartNew();

                var controlledDelay = TimeProvider.Delay(500);
                delorean.Advance(500);
                await controlledDelay;

                watch.Stop();

                Assert.IsTrue(watch.ElapsedMilliseconds < 100);
            }
        }

        [TestMethod]
        public async Task Frozen_TimeSpan_FollowsTimeAdvances()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var watch = Stopwatch.StartNew();

                var controlledDelay = TimeProvider.Delay(TimeSpan.FromMilliseconds(500));
                delorean.Advance(500);
                await controlledDelay;

                watch.Stop();

                Assert.IsTrue(watch.ElapsedMilliseconds < 100);
            }
        }

        [TestMethod]
        public async Task Thawed_ContinuesRealtimeBasedOnTargetTime()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var first = TimeProvider.Delay(5);

                var second = TimeProvider.Delay(100);

                await Task.Delay(5);

                delorean.Thaw();

                Assert.IsTrue(first.Wait(1));

                Assert.IsFalse(second.Wait(25));
            }
        }

        [TestMethod]
        public async Task Frozen_Int32_CancellationTokenStartsCancelled()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var token = new CancellationToken(true);

                var assert = Assert.ThrowsExceptionAsync<TaskCanceledException>(
                    async () => await TimeProvider.Delay(5, token));

                Assert.IsTrue(assert.Wait(100));

                await assert;
            }
        }

        [TestMethod]
        public async Task Frozen_Int32_CancellationTokenBecomesCancelled()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var token = new CancellationTokenSource();

                var assert = Assert.ThrowsExceptionAsync<TaskCanceledException>(
                    async () => await TimeProvider.Delay(5, token.Token));

                token.Cancel();

                Assert.IsTrue(assert.Wait(100));

                await assert;
            }
        }

        [TestMethod]
        public async Task Frozen_Int32_NoDelay_ReturnsImmediately()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var delay = TimeProvider.Delay(0);

                Assert.IsTrue(delay.Wait(1));

                await delay;
            }
        }

        [TestMethod]
        public void Frozen_Int32_Infinite_NeverReturns()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var delay = TimeProvider.Delay(-1);

                delorean.Advance(DateTime.MaxValue - TimeProvider.UtcNow);

                Assert.IsFalse(delay.Wait(1));
            }
        }

        [TestMethod]
        public void Frozen_Int32_InvalidWait()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                Assert.ThrowsException<ArgumentOutOfRangeException>(() => TimeProvider.Delay(-2));
            }
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(0)]
        [DataRow(-1)]
        public async Task Frozen_TimeSpan_CancellationTokenStartsCancelled(double milliseconds)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var token = new CancellationToken(true);

                var assert = Assert.ThrowsExceptionAsync<TaskCanceledException>(
                    async () => await TimeProvider.Delay(TimeSpan.FromMilliseconds(milliseconds), token));

                Assert.IsTrue(assert.Wait(100));

                await assert;
            }
        }

        [TestMethod]
        public async Task Frozen_TimeSpan_CancellationTokenBecomesCancelled()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var token = new CancellationTokenSource();

                var assert = Assert.ThrowsExceptionAsync<TaskCanceledException>(
                    async () => await TimeProvider.Delay(TimeSpan.FromMilliseconds(5), token.Token));

                token.Cancel();

                Assert.IsTrue(assert.Wait(100));

                await assert;
            }
        }

        [DataTestMethod]
        [DataRow(0.4999999999)]
        [DataRow(0.0)]
        [DataRow(-0.4999999999)]
        public async Task Frozen_TimeSpan_NoDelay_ReturnsImmediately(double milliseconds)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var delay = TimeProvider.Delay(TimeSpan.FromMilliseconds(milliseconds));

                Assert.IsTrue(delay.Wait(1));

                await delay;
            }
        }

        [DataTestMethod]
        [DataRow(-0.5)]
        [DataRow(-1)]
        [DataRow(-1.49999999)]
        public void Frozen_TimeSpan_Infinite_NeverReturns(double milliseconds)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var delay = TimeProvider.Delay(TimeSpan.FromMilliseconds(milliseconds));

                delorean.Advance(DateTime.MaxValue - TimeProvider.UtcNow);

                Assert.IsFalse(delay.Wait(1));
            }
        }

        [DataTestMethod]
        [DataRow(-1.5)]
        [DataRow(-2)]
        public void Frozen_TimeSpan_InvalidWait(double milliseconds)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                Assert.ThrowsException<ArgumentOutOfRangeException>(() => TimeProvider.Delay(TimeSpan.FromMilliseconds(milliseconds)));
            }
        }
    }
}
