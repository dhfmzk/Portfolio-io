using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        private const float DefaultJumpVelocity = 5.5f;

        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpVelocity = DefaultJumpVelocity;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.12f;
        [SerializeField] private LayerMask groundMask = ~0;

        private Rigidbody2D _body;
        private float _moveInput;
        private bool _jumpRequested;
        private bool _movementPaused;

        public bool Grounded => _body == null || IsGrounded();

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
