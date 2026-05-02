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
