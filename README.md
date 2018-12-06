# TimeMachine

TimeMachine is intended to help speed up/enable testing code that normally requires the passage of actual calendar time. The target cases are code that require use of `Task.Delay`, `Timer`, `DateTime.UtcNow` and other calendar-time related constructs to aid in testing scenarios. It is intended to be a drop-in replacement where possible.

## TimeMachine.TimeProvider

`TimeProvider` is a static class that contains methods and properties that replace `Task.Delay`, the `System.Threading.Timer` constructors, and `DateTime.UtcNow`. Generally speaking, it should be a drop-in replacement for those methods and properties. During normal execution, there should be no difference in the functionality provided by TimeProvider vs the original targets.

## TimeMachine.Delorean

The `TimeMachine.Delorean` package consists of a single class named `Delorean` in the `TimeMachine` namespace. This class allows controlling the flow of time in testing scenarios. To be able to manipulate time, fire up a `Delorean` before any timer or delay related code and call `Freeze()` on the `Delorean` instance:

```
using (var delorean = new Delorean())
{
    delorean.Freeze();

    // assert that time is now frozen

    var firstNow = TimeProvider.UtcNow;
	
    // pass time in the real world
    Thread.Sleep(1000);

    var secondNow = TimeProvider.UtcNow;

    Assert.AreEqual(firstNow, secondNow);
}
```

You can control the flow of time using the `Advance` methods on your `Delorean` instance:

```
using (var delorean = new Delorean())
{
	delorean.Freeze();

    var firstNow = TimeProvider.UtcNow;

    delorean.Advance(TimeSpan.FromDays(1.0));

    Assert.AreEqual(firstNow.AddDays(1.0), TimeProvider.UtcNow);
}
```

## TimeProvider.UtcNow

Use `TimeProvider.UtcNow` in place of `DateTime.UtcNow`.

If time is frozen this property returns the current frozen time.

If time is not frozen this property returns the result of `DateTime.UtcNow`.

## TimeProvider.Delay

Use `TimeProvider.Delay` in place of `Task.Delay`.

If time is frozen when this method is called the returned task will complete when time is manually advanced by the required interval. When time is thawed, the task will complete when the real-world time reaches catches up to the frozen time. So, for example, if you ran code like: 

```
using (var delorean = new Delorean())
{
    delorean.Freeze();

    delorean.Advance(TimeSpan.FromDays(1.0));

    var delay = TimeProvider.Delay(TimeSpan.FromMilliseconds(1));

    delorean.Thaw();
}
```

The `delay` task will not complete for approximately one real-world day.

If time is not frozen this method is a passthrough to `Task.Delay`.

## TimeProvider.Timer

Use the `TimeProvider.Timer` overloads in place of the `System.Threading.Timer()` constructors.

If time is frozen the returned timer will fire when time is manually advanced by the required interval. When time is thawed, the `Timer` instance will attempt to resume firing on its normal schedule based on the frozen time, using rules similar to `TimeProvider.Delay`.

If time is not frozen, the returned timer is a light wrapper around `System.Threading.Timer`.