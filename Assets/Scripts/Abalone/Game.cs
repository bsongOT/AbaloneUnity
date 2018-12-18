﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Abalone.Board;

namespace Abalone
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private Transform marbleContainer;
        [SerializeField] private Marble marblePrefab;
        private BoardData boardData;
        private GameContext context;

        private void Awake()
        {
            var boardString = File.ReadAllText("Assets/Maps/basic.abalone");
            boardData = BoardDataParser.Parse(boardString);
            context = new GameContext(boardData);
            CreateBoard();
            CreateMarbles();
        }

        private void CreateBoard()
        {
            var size = boardData.arraySize;
            var cutThreshold = boardData.side - 1;
            var positionOffset = new AxialCoord(-cutThreshold, -cutThreshold);
            // for (var x = 0; x < size; x++)
            // {
            //     for (var z = 0; z < size; z++)
            //     {
            //         if (Mathf.Abs(x + positionOffset.x + z + positionOffset.z) > cutThreshold) continue;
            //         CreateMarble(0, new AxialCoord(x, z), positionOffset);
            //     }
            // } 
        }

        private void CreateMarbles()
        {
            var size = boardData.arraySize;
            var cutThreshold = boardData.side - 1;
            var positionOffset = new AxialCoord(-cutThreshold, -cutThreshold);

            for (var x = 0; x < size; x++)
            {
                for (var z = 0; z < size; z++)
                {
                    var playerIndex = boardData.data[x, z];
                    if (playerIndex == 0) continue;
                    CreateMarble(playerIndex, new AxialCoord(x, z), positionOffset);
                }
            }
        }

        private void CreateMarble(int playerIndex, AxialCoord coord, AxialCoord positionOffset)
        {
            var marbleObject = Instantiate(marblePrefab, (coord + positionOffset).ToWorld(), Quaternion.identity, marbleContainer);
            marbleObject.name = coord.ToString();

            var marble = marbleObject.GetComponent<Marble>();
            marble.SetColor(context.GetPlayerColor(playerIndex));
        }
    }
}