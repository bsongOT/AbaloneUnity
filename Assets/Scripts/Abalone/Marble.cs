using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abalone
{
    public class Marble : MonoBehaviour
    {
        public AxialCoord arrayPosition { get; private set; }
        public CubeCoord visiblePosition => arrayPosition + boardSettings.placementOffset;

        public AnimationCurve yCurve;

        public int playerIndex { get; private set; }
        private new Renderer renderer;
        private BoardSettings boardSettings;

        [SerializeField] private Color overColor;
        [SerializeField] private Color selectColor;
        private bool CanPaintOverColor = true;
        private Color originalColor;

        private void Awake()
        {
            renderer = GetComponent<Renderer>();
        }

        private void OnMouseOver()
        {
            if (CanPaintOverColor)
            {
                renderer.material.color = overColor;
            }
        }

        private void OnMouseExit()
        {
            if (CanPaintOverColor)
            {
                renderer.material.color = originalColor;
            }
        }

        public void Init(BoardSettings boardSettings, Color color, AxialCoord arrayPosition, int playerIndex)
        {
            this.boardSettings = boardSettings;
            this.playerIndex = playerIndex;
            SetColor(color);
            SetPosition(arrayPosition);
        }

        public void SetColor(Color color)
        {
            originalColor = color;
            renderer.material.color = color;
        }

        public void SetPosition(AxialCoord arrayPosition)
        {
            this.arrayPosition = arrayPosition;
        }

        public void SetArrayPosition(AxialCoord axialCoord)
        {
            arrayPosition = axialCoord;
        }

        public void PaintOrigin()
        {
            renderer.material.color = originalColor;
        }

        public void PaintSelectColor()
        {
            CanPaintOverColor = false;
            renderer.material.color = selectColor;
        }
    }
}
