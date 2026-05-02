using NUnit.Framework;
using Player;
using UnityEngine;

public class PlayerControllerTests
{
    [Test]
    public void ResolveHorizontalVelocityKeepsCurrentYVelocity()
    {
        var velocity = PlayerController.ResolveHorizontalVelocity(new Vector2(0f, -4f), 1f, 6f);

        Assert.AreEqual(6f, velocity.x);
        Assert.AreEqual(-4f, velocity.y);
    }

    [Test]
    public void ResolveJumpVelocityOnlyChangesYWhenGroundedAndRequested()
    {
        var velocity = PlayerController.ResolveJumpVelocity(new Vector2(3f, -1f), true, true, 9f);

        Assert.AreEqual(3f, velocity.x);
        Assert.AreEqual(9f, velocity.y);
    }

    [Test]
    public void ResolveJumpVelocityDoesNothingWhenAirborne()
    {
        var velocity = PlayerController.ResolveJumpVelocity(new Vector2(3f, -1f), false, true, 9f);

        Assert.AreEqual(3f, velocity.x);
        Assert.AreEqual(-1f, velocity.y);
    }
}
