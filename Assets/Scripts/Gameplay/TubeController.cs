using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

namespace WaterSort.Core
{
    /// <summary>
    /// Controls a single tube - manages liquid stack, visual representation, animations
    /// </summary>
    public class TubeController : MonoBehaviour
    {
        [Header("Tube Settings")]
        public int TubeIndex;
        public int MaxCapacity = 4;

        [Header("Liquid Stack")]
        private List<LiquidSegment> _liquidStack = new List<LiquidSegment>();

        [Header("Visual References")]
        public Transform LiquidContainer;
        public SpriteRenderer TubeRenderer;
        public GameObject LiquidSegmentPrefab;

        [Header("Animation Settings")]
        public float SegmentHeight = 0.25f;
        public float SelectionScale = 1.15f;
        public float PourDuration = 0.4f;

        private bool _isSelected = false;
        private Vector3 _originalScale;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        public void Initialize(int index, List<LiquidColor> initialColors)
        {
            TubeIndex = index;

            // Clear any existing liquid
            ClearLiquid();

            // Create liquid segments from bottom to top
            foreach (var color in initialColors)
            {
                AddLiquidInstant(color, 1);
            }
        }

        private void ClearLiquid()
        {
            foreach (var segment in _liquidStack)
            {
                if (segment.VisualObject != null)
                    Destroy(segment.VisualObject);
            }
            _liquidStack.Clear();
        }

        public void AddLiquid(LiquidColor color, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                AddLiquidWithAnimation(color);
            }
        }

        private void AddLiquidInstant(LiquidColor color, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                CreateLiquidSegment(color, _liquidStack.Count);
            }
        }

        private void AddLiquidWithAnimation(LiquidColor color)
        {
            int targetIndex = _liquidStack.Count;
            GameObject segmentObj = CreateLiquidSegment(color, targetIndex);

            // Animate pour in
            Transform segmentTransform = segmentObj.transform;
            Vector3 finalPos = segmentTransform.localPosition;
            segmentTransform.localPosition = finalPos + Vector3.up * 2f;

            segmentTransform.DOLocalMove(finalPos, PourDuration)
                .SetEase(Ease.OutBounce);
        }

        private GameObject CreateLiquidSegment(LiquidColor color, int index)
        {
            GameObject segmentObj = Instantiate(LiquidSegmentPrefab, LiquidContainer);
            SpriteRenderer renderer = segmentObj.GetComponent<SpriteRenderer>();

            if (renderer != null)
            {
                renderer.color = GetColorFromEnum(color);
            }

            // Position based on index
            float yPos = index * SegmentHeight;
            segmentObj.transform.localPosition = new Vector3(0, yPos, 0);

            LiquidSegment segment = new LiquidSegment
            {
                Color = color,
                VisualObject = segmentObj
            };

            _liquidStack.Add(segment);
            return segmentObj;
        }

        public void RemoveTop(int amount)
        {
            for (int i = 0; i < amount && _liquidStack.Count > 0; i++)
            {
                int lastIndex = _liquidStack.Count - 1;
                LiquidSegment segment = _liquidStack[lastIndex];

                // Animate removal
                if (segment.VisualObject != null)
                {
                    segment.VisualObject.transform.DOScale(0, 0.2f).OnComplete(() =>
                    {
                        Destroy(segment.VisualObject);
                    });
                }

                _liquidStack.RemoveAt(lastIndex);
            }
        }

        public LiquidColor GetTopColor()
        {
            if (_liquidStack.Count == 0)
                return LiquidColor.None;

            return _liquidStack[_liquidStack.Count - 1].Color;
        }

        public int GetTopColorCount()
        {
            if (_liquidStack.Count == 0)
                return 0;

            LiquidColor topColor = GetTopColor();
            int count = 0;

            for (int i = _liquidStack.Count - 1; i >= 0; i--)
            {
                if (_liquidStack[i].Color == topColor)
                    count++;
                else
                    break;
            }

            return count;
        }

        public bool IsEmpty()
        {
            return _liquidStack.Count == 0;
        }

        public bool IsFull()
        {
            return _liquidStack.Count >= MaxCapacity;
        }

        public bool IsComplete()
        {
            // Complete if empty OR filled with single color
            if (_liquidStack.Count == 0)
                return true;

            if (_liquidStack.Count != MaxCapacity)
                return false;

            LiquidColor firstColor = _liquidStack[0].Color;
            return _liquidStack.All(seg => seg.Color == firstColor);
        }

        public int GetAvailableSpace()
        {
            return MaxCapacity - _liquidStack.Count;
        }

        public void Select()
        {
            _isSelected = true;

            // Scale up animation
            transform.DOScale(_originalScale * SelectionScale, 0.2f)
                .SetEase(Ease.OutBack);

            // Glow effect (if you have a glow sprite)
            // TubeRenderer.material.SetFloat("_GlowIntensity", 1f);
        }

        public void Deselect()
        {
            _isSelected = false;

            // Scale back animation
            transform.DOScale(_originalScale, 0.2f)
                .SetEase(Ease.InBack);

            // Remove glow
            // TubeRenderer.material.SetFloat("_GlowIntensity", 0f);
        }

        public void PlayInvalidMoveAnimation()
        {
            // Shake animation to indicate invalid move
            transform.DOShakePosition(0.3f, 0.1f, 20, 90, false, true);
        }

        private Color GetColorFromEnum(LiquidColor liquidColor)
        {
            switch (liquidColor)
            {
                case LiquidColor.Red: return new Color(1f, 0.2f, 0.2f);
                case LiquidColor.Blue: return new Color(0.2f, 0.4f, 1f);
                case LiquidColor.Green: return new Color(0.2f, 0.9f, 0.3f);
                case LiquidColor.Yellow: return new Color(1f, 0.9f, 0.2f);
                case LiquidColor.Purple: return new Color(0.7f, 0.2f, 0.9f);
                case LiquidColor.Orange: return new Color(1f, 0.5f, 0.1f);
                case LiquidColor.Pink: return new Color(1f, 0.4f, 0.7f);
                case LiquidColor.Cyan: return new Color(0.2f, 0.9f, 0.9f);
                case LiquidColor.Brown: return new Color(0.6f, 0.4f, 0.2f);
                case LiquidColor.Gray: return new Color(0.6f, 0.6f, 0.6f);
                case LiquidColor.Lime: return new Color(0.7f, 1f, 0.2f);
                case LiquidColor.Teal: return new Color(0.2f, 0.7f, 0.6f);
                default: return Color.white;
            }
        }

        public List<LiquidSegment> GetLiquidStack()
        {
            return new List<LiquidSegment>(_liquidStack);
        }
    }

    [System.Serializable]
    public class LiquidSegment
    {
        public LiquidColor Color;
        public GameObject VisualObject;
    }

    public enum LiquidColor
    {
        None = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4,
        Purple = 5,
        Orange = 6,
        Pink = 7,
        Cyan = 8,
        Brown = 9,
        Gray = 10,
        Lime = 11,
        Teal = 12
    }
}
