using System;
using System.Linq;

namespace Abalone.Board
{
    public class BoardData
    {
        public string name { get; internal set; }
        public int side { get; internal set; }
        public string[] players { get; internal set; }
        public int[,] data { get; internal set; }
        public int arraySize => side * 2 - 1;

        public int GetAt(AxialCoord position)
        {
            return data[position.x, position.z];
        }

        // Player 클래스 만들자..
        public void SetAt(AxialCoord position, int playerIndex)
        {
            data[position.x, position.z] = playerIndex;
        }
    }
}