using NUnit.Framework;
using Portfolio;
using UI;
using UnityEngine;

public class InteractionPromptUITests
{
    [Test]
    public void ResolvePromptTextInvitesExplorationWithoutFocusedExhibit()
    {
        var prompt = InteractionPromptUI.ResolvePromptText(null);

        Assert.AreEqual("Explore the gallery", prompt);
    }

    [Test]
    public void ResolvePromptTextNamesFocusedExhibit()
    {
        var exhibitObject = new GameObject("Project Exhibit");
        var exhibit = exhibitObject.AddComponent<InteractableExhibit>();
        var data = ScriptableObject.CreateInstance<PortfolioExhibitData>();
        data.Title = "Player 1 Portfolio";
        exhibit.SetData(data);

        var prompt = InteractionPromptUI.ResolvePromptText(exhibit);

        Assert.AreEqual("Press E - Player 1 Portfolio", prompt);
    }
}
