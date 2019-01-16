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
        private int howManyIsChosen = 1;
        private int howManyWillPush;
        private bool dragDirectionFixed;
        private bool wasValidMove;
        private bool opponentPush;

        private void Awake()
        {
            var boardString = File.ReadAllText("Assets/Maps/basic.abalone");
            var gameData = BoardStringParser.Parse(boardString);
            context = new GameContext(gameData);
            board.Create(gameData);
            context = board.context;
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

                    if (context.currentPlayerIndex != marbleToBeMoved.playerIndex && chosenDirection.ToCoord() != moveDirection && chosenDirection.ToCoord() != moveDirection * (-1))
                        return false;
                }
            }

            if (chosenDirection.ToCoord() == moveDirection || chosenDirection.ToCoord() == moveDirection * (-1))
            {
                howManyWillPush = 0;

                while (true)
                {
                    CubeCoord pushTargetPosition = new CubeCoord(0, 0, 0);

                    if(chosenDirection.ToCoord() == moveDirection)
                        pushTargetPosition = chosenStart + chosenDirection.ToCoord() * howMany + moveDirection * howManyWillPush;
                    if(chosenDirection.ToCoord() == moveDirection * (-1))
                        pushTargetPosition = chosenStart - chosenDirection.ToCoord() + moveDirection * howManyWillPush;

                    if (FindWithCoord((AxialCoord)pushTargetPosition - board.settings.placementOffset) != null)
                    {
                        var pushTarget = FindWithCoord((AxialCoord)pushTargetPosition - board.settings.placementOffset);

                        if (pushTarget.playerIndex == context.currentPlayerIndex)
                            return false;

                        howManyWillPush++;
                    }

                    else
                    {
                        break;
                    }
                }
                if (howMany <= howManyWillPush)
                    return false;
                else
                    opponentPush = true;
            }

            return true;
        }

        private Marble FindWithCoord(AxialCoord axialCoord)
        {
            if (axialCoord.x < 0 || axialCoord.x >= board.settings.arraySize)
                return null;
            if (axialCoord.z < 0 || axialCoord.z >= board.settings.arraySize)
                return null;
            if (Mathf.Abs(axialCoord.x + board.settings.placementOffset.x + axialCoord.z + board.settings.placementOffset.z) > board.settings.cutThreshold)
                return null;
            if (context.marbles[axialCoord.x, axialCoord.z] != null)
                return context.marbles[axialCoord.x, axialCoord.z].GetComponent<Marble>();
            return null;
        }

        private bool WasChosen(CubeCoord cubeCoord, CubeDirection cubeDirection, int chosen)
        {
            switch (chosen) {
                case 1:
                    return cubeCoord == new CubeCoord(0, 0, 0); break;
                case 2: 
                    return cubeCoord == new CubeCoord(0, 0, 0) || cubeCoord == cubeDirection.ToCoord(); break;
                case 3:
                    return cubeCoord == new CubeCoord(0, 0, 0) || cubeCoord == cubeDirection.ToCoord() || cubeCoord == cubeDirection.ToCoord() * 2; break;
                default:
                    return false; break;
            }
        }

        private void SelectCancel()
        {
            for (int i = 0; i < howManyIsChosen; i++)
            {
                var chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * i;
                var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                marbles.PaintOrigin(true);
            }
            context.playerContext = "Choose";
        }

        private void HandleMarbleMove()
        {
            var currentMousePosition = MouseUtil.GetWorld(mainCamera);
            //Debug.Log(currentMousePosition);

            if (Input.GetKeyDown(KeyCode.Escape) && context.playerContext == "Move")
            {
                SelectCancel();
            }

            if (!dragStarted && Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hit))
                {
                    var marble = hit.transform.GetComponent<Marble>();
                    
                    if (marble != null)
                    {
                        switch (context.playerContext)
                        {
                            case "Choose":
                                if (context.currentPlayerIndex == marble.playerIndex && !marble.fallen)
                                {
                                    marble.PaintSelectColor();
                                    draggingMarble = marble;
                                    dragStarted = true;
                                    dragStartMousePosition = marble.visiblePosition.ToWorld();
                                    dragStartMarblePosition = marble.visiblePosition;
                                }
                                break;

                            case "Move":
                                if (WasChosen(marble.visiblePosition - chosenMarbleStart, chooseDirection, howManyIsChosen))
                                {
                                     dragStarted = true;
                                     dragStartMousePosition = marble.visiblePosition.ToWorld();
                                }

                                else
                                {
                                    SelectCancel();
                                }
                                break;

                            default:
                                break;
                        }
                        if (marble.fallen)
                        {
                            draggingMarble = marble;
                            dragStarted = true;
                            dragStartMousePosition = marble.visiblePosition.ToWorld();
                            dragStartMarblePosition = marble.visiblePosition;
                        }
                    }
                }

                else if (context.playerContext == "Move")
                {
                    SelectCancel();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                switch (context.playerContext) {
                    case "Choose":
                        if (draggingMarble != null)
                        {
                            if (!draggingMarble.fallen) {
                                chosenMarbleStart = dragStartMarblePosition;
                                chooseDirection = dragDirection;
                                context.playerContext = "Move";
                                draggingMarble = null;
                            }
                        }
                        break;
                    case "Move":
                        if (wasValidMove)
                        {
                            for (int i = 0; i < howManyIsChosen; i++)
                            {
                                var chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * i;
                                var worldChosenPosition = chosenPosition.ToWorld();
                                var worldMovePosition = (chosenPosition + dragDirection.ToCoord()).ToWorld();
                                var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                                marbles.transform.localPosition = worldMovePosition;
                                marbles.SetArrayPosition((AxialCoord)chosenPosition - board.settings.placementOffset + (AxialCoord)(dragDirection.ToCoord()));
                                marbles.PaintOrigin(true);
                            }

                            if (opponentPush)
                            {
                                for (int i = 0; i < howManyWillPush; i++)
                                {
                                    CubeCoord chosenPosition = new CubeCoord(0, 0, 0);
                                    if (chooseDirection.ToCoord() == dragDirection.ToCoord())
                                        chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * (howManyIsChosen + i);
                                    if (chooseDirection.ToCoord() == dragDirection.ToCoord() * (-1))
                                        chosenPosition = chosenMarbleStart - chooseDirection.ToCoord() + dragDirection.ToCoord() * i;

                                    var movePosition = chosenPosition + dragDirection.ToCoord();
                                    var worldChosenPosition = chosenPosition.ToWorld();
                                    var worldMovePosition = (movePosition).ToWorld();
                                    var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                                    marbles.transform.localPosition = worldMovePosition;
                                    marbles.SetArrayPosition((AxialCoord)chosenPosition - board.settings.placementOffset + (AxialCoord)(dragDirection.ToCoord()));
                                }

                                for (int j = 0; j < howManyWillPush; j++)
                                {
                                    int i;
                                    if (chooseDirection.ToCoord() == dragDirection.ToCoord() || chooseDirection.ToCoord() == dragDirection.ToCoord() * (-1))
                                    {
                                        i = howManyWillPush - j - 1;
                                    }
                                    else
                                    {
                                        i = j;
                                    }

                                    CubeCoord chosenPosition = new CubeCoord(0, 0, 0);
                                    if (chooseDirection.ToCoord() == dragDirection.ToCoord())
                                        chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * (howManyIsChosen + i);
                                    if (chooseDirection.ToCoord() == dragDirection.ToCoord() * (-1))
                                        chosenPosition = chosenMarbleStart - chooseDirection.ToCoord() + dragDirection.ToCoord() * i;

                                    var beforePosition = (AxialCoord)chosenPosition - board.settings.placementOffset;
                                    var afterPosition = beforePosition + (AxialCoord)(dragDirection.ToCoord());
                                    gamedata.SetAt(beforePosition, 0);
                                    if (afterPosition.x < 0 || afterPosition.x >= board.settings.arraySize)
                                    {
                                        FindWithCoord(beforePosition).FallAnimation(dragDirection.ToCoord().ToWorld());
                                        continue;
                                    }
                                    if (afterPosition.z < 0 || afterPosition.z >= board.settings.arraySize)
                                    {
                                        FindWithCoord(beforePosition).FallAnimation(dragDirection.ToCoord().ToWorld());
                                        continue;
                                    }

                                    if (Mathf.Abs(afterPosition.x + board.settings.placementOffset.x + afterPosition.z + board.settings.placementOffset.z) > board.settings.cutThreshold)
                                    {
                                        gamedata.SetAt(afterPosition, 0);
                                        context.marbles[beforePosition.x, beforePosition.z] = null;
                                        FindWithCoord(beforePosition).FallAnimation(dragDirection.ToCoord().ToWorld());
                                        continue;
                                    }
                                    gamedata.SetAt(afterPosition, context.currentPlayerIndex);
                                    context.MoveData(beforePosition, afterPosition);
                                }
                            }

                            for (int j = 0; j < howManyIsChosen; j++)
                            {
                                int i;
                                if (chooseDirection.ToCoord() == dragDirection.ToCoord())
                                {
                                    i = howManyIsChosen - j - 1;
                                }
                                else {
                                    i = j;
                                }
                                var chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * i;
                                var beforePosition = (AxialCoord)chosenPosition - board.settings.placementOffset;
                                var afterPosition = beforePosition + (AxialCoord)(dragDirection.ToCoord());
                                gamedata.SetAt(beforePosition, 0);
                                gamedata.SetAt(afterPosition, context.currentPlayerIndex);
                                context.MoveData(beforePosition, afterPosition);
                            }
                            opponentPush = false;
                            context.NextTurn();
                            context.playerContext = "Choose";
                            wasValidMove = false;
                        }
                        else
                        {
                            for (int i = 0; i < howManyIsChosen; i++)
                            {
                                var chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * i;
                                var worldChosenPosition = chosenPosition.ToWorld();
                                var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                                marbles.transform.localPosition = worldChosenPosition;
                            }

                            if (opponentPush)
                            {
                                for (int i = 0; i < howManyWillPush; i++)
                                {
                                    CubeCoord chosenPosition = new CubeCoord(0, 0, 0);
                                    if (chooseDirection.ToCoord() == dragDirection.ToCoord())
                                        chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * (howManyIsChosen + i);
                                    if (chooseDirection.ToCoord() == dragDirection.ToCoord() * (-1))
                                        chosenPosition = chosenMarbleStart - chooseDirection.ToCoord() + dragDirection.ToCoord() * i;

                                    var worldChosenPosition = chosenPosition.ToWorld();
                                    var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                                    marbles.transform.localPosition = worldChosenPosition;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                if (draggingMarble != null)
                {
                    if (draggingMarble.fallen)
                    {
                        draggingMarble.GetComponent<Rigidbody>().isKinematic = false;
                    }
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
                dragDirectionFixed = (t > 0.15);

                switch (context.playerContext)
                {
                    case "Choose":
                        var secondMarble = FindWithCoord((AxialCoord)(startPosition + directionCoord) - board.settings.placementOffset);
                        var thirdMarble = FindWithCoord((AxialCoord)(startPosition + directionCoord * 2) - board.settings.placementOffset);

                        if (t <= 0.15)
                        {
                            if (secondMarble != null)
                                secondMarble.PaintOrigin(false);
                            howManyIsChosen = 1;
                        }

                        if (t > 0.15 && secondMarble != null && secondMarble.playerIndex == context.currentPlayerIndex)
                        {
                            secondMarble.PaintSelectColor();
                            if(thirdMarble != null)
                                thirdMarble.PaintOrigin(false);
                            howManyIsChosen = 2;
                        }

                        if (t > 0.43 && thirdMarble != null && thirdMarble.playerIndex == context.currentPlayerIndex)
                        {
                            thirdMarble.PaintSelectColor();
                            howManyIsChosen = 3;
                        }

                        break;
                    case "Move":
                        if (!CanPushMarble(chosenMarbleStart, chooseDirection, howManyIsChosen, directionCoord))
                        {
                            t = 0.05f;
                        }

                        for (int i = 0; i < howManyIsChosen; i++) {
                            var chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * i;
                            var worldChosenPosition = chosenPosition.ToWorld();
                            var worldMovePosition = (chosenPosition + directionCoord).ToWorld();
                            var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                            var marbleY = marbles.yCurve.Evaluate(t);
                            marbles.transform.localPosition = Vector3.Lerp(worldChosenPosition, worldMovePosition, t) + new Vector3(0, marbleY, 0);
                        }
                        if (opponentPush)
                        {
                            for (int i = 0; i < howManyWillPush; i++)
                            {
                                CubeCoord chosenPosition = new CubeCoord(0, 0, 0);
                                if (chooseDirection.ToCoord() == directionCoord)
                                    chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * (howManyIsChosen + i);
                                if (chooseDirection.ToCoord() == directionCoord * (-1))
                                    chosenPosition = chosenMarbleStart - chooseDirection.ToCoord() + directionCoord * i;
                                var worldChosenPosition = chosenPosition.ToWorld();
                                var worldMovePosition = (chosenPosition + directionCoord).ToWorld();
                                var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                                var marbleY = marbles.yCurve.Evaluate(t);
                                marbles.transform.localPosition = Vector3.Lerp(worldChosenPosition, worldMovePosition, t) + new Vector3(0, marbleY, 0);
                            }
                        }

                        wasValidMove = (t >= 1);
                        break;
                    default:
                        break;
                }

                if (draggingMarble != null) {
                    if (draggingMarble.fallen)
                        draggingMarble.transform.localPosition = draggingMarble.DragLimit(draggingMarble.transform.localPosition, currentMousePosition);
                }

                Debug.DrawLine(worldStartPosition, worldEndPosition);
            }
        }
    }
}