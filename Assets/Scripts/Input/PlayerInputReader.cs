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
