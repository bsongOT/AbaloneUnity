using System.Collections;
using System.Collections.Generic;
using System.IO;
using Abalone.Data;
using UnityEngine;

namespace Abalone
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Board board;
        private GameContext context;

        private bool dragStarted;
        private Vector3 dragStartMousePosition;
        private CubeCoord dragStartMarblePosition;
        private Marble draggingMarble;
        private CubeDirection dragDirection;
        private bool dragDirectionFixed;
        private bool wasValidMove;

        private void Awake()
        {
            var boardString = File.ReadAllText("Assets/Maps/basic.abalone");
            var gameData = BoardStringParser.Parse(boardString);
            context = new GameContext(gameData);
            board.Create(gameData);
        }

        private void Update()
        {
            HandleMarbleMove();
        }

        private void HandleMarbleMove()
        {
            var currentMousePosition = MouseUtil.GetWorld(mainCamera);

            if (!dragStarted && Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hit))
                {
                    var marble = hit.transform.GetComponent<Marble>();
                    if (marble != null)
                    {
                        draggingMarble = marble;
                        dragStarted = true;
                        dragStartMousePosition = currentMousePosition;
                        dragStartMarblePosition = marble.visiblePosition;
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (draggingMarble != null)
                {
                    if (wasValidMove)
                    {

                    }
                    draggingMarble.transform.localPosition = dragStartMarblePosition.ToWorld();
                    draggingMarble = null;
                    dragDirectionFixed = false;
                }
                dragStarted = false;
            }

            if (dragStarted)
            {
                var angle = Mathf.Atan2(dragStartMousePosition.z - currentMousePosition.z, dragStartMousePosition.x - currentMousePosition.x) * Mathf.Rad2Deg;

                if (!dragDirectionFixed)
                {
                    dragDirection = (CubeDirection)Mathf.Round((angle + 150) / 60);
                }
                var directionCoord = dragDirection.ToCoord();

                var startPosition = dragStartMarblePosition;
                var endPosition = startPosition + directionCoord;

                var worldStartPosition = startPosition.ToWorld();
                var worldEndPosition = endPosition.ToWorld();

                const float dragThreshold = 6f;
                var dragLength = Vector3.Dot(currentMousePosition - dragStartMousePosition, directionCoord.ToWorld().normalized);

                var t = dragLength / dragThreshold;

                if (t > 0.15)
                {
                    dragDirectionFixed = true;
                }

                var marbleY = draggingMarble.yCurve.Evaluate(t);
                draggingMarble.transform.localPosition = Vector3.Lerp(worldStartPosition, worldEndPosition, t) + new Vector3(0, marbleY, 0);

                if (t > 0.1)
                {
                    // if (!CanPushMarble(draggingMarble, dragDirection))
                    // {
                    //     t = 0.1f;
                    // }
                }
                else if (Mathf.Approximately(t, 1))
                {
                    wasValidMove = true;
                }

                Debug.DrawLine(worldStartPosition, worldEndPosition);
            }
        }
    }
}