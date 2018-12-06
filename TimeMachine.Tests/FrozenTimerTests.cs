using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;

namespace TimeMachine.Tests
{
    [TestClass]
    public class FrozenTimerTests
    {
        [TestMethod]
        public void Callback_CallbackIsNull()
        {
            using (var delorean = new Delorean())
            {
                Assert.ThrowsException<ArgumentNullException>(() => TimeProvider.Timer(null));
                Assert.ThrowsException<ArgumentNullException>(() => TimeProvider.Timer(null, null, 0, 0));
                Assert.ThrowsException<ArgumentNullException>(() => TimeProvider.Timer(null, null, 0u, 0u));
                Assert.ThrowsException<ArgumentNullException>(() => TimeProvider.Timer(null, null, TimeSpan.Zero, TimeSpan.Zero));
                Assert.ThrowsException<ArgumentNullException>(() => TimeProvider.Timer(null, null, 0L, 0L));
            }
        }

        [TestMethod]
        public void Callback_NeverGetsCalled()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var fired = 0;
                object actualState = null;

                using (var timer = TimeProvider.Timer((state) => { actualState = state; fired++; }))
                {
                    delorean.Advance(int.MaxValue - 1);

                    Assert.AreEqual(0, fired);
                    Assert.IsNull(actualState);
                }
            }
        }

        [DataTestMethod]
        [DataRow(-2, -1)]
        [DataRow(-1, -2)]
        public void Callback_Int32_Invalid(int initial, int period)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => TimeProvider.Timer((state) => { }, null, initial, period));
            }
        }

        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        public void Callback_Int32_HasInitialButNotInterval(int period)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                object expectedState = new object();

                var fired = 0;
                object actualState = null;

                using (var timer = TimeProvider.Timer((state) => { actualState = state; fired++; }, expectedState, 100, period))
                {
                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);
                    Assert.AreSame(expectedState, actualState);

                    actualState = null;

                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);
                    Assert.IsNull(actualState);
                }
            }
        }

        [TestMethod]
        public void Callback_Int32_HasInitialAndInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                object expectedState = new object();

                int fired = 0;
                object actualState = null;

                using (var timer = TimeProvider.Timer((state) => { actualState = state; fired++; }, expectedState, 100, 100))
                {
                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);
                    Assert.AreSame(expectedState, actualState);

                    actualState = null;

                    delorean.Advance(100);

                    Assert.AreEqual(2, fired);
                    Assert.AreSame(expectedState, actualState);

                    actualState = null;

                    delorean.Advance(100);

                    Assert.AreEqual(3, fired);
                    Assert.AreSame(expectedState, actualState);
                }
            }
        }

        [TestMethod]
        public void Callback_Int32_HasInitialGreaterThanInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++, null, 100, 50))
                {
                    delorean.Advance(50);

                    Assert.AreEqual(0, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(2, fired);
                }
            }
        }

        [TestMethod]
        public void Callback_Int32_HasInitialAndIntervalHitInSingleAdvance()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }, null, 100, 50))
                {
                    delorean.Advance(200);

                    Assert.AreEqual(3, fired);
                }
            }
        }

        [TestMethod]
        public void Callback_Int32_MultipleIntervalHitInSingleAdvance()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                object expectedState = new object();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }, null, 100, 50))
                {
                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(150);

                    Assert.AreEqual(4, fired);
                }
            }
        }

        [TestMethod]
        public void Callback_Int32_HasIntervalButNotInitial()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }, null, -1, 50))
                {
                    delorean.Advance(1000);

                    Assert.AreEqual(0, fired);
                }
            }
        }

        [DataTestMethod]
        [DataRow(-2L, -1L)]
        [DataRow(-1L, -2L)]
        [DataRow(1L + uint.MaxValue, -1L)]
        [DataRow(-1L, 1L + uint.MaxValue)]
        public void Callback_TimeSpan_Invalid(long initial, long period)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => TimeProvider.Timer((state) => { }, null, TimeSpan.FromMilliseconds(initial), TimeSpan.FromMilliseconds(period)));
            }
        }

        [DataTestMethod]
        [DataRow(-1L)]
        [DataRow(0L)]
        public void Callback_TimeSpan_HasInitialButNotInterval(long period)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                object expectedState = new object();

                var fired = 0;
                object actualState = null;

                using (var timer = TimeProvider.Timer((state) => { actualState = state; fired++; }, expectedState, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(period)))
                {
                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);
                    Assert.AreSame(expectedState, actualState);

                    actualState = null;

                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);
                    Assert.IsNull(actualState);
                }
            }
        }

        [TestMethod]
        public void Callback_TimeSpan_HasInitialAndInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                object expectedState = new object();

                int fired = 0;
                object actualState = null;

                using (var timer = TimeProvider.Timer((state) => { actualState = state; fired++; }, expectedState, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100)))
                {
                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);
                    Assert.AreSame(expectedState, actualState);

                    actualState = null;

                    delorean.Advance(100);

                    Assert.AreEqual(2, fired);
                    Assert.AreSame(expectedState, actualState);

                    actualState = null;

                    delorean.Advance(100);

                    Assert.AreEqual(3, fired);
                    Assert.AreSame(expectedState, actualState);
                }
            }
        }

        [TestMethod]
        public void Callback_TimeSpan_HasInitialGreaterThanInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50)))
                {
                    delorean.Advance(50);

                    Assert.AreEqual(0, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(2, fired);
                }
            }
        }

        [TestMethod]
        public void Callback_TimeSpan_HasInitialAndIntervalHitInSingleAdvance()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50)))
                {
                    delorean.Advance(200);

                    Assert.AreEqual(3, fired);
                }
            }
        }

        [TestMethod]
        public void Callback_TimeSpan_MultipleIntervalHitInSingleAdvance()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                object expectedState = new object();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }, null, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50)))
                {
                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(150);

                    Assert.AreEqual(4, fired);
                }
            }
        }

        [TestMethod]
        public void Callback_TimeSpan_HasIntervalButNotInitial()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }, null, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(50)))
                {
                    delorean.Advance(1000);

                    Assert.AreEqual(0, fired);
                }
            }
        }

        [DataTestMethod]
        [DataRow(-2L, -1L)]
        [DataRow(-1L, -2L)]
        [DataRow(1L + uint.MaxValue, -1L)]
        [DataRow(-1L, 1L + uint.MaxValue)]
        public void Callback_Int64_Invalid(long initial, long period)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => TimeProvider.Timer((state) => { }, null, initial, period));
            }
        }

        [DataTestMethod]
        [DataRow(-1L)]
        [DataRow(0L)]
        public void Callback_Int64_HasInitialButNotInterval(long period)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                object expectedState = new object();

                var fired = 0;
                object actualState = null;

                using (var timer = TimeProvider.Timer((state) => { actualState = state; fired++; }, expectedState, 100L, period))
                {
                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);
                    Assert.AreSame(expectedState, actualState);

                    actualState = null;

                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);
                    Assert.IsNull(actualState);
                }
            }
        }

        [TestMethod]
        public void Callback_Int64_HasInitialAndInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                object expectedState = new object();

                int fired = 0;
                object actualState = null;

                using (var timer = TimeProvider.Timer((state) => { actualState = state; fired++; }, expectedState, 100L, 100L))
                {
                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);
                    Assert.AreSame(expectedState, actualState);

                    actualState = null;

                    delorean.Advance(100);

                    Assert.AreEqual(2, fired);
                    Assert.AreSame(expectedState, actualState);

                    actualState = null;

                    delorean.Advance(100);

                    Assert.AreEqual(3, fired);
                    Assert.AreSame(expectedState, actualState);
                }
            }
        }

        [TestMethod]
        public void Callback_Int64_HasInitialGreaterThanInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++, null, 100L, 50L))
                {
                    delorean.Advance(50);

                    Assert.AreEqual(0, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(2, fired);
                }
            }
        }

        [TestMethod]
        public void Callback_Int64_HasInitialAndIntervalHitInSingleAdvance()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }, null, 100L, 50L))
                {
                    delorean.Advance(200);

                    Assert.AreEqual(3, fired);
                }
            }
        }

        [TestMethod]
        public void Callback_Int64_MultipleIntervalHitInSingleAdvance()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                object expectedState = new object();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }, null, 100L, 50L))
                {
                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(150);

                    Assert.AreEqual(4, fired);
                }
            }
        }

        [TestMethod]
        public void Callback_Int64_HasIntervalButNotInitial()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }, null, -1L, 50L))
                {
                    delorean.Advance(1000);

                    Assert.AreEqual(0, fired);
                }
            }
        }

        [DataTestMethod]
        [DataRow(uint.MaxValue)]
        [DataRow(0u)]
        public void Callback_UInt32_HasInitialButNotInterval(uint period)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                object expectedState = new object();

                var fired = 0;
                object actualState = null;

                using (var timer = TimeProvider.Timer((state) => { actualState = state; fired++; }, expectedState, 100u, period))
                {
                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);
                    Assert.AreSame(expectedState, actualState);

                    actualState = null;

                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);
                    Assert.IsNull(actualState);
                }
            }
        }

        [TestMethod]
        public void Callback_UInt32_HasInitialAndInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                object expectedState = new object();

                int fired = 0;
                object actualState = null;

                using (var timer = TimeProvider.Timer((state) => { actualState = state; fired++; }, expectedState, 100u, 100u))
                {
                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);
                    Assert.AreSame(expectedState, actualState);

                    actualState = null;

                    delorean.Advance(100);

                    Assert.AreEqual(2, fired);
                    Assert.AreSame(expectedState, actualState);

                    actualState = null;

                    delorean.Advance(100);

                    Assert.AreEqual(3, fired);
                    Assert.AreSame(expectedState, actualState);
                }
            }
        }

        [TestMethod]
        public void Callback_UInt32_HasInitialGreaterThanInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++, null, 100u, 50u))
                {
                    delorean.Advance(50);

                    Assert.AreEqual(0, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(2, fired);
                }
            }
        }

        [TestMethod]
        public void Callback_UInt32_HasInitialAndIntervalHitInSingleAdvance()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }, null, 100u, 50u))
                {
                    delorean.Advance(200);

                    Assert.AreEqual(3, fired);
                }
            }
        }

        [TestMethod]
        public void Callback_UInt32_MultipleIntervalHitInSingleAdvance()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                object expectedState = new object();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }, null, 100u, 50u))
                {
                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(150);

                    Assert.AreEqual(4, fired);
                }
            }
        }

        [TestMethod]
        public void Callback_UInt32_HasIntervalButNotInitial()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }, null, uint.MaxValue, 50u))
                {
                    delorean.Advance(1000);

                    Assert.AreEqual(0, fired);
                }
            }
        }

        [DataTestMethod]
        [DataRow(-2L, -1L)]
        [DataRow(-1L, -2L)]
        [DataRow(1L + uint.MaxValue, -1L)]
        [DataRow(-1L, 1L + uint.MaxValue)]
        public void Change_Int64_Invalid(long initial, long period)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();
                var timer = TimeProvider.Timer((state) => { });
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => timer.Change(initial, period));
            }
        }

        [DataTestMethod]
        [DataRow(-2, -1)]
        [DataRow(-1, -2)]
        public void Change_Int32_Invalid(int initial, int period)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();
                var timer = TimeProvider.Timer((state) => { });
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => timer.Change(initial, period));
            }
        }

        [DataTestMethod]
        [DataRow(-2L, -1L)]
        [DataRow(-1L, -2L)]
        [DataRow(1L + uint.MaxValue, -1L)]
        [DataRow(-1L, 1L + uint.MaxValue)]
        public void Change_TimeSpan_Invalid(long initial, long period)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();
                var timer = TimeProvider.Timer((state) => { });
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => timer.Change(TimeSpan.FromMilliseconds(initial), TimeSpan.FromMilliseconds(period)));
            }
        }

        [DataTestMethod]
        [DataRow(-1L)]
        [DataRow(0L)]
        public void Change_Int64_HasInitialButNotInterval(long period)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++))
                {
                    timer.Change(100L, period);

                    delorean.Advance(1000);

                    Assert.AreEqual(1, fired);
                }
            }
        }

        [TestMethod]
        public void Change_Int64_HasInitialAndInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }))
                {
                    timer.Change(100L, 100L);

                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(100);

                    Assert.AreEqual(2, fired);
                }
            }
        }

        [TestMethod]
        public void Change_Int64_HasInitialGreaterThanInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++))
                {
                    timer.Change(100L, 50L);

                    delorean.Advance(50);

                    Assert.AreEqual(0, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(2, fired);
                }
            }
        }

        [TestMethod]
        public void Change_Int64_HasInitialAndIntervalHitInSingleAdvance()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++))
                {
                    timer.Change(100L, 50L);

                    delorean.Advance(200);

                    Assert.AreEqual(3, fired);
                }
            }
        }

        [TestMethod]
        public void Change_Int64_HasIntervalButNotInitial()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }))
                {
                    timer.Change(-1L, 50L);

                    delorean.Advance(1000);

                    Assert.AreEqual(0, fired);
                }
            }
        }

        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        public void Change_Int32_HasInitialButNotInterval(int period)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++))
                {
                    timer.Change(100, period);

                    delorean.Advance(1000);

                    Assert.AreEqual(1, fired);
                }
            }
        }

        [TestMethod]
        public void Change_Int32_HasInitialAndInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }))
                {
                    timer.Change(100, 100);

                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(100);

                    Assert.AreEqual(2, fired);
                }
            }
        }

        [TestMethod]
        public void Change_Int32_HasInitialGreaterThanInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++))
                {
                    timer.Change(100, 50);

                    delorean.Advance(50);

                    Assert.AreEqual(0, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(2, fired);
                }
            }
        }

        [TestMethod]
        public void Change_Int32_HasInitialAndIntervalHitInSingleAdvance()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++))
                {
                    timer.Change(100, 50);

                    delorean.Advance(200);

                    Assert.AreEqual(3, fired);
                }
            }
        }

        [TestMethod]
        public void Change_Int32_HasIntervalButNotInitial()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }))
                {
                    timer.Change(-1, 50);

                    delorean.Advance(1000);

                    Assert.AreEqual(0, fired);
                }
            }
        }

        [DataTestMethod]
        [DataRow(uint.MaxValue)]
        [DataRow(0u)]
        public void Change_UInt32_HasInitialButNotInterval(uint period)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++))
                {
                    timer.Change(100u, period);

                    delorean.Advance(1000);

                    Assert.AreEqual(1, fired);
                }
            }
        }

        [TestMethod]
        public void Change_UInt32_HasInitialAndInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }))
                {
                    timer.Change(100u, 100u);

                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(100);

                    Assert.AreEqual(2, fired);
                }
            }
        }

        [TestMethod]
        public void Change_UInt32_HasInitialGreaterThanInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++))
                {
                    timer.Change(100u, 50u);

                    delorean.Advance(50);

                    Assert.AreEqual(0, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(2, fired);
                }
            }
        }

        [TestMethod]
        public void Change_UInt32_HasInitialAndIntervalHitInSingleAdvance()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++))
                {
                    timer.Change(100u, 50u);

                    delorean.Advance(200);

                    Assert.AreEqual(3, fired);
                }
            }
        }

        [TestMethod]
        public void Change_UInt32_HasIntervalButNotInitial()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }))
                {
                    timer.Change(uint.MaxValue, 50u);

                    delorean.Advance(1000);

                    Assert.AreEqual(0, fired);
                }
            }
        }


        [DataTestMethod]
        [DataRow(-1L)]
        [DataRow(0L)]
        public void Change_TimeSpan_HasInitialButNotInterval(long period)
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                var fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++))
                {
                    timer.Change(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(period));

                    delorean.Advance(1000);

                    Assert.AreEqual(1, fired);
                }
            }
        }

        [TestMethod]
        public void Change_TimeSpan_HasInitialAndInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }))
                {
                    timer.Change(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));

                    delorean.Advance(100);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(100);

                    Assert.AreEqual(2, fired);
                }
            }
        }

        [TestMethod]
        public void Change_TimeSpan_HasInitialGreaterThanInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++))
                {
                    timer.Change(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50));

                    delorean.Advance(50);

                    Assert.AreEqual(0, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(1, fired);

                    delorean.Advance(50);

                    Assert.AreEqual(2, fired);
                }
            }
        }

        [TestMethod]
        public void Change_TimeSpan_HasInitialAndIntervalHitInSingleAdvance()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => fired++))
                {
                    timer.Change(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50));

                    delorean.Advance(200);

                    Assert.AreEqual(3, fired);
                }
            }
        }

        [TestMethod]
        public void Change_TimeSpan_HasIntervalButNotInitial()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                int fired = 0;

                using (var timer = TimeProvider.Timer((state) => { fired++; }))
                {
                    timer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(50));

                    delorean.Advance(1000);

                    Assert.AreEqual(0, fired);
                }
            }
        }

        [TestMethod]
        public void Thawed_InitialNotHit_ContinuesRealtime()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                using (var evt = new AutoResetEvent(false))
                using (var timer = TimeProvider.Timer((state) => evt.Set(), null, 25, -1))
                {
                    delorean.Thaw();

                    Assert.IsTrue(evt.WaitOne(50));

                    Assert.IsFalse(evt.WaitOne(25));
                }
            }
        }

        [TestMethod]
        public void Thawed_InitialHit_NoInterval_Stops()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                using (var evt = new AutoResetEvent(false))
                using (var timer = TimeProvider.Timer((state) => evt.Set(), null, 25, -1))
                {
                    delorean.Advance(25);

                    evt.Reset();

                    delorean.Thaw();

                    Assert.IsFalse(evt.WaitOne(25));
                }
            }
        }

        [TestMethod]
        public void Thawed_InitialNotHit_HasInterval_UsesInitial()
        {
            using (var delorean = new Delorean())
            using (var evt = new AutoResetEvent(false))
            {
                delorean.Freeze();

                using (var timer = TimeProvider.Timer((state) => evt.Set(), null, 50, 500))
                {
                    delorean.Thaw();

                    var watch = Stopwatch.StartNew();

                    Assert.IsTrue(evt.WaitOne(100));

                    Assert.IsTrue(watch.ElapsedMilliseconds > 25);
                }
            }
        }

        [TestMethod]
        public void Thawed_InitialHit_HasInterval_UsesInterval()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                using (var evt = new AutoResetEvent(false))
                using (var timer = TimeProvider.Timer((state) => evt.Set(), null, 250, 25))
                {
                    delorean.Advance(250);
                    evt.Reset();

                    delorean.Thaw();

                    Assert.IsTrue(evt.WaitOne(100));
                }
            }
        }

        [TestMethod]
        public void Thawed_Change_FunctionsAsNormalTimer()
        {
            using (var delorean = new Delorean())
            {
                delorean.Freeze();

                using (var evt = new AutoResetEvent(false))
                using (var timer = TimeProvider.Timer((state) => evt.Set(), null, -1, -1))
                {
                    evt.Reset();

                    delorean.Thaw();

                    timer.Change(25, 25);

                    Assert.IsTrue(evt.WaitOne(50));
                }
            }
        }
    }
}