using NUnit.Framework;
using Portfolio;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class PortfolioPanelControllerTests
{
    [Test]
    public void ShowCreatesOneButtonPerVisibleLink()
    {
        var root = new GameObject("Panel");
        var controller = root.AddComponent<PortfolioPanelController>();
        var linkRoot = new GameObject("Link Root").transform;
        linkRoot.SetParent(root.transform, false);
        var template = CreateButtonTemplate(linkRoot);
        controller.Configure(
            root,
            CreateText("Category"),
            CreateText("Title"),
            CreateText("Subtitle"),
            CreateText("Body"),
            CreateText("Stack"),
            CreateText("Links"),
            linkRoot,
            template);
        var data = ScriptableObject.CreateInstance<PortfolioExhibitData>();
        data.Links = new[]
        {
            new PortfolioLink("GitHub", "https://github.com/dhfmzk"),
            new PortfolioLink("Playable Build", "https://dhfmzk.github.io/Portfolio-io/")
        };

        controller.Show(data);

        Assert.AreEqual(2, controller.RenderedLinkCount);
    }

    private static TextMeshProUGUI CreateText(string name)
    {
        return new GameObject(name).AddComponent<TextMeshProUGUI>();
    }

    private static Button CreateButtonTemplate(Transform parent)
    {
        var buttonObject = new GameObject("Button Template");
        buttonObject.transform.SetParent(parent, false);
        var button = buttonObject.AddComponent<Button>();
        var label = CreateText("Button Label");
        label.transform.SetParent(buttonObject.transform, false);
        return button;
    }
}
