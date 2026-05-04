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
    public void ResolveFrameColumnAnimatesIdleFrames()
    {
        Assert.AreEqual(2, PlayerSpriteAnimator.ResolveFrameColumn(PlayerSpriteAnimationState.Idle, 0.5f, 4, 4f));
        Assert.AreEqual(2, PlayerSpriteAnimator.ResolveFrameColumn(PlayerSpriteAnimationState.Walk, 0.25f, 4, 8f));
    }

    [Test]
    public void TryResolveDebugStateOverrideReadsWalkQuery()
    {
        var found = PlayerSpriteAnimator.TryResolveDebugStateOverride(
            "http://127.0.0.1:4188/?v=walk-preview&debugAnimation=walk",
            out var state);

        Assert.IsTrue(found);
        Assert.AreEqual(PlayerSpriteAnimationState.Walk, state);
    }

    [Test]
    public void TryResolveDebugStateOverrideIgnoresUnknownQuery()
    {
        var found = PlayerSpriteAnimator.TryResolveDebugStateOverride(
            "http://127.0.0.1:4188/?v=walk-preview&debugAnimation=jump",
            out var state);

        Assert.IsFalse(found);
        Assert.AreEqual(PlayerSpriteAnimationState.Idle, state);
    }

    [Test]
    public void DeveloperSpriteSheetIsAvailableAsRuntimeResource()
    {
        var texture = Resources.Load<Texture2D>("Characters/developer-spritesheet");

        Assert.IsNotNull(texture);
        Assert.IsTrue(texture.isReadable);
    }

    [Test]
    public void DeveloperSpriteSheetUsesIntegerFrameGrid()
    {
        var texture = Resources.Load<Texture2D>("Characters/developer-spritesheet");

        Assert.IsNotNull(texture);
        Assert.AreEqual(0, texture.width % 4);
        Assert.AreEqual(0, texture.height % 3);
    }

    [Test]
    public void DeveloperSpriteSheetDoesNotBleedIdleFeetIntoWalkCells()
    {
        var texture = Resources.Load<Texture2D>("Characters/developer-spritesheet");

        Assert.IsNotNull(texture);
        var pixels = texture.GetPixels32();
        var frameWidth = texture.width / 4;
        var frameHeight = texture.height / 3;
        var walkRowBottom = frameHeight;
        const int topBand = 32;

        for (var column = 0; column < 4; column++)
        {
            var left = column * frameWidth;
            var topBandBottom = walkRowBottom + frameHeight - topBand;
            var leakedPixels = 0;
            for (var y = topBandBottom; y < walkRowBottom + frameHeight; y++)
            {
                for (var x = left; x < left + frameWidth; x++)
                {
                    if (pixels[y * texture.width + x].a > 8)
                    {
                        leakedPixels++;
                    }
                }
            }

            Assert.AreEqual(0, leakedPixels, $"Walk frame {column} has opaque pixels in the top {topBand}px band.");
        }
    }

    [Test]
    public void CreateFramesSlicesFixedCellsAndAnchorsAtFeet()
    {
        var texture = new Texture2D(8, 4, TextureFormat.RGBA32, false);
        var clear = new Color32(0, 0, 0, 0);
        var opaque = new Color32(255, 255, 255, 255);

        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        PaintRect(texture, 2, 1, 2, 3, opaque);
        PaintRect(texture, 5, 0, 2, 3, opaque);
        texture.Apply();

        var frames = PlayerSpriteAnimator.CreateFrames(texture, 2, 1, 1f);

        try
        {
            Assert.AreEqual(new Rect(0f, 0f, 4f, 4f), frames[0].rect);
            Assert.AreEqual(new Rect(4f, 0f, 4f, 4f), frames[1].rect);
            Assert.AreEqual(new Vector2(3f, 1f), frames[0].pivot);
            Assert.AreEqual(new Vector2(2f, 0f), frames[1].pivot);
        }
        finally
        {
            foreach (var frame in frames)
            {
                Object.DestroyImmediate(frame);
            }

            Object.DestroyImmediate(texture);
        }
    }

    [Test]
    public void CreateFramesAnchorsPivotToBottomFootContact()
    {
        var texture = new Texture2D(6, 5, TextureFormat.RGBA32, false);
        var clear = new Color32(0, 0, 0, 0);
        var opaque = new Color32(255, 255, 255, 255);

        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        PaintRect(texture, 0, 1, 6, 3, opaque);
        PaintRect(texture, 1, 0, 2, 1, opaque);
        texture.Apply();

        var frames = PlayerSpriteAnimator.CreateFrames(texture, 1, 1, 1f);

        try
        {
            Assert.AreEqual(new Rect(0f, 0f, 6f, 5f), frames[0].rect);
            Assert.AreEqual(new Vector2(2f, 0f), frames[0].pivot);
        }
        finally
        {
            foreach (var frame in frames)
            {
                Object.DestroyImmediate(frame);
            }

            Object.DestroyImmediate(texture);
        }
    }

    [Test]
    public void CreateFramesKeepsFixedCellsAndPixelsPerUnitAcrossAnimationRows()
    {
        var texture = new Texture2D(4, 12, TextureFormat.RGBA32, false);
        var clear = new Color32(0, 0, 0, 0);
        var opaque = new Color32(255, 255, 255, 255);

        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        PaintRect(texture, 1, 6, 2, 5, opaque);
        PaintRect(texture, 1, 0, 2, 3, opaque);
        texture.Apply();

        var frames = PlayerSpriteAnimator.CreateFrames(texture, 1, 2, 1f);

        try
        {
            Assert.AreEqual(new Rect(0f, 6f, 4f, 6f), frames[0].rect);
            Assert.AreEqual(new Rect(0f, 0f, 4f, 6f), frames[1].rect);
            Assert.AreEqual(0f, frames[0].pivot.y);
            Assert.AreEqual(0f, frames[1].pivot.y);
            Assert.AreEqual(1f, frames[0].pixelsPerUnit, 0.001f);
            Assert.AreEqual(1f, frames[1].pixelsPerUnit, 0.001f);
        }
        finally
        {
            foreach (var frame in frames)
            {
                Object.DestroyImmediate(frame);
            }

            Object.DestroyImmediate(texture);
        }
    }

    [Test]
    public void CreateFramesKeepsConsistentWidthWithinAnimationRow()
    {
        var texture = new Texture2D(8, 4, TextureFormat.RGBA32, false);
        var clear = new Color32(0, 0, 0, 0);
        var opaque = new Color32(255, 255, 255, 255);

        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, clear);
            }
        }

        PaintRect(texture, 1, 0, 2, 4, opaque);
        PaintRect(texture, 5, 0, 1, 4, opaque);
        texture.Apply();

        var frames = PlayerSpriteAnimator.CreateFrames(texture, 2, 1, 1f);

        try
        {
            Assert.AreEqual(4f, frames[0].rect.width);
            Assert.AreEqual(4f, frames[1].rect.width);
        }
        finally
        {
            foreach (var frame in frames)
            {
                Object.DestroyImmediate(frame);
            }

            Object.DestroyImmediate(texture);
        }
    }

    private static void PaintRect(Texture2D texture, int left, int bottom, int width, int height, Color color)
    {
        for (var y = bottom; y < bottom + height; y++)
        {
            for (var x = left; x < left + width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }
    }
}
