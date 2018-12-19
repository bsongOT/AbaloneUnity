using System.Collections;
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
        [SerializeField] private Transform boardContainer;
        [SerializeField] private GameObject boardInnerPartPrefab;
        [SerializeField] private GameObject boardOuterSidePrefab;
        [SerializeField] private GameObject boardOuterVertexPrefab;

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

            for (var x = 0; x < size; x++)
            {
                for (var z = 0; z < size; z++)
                {
                    if (Mathf.Abs(x + positionOffset.x + z + positionOffset.z) > cutThreshold) continue;
                    CreateBoardInnerPart(new AxialCoord(x, z), positionOffset);
                }
            }

            CreateBoardOuterParts(positionOffset);
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

        private void CreateBoardInnerPart(AxialCoord coord, AxialCoord positionOffset)
        {
            var partObject = Instantiate(boardInnerPartPrefab, (coord + positionOffset).ToWorld(), Quaternion.identity, boardContainer);
            partObject.name = $"Inner{coord}";
            partObject.transform.localPosition += new Vector3(0, Constants.boardOffsetY, 0);
        }

        private void CreateBoardOuterParts(AxialCoord positionOffset)
        {
            var radius = boardData.side - 1;
            var coord = CubeDirection.BottomRight.ToCoord() * radius;

            for (int direction = 0; direction < 6; direction++)
            {
                for (int i = 0; i <= radius; i++)
                {
                    var rotation = Quaternion.Euler(0, (direction - 1) * -60, 0);
                    var prefab = boardOuterSidePrefab;

                    if (i == 0)
                    {
                        prefab = boardOuterVertexPrefab;
                    }

                    var partObject = Instantiate(prefab, coord.ToWorld(), rotation, boardContainer);
                    partObject.name = $"Outer{((AxialCoord)coord - positionOffset)}";
                    partObject.transform.localPosition += new Vector3(0, Constants.boardOffsetY, 0);

                    if (i == 0) continue;
                    coord += ((CubeDirection)direction).ToCoord();
                }
            }
        }
    }
}