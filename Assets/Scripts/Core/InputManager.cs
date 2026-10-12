using UnityEngine;
using UnityEngine.EventSystems;

namespace WaterSort.Core
{
    /// <summary>
    /// Handles all player input (touch/click) for tube selection
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        private static InputManager _instance;
        public static InputManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<InputManager>();
                }
                return _instance;
            }
        }

        [Header("Input State")]
        private TubeController _selectedTube = null;

        [Header("Settings")]
        public LayerMask TubeLayer;
        [Tooltip("Max finger movement in pixels for a touch to still count as a tap")]
        public float MaxTapDistance = 30f;

        [Header("Sounds")]
        public AudioClip SelectSound;
        public AudioClip PourSound;
        public AudioClip ErrorSound;

        private Vector3 _touchStartPos;
        private bool _isDragging = false;

        private void Awake()
        {
            _instance = this;
        }

        private void Update()
        {
            // Only handle input when playing
            if (GameManager.Instance.CurrentState != GameState.Playing)
                return;

            // Ignore taps while pour/undo animations are running
            if (LevelManager.Instance != null && LevelManager.Instance.IsBusy)
                return;

            HandleInput();
        }

        private void HandleInput()
        {
            #if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseInput();
            #elif UNITY_ANDROID || UNITY_IOS
            HandleTouchInput();
            #endif
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _touchStartPos = Input.mousePosition;
                _isDragging = false;
            }
            else if (Input.GetMouseButton(0))
            {
                float distance = Vector3.Distance(_touchStartPos, Input.mousePosition);
                if (distance > MaxTapDistance)
                {
                    _isDragging = true;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (!_isDragging && !IsPointerOverUI(-1))
                {
                    HandleTap(Input.mousePosition);
                }
                _isDragging = false;
            }
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        _touchStartPos = touch.position;
                        _isDragging = false;
                        break;

                    case TouchPhase.Moved:
                        float distance = Vector3.Distance(_touchStartPos, touch.position);
                        if (distance > MaxTapDistance)
                        {
                            _isDragging = true;
                        }
                        break;

                    case TouchPhase.Ended:
                        if (!_isDragging && !IsPointerOverUI(touch.fingerId))
                        {
                            HandleTap(touch.position);
                        }
                        _isDragging = false;
                        break;
                }
            }
        }

        /// <summary>
        /// Taps on buttons/popups must not also select tubes behind them
        /// </summary>
        private bool IsPointerOverUI(int pointerId)
        {
            if (EventSystem.current == null) return false;
            return pointerId < 0
                ? EventSystem.current.IsPointerOverGameObject()
                : EventSystem.current.IsPointerOverGameObject(pointerId);
        }

        private void HandleTap(Vector3 screenPosition)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 worldPosition = cam.ScreenToWorldPoint(screenPosition);

            // Raycast to detect tube
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, TubeLayer);

            if (hit.collider != null)
            {
                TubeController tappedTube = hit.collider.GetComponentInParent<TubeController>();
                if (tappedTube != null)
                {
                    OnTubeTapped(tappedTube);
                }
            }
            else
            {
                // Tapped empty space - deselect
                if (_selectedTube != null)
                {
                    _selectedTube.Deselect();
                    _selectedTube = null;
                }
            }
        }

        private void OnTubeTapped(TubeController tube)
        {
            if (_selectedTube == null)
            {
                // First tap - select tube
                if (!tube.IsEmpty())
                {
                    _selectedTube = tube;
                    _selectedTube.Select();

                    GameManager.Instance.PlaySound(SelectSound);
                    GameManager.Instance.TriggerHaptic();
                }
            }
            else
            {
                // Second tap - try to pour
                if (_selectedTube == tube)
                {
                    // Tapped same tube - deselect
                    _selectedTube.Deselect();
                    _selectedTube = null;
                }
                else
                {
                    // Try to pour from selected to tapped
                    bool success = LevelManager.Instance.TryMove(_selectedTube.TubeIndex, tube.TubeIndex);

                    if (success)
                    {
                        GameManager.Instance.PlaySound(PourSound);
                        GameManager.Instance.TriggerHaptic();
                    }
                    else
                    {
                        // Invalid move - play error feedback
                        tube.PlayInvalidMoveAnimation();
                        GameManager.Instance.PlaySound(ErrorSound);
                    }

                    // Deselect after attempt
                    _selectedTube.Deselect();
                    _selectedTube = null;
                }
            }
        }

        /// <summary>
        /// Forget the selection without animating (the tube is about to be destroyed)
        /// </summary>
        public void ResetSelection()
        {
            _selectedTube = null;
        }

        public void DeselectAll()
        {
            if (_selectedTube != null)
            {
                _selectedTube.Deselect();
                _selectedTube = null;
            }
        }
    }
}
