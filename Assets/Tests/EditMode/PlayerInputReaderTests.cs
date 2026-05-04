using Input;
using NUnit.Framework;
using UnityEngine;

public class PlayerInputReaderTests
{
    [Test]
    public void ResolveMoveInputCombinesKeyboardAndVirtualButtons()
    {
        Assert.AreEqual(1f, PlayerInputReader.ResolveMoveInput(false, false, false, true));
        Assert.AreEqual(-1f, PlayerInputReader.ResolveMoveInput(false, false, true, false));
        Assert.AreEqual(0f, PlayerInputReader.ResolveMoveInput(false, false, true, true));
        Assert.AreEqual(0f, PlayerInputReader.ResolveMoveInput(true, true, false, false));
    }

    [Test]
    public void ResolvePressedStateUsesKeyboardOrVirtualInput()
    {
        Assert.IsTrue(PlayerInputReader.ResolvePressedState(true, false));
        Assert.IsTrue(PlayerInputReader.ResolvePressedState(false, true));
        Assert.IsFalse(PlayerInputReader.ResolvePressedState(false, false));
    }

    [Test]
    public void PlayerInputReaderRunsBeforeInteractionConsumers()
    {
        var attributes = typeof(PlayerInputReader)
            .GetCustomAttributes(typeof(DefaultExecutionOrder), false);

        Assert.AreEqual(1, attributes.Length);
        Assert.Less(((DefaultExecutionOrder)attributes[0]).order, 0);
    }
}
