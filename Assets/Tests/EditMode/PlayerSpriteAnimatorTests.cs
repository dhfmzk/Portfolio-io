using NUnit.Framework;
using Player;
using System.Reflection;
using UnityEngine;

public class PlayerSpriteAnimatorTests
{
    private const int MaxJumpHeightLoss = 12;

    [Test]
    public void ResolveStateUsesJumpWhenAirborne()
    {
        var state = PlayerSpriteAnimator.ResolveState(3f, false);

        Assert.AreEqual(PlayerSpriteAnimationState.Jump, state);
    }

    [Test]
    public void ResolveStateUsesWalkWhenMoving()
    {
        var state = PlayerSpriteAnimator.ResolveState(0.5f, true);

        Assert.AreEqual(PlayerSpriteAnimationState.Walk, state);
    }

    [Test]
    public void ResolveStateUsesIdleWhenStill()
    {
        var state = PlayerSpriteAnimator.ResolveState(0.01f, true);

        Assert.AreEqual(PlayerSpriteAnimationState.Idle, state);
    }

    [Test]
    public void ResolveFrameIndexMapsRowsAndColumns()
    {
        Assert.AreEqual(0, PlayerSpriteAnimator.ResolveFrameIndex(0, 0, PlayerSpriteAnimator.AnimationColumns));
        Assert.AreEqual(15, PlayerSpriteAnimator.ResolveFrameIndex(1, 7, PlayerSpriteAnimator.AnimationColumns));
        Assert.AreEqual(23, PlayerSpriteAnimator.ResolveFrameIndex(2, 7, PlayerSpriteAnimator.AnimationColumns));
    }

    [Test]
    public void ResolveFrameColumnAnimatesEightFrameRows()
    {
        Assert.AreEqual(2, PlayerSpriteAnimator.ResolveFrameColumn(PlayerSpriteAnimationState.Idle, 0.5f, 8, 4f));
        Assert.AreEqual(2, PlayerSpriteAnimator.ResolveFrameColumn(PlayerSpriteAnimationState.Walk, 0.25f, 8, 8f));
        Assert.AreEqual(6, PlayerSpriteAnimator.ResolveFrameColumn(PlayerSpriteAnimationState.Jump, 0.75f, 8, 8f));
    }

    [Test]
    public void ResolveFrameColumnLoopsJumpPreviewRows()
    {
        Assert.AreEqual(0, PlayerSpriteAnimator.ResolveFrameColumn(PlayerSpriteAnimationState.Jump, 1f, 8, 8f));
        Assert.AreEqual(0, PlayerSpriteAnimator.ResolveFrameColumn(PlayerSpriteAnimationState.Idle, 2f, 8, 4f));
    }

    [Test]
    public void IdleAnimationUsesCalmDefaultFrameRate()
    {
        var gameObject = new GameObject("Animator Test");
        gameObject.AddComponent<Rigidbody2D>();
        gameObject.AddComponent<SpriteRenderer>();
        var animator = gameObject.AddComponent<PlayerSpriteAnimator>();

        try
        {
            var field = typeof(PlayerSpriteAnimator)
                .GetField("idleFramesPerSecond", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(field);
            Assert.AreEqual(1.5f, (float)field.GetValue(animator));
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void ResolveDebugPreviewOffsetRaisesOnlyForcedJumpPreview()
    {
        var takeoff = PlayerSpriteAnimator.ResolveDebugPreviewOffset(
            true,
            PlayerSpriteAnimationState.Jump,
            0,
            8,
            1f);
        var apex = PlayerSpriteAnimator.ResolveDebugPreviewOffset(
            true,
            PlayerSpriteAnimationState.Jump,
            4,
            8,
            1f);
        var landing = PlayerSpriteAnimator.ResolveDebugPreviewOffset(
            true,
            PlayerSpriteAnimationState.Jump,
            7,
            8,
            1f);
        var normalJump = PlayerSpriteAnimator.ResolveDebugPreviewOffset(
            false,
            PlayerSpriteAnimationState.Jump,
            4,
            8,
            1f);
        var forcedIdle = PlayerSpriteAnimator.ResolveDebugPreviewOffset(
            true,
            PlayerSpriteAnimationState.Idle,
            4,
            8,
            1f);

        Assert.AreEqual(Vector3.zero, takeoff);
        Assert.Greater(apex.y, 0.9f);
        Assert.AreEqual(Vector3.zero, landing);
        Assert.AreEqual(Vector3.zero, normalJump);
        Assert.AreEqual(Vector3.zero, forcedIdle);
    }

    [Test]
    public void TryResolveDebugStateOverrideReadsJumpQuery()
    {
        var found = PlayerSpriteAnimator.TryResolveDebugStateOverride(
            "http://127.0.0.1:4188/?v=jump-preview&debugAnimation=jump",
            out var state);

        Assert.IsTrue(found);
        Assert.AreEqual(PlayerSpriteAnimationState.Jump, state);
    }

    [Test]
    public void TryResolveDebugStateOverrideIgnoresUnknownQuery()
    {
        var found = PlayerSpriteAnimator.TryResolveDebugStateOverride(
            "http://127.0.0.1:4188/?v=walk-preview&debugAnimation=roll",
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
    public void DeveloperSpriteSheetUsesEightByThreeFrameGrid()
    {
        var texture = Resources.Load<Texture2D>("Characters/developer-spritesheet");

        Assert.IsNotNull(texture);
        Assert.AreEqual(0, texture.width % PlayerSpriteAnimator.AnimationColumns);
        Assert.AreEqual(0, texture.height % PlayerSpriteAnimator.AnimationRows);
        Assert.AreEqual(2560, texture.width);
        Assert.AreEqual(1260, texture.height);
    }

    [Test]
    public void DeveloperSpriteSheetDoesNotBleedIdleFeetIntoWalkCells()
    {
        var texture = Resources.Load<Texture2D>("Characters/developer-spritesheet");

        Assert.IsNotNull(texture);
        var pixels = texture.GetPixels32();
        var frameWidth = texture.width / PlayerSpriteAnimator.AnimationColumns;
        var frameHeight = texture.height / PlayerSpriteAnimator.AnimationRows;
        var walkRowBottom = frameHeight * (PlayerSpriteAnimator.AnimationRows - (int)PlayerSpriteAnimationState.Walk - 1);
        const int topBand = 32;

        for (var column = 0; column < PlayerSpriteAnimator.AnimationColumns; column++)
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
    public void DeveloperSpriteSheetKeepsJumpPoseProportionsStable()
    {
        var texture = Resources.Load<Texture2D>("Characters/developer-spritesheet");

        Assert.IsNotNull(texture);
        var walkBounds = ResolveFrameAlphaBounds(texture, PlayerSpriteAnimationState.Walk, 0);

        for (var column = 0; column < PlayerSpriteAnimator.AnimationColumns; column++)
        {
            var jumpBounds = ResolveFrameAlphaBounds(texture, PlayerSpriteAnimationState.Jump, column);

            Assert.GreaterOrEqual(
                jumpBounds.height,
                walkBounds.height - MaxJumpHeightLoss,
                $"Jump frame {column} collapses too far compared to the walking pose.");
            Assert.LessOrEqual(
                Mathf.Abs(jumpBounds.width - walkBounds.width),
                36,
                $"Jump frame {column} changes width too far compared to the walking pose.");
        }
    }

    [Test]
    public void DeveloperSpriteSheetKeepsLoopFrameHeightsStable()
    {
        var texture = Resources.Load<Texture2D>("Characters/developer-spritesheet");

        Assert.IsNotNull(texture);
        Assert.LessOrEqual(ResolveFrameHeightSpread(texture, PlayerSpriteAnimationState.Idle), 4);
        Assert.LessOrEqual(ResolveFrameHeightSpread(texture, PlayerSpriteAnimationState.Walk), 4);
    }

    [Test]
    public void DeveloperSpriteSheetUsesSmoothJumpPoseTransition()
    {
        var texture = Resources.Load<Texture2D>("Characters/developer-spritesheet");

        Assert.IsNotNull(texture);
        var walkFirst = ResolveFrameAlphaBounds(texture, PlayerSpriteAnimationState.Walk, 0);
        var jumpFirst = ResolveFrameAlphaBounds(texture, PlayerSpriteAnimationState.Jump, 0);
        var jumpLast = ResolveFrameAlphaBounds(texture, PlayerSpriteAnimationState.Jump, PlayerSpriteAnimator.AnimationColumns - 1);

        Assert.Greater(jumpFirst.height, walkFirst.height * 0.9f);
        Assert.GreaterOrEqual(jumpLast.height, walkFirst.height - MaxJumpHeightLoss);
        Assert.LessOrEqual(ResolveMaxAdjacentFrameHeightDelta(texture, PlayerSpriteAnimationState.Jump), 48);
    }

    [Test]
    public void DeveloperSpriteSheetKeepsJumpFootContactStable()
    {
        var texture = Resources.Load<Texture2D>("Characters/developer-spritesheet");

        Assert.IsNotNull(texture);
        var minContactX = float.MaxValue;
        var maxContactX = float.MinValue;
        for (var column = 0; column < PlayerSpriteAnimator.AnimationColumns; column++)
        {
            var contactX = ResolveFrameContactCenterX(texture, PlayerSpriteAnimationState.Jump, column);
            minContactX = Mathf.Min(minContactX, contactX);
            maxContactX = Mathf.Max(maxContactX, contactX);
        }

        Assert.LessOrEqual(maxContactX - minContactX, 36f);
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

    private static int ResolveFrameHeightSpread(Texture2D texture, PlayerSpriteAnimationState state)
    {
        var minHeight = int.MaxValue;
        var maxHeight = 0;

        for (var column = 0; column < PlayerSpriteAnimator.AnimationColumns; column++)
        {
            var height = ResolveFrameAlphaBounds(texture, state, column).height;
            minHeight = Mathf.Min(minHeight, height);
            maxHeight = Mathf.Max(maxHeight, height);
        }

        return maxHeight - minHeight;
    }

    private static int ResolveMaxAdjacentFrameHeightDelta(Texture2D texture, PlayerSpriteAnimationState state)
    {
        var maxDelta = 0;
        var previousHeight = ResolveFrameAlphaBounds(texture, state, 0).height;
        for (var column = 1; column < PlayerSpriteAnimator.AnimationColumns; column++)
        {
            var height = ResolveFrameAlphaBounds(texture, state, column).height;
            maxDelta = Mathf.Max(maxDelta, Mathf.Abs(height - previousHeight));
            previousHeight = height;
        }

        return maxDelta;
    }

    private static RectInt ResolveFrameAlphaBounds(Texture2D texture, PlayerSpriteAnimationState state, int column)
    {
        var pixels = texture.GetPixels32();
        var frameWidth = texture.width / PlayerSpriteAnimator.AnimationColumns;
        var frameHeight = texture.height / PlayerSpriteAnimator.AnimationRows;
        var left = column * frameWidth;
        var rowBottom = frameHeight * (PlayerSpriteAnimator.AnimationRows - (int)state - 1);
        var minX = left + frameWidth;
        var maxX = left - 1;
        var minY = rowBottom + frameHeight;
        var maxY = rowBottom - 1;

        for (var y = rowBottom; y < rowBottom + frameHeight; y++)
        {
            for (var x = left; x < left + frameWidth; x++)
            {
                if (pixels[y * texture.width + x].a == 0)
                {
                    continue;
                }

                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }
        }

        return maxX < minX || maxY < minY
            ? new RectInt(left, rowBottom, 0, 0)
            : new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private static float ResolveFrameContactCenterX(Texture2D texture, PlayerSpriteAnimationState state, int column)
    {
        var pixels = texture.GetPixels32();
        var frameWidth = texture.width / PlayerSpriteAnimator.AnimationColumns;
        var left = column * frameWidth;
        var bounds = ResolveFrameAlphaBounds(texture, state, column);
        var minX = left + frameWidth;
        var maxX = left - 1;

        for (var x = bounds.xMin; x < bounds.xMax; x++)
        {
            if (pixels[bounds.yMin * texture.width + x].a == 0)
            {
                continue;
            }

            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
        }

        return maxX < minX
            ? left + frameWidth * 0.5f
            : (minX + maxX + 1f) * 0.5f - left;
    }

}
