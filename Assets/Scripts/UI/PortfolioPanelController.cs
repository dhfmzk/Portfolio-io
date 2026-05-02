using System.Collections.Generic;
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
        [SerializeField] private TextMeshProUGUI linksText;
        [SerializeField] private Transform linkButtonRoot;
        [SerializeField] private Button linkButtonTemplate;
        [SerializeField] private Button closeButton;

        private readonly List<Button> _renderedLinkButtons = new();

        public bool IsOpen => root != null && root.activeSelf;
        public int RenderedLinkCount => _renderedLinkButtons.Count;

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
            SetText(linksText, string.IsNullOrWhiteSpace(viewModel.LinkSummary) ? string.Empty : $"Links: {viewModel.LinkSummary}");
            RenderLinks(viewModel.Links);

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

        public void Configure(
            GameObject panelRoot,
            TextMeshProUGUI category,
            TextMeshProUGUI title,
            TextMeshProUGUI subtitle,
            TextMeshProUGUI body,
            TextMeshProUGUI stack,
            TextMeshProUGUI links = null,
            Transform buttonRoot = null,
            Button buttonTemplate = null)
        {
            root = panelRoot;
            categoryText = category;
            titleText = title;
            subtitleText = subtitle;
            bodyText = body;
            stackText = stack;
            linksText = links;
            linkButtonRoot = buttonRoot;
            linkButtonTemplate = buttonTemplate;
            Hide();
        }

        private static void SetText(TextMeshProUGUI target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }

        private void RenderLinks(PortfolioLink[] links)
        {
            ClearRenderedLinks();
            if (links == null || linkButtonRoot == null || linkButtonTemplate == null)
            {
                return;
            }

            foreach (var link in links)
            {
                if (link == null || string.IsNullOrWhiteSpace(link.Url))
                {
                    continue;
                }

                var button = Instantiate(linkButtonTemplate, linkButtonRoot);
                button.gameObject.SetActive(true);
                SetButtonLabel(button, string.IsNullOrWhiteSpace(link.Label) ? link.Url : link.Label);
                var targetUrl = link.Url;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => Application.OpenURL(targetUrl));
                _renderedLinkButtons.Add(button);
            }
        }

        private void ClearRenderedLinks()
        {
            foreach (var button in _renderedLinkButtons)
            {
                if (button == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(button.gameObject);
                }
                else
                {
                    DestroyImmediate(button.gameObject);
                }
            }

            _renderedLinkButtons.Clear();
        }

        private static void SetButtonLabel(Button button, string label)
        {
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = label ?? string.Empty;
            }
        }
    }
}
