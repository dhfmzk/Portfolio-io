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
        private bool _hasDebugStateOverride;
        private PlayerSpriteAnimationState _debugStateOverride;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _body = GetComponent<Rigidbody2D>();
            _inputReader = GetComponent<PlayerInputReader>();
            _hasDebugStateOverride = TryResolveDebugStateOverride(Application.absoluteURL, out _debugStateOverride);
            LoadFrames();
        }

        private void Update()
        {
            if (_frames == null || _frames.Length == 0 || _spriteRenderer == null)
            {
                return;
            }

            if (!_hasDebugStateOverride &&
                ((_inputReader != null && _inputReader.InteractionPressedThisFrame) ||
                UnityEngine.Input.GetKeyDown(KeyCode.E)))
            {
                _interactTimeRemaining = interactDuration;
            }

            if (_interactTimeRemaining > 0f)
            {
                _interactTimeRemaining = Mathf.Max(0f, _interactTimeRemaining - Time.deltaTime);
            }

            var horizontalSpeed = _body != null ? _body.velocity.x : 0f;
            var nextState = _hasDebugStateOverride
                ? _debugStateOverride
                : ResolveState(horizontalSpeed, _interactTimeRemaining > 0f);
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

            var frame = ResolveFrameColumn(_state, _stateTime, Columns, ResolveFramesPerSecond(_state));
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

        public static int ResolveFrameColumn(
            PlayerSpriteAnimationState state,
            float stateTime,
            int columns,
            float framesPerSecond)
        {
            return Mathf.FloorToInt(stateTime * framesPerSecond) % columns;
        }

        public static bool TryResolveDebugStateOverride(
            string absoluteUrl,
            out PlayerSpriteAnimationState state)
        {
            state = PlayerSpriteAnimationState.Idle;
            if (string.IsNullOrEmpty(absoluteUrl))
            {
                return false;
            }

            var queryStart = absoluteUrl.IndexOf('?');
            if (queryStart < 0 || queryStart == absoluteUrl.Length - 1)
            {
                return false;
            }

            var fragmentStart = absoluteUrl.IndexOf('#', queryStart + 1);
            var queryEnd = fragmentStart >= 0 ? fragmentStart : absoluteUrl.Length;
            var query = absoluteUrl.Substring(queryStart + 1, queryEnd - queryStart - 1);
            var pairs = query.Split('&');
            foreach (var pair in pairs)
            {
                var separator = pair.IndexOf('=');
                if (separator <= 0)
                {
                    continue;
                }

                var key = pair.Substring(0, separator);
                if (!key.Equals("debugAnimation", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var value = pair.Substring(separator + 1);
                return System.Enum.TryParse(value, true, out state);
            }

            return false;
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

        public static Sprite[] CreateFrames(Texture2D texture, int columns, int rows, float pixelsPerUnit)
        {
            var frames = new Sprite[columns * rows];
            var pixels = ReadPixels(texture);
            var metrics = CreateFrameMetrics(texture, pixels, columns, rows);
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var index = ResolveFrameIndex(row, column, columns);
                    var metric = metrics[index];
                    var rect = ResolveFrameRect(metric);
                    frames[ResolveFrameIndex(row, column, columns)] = Sprite.Create(
                        texture,
                        rect,
                        ResolveFramePivot(metric, rect),
                        pixelsPerUnit);
                }
            }

            return frames;
        }

        private static Color32[] ReadPixels(Texture2D texture)
        {
            try
            {
                return texture.GetPixels32();
            }
            catch (UnityException)
            {
                return null;
            }
        }

        private static FrameMetrics[] CreateFrameMetrics(Texture2D texture, Color32[] pixels, int columns, int rows)
        {
            var metrics = new FrameMetrics[columns * rows];
            var frameWidth = texture.width / columns;
            var frameHeight = texture.height / rows;
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var cell = new RectInt(
                        frameWidth * column,
                        frameHeight * (rows - row - 1),
                        frameWidth,
                        frameHeight);
                    metrics[ResolveFrameIndex(row, column, columns)] = ResolveFrameMetrics(texture, pixels, cell);
                }
            }

            return metrics;
        }

        private static FrameMetrics ResolveFrameMetrics(Texture2D texture, Color32[] pixels, RectInt cell)
        {
            if (pixels == null || pixels.Length != texture.width * texture.height)
            {
                return new FrameMetrics(cell, cell, cell.center.x, cell.yMin, false);
            }

            var minX = cell.xMax;
            var maxX = cell.xMin - 1;
            var minY = cell.yMax;
            var maxY = cell.yMin - 1;

            for (var y = cell.yMin; y < cell.yMax; y++)
            {
                for (var x = cell.xMin; x < cell.xMax; x++)
                {
                    if (pixels[y * texture.width + x].a == 0)
                    {
                        continue;
                    }

                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            if (maxX < minX || maxY < minY)
            {
                return new FrameMetrics(cell, cell, cell.center.x, cell.yMin, false);
            }

            var minContactX = maxX + 1;
            var maxContactX = minX - 1;
            for (var x = minX; x <= maxX; x++)
            {
                if (pixels[minY * texture.width + x].a == 0)
                {
                    continue;
                }

                minContactX = Mathf.Min(minContactX, x);
                maxContactX = Mathf.Max(maxContactX, x);
            }

            if (maxContactX < minContactX)
            {
                minContactX = minX;
                maxContactX = maxX;
            }

            var contactCenter = (minContactX + maxContactX + 1f) * 0.5f;
            return new FrameMetrics(
                cell,
                new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1),
                contactCenter,
                minY,
                true);
        }

        private static Rect ResolveFrameRect(FrameMetrics metric)
        {
            return new Rect(metric.Cell.x, metric.Cell.y, metric.Cell.width, metric.Cell.height);
        }

        private static Vector2 ResolveFramePivot(FrameMetrics metric, Rect rect)
        {
            if (!metric.HasPixels || rect.width <= 0f || rect.height <= 0f)
            {
                return new Vector2(0.5f, 0f);
            }

            return new Vector2(
                Mathf.Clamp01((metric.ContactX - rect.xMin) / rect.width),
                Mathf.Clamp01((metric.ContactY - rect.yMin) / rect.height));
        }

        private readonly struct FrameMetrics
        {
            public readonly RectInt Cell;
            public readonly RectInt Bounds;
            public readonly float ContactX;
            public readonly int ContactY;
            public readonly bool HasPixels;

            public FrameMetrics(RectInt cell, RectInt bounds, float contactX, int contactY, bool hasPixels)
            {
                Cell = cell;
                Bounds = bounds;
                ContactX = contactX;
                ContactY = contactY;
                HasPixels = hasPixels;
            }
        }
    }
}
