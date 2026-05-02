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

            var interaction = player.GetComponent<InteractionSystem>();
            interaction.ExhibitOpened += panel.Show;
            interaction.ExhibitClosed += panel.Hide;

            CreateExhibit(new Vector3(-5f, 0.75f, 0f), "About", "Developer Profile", "I build compact, useful software with playful presentation.", PortfolioExhibitCategory.About);
            CreateExhibit(new Vector3(0f, 0.75f, 0f), "Project", "Player 1 Portfolio", "A WebGL portfolio presented as a side-scrolling gallery.", PortfolioExhibitCategory.Project);
            CreateExhibit(new Vector3(5f, 0.75f, 0f), "Contact", "Contact Gate", "GitHub: dhfmzk\nProject: Portfolio-io", PortfolioExhibitCategory.Contact);
        }

        public static PortfolioExhibitData CreateDraftExhibitData(
            string category,
            string title,
            string body,
            PortfolioExhibitCategory exhibitCategory)
        {
            var data = ScriptableObject.CreateInstance<PortfolioExhibitData>();
            data.Id = CreateId(title);
            data.Category = exhibitCategory;
            data.Title = title ?? string.Empty;
            data.Subtitle = category ?? string.Empty;
            data.Body = body ?? string.Empty;
            data.StackTags = new[] { "Unity", "WebGL" };
            data.Links = new[] { new PortfolioLink("Repository", "https://github.com/dhfmzk/Portfolio-io") };
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
            ground.transform.localScale = new Vector3(24f, 0.2f, 1f);
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

        private static void CreateExhibit(
            Vector3 position,
            string category,
            string title,
            string body,
            PortfolioExhibitCategory exhibitCategory)
        {
            var exhibitObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            exhibitObject.name = $"{category} Exhibit";
            exhibitObject.transform.position = position;
            exhibitObject.transform.localScale = new Vector3(1f, 1.5f, 1f);
            Destroy(exhibitObject.GetComponent<BoxCollider>());
            exhibitObject.AddComponent<BoxCollider2D>();
            var exhibit = exhibitObject.AddComponent<InteractableExhibit>();
            exhibit.SetData(CreateDraftExhibitData(category, title, body, exhibitCategory));
        }
    }
}
