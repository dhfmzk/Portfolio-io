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
            SetText(linksText, string.IsNullOrWhiteSpace(viewModel.LinkSummary) ? string.Empty : $"Links: {viewModel.LinkSummary}");

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
            TextMeshProUGUI links = null)
        {
            root = panelRoot;
            categoryText = category;
            titleText = title;
            subtitleText = subtitle;
            bodyText = body;
            stackText = stack;
            linksText = links;
            Hide();
        }

        private static void SetText(TextMeshProUGUI target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }
    }
}
