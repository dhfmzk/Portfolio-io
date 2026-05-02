using Portfolio;
using TMPro;
using UnityEngine;

namespace UI
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI promptText;

        public void Configure(TextMeshProUGUI text)
        {
            promptText = text;
            Show(null);
        }

        public void Show(InteractableExhibit exhibit)
        {
            if (promptText != null)
            {
                promptText.text = ResolvePromptText(exhibit);
            }
        }

        public static string ResolvePromptText(InteractableExhibit exhibit)
        {
            var title = exhibit != null && exhibit.Data != null ? exhibit.Data.Title : string.Empty;
            return string.IsNullOrWhiteSpace(title)
                ? "Explore the gallery"
                : $"Press E - {title}";
        }
    }
}
