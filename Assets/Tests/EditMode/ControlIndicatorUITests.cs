using NUnit.Framework;
using UI;

public class ControlIndicatorUITests
{
    [Test]
    public void ResolveLabelColorUsesActiveColorWhenPressed()
    {
        var inactive = UnityEngine.Color.gray;
        var active = UnityEngine.Color.white;

        var resolved = ControlIndicatorUI.ResolveLabelColor(true, active, inactive);

        Assert.AreEqual(active, resolved);
    }

    [Test]
    public void ResolveLabelColorUsesInactiveColorWhenReleased()
    {
        var inactive = UnityEngine.Color.gray;
        var active = UnityEngine.Color.white;

        var resolved = ControlIndicatorUI.ResolveLabelColor(false, active, inactive);

        Assert.AreEqual(inactive, resolved);
    }
}
