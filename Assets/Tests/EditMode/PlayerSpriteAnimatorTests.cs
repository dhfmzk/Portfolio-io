using NUnit.Framework;
using Player;
using UnityEngine;

public class PlayerSpriteAnimatorTests
{
    [Test]
    public void ResolveStatePrioritizesInteraction()
    {
        var state = PlayerSpriteAnimator.ResolveState(3f, true);

        Assert.AreEqual(PlayerSpriteAnimationState.Interact, state);
    }

    [Test]
    public void ResolveStateUsesWalkWhenMoving()
    {
        var state = PlayerSpriteAnimator.ResolveState(0.5f, false);

        Assert.AreEqual(PlayerSpriteAnimationState.Walk, state);
    }

    [Test]
    public void ResolveStateUsesIdleWhenStill()
    {
        var state = PlayerSpriteAnimator.ResolveState(0.01f, false);

        Assert.AreEqual(PlayerSpriteAnimationState.Idle, state);
    }

    [Test]
    public void ResolveFrameIndexMapsRowsAndColumns()
    {
        Assert.AreEqual(0, PlayerSpriteAnimator.ResolveFrameIndex(0, 0, 4));
        Assert.AreEqual(7, PlayerSpriteAnimator.ResolveFrameIndex(1, 3, 4));
        Assert.AreEqual(11, PlayerSpriteAnimator.ResolveFrameIndex(2, 3, 4));
    }

    [Test]
    public void DeveloperSpriteSheetIsAvailableAsRuntimeResource()
    {
        var texture = Resources.Load<Texture2D>("Characters/developer-spritesheet");

        Assert.IsNotNull(texture);
    }
}
