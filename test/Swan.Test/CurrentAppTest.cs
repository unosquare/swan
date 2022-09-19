namespace Swan.Test;

using Platform;

[TestFixture]
public class CurrentAppTest
{
    [Test]
    public void IsSingleInstanceTest() => Assert.IsTrue(SwanRuntime.IsTheOnlyInstance);
}
