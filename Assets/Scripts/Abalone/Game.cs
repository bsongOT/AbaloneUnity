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
        private GameData gamedata;
        private GameContext context;

        private bool dragStarted;
        private Vector3 dragStartMousePosition;
        private CubeCoord dragStartMarblePosition;
        private Marble draggingMarble;
        private CubeDirection dragDirection;
        private CubeDirection chooseDirection;
        private CubeCoord chosenMarbleStart;
        //Test ON
        private int howManyIsChosen = 1;
        //Test OFF
        private bool dragDirectionFixed;
        private bool wasValidMove;

        private void Awake()
        {
            var boardString = File.ReadAllText("Assets/Maps/basic.abalone");
            var gameData = BoardStringParser.Parse(boardString);
            context = new GameContext(gameData);
            board.Create(gameData);
            context.marbles = new GameObject[board.settings.arraySize, board.settings.arraySize];
            for (int i = 0; i < board.settings.arraySize; i++)
            {
                for (int j = 0; j < board.settings.arraySize; j++)
                {
                    context.marbles[i, j] = board.context.marbles[i, j];
                }
            }
            gamedata = gameData;
        }

        private void Update()
        {
            HandleMarbleMove();
        }

        private bool CanPushMarble(CubeCoord chosenStart, CubeDirection chosenDirection, int howMany, CubeCoord moveDirection)
        {
            for (int i = 0; i < howMany; i++)
            {
                var positionToBeMoved = chosenStart + chosenDirection.ToCoord() * i + moveDirection;

                if (positionToBeMoved.x * positionToBeMoved.x >= board.settings.side * board.settings.side)
                    return false;
                
                if (positionToBeMoved.z * positionToBeMoved.z >= board.settings.side * board.settings.side)
                    return false;

                if (Mathf.Abs(positionToBeMoved.x + positionToBeMoved.z) > board.settings.cutThreshold)
                    return false;
                if (FindWithCoord((AxialCoord)positionToBeMoved - board.settings.placementOffset) != null)
                {
                    var marbleToBeMoved = FindWithCoord((AxialCoord)positionToBeMoved - board.settings.placementOffset);
                    if (context.currentPlayerIndex == marbleToBeMoved.playerIndex && !WasChosen(marbleToBeMoved.visiblePosition - chosenStart, chosenDirection, howMany))
                        return false;
                }
            }
            return true;
        }

        private Marble FindWithCoord(AxialCoord axialCoord)
        {
            if (context.marbles[axialCoord.x, axialCoord.z] != null)
            {
                return context.marbles[axialCoord.x, axialCoord.z].GetComponent<Marble>();
            }
            return null;
        }

        private bool WasChosen(CubeCoord cubeCoord, CubeDirection cubeDirection, int chosen)
        {
            if (chosen == 1)
                return cubeCoord == new CubeCoord(0, 0, 0);
            if (chosen == 2)
                return cubeCoord == new CubeCoord(0, 0, 0) || cubeCoord == cubeDirection.ToCoord();
            if (chosen == 3)
                return cubeCoord == new CubeCoord(0, 0, 0) || cubeCoord == cubeDirection.ToCoord() || cubeCoord == cubeDirection.ToCoord() * 2;
            return false;
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
                        if (context.playerContext == "Choose" && context.currentPlayerIndex == marble.playerIndex)
                        {
                            marble.PaintSelectColor();
                            draggingMarble = marble;
                            dragStarted = true;
                            dragStartMousePosition = marble.visiblePosition.ToWorld();
                            dragStartMarblePosition = marble.visiblePosition;
                        }

                        if (context.playerContext == "Move" && WasChosen(marble.visiblePosition - chosenMarbleStart, chooseDirection, howManyIsChosen))
                        {
                            dragStarted = true;
                            dragStartMousePosition = marble.visiblePosition.ToWorld();
                        }
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                switch (context.playerContext) {
                    case "Choose":
                        if (draggingMarble != null)
                        {
                        chosenMarbleStart = dragStartMarblePosition;
                        chooseDirection = dragDirection;
                        context.playerContext = "Move";
                        draggingMarble = null;
                        }
                        break;
                    case "Move":
                        if (wasValidMove)
                        {
                            //Test Start
                            /*
                            draggingMarble.transform.localPosition = (dragStartMarblePosition + dragDirection.ToCoord()).ToWorld();
                            gamedata.SetAt(dragStartMarblePosition.ToAxialCoord() - board.settings.placementOffset, 0);
                            gamedata.SetAt(dragStartMarblePosition.ToAxialCoord() - board.settings.placementOffset + dragDirection.ToAxialCoord(), context.currentPlayerIndex);
                            draggingMarble.SetArrayPosition(dragStartMarblePosition.ToAxialCoord() - board.settings.placementOffset + dragDirection.ToAxialCoord());
                            context.NextTurn();
                            wasValidMove = false;
                            */
                            for (int i = 0; i < howManyIsChosen; i++)
                            {
                                var chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * i;
                                var worldChosenPosition = chosenPosition.ToWorld();
                                var worldMovePosition = (chosenPosition + dragDirection.ToCoord()).ToWorld();
                                var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                                marbles.transform.localPosition = worldMovePosition;
                                gamedata.SetAt((AxialCoord)chosenPosition - board.settings.placementOffset, 0);
                                gamedata.SetAt((AxialCoord)chosenPosition - board.settings.placementOffset + (AxialCoord)(dragDirection.ToCoord()), context.currentPlayerIndex);
                                marbles.SetArrayPosition((AxialCoord)chosenPosition - board.settings.placementOffset + (AxialCoord)(dragDirection.ToCoord()));
                                marbles.PaintOrigin();
                            }
                            context.NextTurn();
                            context.playerContext = "Choose";
                            wasValidMove = false;
                            //Test End
                        }
                        else
                        {
                            //draggingMarble.transform.localPosition = dragStartMarblePosition.ToWorld();
                            for (int i = 0; i < howManyIsChosen; i++)
                            {
                                var chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * i;
                                var worldChosenPosition = chosenPosition.ToWorld();
                                var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                                marbles.transform.localPosition = worldChosenPosition;
                            }
                        }
                        break;
                    default:
                        break;
                }

                dragDirectionFixed = false;
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

                switch (context.playerContext)
                {
                    case "Choose":
                        if (t > 0.15)
                        {
                            var secondMarble = FindWithCoord((AxialCoord)(startPosition + directionCoord) - board.settings.placementOffset);
                            secondMarble.PaintSelectColor();
                            howManyIsChosen = 2;
                        }
                        if (t > 0.43)
                        {
                            var thirdMarble = FindWithCoord((AxialCoord)(startPosition + directionCoord * 2) - board.settings.placementOffset);
                            thirdMarble.PaintSelectColor();
                            howManyIsChosen = 3;
                        }
                        break;
                    case "Move":
                        if (t > 0.15)
                        {
                            dragDirectionFixed = true;
                        }

                        if (!CanPushMarble(chosenMarbleStart, chooseDirection, howManyIsChosen, directionCoord))
                        {
                            t = 0.1f;
                        }

                        for (int i = 0; i < howManyIsChosen; i++) {
                            var chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * i;
                            var worldChosenPosition = chosenPosition.ToWorld();
                            var worldMovePosition = (chosenPosition + directionCoord).ToWorld();
                            var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                            var marbleY = marbles.yCurve.Evaluate(t);
                            marbles.transform.localPosition = Vector3.Lerp(worldChosenPosition, worldMovePosition, t) + new Vector3(0, marbleY, 0);
                        }

                        if (t >= 1)
                        {
                            wasValidMove = true;
                        }
                        break;
                    default:
                        break;
                }

                Debug.DrawLine(worldStartPosition, worldEndPosition);
            }
        }
    }
}