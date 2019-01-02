using Abalone.Data;
using UnityEngine;

namespace Abalone
{
    // Holds match's information and utility functions
    public class GameContext
    {
        public readonly Board board;
        public int currentPlayerIndex { get; private set; }
        public int playerCount;

        // TODO : exterminated marble numbers

        public GameContext(GameData gameData)
        {
        }

        public void NextTurn()
        {
            currentPlayerIndex++;
            if (currentPlayerIndex >= playerCount)
            {
                currentPlayerIndex = 0;
            }
        }
    }
}