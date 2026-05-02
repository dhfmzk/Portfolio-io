using NUnit.Framework;
using Portfolio;

public class PortfolioPanelViewModelTests
{
    [Test]
    public void FromDataCopiesReadableFieldsAndTags()
    {
        var data = CreateExhibit();

        var viewModel = PortfolioPanelViewModel.FromData(data);

        Assert.AreEqual("project-01", viewModel.Id);
        Assert.AreEqual("Project", viewModel.CategoryLabel);
        Assert.AreEqual("WebGL Portfolio", viewModel.Title);
        Assert.AreEqual("Playable portfolio", viewModel.Subtitle);
        Assert.AreEqual("A side-scrolling gallery.", viewModel.Body);
        CollectionAssert.AreEqual(new[] { "Unity", "WebGL" }, viewModel.StackTags);
        Assert.AreEqual("GitHub", viewModel.Links[0].Label);
        Assert.AreEqual("https://github.com/dhfmzk/Portfolio-io", viewModel.Links[0].Url);
    }

    [Test]
    public void FromDataReturnsEmptyViewModelForNullData()
    {
        var viewModel = PortfolioPanelViewModel.FromData(null);

        Assert.AreEqual(string.Empty, viewModel.Id);
        Assert.AreEqual("Unknown", viewModel.CategoryLabel);
        Assert.AreEqual(string.Empty, viewModel.Title);
        Assert.AreEqual(0, viewModel.StackTags.Length);
        Assert.AreEqual(0, viewModel.Links.Length);
    }

    [Test]
    public void StackSummaryJoinsTagsForCompactDisplay()
    {
        var data = UnityEngine.ScriptableObject.CreateInstance<PortfolioExhibitData>();
        data.StackTags = new[] { "Unity", "C#", "WebGL" };

        var viewModel = PortfolioPanelViewModel.FromData(data);

        Assert.AreEqual("Unity / C# / WebGL", viewModel.StackSummary);
    }

    private static PortfolioExhibitData CreateExhibit()
    {
        var data = UnityEngine.ScriptableObject.CreateInstance<PortfolioExhibitData>();
        data.Id = "project-01";
        data.Category = PortfolioExhibitCategory.Project;
        data.Title = "WebGL Portfolio";
        data.Subtitle = "Playable portfolio";
        data.Body = "A side-scrolling gallery.";
        data.StackTags = new[] { "Unity", "WebGL" };
        data.Links = new[]
        {
            new PortfolioLink("GitHub", "https://github.com/dhfmzk/Portfolio-io")
        };
        data.DisplayOrder = 10;
        return data;
    }
}
