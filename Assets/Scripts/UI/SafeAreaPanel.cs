using UnityEngine;

namespace WaterSort.Core
{
    /// <summary>
    /// Fits this RectTransform to Screen.safeArea so UI stays clear of notches
    /// and rounded corners. Put it on a full-screen panel under the Canvas.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaPanel : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _applied;
        private Vector2Int _screenSize;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea != _applied || Screen.width != _screenSize.x || Screen.height != _screenSize.y)
                Apply();
        }

        private void Apply()
        {
            Rect safeArea = Screen.safeArea;
            _applied = safeArea;
            _screenSize = new Vector2Int(Screen.width, Screen.height);

            if (Screen.width <= 0 || Screen.height <= 0) return;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
