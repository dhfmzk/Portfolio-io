using Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ControlIndicatorUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI leftText;
        [SerializeField] private TextMeshProUGUI rightText;
        [SerializeField] private TextMeshProUGUI jumpText;
        [SerializeField] private TextMeshProUGUI interactText;
        [SerializeField] private Image leftImage;
        [SerializeField] private Image rightImage;
        [SerializeField] private Image jumpImage;
        [SerializeField] private Image interactImage;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite pressedSprite;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        [SerializeField] private PlayerInputReader inputReader;

        public void Configure(
            TextMeshProUGUI left,
            TextMeshProUGUI right,
            TextMeshProUGUI jump,
            TextMeshProUGUI interact)
        {
            leftText = left;
            rightText = right;
            jumpText = jump;
            interactText = interact;
        }

        public void Configure(
            TextMeshProUGUI left,
            TextMeshProUGUI right,
            TextMeshProUGUI jump,
            TextMeshProUGUI interact,
            Image leftKey,
            Image rightKey,
            Image jumpKey,
            Image interactKey,
            Sprite keyNormal,
            Sprite keyPressed)
        {
            Configure(left, right, jump, interact);
            leftImage = leftKey;
            rightImage = rightKey;
            jumpImage = jumpKey;
            interactImage = interactKey;
            normalSprite = keyNormal;
            pressedSprite = keyPressed;
        }

        public void SetInputReader(PlayerInputReader reader)
        {
            inputReader = reader;
        }

        private void Update()
        {
            var leftPressed = inputReader != null
                ? inputReader.LeftPressed
                : UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow);
            var rightPressed = inputReader != null
                ? inputReader.RightPressed
                : UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow);
            var jumpPressed = inputReader != null
                ? inputReader.JumpPressed
                : UnityEngine.Input.GetKey(KeyCode.Space);
            var interactPressed = inputReader != null
                ? inputReader.InteractPressed
                : UnityEngine.Input.GetKey(KeyCode.E);

            Apply(leftText, leftImage, leftPressed);
            Apply(rightText, rightImage, rightPressed);
            Apply(jumpText, jumpImage, jumpPressed);
            Apply(interactText, interactImage, interactPressed);
        }

        private void Apply(TextMeshProUGUI target, Image image, bool pressed)
        {
            if (target != null)
            {
                target.color = ResolveLabelColor(pressed, activeColor, inactiveColor);
            }

            if (image != null)
            {
                image.sprite = ResolveKeySprite(pressed, normalSprite, pressedSprite);
            }
        }

        public static Color ResolveLabelColor(bool pressed, Color active, Color inactive)
        {
            return pressed ? active : inactive;
        }

        public static Sprite ResolveKeySprite(bool pressed, Sprite normal, Sprite pressedSprite)
        {
            return pressed && pressedSprite != null ? pressedSprite : normal;
        }
    }
}
