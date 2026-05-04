using Bootstrap;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Portfolio;
using Input;
using Player;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GalleryBootstrapTests
{
    [Test]
    public void CreateDraftExhibitDataBuildsPlayablePortfolioEntry()
    {
        var data = GalleryBootstrap.CreateDraftExhibitData(
            "Project",
            "Player 1 Portfolio",
            "A WebGL gallery.",
            PortfolioExhibitCategory.Project);

        Assert.AreEqual("player-1-portfolio", data.Id);
        Assert.AreEqual(PortfolioExhibitCategory.Project, data.Category);
        Assert.AreEqual("Player 1 Portfolio", data.Title);
        Assert.AreEqual("Project", data.Subtitle);
        Assert.AreEqual("A WebGL gallery.", data.Body);
        CollectionAssert.AreEqual(new[] { "Unity", "WebGL" }, data.StackTags);
        Assert.AreEqual("Repository", data.Links[0].Label);
    }

    [Test]
    public void CreateDefaultExhibitDataBuildsFullMvpRoute()
    {
        var exhibits = GalleryBootstrap.CreateDefaultExhibitData();

        Assert.AreEqual(6, exhibits.Length);
        CollectionAssert.AreEqual(
            new[]
            {
                PortfolioExhibitCategory.About,
                PortfolioExhibitCategory.Project,
                PortfolioExhibitCategory.Project,
                PortfolioExhibitCategory.Project,
                PortfolioExhibitCategory.Skill,
                PortfolioExhibitCategory.Contact
            },
            exhibits.Select(exhibit => exhibit.Category).ToArray());
        CollectionAssert.Contains(exhibits.Select(exhibit => exhibit.Title), "Player 1 Portfolio");
        CollectionAssert.Contains(exhibits.Select(exhibit => exhibit.Title), "Skill Wall");
        CollectionAssert.Contains(exhibits.Select(exhibit => exhibit.Title), "Contact Gate");
    }

    [Test]
    public void CreateRuntimePlayerBuildsRequired2DComponents()
    {
        var player = GalleryBootstrap.CreateRuntimePlayer();

        try
        {
            Assert.IsNotNull(player);
            Assert.IsNotNull(player.GetComponent<BoxCollider2D>());
            Assert.IsNotNull(player.GetComponent<Rigidbody2D>());
            Assert.IsNotNull(player.GetComponent<PlayerController>());
            Assert.IsNotNull(player.GetComponent<PlayerInputReader>());
            Assert.IsNotNull(player.GetComponent<PlayerSpriteAnimator>());
            Assert.IsNotNull(player.GetComponent<InteractionSystem>());
            Assert.AreEqual(new Vector3(-11.5f, 0f, 0f), player.transform.position);
            Assert.AreEqual(Vector3.one, player.transform.localScale);
            Assert.AreEqual(1f, player.GetComponent<SpriteRenderer>().sprite.bounds.size.x, 0.001f);
            var collider = player.GetComponent<BoxCollider2D>();
            Assert.AreEqual(new Vector2(0f, 0.55f), collider.offset);
            Assert.AreEqual(new Vector2(0.55f, 1.1f), collider.size);
        }
        finally
        {
            Object.DestroyImmediate(player);
        }
    }

    [Test]
    public void CreateRuntimePlayerUsesGroundedJumpTuning()
    {
        var player = GalleryBootstrap.CreateRuntimePlayer();

        try
        {
            var body = player.GetComponent<Rigidbody2D>();
            var controller = player.GetComponent<PlayerController>();
            var jumpVelocityField = typeof(PlayerController)
                .GetField("jumpVelocity", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(jumpVelocityField);
            var jumpVelocity = (float)jumpVelocityField.GetValue(controller);
            var apexHeight = jumpVelocity * jumpVelocity / (2f * Mathf.Abs(Physics2D.gravity.y) * body.gravityScale);

            Assert.AreEqual(1.5f, body.gravityScale);
            Assert.GreaterOrEqual(apexHeight, 0.9f);
            Assert.LessOrEqual(apexHeight, 1.2f);
        }
        finally
        {
            Object.DestroyImmediate(player);
        }
    }

    [Test]
    public void CreateBackdropPlacesFloorAccentBelowPlayerFeet()
    {
        typeof(GalleryBootstrap)
            .GetMethod("CreateBackdrop", BindingFlags.Static | BindingFlags.NonPublic)
            ?.Invoke(null, null);

        try
        {
            var accent = GameObject.Find("Gallery Floor Accent");

            Assert.IsNotNull(accent);
            Assert.AreEqual(
                0f,
                accent.transform.position.y + accent.transform.localScale.y * 0.5f,
                0.001f);
            Assert.Greater(accent.GetComponent<SpriteRenderer>().sortingOrder, 0);
        }
        finally
        {
            DestroyNamed("Gallery Backdrop");
            DestroyNamed("Gallery Floor Fill");
            DestroyNamed("Gallery Floor Accent");
        }
    }

    [Test]
    public void PrepareSceneForRuntimeBootstrapDisablesExistingSceneCanvases()
    {
        var sceneCanvasObject = new GameObject("Canvas");
        sceneCanvasObject.AddComponent<Canvas>();
        var cameraObject = new GameObject("Main Camera");
        cameraObject.AddComponent<Camera>();

        try
        {
            GalleryBootstrap.PrepareSceneForRuntimeBootstrap();

            Assert.IsFalse(sceneCanvasObject.activeSelf);
            Assert.IsTrue(cameraObject.activeSelf);
        }
        finally
        {
            Object.DestroyImmediate(sceneCanvasObject);
            Object.DestroyImmediate(cameraObject);
        }
    }

    [Test]
    public void CreateRuntimeControlIndicatorBuildsDedicatedHintCanvas()
    {
        var indicator = GalleryBootstrap.CreateRuntimeControlIndicator();

        try
        {
            Assert.IsNotNull(indicator);
            Assert.IsNotNull(indicator.GetComponent<Canvas>());
            Assert.IsNotNull(indicator.GetComponent<ControlIndicatorUI>());
            var firstKeyImage = indicator.transform.Find("Control Hints/Left")?.GetComponent<Image>();
            Assert.IsNotNull(firstKeyImage);
            Assert.IsNotNull(firstKeyImage.sprite);
            Assert.AreEqual("BUTTON_BASE_UP", firstKeyImage.sprite.name);
            Assert.IsNotNull(indicator.GetComponent<GraphicRaycaster>());
            Assert.IsNotNull(firstKeyImage.GetComponent<VirtualControlButton>());
            Assert.IsInstanceOf<IPointerClickHandler>(firstKeyImage.GetComponent<VirtualControlButton>());
        }
        finally
        {
            Object.DestroyImmediate(indicator);
        }
    }

    [Test]
    public void CreateRuntimeInteractionPromptAnchorsHintNearBottom()
    {
        var prompt = GalleryBootstrap.CreateRuntimeInteractionPrompt();

        try
        {
            var text = prompt.GetComponentInChildren<TextMeshProUGUI>();

            Assert.IsNotNull(text);
            Assert.AreEqual(new Vector2(0.5f, 0f), text.rectTransform.anchorMin);
            Assert.AreEqual(new Vector2(0.5f, 0f), text.rectTransform.anchorMax);
            Assert.Greater(text.rectTransform.anchoredPosition.y, 70f);
        }
        finally
        {
            Object.DestroyImmediate(prompt.gameObject);
        }
    }

    private static void DestroyNamed(string objectName)
    {
        var found = GameObject.Find(objectName);
        if (found != null)
        {
            Object.DestroyImmediate(found);
        }
    }
}
