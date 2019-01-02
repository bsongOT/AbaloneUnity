using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Abalone
{
    public class Marble : MonoBehaviour
    {
        public AxialCoord arrayPosition { get; private set; }
        public CubeCoord visiblePosition => arrayPosition + context.board.settings.placementOffset;

        public AnimationCurve yCurve;

        private int playerIndex;
        private new Renderer renderer;
        private GameContext context;

        [SerializeField] private Color overColor;
        private Color originalColor;

        private void Awake()
        {
            renderer = GetComponent<Renderer>();
        }

        private void OnMouseOver()
        {
            renderer.material.color = overColor;
        }

        private void OnMouseExit()
        {
            renderer.material.color = originalColor;
        }

        public void Init(GameContext context, Color color, AxialCoord arrayPosition)
        {
            this.context = context;
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
    }
}
