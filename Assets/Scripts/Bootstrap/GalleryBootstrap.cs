using CameraSystem;
using Input;
using Player;
using Portfolio;
using System;
using System.Linq;
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
            var prompt = CreateInteractionPrompt();

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

        private static GameObject CreatePlayer()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Cube);
            player.name = "Player";
            player.transform.position = new Vector3(-8f, 1f, 0f);
            player.transform.localScale = new Vector3(0.8f, 1.2f, 1f);
            Destroy(player.GetComponent<BoxCollider>());
            player.AddComponent<BoxCollider2D>();
            var body = player.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            var controller = player.AddComponent<PlayerController>();
            var input = player.AddComponent<PlayerInputReader>();
            var interaction = player.AddComponent<InteractionSystem>();
            interaction.Configure(player.transform, input, controller);
            return player;
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Gallery Ground";
            ground.transform.position = new Vector3(0f, -0.1f, 0f);
            ground.transform.localScale = new Vector3(34f, 0.2f, 1f);
            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.16f, 0.17f, 0.18f, 1f);
            }

            Destroy(ground.GetComponent<BoxCollider>());
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
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            canvasObject.AddComponent<GraphicRaycaster>();

            var panelObject = new GameObject("Portfolio Panel");
            panelObject.transform.SetParent(canvasObject.transform, false);
            var image = panelObject.AddComponent<Image>();
            image.color = new Color(0.05f, 0.05f, 0.06f, 0.94f);
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

        private static InteractionPromptUI CreateInteractionPrompt()
        {
            var canvasObject = new GameObject("Interaction Prompt Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            var text = CreateText(canvasObject.transform, "Prompt", new Vector2(0f, -285f), 20, new Vector2(520f, 42f));
            text.color = new Color(0.95f, 0.92f, 0.78f, 1f);

            var prompt = canvasObject.AddComponent<InteractionPromptUI>();
            prompt.Configure(text);
            return prompt;
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
            var exhibitObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exhibitObject.name = $"{definition.Category} Exhibit";
            exhibitObject.transform.position = definition.Position;
            exhibitObject.transform.localScale = new Vector3(1f, 1.5f, 1f);
            var renderer = exhibitObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = definition.Color;
            }

            Destroy(exhibitObject.GetComponent<BoxCollider>());
            exhibitObject.AddComponent<BoxCollider2D>();
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
            var highlight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            highlight.name = "Focus Highlight";
            highlight.transform.SetParent(parent, false);
            highlight.transform.localPosition = new Vector3(0f, 0f, -0.08f);
            highlight.transform.localScale = new Vector3(1.18f, 1.12f, 0.1f);
            var renderer = highlight.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(1f, 0.82f, 0.24f, 1f);
            }

            Destroy(highlight.GetComponent<BoxCollider>());
            highlight.SetActive(false);
            return highlight;
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
    }
}
