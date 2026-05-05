using CameraSystem;
using Input;
using Player;
using Portfolio;
using System;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
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
            PrepareSceneForRuntimeBootstrap();
            var player = CreateRuntimePlayer();
            CreateBackdrop();
            CreateGround();
            CreateCamera(player.transform);
            EnsureEventSystem();
            var panel = CreatePortfolioPanel();
            var prompt = CreateRuntimeInteractionPrompt();
            CreateRuntimeControlIndicator(player.GetComponent<PlayerInputReader>());

            var interaction = player.GetComponent<InteractionSystem>();
            interaction.ExhibitOpened += panel.Show;
            interaction.ExhibitClosed += panel.Hide;
            interaction.FocusedExhibitChanged += prompt.Show;

            CreateStageSign();
            foreach (var definition in CreateDefaultDefinitions())
            {
                CreateExhibit(definition);
            }
        }

        public static PortfolioExhibitData[] CreateDefaultExhibitData()
        {
            return CreateDefaultDefinitions()
                .Select(definition => CreateDraftExhibitData(
                    definition.Category,
                    definition.Title,
                    definition.Body,
                    definition.ExhibitCategory,
                    definition.StackTags,
                    definition.Links))
                .ToArray();
        }

        public static PortfolioExhibitData CreateDraftExhibitData(
            string category,
            string title,
            string body,
            PortfolioExhibitCategory exhibitCategory)
        {
            return CreateDraftExhibitData(
                category,
                title,
                body,
                exhibitCategory,
                new[] { "Unity", "WebGL" },
                new[] { new PortfolioLink("Repository", "https://github.com/dhfmzk/Portfolio-io") });
        }

        private static PortfolioExhibitData CreateDraftExhibitData(
            string category,
            string title,
            string body,
            PortfolioExhibitCategory exhibitCategory,
            string[] stackTags,
            PortfolioLink[] links)
        {
            var data = ScriptableObject.CreateInstance<PortfolioExhibitData>();
            data.Id = CreateId(title);
            data.Category = exhibitCategory;
            data.Title = title ?? string.Empty;
            data.Subtitle = category ?? string.Empty;
            data.Body = body ?? string.Empty;
            data.StackTags = stackTags?.Where(tag => !string.IsNullOrWhiteSpace(tag)).ToArray()
                ?? Array.Empty<string>();
            data.Links = links?.Where(link => link != null && !string.IsNullOrWhiteSpace(link.Url)).ToArray()
                ?? Array.Empty<PortfolioLink>();
            return data;
        }

        private static string CreateId(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return string.Empty;
            }

            return title.Trim().ToLowerInvariant().Replace(" ", "-");
        }

        public static void PrepareSceneForRuntimeBootstrap()
        {
            foreach (var canvas in FindObjectsOfType<Canvas>())
            {
                if (canvas == null)
                {
                    continue;
                }

                var canvasObject = canvas.gameObject;
                if (canvasObject.name == "Canvas")
                {
                    canvasObject.SetActive(false);
                }
            }
        }

        public static GameObject CreateRuntimePlayer()
        {
            var player = CreateBlock(
                "Player",
                new Vector3(-11.5f, 0f, 0f),
                Vector3.one,
                new Color(0.95f, 0.92f, 0.78f, 1f),
                true,
                4);
            var collider = player.GetComponent<BoxCollider2D>();
            collider.offset = new Vector2(0f, 0.55f);
            collider.size = new Vector2(0.55f, 1.1f);
            var placeholderRenderer = player.GetComponent<SpriteRenderer>();
            placeholderRenderer.enabled = false;
            CreatePlayerVisual(player.transform, placeholderRenderer);
            var body = player.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.gravityScale = 1.5f;
            var controller = player.AddComponent<PlayerController>();
            var input = player.AddComponent<PlayerInputReader>();
            var interaction = player.AddComponent<InteractionSystem>();
            player.AddComponent<PlayerSpriteAnimator>();
            interaction.Configure(player.transform, input, controller);
            return player;
        }

        private static void CreatePlayerVisual(Transform parent, SpriteRenderer placeholderRenderer)
        {
            var visual = new GameObject("Player Visual");
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one;

            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = placeholderRenderer.sprite;
            renderer.color = Color.white;
            renderer.sortingOrder = placeholderRenderer.sortingOrder;
        }

        private static void CreateBackdrop()
        {
            CreateBlock(
                "Gallery Backdrop",
                new Vector3(1.25f, 2.8f, 0f),
                new Vector3(42f, 9f, 1f),
                new Color(0.07f, 0.1f, 0.11f, 1f),
                false,
                -30);
            CreateBlock(
                "Gallery Floor Fill",
                new Vector3(1.25f, -0.56f, 0f),
                new Vector3(42f, 0.9f, 1f),
                new Color(0.2f, 0.24f, 0.24f, 1f),
                false,
                -20);
            CreateBlock(
                "Gallery Floor Accent",
                new Vector3(1.25f, -0.04f, 0f),
                new Vector3(42f, 0.08f, 1f),
                new Color(0.82f, 0.73f, 0.48f, 1f),
                false,
                1);
        }

        private static void CreateGround()
        {
            var ground = CreateBlock(
                "Gallery Ground",
                new Vector3(0f, -0.1f, 0f),
                new Vector3(34f, 0.2f, 1f),
                new Color(0.16f, 0.17f, 0.18f, 1f),
                true,
                0);
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
            camera.orthographicSize = 4f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.1f, 0.11f, 1f);
            camera.rect = new Rect(0f, 0f, 1f, 1f);
            camera.transform.position = new Vector3(target.position.x, 1.3f, -10f);
            var follow = camera.gameObject.GetComponent<CameraFollow>() ?? camera.gameObject.AddComponent<CameraFollow>();
            follow.SetTarget(target);
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static PortfolioPanelController CreatePortfolioPanel()
        {
            var canvasObject = new GameObject("Portfolio Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            canvasObject.AddComponent<GraphicRaycaster>();

            var panelObject = new GameObject("Portfolio Panel");
            panelObject.transform.SetParent(canvasObject.transform, false);
            var image = panelObject.AddComponent<Image>();
            image.color = new Color(0.05f, 0.05f, 0.06f, 0.99f);
            var rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(720f, 380f);
            rect.anchoredPosition = Vector2.zero;

            var controller = panelObject.AddComponent<PortfolioPanelController>();
            var category = CreateText(panelObject.transform, "Category", new Vector2(0f, 145f), 18, new Vector2(620f, 32f));
            var title = CreateText(panelObject.transform, "Title", new Vector2(0f, 105f), 30, new Vector2(620f, 42f));
            var subtitle = CreateText(panelObject.transform, "Subtitle", new Vector2(0f, 62f), 20, new Vector2(620f, 34f));
            var body = CreateText(panelObject.transform, "Body", new Vector2(0f, -18f), 18, new Vector2(620f, 120f));
            var stack = CreateText(panelObject.transform, "Stack", new Vector2(0f, -112f), 16, new Vector2(620f, 30f));
            var links = CreateText(panelObject.transform, "Links", new Vector2(0f, -138f), 15, new Vector2(620f, 24f));
            var linkButtonRoot = CreateLinkButtonRoot(panelObject.transform);
            var linkButtonTemplate = CreateLinkButtonTemplate(linkButtonRoot);
            controller.Configure(panelObject, category, title, subtitle, body, stack, links, linkButtonRoot, linkButtonTemplate);
            return controller;
        }

        private static Transform CreateLinkButtonRoot(Transform parent)
        {
            var rootObject = new GameObject("Link Buttons");
            rootObject.transform.SetParent(parent, false);
            var rect = rootObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(620f, 34f);
            rect.anchoredPosition = new Vector2(0f, -168f);
            var layout = rootObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.spacing = 12f;
            return rootObject.transform;
        }

        private static Button CreateLinkButtonTemplate(Transform parent)
        {
            var buttonObject = new GameObject("Link Button Template");
            buttonObject.transform.SetParent(parent, false);
            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.86f, 0.82f, 0.62f, 1f);
            var button = buttonObject.AddComponent<Button>();
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(160f, 30f);

            var label = CreateText(buttonObject.transform, "Label", Vector2.zero, 14, new Vector2(150f, 24f));
            label.color = new Color(0.05f, 0.05f, 0.06f, 1f);

            buttonObject.SetActive(false);
            return button;
        }

        public static InteractionPromptUI CreateRuntimeInteractionPrompt()
        {
            var canvasObject = new GameObject("Interaction Prompt Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            var text = CreateText(canvasObject.transform, "Prompt", new Vector2(0f, 86f), 18, new Vector2(520f, 34f));
            var textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(0.5f, 0f);
            textRect.anchorMax = new Vector2(0.5f, 0f);
            textRect.pivot = new Vector2(0.5f, 0f);
            text.color = new Color(0.95f, 0.92f, 0.78f, 1f);

            var prompt = canvasObject.AddComponent<InteractionPromptUI>();
            prompt.Configure(text);
            return prompt;
        }

        public static GameObject CreateRuntimeControlIndicator(PlayerInputReader inputReader = null)
        {
            var canvasObject = new GameObject("Control Hint Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            var hintRoot = new GameObject("Control Hints");
            hintRoot.transform.SetParent(canvasObject.transform, false);
            var rect = hintRoot.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(320f, 42f);
            rect.anchoredPosition = new Vector2(0f, 28f);

            var layout = hintRoot.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            layout.spacing = 8f;

            var normalSprite = Resources.Load<Sprite>("BUTTON_BASE_UP");
            var pressedSprite = Resources.Load<Sprite>("BUTTON_BASE_DOWN");
            var leftKey = CreateControlKey(hintRoot.transform, "Left", "A", new Vector2(64f, 27f), normalSprite, inputReader, VirtualControlAction.Left);
            var rightKey = CreateControlKey(hintRoot.transform, "Right", "D", new Vector2(64f, 27f), normalSprite, inputReader, VirtualControlAction.Right);
            var jumpKey = CreateControlKey(hintRoot.transform, "Jump", "Space", new Vector2(72f, 27f), normalSprite, inputReader, VirtualControlAction.Jump);
            var interactKey = CreateControlKey(hintRoot.transform, "Interact", "E", new Vector2(64f, 27f), normalSprite, inputReader, VirtualControlAction.Interact);

            var indicator = canvasObject.AddComponent<ControlIndicatorUI>();
            indicator.Configure(
                leftKey.Label,
                rightKey.Label,
                jumpKey.Label,
                interactKey.Label,
                leftKey.Image,
                rightKey.Image,
                jumpKey.Image,
                interactKey.Image,
                normalSprite,
                pressedSprite);
            indicator.SetInputReader(inputReader);
            return canvasObject;
        }

        private static ControlKeyElements CreateControlKey(
            Transform parent,
            string name,
            string label,
            Vector2 size,
            Sprite keySprite,
            PlayerInputReader inputReader,
            VirtualControlAction action)
        {
            var keyObject = new GameObject(name);
            keyObject.transform.SetParent(parent, false);
            var image = keyObject.AddComponent<Image>();
            image.sprite = keySprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = true;
            var rect = keyObject.GetComponent<RectTransform>();
            rect.sizeDelta = size;

            var text = CreateText(keyObject.transform, "Label", Vector2.zero, 14, new Vector2(size.x - 8f, size.y - 4f));
            text.text = label;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.raycastTarget = false;
            var virtualButton = keyObject.AddComponent<VirtualControlButton>();
            virtualButton.Configure(inputReader, action);
            return new ControlKeyElements(image, text);
        }

        private static TextMeshProUGUI CreateText(
            Transform parent,
            string name,
            Vector2 position,
            int size,
            Vector2 sizeDelta)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = true;
            var rect = text.GetComponent<RectTransform>();
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = position;
            return text;
        }

        private static void CreateExhibit(ExhibitDefinition definition)
        {
            var exhibitObject = CreateBlock(
                $"{definition.Category} Exhibit",
                definition.Position,
                new Vector3(0.86f, 1.08f, 1f),
                definition.Color,
                false,
                2);
            var exhibit = exhibitObject.AddComponent<InteractableExhibit>();
            exhibit.SetData(CreateDraftExhibitData(
                definition.Category,
                definition.Title,
                definition.Body,
                definition.ExhibitCategory,
                definition.StackTags,
                definition.Links));
            exhibit.SetHighlight(CreateHighlight(exhibitObject.transform));
            CreateWorldLabel(exhibitObject.transform, definition.Category, definition.Title);
        }

        private static GameObject CreateHighlight(Transform parent)
        {
            var highlight = CreateBlock(
                "Focus Highlight",
                Vector3.zero,
                new Vector3(1.18f, 1.12f, 0.1f),
                new Color(1f, 0.82f, 0.24f, 1f),
                false,
                1);
            highlight.transform.SetParent(parent, false);
            highlight.transform.localPosition = new Vector3(0f, 0f, -0.08f);
            highlight.SetActive(false);
            return highlight;
        }

        private static GameObject CreateBlock(
            string name,
            Vector3 position,
            Vector3 scale,
            Color color,
            bool includeCollider,
            int sortingOrder)
        {
            var block = new GameObject(name);
            block.transform.position = position;
            block.transform.localScale = scale;

            var renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateBlockSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            if (includeCollider)
            {
                block.AddComponent<BoxCollider2D>();
            }

            return block;
        }

        private static Sprite CreateBlockSprite()
        {
            var texture = Texture2D.whiteTexture;
            var pixelsPerUnit = Mathf.Max(texture.width, texture.height);
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit);
        }

        private static void CreateWorldLabel(Transform parent, string category, string title)
        {
            var labelObject = new GameObject("World Label");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.35f, -0.15f);
            var label = labelObject.AddComponent<TextMeshPro>();
            label.text = $"{category}\n{title}";
            label.fontSize = 2.8f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.94f, 0.94f, 0.9f, 1f);
            label.rectTransform.sizeDelta = new Vector2(5.2f, 1.4f);
        }

        private static void CreateStageSign()
        {
            var signObject = new GameObject("Stage Title");
            signObject.transform.position = new Vector3(-10.5f, 2.5f, 0f);
            var sign = signObject.AddComponent<TextMeshPro>();
            sign.text = "Player 1 Portfolio";
            sign.fontSize = 3.4f;
            sign.alignment = TextAlignmentOptions.Left;
            sign.color = new Color(0.95f, 0.92f, 0.78f, 1f);
            sign.rectTransform.sizeDelta = new Vector2(8f, 1.4f);
        }

        private static ExhibitDefinition[] CreateDefaultDefinitions()
        {
            return new[]
            {
                new ExhibitDefinition(
                    new Vector3(-10f, 0.75f, 0f),
                    "About",
                    "Developer Profile",
                    "I build compact, useful software and present it through playful interfaces. This portfolio is the first playable shell for that direction.",
                    PortfolioExhibitCategory.About,
                    new[] { "Unity", "C#", "Product UX" },
                    new[] { new PortfolioLink("GitHub", "https://github.com/dhfmzk") },
                    new Color(0.26f, 0.52f, 0.72f, 1f)),
                new ExhibitDefinition(
                    new Vector3(-5.5f, 0.75f, 0f),
                    "Project",
                    "Player 1 Portfolio",
                    "A Unity WebGL portfolio structured as a side-scrolling gallery, built for GitHub Pages and rapid iteration.",
                    PortfolioExhibitCategory.Project,
                    new[] { "Unity", "WebGL", "GitHub Pages" },
                    new[]
                    {
                        new PortfolioLink("Repository", "https://github.com/dhfmzk/Portfolio-io"),
                        new PortfolioLink("Playable Build", "https://dhfmzk.github.io/Portfolio-io/")
                    },
                    new Color(0.75f, 0.48f, 0.25f, 1f)),
                new ExhibitDefinition(
                    new Vector3(-1f, 0.75f, 0f),
                    "Project",
                    "Agentic Build Flow",
                    "A local Codex-driven workflow for planning, testing, building, and publishing small portfolio milestones without a heavy pipeline.",
                    PortfolioExhibitCategory.Project,
                    new[] { "Codex", "TDD", "Unity" },
                    new[] { new PortfolioLink("Project Notes", "https://github.com/dhfmzk/Portfolio-io") },
                    new Color(0.5f, 0.62f, 0.32f, 1f)),
                new ExhibitDefinition(
                    new Vector3(3.5f, 0.75f, 0f),
                    "Project",
                    "WebGL Deploy Loop",
                    "A low-overhead deployment loop that builds Unity WebGL into docs/ and hosts it directly from GitHub Pages.",
                    PortfolioExhibitCategory.Project,
                    new[] { "Unity Build", "WebGL", "Docs Hosting" },
                    new[] { new PortfolioLink("Deployment Doc", "https://github.com/dhfmzk/Portfolio-io/blob/main/DEPLOYMENT.md") },
                    new Color(0.45f, 0.4f, 0.7f, 1f)),
                new ExhibitDefinition(
                    new Vector3(8f, 0.75f, 0f),
                    "Skill",
                    "Skill Wall",
                    "Unity, C#, WebGL, product UX, automation, testing, and deployment. The emphasis is on shipping small coherent systems.",
                    PortfolioExhibitCategory.Skill,
                    new[] { "Unity", "C#", "Testing", "Automation" },
                    new[] { new PortfolioLink("Repository", "https://github.com/dhfmzk/Portfolio-io") },
                    new Color(0.38f, 0.62f, 0.56f, 1f)),
                new ExhibitDefinition(
                    new Vector3(12.5f, 0.75f, 0f),
                    "Contact",
                    "Contact Gate",
                    "GitHub: dhfmzk\nProject: Portfolio-io\nThe public build is meant to become the main portfolio entry point.",
                    PortfolioExhibitCategory.Contact,
                    new[] { "GitHub", "Portfolio", "WebGL" },
                    new[] { new PortfolioLink("GitHub", "https://github.com/dhfmzk") },
                    new Color(0.68f, 0.38f, 0.46f, 1f))
            };
        }

        private readonly struct ExhibitDefinition
        {
            public readonly Vector3 Position;
            public readonly string Category;
            public readonly string Title;
            public readonly string Body;
            public readonly PortfolioExhibitCategory ExhibitCategory;
            public readonly string[] StackTags;
            public readonly PortfolioLink[] Links;
            public readonly Color Color;

            public ExhibitDefinition(
                Vector3 position,
                string category,
                string title,
                string body,
                PortfolioExhibitCategory exhibitCategory,
                string[] stackTags,
                PortfolioLink[] links,
                Color color)
            {
                Position = position;
                Category = category;
                Title = title;
                Body = body;
                ExhibitCategory = exhibitCategory;
                StackTags = stackTags;
                Links = links;
                Color = color;
            }
        }

        private readonly struct ControlKeyElements
        {
            public readonly Image Image;
            public readonly TextMeshProUGUI Label;

            public ControlKeyElements(Image image, TextMeshProUGUI label)
            {
                Image = image;
                Label = label;
            }
        }
    }
}
