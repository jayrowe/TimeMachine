using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace TimeMachine.Tests
{
    [TestClass]
    public class ThawedTimerTests
    {
        [TestMethod]
        public void CallbackOnly_null()
        {
            var ex = Assert.ThrowsException<ArgumentNullException>(() => TimeProvider.Timer(null));

            StringAssert.Contains(ex.StackTrace, "System.Threading.Timer");
        }

        [TestMethod]
        public void CallbackOnly_DoesNotFire()
        {
            bool fired = false;

            using (var timer = TimeProvider.Timer((o) => fired = true))
            {
                Thread.Sleep(100);
            }

            Assert.IsFalse(fired);
        }

        [DataTestMethod]
        [DataRow(-2, -1)]
        [DataRow(-1, -2)]
        public void Callback_Int32_Invalid(int initial, int period)
        {
            var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() => TimeProvider.Timer((state) => { }, null, initial, period));

            StringAssert.Contains(ex.StackTrace, "System.Threading.Timer");
        }

        [TestMethod]
        public void Callback_Int32_InitialButNoInterval()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release(), null, 50, -1))
            {
                Assert.IsFalse(semaphore.Wait(25));
                Assert.IsTrue(semaphore.Wait(75));
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Callback_Int32_IntervalButNoInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release(), null, -1, 50))
            {
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Callback_Int32_IntervalAndInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release(), null, 100, 50))
            {
                Assert.IsFalse(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(50));
                Assert.IsTrue(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(75));
            }
        }

        [DataTestMethod]
        [DataRow(-2L, -1L)]
        [DataRow(-1L, -2L)]
        [DataRow(1L + uint.MaxValue, -1L)]
        [DataRow(-1L, 1L + uint.MaxValue)]
        public void Callback_TimeSpan_Invalid(long initial, long period)
        {
            var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() => TimeProvider.Timer((state) => { }, null, TimeSpan.FromMilliseconds(initial), TimeSpan.FromMilliseconds(period)));

            StringAssert.Contains(ex.StackTrace, "System.Threading.Timer");
        }

        [TestMethod]
        public void Callback_TimeSpan_InitialButNoInterval()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release(), null, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(-1)))
            {
                Assert.IsFalse(semaphore.Wait(25));
                Assert.IsTrue(semaphore.Wait(50));
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Callback_TimeSpan_IntervalButNoInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release(), null, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(50)))
            {
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Callback_TimeSpan_IntervalAndInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release(), null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50)))
            {
                Assert.IsFalse(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(50));
                Assert.IsTrue(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(75));
            }
        }

        [DataTestMethod]
        [DataRow(-2L, -1L)]
        [DataRow(-1L, -2L)]
        [DataRow(1L + uint.MaxValue, -1L)]
        [DataRow(-1L, 1L + uint.MaxValue)]
        public void Callback_Int64_Invalid(long initial, long period)
        {
            var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() => TimeProvider.Timer((state) => { }, null, initial, period));

            StringAssert.Contains(ex.StackTrace, "System.Threading.Timer");
        }

        [TestMethod]
        public void Callback_Int64_InitialButNoInterval()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release(), null, 50L, -1L))
            {
                Assert.IsFalse(semaphore.Wait(25));
                Assert.IsTrue(semaphore.Wait(50));
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Callback_Int64_IntervalButNoInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release(), null, -1L, 50L))
            {
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Callback_Int64_IntervalAndInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release(), null, 100L, 50L))
            {
                Assert.IsFalse(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(50));
                Assert.IsTrue(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Callback_UInt32_InitialButNoInterval()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release(), null, 50u, uint.MaxValue))
            {
                Assert.IsFalse(semaphore.Wait(25));
                Assert.IsTrue(semaphore.Wait(50));
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Callback_UInt32_IntervalButNoInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release(), null, uint.MaxValue, 50u))
            {
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Callback_UInt32_IntervalAndInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release(), null, 100u, 50u))
            {
                Assert.IsFalse(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(50));
                Assert.IsTrue(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(75));
            }
        }

        [DataTestMethod]
        [DataRow(-2, -1)]
        [DataRow(-1, -2)]
        public void Change_Int32_Invalid(int initial, int period)
        {
            var timer = TimeProvider.Timer((state) => { });

            var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() => timer.Change(initial, period));

            StringAssert.Contains(ex.StackTrace, "System.Threading.Timer");
        }

        [TestMethod]
        public void Change_Int32_InitialButNoInterval()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release()))
            {
                timer.Change(50, -1);

                Assert.IsFalse(semaphore.Wait(25));
                Assert.IsTrue(semaphore.Wait(75));
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Change_Int32_IntervalButNoInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release()))
            {
                timer.Change(-1, 50);
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Change_Int32_IntervalAndInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release()))
            {
                timer.Change(100, 50);
                Assert.IsFalse(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(50));
                Assert.IsTrue(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(75));
            }
        }

        [DataTestMethod]
        [DataRow(-2L, -1L)]
        [DataRow(-1L, -2L)]
        [DataRow(1L + uint.MaxValue, -1L)]
        [DataRow(-1L, 1L + uint.MaxValue)]
        public void Change_TimeSpan_Invalid(long initial, long period)
        {
            var timer = TimeProvider.Timer((state) => { });

            var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() => timer.Change(TimeSpan.FromMilliseconds(initial), TimeSpan.FromMilliseconds(period)));

            StringAssert.Contains(ex.StackTrace, "System.Threading.Timer");
        }

        [TestMethod]
        public void Change_TimeSpan_InitialButNoInterval()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release()))
            {
                timer.Change(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(-1));

                Assert.IsFalse(semaphore.Wait(25));
                Assert.IsTrue(semaphore.Wait(75));
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Change_TimeSpan_IntervalButNoInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release()))
            {
                timer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(50));
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Change_TimeSpan_IntervalAndInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release()))
            {
                timer.Change(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50));
                Assert.IsFalse(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(50));
                Assert.IsTrue(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(75));
            }
        }

        [DataTestMethod]
        [DataRow(-2L, -1L)]
        [DataRow(-1L, -2L)]
        [DataRow(1L + uint.MaxValue, -1L)]
        [DataRow(-1L, 1L + uint.MaxValue)]
        public void Change_Int64_Invalid(long initial, long period)
        {
            var timer = TimeProvider.Timer((state) => { });

            var ex = Assert.ThrowsException<ArgumentOutOfRangeException>(() => timer.Change(initial, period));

            StringAssert.Contains(ex.StackTrace, "System.Threading.Timer");
        }

        [TestMethod]
        public void Change_Int64_InitialButNoInterval()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release()))
            {
                timer.Change(50L, -1L);

                Assert.IsFalse(semaphore.Wait(25));
                Assert.IsTrue(semaphore.Wait(75));
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Change_Int64_IntervalButNoInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release()))
            {
                timer.Change(-1L, 50L);
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Change_Int64_IntervalAndInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release()))
            {
                timer.Change(100L, 50L);
                Assert.IsFalse(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(50));
                Assert.IsTrue(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Change_UInt32_InitialButNoInterval()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release()))
            {
                timer.Change(50u, uint.MaxValue);

                Assert.IsFalse(semaphore.Wait(25));
                Assert.IsTrue(semaphore.Wait(75));
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Change_UInt32_IntervalButNoInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release()))
            {
                timer.Change(uint.MaxValue, 50u);
                Assert.IsFalse(semaphore.Wait(75));
            }
        }

        [TestMethod]
        public void Change_UInt32_IntervalAndInitial()
        {
            var semaphore = new SemaphoreSlim(0);

            using (var timer = TimeProvider.Timer(state => semaphore.Release()))
            {
                timer.Change(100u, 50u);
                Assert.IsFalse(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(50));
                Assert.IsTrue(semaphore.Wait(75));
                Assert.IsTrue(semaphore.Wait(75));
            }
        }
    }
}
