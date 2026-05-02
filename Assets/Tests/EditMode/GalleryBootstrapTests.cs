using Bootstrap;
using NUnit.Framework;
using Portfolio;

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
}
