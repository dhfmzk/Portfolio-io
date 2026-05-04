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

    [Test]
    public void ResolveKeySpriteUsesPressedSpriteWhenPressed()
    {
        var normal = UnityEngine.Sprite.Create(
            UnityEngine.Texture2D.whiteTexture,
            new UnityEngine.Rect(0f, 0f, 1f, 1f),
            new UnityEngine.Vector2(0.5f, 0.5f));
        var pressed = UnityEngine.Sprite.Create(
            UnityEngine.Texture2D.whiteTexture,
            new UnityEngine.Rect(0f, 0f, 1f, 1f),
            new UnityEngine.Vector2(0.5f, 0.5f));

        try
        {
            Assert.AreEqual(pressed, ControlIndicatorUI.ResolveKeySprite(true, normal, pressed));
            Assert.AreEqual(normal, ControlIndicatorUI.ResolveKeySprite(false, normal, pressed));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(normal);
            UnityEngine.Object.DestroyImmediate(pressed);
        }
    }
}
