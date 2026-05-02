# Player 1 Portfolio MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first playable Unity WebGL portfolio vertical slice: movement, camera follow, interactable exhibits, readable content panels, and regenerated GitHub Pages build output.

**Architecture:** Keep the first playable milestone code-driven and low overhead. Runtime scripts create or wire the minimal gallery world, while data and interaction logic stay in small testable classes. Scene polish can move into prefabs after the core loop is proven.

**Tech Stack:** Unity 2022.3.62f3, C# MonoBehaviours, Unity 2D physics, uGUI/TextMesh Pro, Unity Test Framework, local WebGL build to `docs/`.

---

## File Structure

Create these production files:

- `Assets/Scripts/Portfolio/PortfolioExhibitCategory.cs`: enum for exhibit categories.
- `Assets/Scripts/Portfolio/PortfolioLink.cs`: serializable link label/url pair.
- `Assets/Scripts/Portfolio/PortfolioExhibitData.cs`: ScriptableObject content model.
- `Assets/Scripts/Portfolio/PortfolioPanelViewModel.cs`: pure formatting model for UI rendering.
- `Assets/Scripts/Portfolio/InteractableExhibit.cs`: trigger/metadata component for exhibits.
- `Assets/Scripts/Portfolio/InteractionSystem.cs`: nearest-exhibit selection and panel open/close coordination.
- `Assets/Scripts/UI/PortfolioPanelController.cs`: renders portfolio content into TMP text and buttons.
- `Assets/Scripts/Input/PlayerInputReader.cs`: reads keyboard state.
- `Assets/Scripts/Player/PlayerController.cs`: applies movement/jump to `Rigidbody2D`.
- `Assets/Scripts/Camera/CameraFollow.cs`: follows the player with optional bounds.
- `Assets/Scripts/Bootstrap/GalleryBootstrap.cs`: creates the first vertical-slice scene at runtime.

Modify these existing files:

- `Assets/Scripts/UI/ControlIndicatorUI.cs`: replace empty stub with key-state image/text toggling.
- `Assets/Scripts/Debug/DebugInfoUI.cs`: remove public-facing profanity and fix early FPS averaging.
- `README.md`: add a short current-MVP description once playable.

Create these tests:

- `Assets/Tests/EditMode/PortfolioPanelViewModelTests.cs`
- `Assets/Tests/EditMode/InteractionSystemTests.cs`
- `Assets/Tests/EditMode/PlayerControllerTests.cs`
- `Assets/Tests/EditMode/ControlIndicatorUITests.cs`

Verification commands:

```bash
"/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity" \
  -batchmode \
  -quit \
  -projectPath "$(pwd)" \
  -runTests \
  -testPlatform EditMode \
  -testResults TestResults/EditMode.xml
```

```bash
"/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity" \
  -batchmode \
  -quit \
  -projectPath "$(pwd)" \
  -executeMethod Portfolio.Build.WebGLPagesBuild.Build
```

---

### Task 1: Portfolio Content Model

**Files:**
- Create: `Assets/Scripts/Portfolio/PortfolioExhibitCategory.cs`
- Create: `Assets/Scripts/Portfolio/PortfolioLink.cs`
- Create: `Assets/Scripts/Portfolio/PortfolioExhibitData.cs`
- Create: `Assets/Scripts/Portfolio/PortfolioPanelViewModel.cs`
- Test: `Assets/Tests/EditMode/PortfolioPanelViewModelTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `Assets/Tests/EditMode/PortfolioPanelViewModelTests.cs`:

```csharp
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
```

- [ ] **Step 2: Run the tests and verify RED**

Run the EditMode test command above.

Expected: compile failure because `PortfolioExhibitData`, `PortfolioExhibitCategory`, `PortfolioLink`, and `PortfolioPanelViewModel` do not exist.

- [ ] **Step 3: Add the content model implementation**

Create `Assets/Scripts/Portfolio/PortfolioExhibitCategory.cs`:

```csharp
namespace Portfolio
{
    public enum PortfolioExhibitCategory
    {
        Unknown = 0,
        About = 1,
        Project = 2,
        Skill = 3,
        Contact = 4
    }
}
```

Create `Assets/Scripts/Portfolio/PortfolioLink.cs`:

```csharp
using System;

namespace Portfolio
{
    [Serializable]
    public class PortfolioLink
    {
        public string Label;
        public string Url;

        public PortfolioLink()
        {
            Label = string.Empty;
            Url = string.Empty;
        }

        public PortfolioLink(string label, string url)
        {
            Label = label ?? string.Empty;
            Url = url ?? string.Empty;
        }
    }
}
```

Create `Assets/Scripts/Portfolio/PortfolioExhibitData.cs`:

```csharp
using UnityEngine;

namespace Portfolio
{
    [CreateAssetMenu(menuName = "Portfolio/Exhibit Data", fileName = "PortfolioExhibit")]
    public class PortfolioExhibitData : ScriptableObject
    {
        public string Id = string.Empty;
        public PortfolioExhibitCategory Category = PortfolioExhibitCategory.Unknown;
        public string Title = string.Empty;
        public string Subtitle = string.Empty;
        [TextArea(3, 8)] public string Body = string.Empty;
        public string[] StackTags = new string[0];
        public PortfolioLink[] Links = new PortfolioLink[0];
        public int DisplayOrder;
    }
}
```

Create `Assets/Scripts/Portfolio/PortfolioPanelViewModel.cs`:

```csharp
using System;
using System.Linq;

namespace Portfolio
{
    public readonly struct PortfolioPanelViewModel
    {
        public readonly string Id;
        public readonly string CategoryLabel;
        public readonly string Title;
        public readonly string Subtitle;
        public readonly string Body;
        public readonly string[] StackTags;
        public readonly PortfolioLink[] Links;

        private PortfolioPanelViewModel(
            string id,
            string categoryLabel,
            string title,
            string subtitle,
            string body,
            string[] stackTags,
            PortfolioLink[] links)
        {
            Id = id;
            CategoryLabel = categoryLabel;
            Title = title;
            Subtitle = subtitle;
            Body = body;
            StackTags = stackTags;
            Links = links;
        }

        public static PortfolioPanelViewModel FromData(PortfolioExhibitData data)
        {
            if (data == null)
            {
                return new PortfolioPanelViewModel(
                    string.Empty,
                    "Unknown",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    Array.Empty<string>(),
                    Array.Empty<PortfolioLink>());
            }

            var stackTags = data.StackTags?.Where(tag => !string.IsNullOrWhiteSpace(tag)).ToArray()
                ?? Array.Empty<string>();
            var links = data.Links?.Where(link => link != null && !string.IsNullOrWhiteSpace(link.Url)).ToArray()
                ?? Array.Empty<PortfolioLink>();

            return new PortfolioPanelViewModel(
                data.Id ?? string.Empty,
                data.Category.ToString(),
                data.Title ?? string.Empty,
                data.Subtitle ?? string.Empty,
                data.Body ?? string.Empty,
                stackTags,
                links);
        }
    }
}
```

- [ ] **Step 4: Run the tests and verify GREEN**

Run the EditMode test command.

Expected: `PortfolioPanelViewModelTests` passes.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Portfolio Assets/Tests/EditMode/PortfolioPanelViewModelTests.cs
git commit -m "feat: add portfolio exhibit content model"
```

---

### Task 2: Player Movement Core

**Files:**
- Create: `Assets/Scripts/Player/PlayerController.cs`
- Create: `Assets/Scripts/Input/PlayerInputReader.cs`
- Test: `Assets/Tests/EditMode/PlayerControllerTests.cs`

- [ ] **Step 1: Write the failing movement tests**

Create `Assets/Tests/EditMode/PlayerControllerTests.cs`:

```csharp
using NUnit.Framework;
using Player;
using UnityEngine;

public class PlayerControllerTests
{
    [Test]
    public void ResolveHorizontalVelocityKeepsCurrentYVelocity()
    {
        var velocity = PlayerController.ResolveHorizontalVelocity(new Vector2(0f, -4f), 1f, 6f);

        Assert.AreEqual(6f, velocity.x);
        Assert.AreEqual(-4f, velocity.y);
    }

    [Test]
    public void ResolveJumpVelocityOnlyChangesYWhenGroundedAndRequested()
    {
        var velocity = PlayerController.ResolveJumpVelocity(new Vector2(3f, -1f), true, true, 9f);

        Assert.AreEqual(3f, velocity.x);
        Assert.AreEqual(9f, velocity.y);
    }

    [Test]
    public void ResolveJumpVelocityDoesNothingWhenAirborne()
    {
        var velocity = PlayerController.ResolveJumpVelocity(new Vector2(3f, -1f), false, true, 9f);

        Assert.AreEqual(3f, velocity.x);
        Assert.AreEqual(-1f, velocity.y);
    }
}
```

- [ ] **Step 2: Run the tests and verify RED**

Run the EditMode test command.

Expected: compile failure because `PlayerController` does not exist.

- [ ] **Step 3: Add player movement scripts**

Create `Assets/Scripts/Player/PlayerController.cs`:

```csharp
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpVelocity = 9f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.12f;
        [SerializeField] private LayerMask groundMask = ~0;

        private Rigidbody2D _body;
        private float _moveInput;
        private bool _jumpRequested;
        private bool _movementPaused;

        public void SetMoveInput(float moveInput)
        {
            _moveInput = Mathf.Clamp(moveInput, -1f, 1f);
        }

        public void RequestJump()
        {
            _jumpRequested = true;
        }

        public void SetMovementPaused(bool paused)
        {
            _movementPaused = paused;
            if (paused && _body != null)
            {
                _body.velocity = new Vector2(0f, _body.velocity.y);
            }
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (_movementPaused)
            {
                _jumpRequested = false;
                return;
            }

            var velocity = ResolveHorizontalVelocity(_body.velocity, _moveInput, moveSpeed);
            velocity = ResolveJumpVelocity(velocity, IsGrounded(), _jumpRequested, jumpVelocity);
            _body.velocity = velocity;
            _jumpRequested = false;
        }

        private bool IsGrounded()
        {
            if (groundCheck == null)
            {
                return Mathf.Abs(_body.velocity.y) < 0.01f;
            }

            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask) != null;
        }

        public static Vector2 ResolveHorizontalVelocity(Vector2 currentVelocity, float moveInput, float speed)
        {
            return new Vector2(Mathf.Clamp(moveInput, -1f, 1f) * speed, currentVelocity.y);
        }

        public static Vector2 ResolveJumpVelocity(Vector2 currentVelocity, bool grounded, bool jumpRequested, float jumpSpeed)
        {
            return grounded && jumpRequested
                ? new Vector2(currentVelocity.x, jumpSpeed)
                : currentVelocity;
        }
    }
}
```

Create `Assets/Scripts/Input/PlayerInputReader.cs`:

```csharp
using Player;
using UnityEngine;

namespace Input
{
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;

        public bool InteractionPressedThisFrame { get; private set; }
        public bool ClosePressedThisFrame { get; private set; }

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }
        }

        private void Update()
        {
            var move = 0f;
            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow))
            {
                move -= 1f;
            }

            if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow))
            {
                move += 1f;
            }

            playerController?.SetMoveInput(move);

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                playerController?.RequestJump();
            }

            InteractionPressedThisFrame = UnityEngine.Input.GetKeyDown(KeyCode.E);
            ClosePressedThisFrame = UnityEngine.Input.GetKeyDown(KeyCode.E) || UnityEngine.Input.GetKeyDown(KeyCode.Escape);
        }
    }
}
```

- [ ] **Step 4: Run the tests and verify GREEN**

Run the EditMode test command.

Expected: `PlayerControllerTests` passes.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Player Assets/Scripts/Input Assets/Tests/EditMode/PlayerControllerTests.cs
git commit -m "feat: add player movement core"
```

---

### Task 3: Interactable Exhibits And Selection

**Files:**
- Create: `Assets/Scripts/Portfolio/InteractableExhibit.cs`
- Create: `Assets/Scripts/Portfolio/InteractionSystem.cs`
- Test: `Assets/Tests/EditMode/InteractionSystemTests.cs`

- [ ] **Step 1: Write the failing interaction tests**

Create `Assets/Tests/EditMode/InteractionSystemTests.cs`:

```csharp
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
```

- [ ] **Step 2: Run the tests and verify RED**

Run the EditMode test command.

Expected: compile failure because `InteractableExhibit` and `InteractionSystem` do not exist.

- [ ] **Step 3: Add interactable exhibit scripts**

Create `Assets/Scripts/Portfolio/InteractableExhibit.cs`:

```csharp
using UnityEngine;

namespace Portfolio
{
    public class InteractableExhibit : MonoBehaviour
    {
        [SerializeField] private PortfolioExhibitData data;
        [SerializeField] private GameObject highlight;

        public PortfolioExhibitData Data => data;

        public void SetData(PortfolioExhibitData exhibitData)
        {
            data = exhibitData;
        }

        public void SetHighlighted(bool highlighted)
        {
            if (highlight != null)
            {
                highlight.SetActive(highlighted);
            }
        }
    }
}
```

Create `Assets/Scripts/Portfolio/InteractionSystem.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using Input;
using Player;
using UI;
using UnityEngine;

namespace Portfolio
{
    public class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PortfolioPanelController panelController;
        [SerializeField] private float interactionRadius = 2.25f;

        private InteractableExhibit _current;

        public void Configure(
            Transform playerTransform,
            PlayerInputReader reader,
            PlayerController controller,
            PortfolioPanelController panel)
        {
            player = playerTransform;
            inputReader = reader;
            playerController = controller;
            panelController = panel;
        }

        private void Update()
        {
            if (panelController != null && panelController.IsOpen)
            {
                if (inputReader != null && inputReader.ClosePressedThisFrame)
                {
                    ClosePanel();
                }

                return;
            }

            var origin = player != null ? player.position : transform.position;
            var exhibits = FindObjectsOfType<InteractableExhibit>()
                .Where(exhibit => Vector3.Distance(exhibit.transform.position, origin) <= interactionRadius)
                .ToArray();

            _current = SelectNearest(origin, exhibits);
            foreach (var exhibit in FindObjectsOfType<InteractableExhibit>())
            {
                exhibit.SetHighlighted(exhibit == _current);
            }

            if (_current != null && inputReader != null && inputReader.InteractionPressedThisFrame)
            {
                OpenPanel(_current);
            }
        }

        private void OpenPanel(InteractableExhibit exhibit)
        {
            playerController?.SetMovementPaused(true);
            panelController?.Show(exhibit.Data);
        }

        private void ClosePanel()
        {
            panelController?.Hide();
            playerController?.SetMovementPaused(false);
        }

        public static InteractableExhibit SelectNearest(Vector3 origin, IEnumerable<InteractableExhibit> exhibits)
        {
            return exhibits?
                .Where(exhibit => exhibit != null)
                .OrderBy(exhibit => Vector3.SqrMagnitude(exhibit.transform.position - origin))
                .FirstOrDefault();
        }
    }
}
```

- [ ] **Step 4: Run the tests and verify GREEN**

Run the EditMode test command.

Expected: `InteractionSystemTests` passes.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Portfolio/InteractableExhibit.cs Assets/Scripts/Portfolio/InteractionSystem.cs Assets/Tests/EditMode/InteractionSystemTests.cs
git commit -m "feat: add portfolio exhibit interactions"
```

---

### Task 4: Portfolio Panel UI Rendering

**Files:**
- Create: `Assets/Scripts/UI/PortfolioPanelController.cs`
- Test: extend `Assets/Tests/EditMode/PortfolioPanelViewModelTests.cs`

- [ ] **Step 1: Add a failing formatting test**

Append this test to `PortfolioPanelViewModelTests`:

```csharp
[Test]
public void StackSummaryJoinsTagsForCompactDisplay()
{
    var data = UnityEngine.ScriptableObject.CreateInstance<PortfolioExhibitData>();
    data.StackTags = new[] { "Unity", "C#", "WebGL" };

    var viewModel = PortfolioPanelViewModel.FromData(data);

    Assert.AreEqual("Unity / C# / WebGL", viewModel.StackSummary);
}
```

- [ ] **Step 2: Run the tests and verify RED**

Run the EditMode test command.

Expected: compile failure because `PortfolioPanelViewModel.StackSummary` does not exist.

- [ ] **Step 3: Add `StackSummary` and panel rendering**

Modify `PortfolioPanelViewModel` to add:

```csharp
public string StackSummary => StackTags.Length == 0 ? string.Empty : string.Join(" / ", StackTags);
```

Create `Assets/Scripts/UI/PortfolioPanelController.cs`:

```csharp
using Portfolio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PortfolioPanelController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI categoryText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private TextMeshProUGUI stackText;
        [SerializeField] private Button closeButton;

        public bool IsOpen => root != null && root.activeSelf;

        private void Awake()
        {
            if (root == null)
            {
                root = gameObject;
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            Hide();
        }

        public void Show(PortfolioExhibitData data)
        {
            var viewModel = PortfolioPanelViewModel.FromData(data);
            SetText(categoryText, viewModel.CategoryLabel);
            SetText(titleText, viewModel.Title);
            SetText(subtitleText, viewModel.Subtitle);
            SetText(bodyText, viewModel.Body);
            SetText(stackText, viewModel.StackSummary);

            if (root != null)
            {
                root.SetActive(true);
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private static void SetText(TextMeshProUGUI target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }

        public void Configure(
            GameObject panelRoot,
            TextMeshProUGUI category,
            TextMeshProUGUI title,
            TextMeshProUGUI subtitle,
            TextMeshProUGUI body,
            TextMeshProUGUI stack)
        {
            root = panelRoot;
            categoryText = category;
            titleText = title;
            subtitleText = subtitle;
            bodyText = body;
            stackText = stack;
            Hide();
        }
    }
}
```

- [ ] **Step 4: Run the tests and verify GREEN**

Run the EditMode test command.

Expected: `PortfolioPanelViewModelTests` passes.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/UI/PortfolioPanelController.cs Assets/Scripts/Portfolio/PortfolioPanelViewModel.cs Assets/Tests/EditMode/PortfolioPanelViewModelTests.cs
git commit -m "feat: render portfolio exhibit panels"
```

---

### Task 5: Camera Follow And Control Indicator

**Files:**
- Create: `Assets/Scripts/Camera/CameraFollow.cs`
- Modify: `Assets/Scripts/UI/ControlIndicatorUI.cs`
- Test: `Assets/Tests/EditMode/ControlIndicatorUITests.cs`

- [ ] **Step 1: Write the failing control indicator test**

Create `Assets/Tests/EditMode/ControlIndicatorUITests.cs`:

```csharp
using NUnit.Framework;
using UI;

public class ControlIndicatorUITests
{
    [Test]
    public void ResolveLabelColorUsesActiveColorWhenPressed()
    {
        var inactive = UnityEngine.Color.gray;
        var active = UnityEngine.Color.white;

        var resolved = ControlIndicatorUI.ResolveLabelColor(true, active, inactive);

        Assert.AreEqual(active, resolved);
    }

    [Test]
    public void ResolveLabelColorUsesInactiveColorWhenReleased()
    {
        var inactive = UnityEngine.Color.gray;
        var active = UnityEngine.Color.white;

        var resolved = ControlIndicatorUI.ResolveLabelColor(false, active, inactive);

        Assert.AreEqual(inactive, resolved);
    }
}
```

- [ ] **Step 2: Run the tests and verify RED**

Run the EditMode test command.

Expected: compile failure because `ControlIndicatorUI.ResolveLabelColor` does not exist.

- [ ] **Step 3: Replace the control indicator stub and add camera follow**

Replace `Assets/Scripts/UI/ControlIndicatorUI.cs`:

```csharp
using TMPro;
using UnityEngine;

namespace UI
{
    public class ControlIndicatorUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI leftText;
        [SerializeField] private TextMeshProUGUI rightText;
        [SerializeField] private TextMeshProUGUI jumpText;
        [SerializeField] private TextMeshProUGUI interactText;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        private void Update()
        {
            Apply(leftText, UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow));
            Apply(rightText, UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow));
            Apply(jumpText, UnityEngine.Input.GetKey(KeyCode.Space));
            Apply(interactText, UnityEngine.Input.GetKey(KeyCode.E));
        }

        private void Apply(TextMeshProUGUI target, bool pressed)
        {
            if (target != null)
            {
                target.color = ResolveLabelColor(pressed, activeColor, inactiveColor);
            }
        }

        public static Color ResolveLabelColor(bool pressed, Color active, Color inactive)
        {
            return pressed ? active : inactive;
        }
    }
}
```

Create `Assets/Scripts/Camera/CameraFollow.cs`:

```csharp
using UnityEngine;

namespace CameraSystem
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);
        [SerializeField] private float smoothTime = 0.12f;
        [SerializeField] private bool clampX;
        [SerializeField] private Vector2 xBounds = new Vector2(-20f, 20f);

        private Vector3 _velocity;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var desired = target.position + offset;
            if (clampX)
            {
                desired.x = Mathf.Clamp(desired.x, xBounds.x, xBounds.y);
            }

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }
    }
}
```

- [ ] **Step 4: Run the tests and verify GREEN**

Run the EditMode test command.

Expected: `ControlIndicatorUITests` passes.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/UI/ControlIndicatorUI.cs Assets/Scripts/Camera Assets/Tests/EditMode/ControlIndicatorUITests.cs
git commit -m "feat: add camera follow and control indicator"
```

---

### Task 6: Runtime Gallery Bootstrap

**Files:**
- Create: `Assets/Scripts/Bootstrap/GalleryBootstrap.cs`
- Modify: `README.md`

- [ ] **Step 1: Add the runtime bootstrap**

Create `Assets/Scripts/Bootstrap/GalleryBootstrap.cs`:

```csharp
using CameraSystem;
using Input;
using Player;
using Portfolio;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Bootstrap
{
    public class GalleryBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrapExists()
        {
            if (FindObjectOfType<GalleryBootstrap>() != null)
            {
                return;
            }

            new GameObject("Gallery Bootstrap").AddComponent<GalleryBootstrap>();
        }

        private void Start()
        {
            var player = CreatePlayer();
            CreateGround();
            CreateCamera(player.transform);
            var panel = CreatePortfolioPanel();
            CreateExhibit(new Vector3(-5f, 0.75f, 0f), "About", "Developer Profile", "I build compact, useful software with playful presentation.", PortfolioExhibitCategory.About, panel, player);
            CreateExhibit(new Vector3(0f, 0.75f, 0f), "Project", "Player 1 Portfolio", "A WebGL portfolio presented as a side-scrolling gallery.", PortfolioExhibitCategory.Project, panel, player);
            CreateExhibit(new Vector3(5f, 0.75f, 0f), "Contact", "Contact Gate", "GitHub: dhfmzk\nProject: Portfolio-io", PortfolioExhibitCategory.Contact, panel, player);
        }

        private static GameObject CreatePlayer()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Cube);
            player.name = "Player";
            player.transform.position = new Vector3(-8f, 1f, 0f);
            player.transform.localScale = new Vector3(0.8f, 1.2f, 1f);
            Object.Destroy(player.GetComponent<BoxCollider>());
            player.AddComponent<BoxCollider2D>();
            var body = player.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            var controller = player.AddComponent<PlayerController>();
            var input = player.AddComponent<PlayerInputReader>();
            var interaction = player.AddComponent<InteractionSystem>();
            interaction.Configure(player.transform, input, controller, null);
            return player;
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Gallery Ground";
            ground.transform.position = new Vector3(0f, -0.1f, 0f);
            ground.transform.localScale = new Vector3(24f, 0.2f, 1f);
            Object.Destroy(ground.GetComponent<BoxCollider>());
            ground.AddComponent<BoxCollider2D>();
            var body = ground.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
        }

        private static void CreateCamera(Transform target)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
            }

            camera.orthographic = true;
            camera.orthographicSize = 4.5f;
            camera.transform.position = new Vector3(target.position.x, 1.5f, -10f);
            var follow = camera.gameObject.GetComponent<CameraFollow>() ?? camera.gameObject.AddComponent<CameraFollow>();
            follow.SetTarget(target);
        }

        private static PortfolioPanelController CreatePortfolioPanel()
        {
            var canvasObject = new GameObject("Portfolio Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            var panelObject = new GameObject("Portfolio Panel");
            panelObject.transform.SetParent(canvasObject.transform, false);
            var image = panelObject.AddComponent<Image>();
            image.color = new Color(0.05f, 0.05f, 0.06f, 0.94f);
            var rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(640f, 320f);
            rect.anchoredPosition = Vector2.zero;

            var controller = panelObject.AddComponent<PortfolioPanelController>();
            var category = CreateText(panelObject.transform, "Category", new Vector2(0f, 120f), 18);
            var title = CreateText(panelObject.transform, "Title", new Vector2(0f, 80f), 30);
            var subtitle = CreateText(panelObject.transform, "Subtitle", new Vector2(0f, 38f), 20);
            var body = CreateText(panelObject.transform, "Body", new Vector2(0f, -35f), 18);
            var stack = CreateText(panelObject.transform, "Stack", new Vector2(0f, -120f), 16);
            controller.Configure(panelObject, category, title, subtitle, body, stack);
            return controller;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, Vector2 position, int size)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            var rect = text.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(560f, 42f);
            rect.anchoredPosition = position;
            return text;
        }

        private static void CreateExhibit(Vector3 position, string category, string title, string body, PortfolioExhibitCategory exhibitCategory, PortfolioPanelController panel, GameObject player)
        {
            var exhibitObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exhibitObject.name = $"{category} Exhibit";
            exhibitObject.transform.position = position;
            exhibitObject.transform.localScale = new Vector3(1f, 1.5f, 1f);
            Object.Destroy(exhibitObject.GetComponent<BoxCollider>());
            var collider = exhibitObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            var exhibit = exhibitObject.AddComponent<InteractableExhibit>();
            var data = ScriptableObject.CreateInstance<PortfolioExhibitData>();
            data.Id = title.ToLowerInvariant().Replace(" ", "-");
            data.Category = exhibitCategory;
            data.Title = title;
            data.Subtitle = category;
            data.Body = body;
            data.StackTags = new[] { "Unity", "WebGL" };
            data.Links = new[] { new PortfolioLink("Repository", "https://github.com/dhfmzk/Portfolio-io") };
            exhibit.SetData(data);
        }
    }
}
```

- [ ] **Step 2: Connect the panel to the interaction system**

In `GalleryBootstrap.Start`, after `var panel = CreatePortfolioPanel();`, add:

```csharp
var interaction = player.GetComponent<InteractionSystem>();
interaction.Configure(
    player.transform,
    player.GetComponent<PlayerInputReader>(),
    player.GetComponent<PlayerController>(),
    panel);
```

- [ ] **Step 3: Update README current status**

Add this line under `What's Here?` in `README.md`:

```markdown
- MVP target: move through a WebGL gallery, inspect exhibits with `E`, and publish through GitHub Pages.
```

- [ ] **Step 4: Run tests**

Run the EditMode test command.

Expected: all EditMode tests pass.

- [ ] **Step 5: Run WebGL build**

Run the WebGL build command.

Expected: Unity exits `0`, `docs/index.html` and `docs/Build/docs.wasm` are regenerated.

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Bootstrap Assets/Scripts/Portfolio/InteractionSystem.cs README.md docs
git commit -m "feat: bootstrap playable portfolio gallery"
```

---

### Task 7: Debug Cleanup And Final Verification

**Files:**
- Modify: `Assets/Scripts/Debug/DebugInfoUI.cs`
- Modify: `DEPLOYMENT.md`

- [ ] **Step 1: Fix debug text and FPS averaging**

Modify `DebugInfoUI.UpdateCanvasSize` null fallback:

```csharp
this.canvasSizeText.text = this.mainCanvas != null ?
    $"CanvasSize: {this.mainCanvas.rect.width} x {this.mainCanvas.rect.height}" :
    "CanvasSize: Main canvas is not assigned";
```

Modify `DebugInfoUI.UpdateFPS` averaging:

```csharp
var totalTime = this._timeQueue.Sum();
var sampleCount = Mathf.Max(1, this._timeQueue.Count);
var averageTime = totalTime / sampleCount;
var fps = averageTime > 0f ? 1.0f / averageTime : 0f;
```

- [ ] **Step 2: Add a deployment note**

Append to `DEPLOYMENT.md`:

```markdown
## Smoke Test

After each WebGL build, serve `docs/` locally and verify the root page plus `Build/docs.loader.js`, `Build/docs.data`, and `Build/docs.wasm` return `200 OK`.
```

- [ ] **Step 3: Run EditMode tests**

Run the EditMode test command.

Expected: all tests pass.

- [ ] **Step 4: Run WebGL build**

Run the WebGL build command.

Expected: Unity exits `0`.

- [ ] **Step 5: Run local static smoke test**

Run:

```bash
python3 -m http.server 4173 --directory docs
```

In another terminal, run:

```bash
curl -I "http://[::1]:4173/"
curl -I "http://[::1]:4173/Build/docs.loader.js"
curl -I "http://[::1]:4173/Build/docs.data"
curl -I "http://[::1]:4173/Build/docs.wasm"
```

Expected: each response is `HTTP/1.0 200 OK`, and `docs.wasm` reports `Content-type: application/wasm`.

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Debug/DebugInfoUI.cs DEPLOYMENT.md docs
git commit -m "chore: verify WebGL portfolio vertical slice"
```

---

## Self-Review Checklist

- Spec coverage: movement, jump, camera follow, exhibits, panel, WebGL deployment, and desktop-first constraints are covered.
- Test coverage: pure content formatting, movement math, interaction selection, and control indicator state are covered by EditMode tests.
- Manual coverage: scene bootstrap, UI wiring, and WebGL output are covered by Unity batch build plus local HTTP smoke test.
- Scope control: no enemies, inventory, multiple scenes, remote CMS, mobile controls, or CI automation are included.
- Deployment: final step regenerates `docs/` through the existing GitHub Pages build script.
