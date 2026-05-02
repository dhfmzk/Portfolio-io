using Input;
using UnityEngine;

namespace Player
{
    public enum PlayerSpriteAnimationState
    {
        Idle = 0,
        Walk = 1,
        Interact = 2
    }

    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerSpriteAnimator : MonoBehaviour
    {
        private const string SpriteSheetResourcePath = "Characters/developer-spritesheet";
        private const int Columns = 4;
        private const int Rows = 3;
        private const float PixelsPerUnit = 320f;

        [SerializeField] private float idleFramesPerSecond = 4f;
        [SerializeField] private float walkFramesPerSecond = 8f;
        [SerializeField] private float interactFramesPerSecond = 8f;
        [SerializeField] private float interactDuration = 0.45f;

        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _body;
        private PlayerInputReader _inputReader;
        private Sprite[] _frames;
        private float _stateTime;
        private float _interactTimeRemaining;
        private PlayerSpriteAnimationState _state;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _body = GetComponent<Rigidbody2D>();
            _inputReader = GetComponent<PlayerInputReader>();
            LoadFrames();
        }

        private void Update()
        {
            if (_frames == null || _frames.Length == 0 || _spriteRenderer == null)
            {
                return;
            }

            if ((_inputReader != null && _inputReader.InteractionPressedThisFrame) ||
                UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                _interactTimeRemaining = interactDuration;
            }

            if (_interactTimeRemaining > 0f)
            {
                _interactTimeRemaining = Mathf.Max(0f, _interactTimeRemaining - Time.deltaTime);
            }

            var horizontalSpeed = _body != null ? _body.velocity.x : 0f;
            var nextState = ResolveState(horizontalSpeed, _interactTimeRemaining > 0f);
            if (nextState != _state)
            {
                _state = nextState;
                _stateTime = 0f;
            }
            else
            {
                _stateTime += Time.deltaTime;
            }

            if (horizontalSpeed < -0.05f)
            {
                _spriteRenderer.flipX = true;
            }
            else if (horizontalSpeed > 0.05f)
            {
                _spriteRenderer.flipX = false;
            }

            var frame = Mathf.FloorToInt(_stateTime * ResolveFramesPerSecond(_state)) % Columns;
            _spriteRenderer.sprite = _frames[ResolveFrameIndex((int)_state, frame, Columns)];
            _spriteRenderer.color = Color.white;
        }

        private void LoadFrames()
        {
            var texture = Resources.Load<Texture2D>(SpriteSheetResourcePath);
            if (texture == null)
            {
                return;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            _frames = CreateFrames(texture, Columns, Rows, PixelsPerUnit);
            if (_frames.Length > 0 && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = _frames[0];
                _spriteRenderer.color = Color.white;
            }
        }

        public static PlayerSpriteAnimationState ResolveState(float horizontalSpeed, bool interactionActive)
        {
            if (interactionActive)
            {
                return PlayerSpriteAnimationState.Interact;
            }

            return Mathf.Abs(horizontalSpeed) > 0.05f
                ? PlayerSpriteAnimationState.Walk
                : PlayerSpriteAnimationState.Idle;
        }

        public static int ResolveFrameIndex(int row, int frame, int columns)
        {
            return row * columns + frame;
        }

        private float ResolveFramesPerSecond(PlayerSpriteAnimationState state)
        {
            return state switch
            {
                PlayerSpriteAnimationState.Walk => walkFramesPerSecond,
                PlayerSpriteAnimationState.Interact => interactFramesPerSecond,
                _ => idleFramesPerSecond
            };
        }

        private static Sprite[] CreateFrames(Texture2D texture, int columns, int rows, float pixelsPerUnit)
        {
            var frames = new Sprite[columns * rows];
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var left = Mathf.RoundToInt(texture.width * column / (float)columns);
                    var right = Mathf.RoundToInt(texture.width * (column + 1) / (float)columns);
                    var bottom = Mathf.RoundToInt(texture.height * (rows - row - 1) / (float)rows);
                    var top = Mathf.RoundToInt(texture.height * (rows - row) / (float)rows);
                    var rect = new Rect(left, bottom, right - left, top - bottom);
                    frames[ResolveFrameIndex(row, column, columns)] = Sprite.Create(
                        texture,
                        rect,
                        new Vector2(0.5f, 0.08f),
                        pixelsPerUnit);
                }
            }

            return frames;
        }
    }
}
