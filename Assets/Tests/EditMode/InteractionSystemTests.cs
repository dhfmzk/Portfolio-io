using NUnit.Framework;
using Portfolio;
using UnityEngine;

public class InteractionSystemTests
{
    [Test]
    public void SelectNearestReturnsClosestActiveExhibit()
    {
        var origin = Vector3.zero;
        var near = CreateExhibit("near", new Vector3(1f, 0f, 0f));
        var far = CreateExhibit("far", new Vector3(5f, 0f, 0f));

        var selected = InteractionSystem.SelectNearest(origin, new[] { far, near });

        Assert.AreEqual(near, selected);
    }

    [Test]
    public void SelectNearestIgnoresNullEntries()
    {
        var exhibit = CreateExhibit("only", new Vector3(2f, 0f, 0f));

        var selected = InteractionSystem.SelectNearest(Vector3.zero, new InteractableExhibit[] { null, exhibit });

        Assert.AreEqual(exhibit, selected);
    }

    private static InteractableExhibit CreateExhibit(string name, Vector3 position)
    {
        var gameObject = new GameObject(name);
        gameObject.transform.position = position;
        return gameObject.AddComponent<InteractableExhibit>();
    }
}
