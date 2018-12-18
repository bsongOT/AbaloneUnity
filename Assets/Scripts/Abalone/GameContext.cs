using UnityEngine;

namespace Abalone
{
    // Holds match's information and utility functions
    public class GameContext
    {
        private Color[] playerColors;

        // TODO : exterminated marble numbers

        public GameContext(Board.BoardData boardData)
        {
            var playerCount = boardData.players.Length;
            playerColors = new Color[playerCount];
            for (var i = 0; i < playerCount; i++)
            {
                ColorUtility.TryParseHtmlString(boardData.players[i], out playerColors[i]);
            }
        }

        public Color GetPlayerColor(int playerIndex)
        {
            return playerColors[playerIndex - 1];
        }
    }
}