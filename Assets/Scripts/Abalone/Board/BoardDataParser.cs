using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Abalone.Board
{
    public class BoardDataParser
    {
        private static Regex playerMarbleNotation = new Regex("([0-9]+)([a-zA-Z][0-9]+)");

        // TODO : handle invalid string
        public static BoardData Parse(string boardString)
        {
            var lines = boardString.Split('\n');

            var boardData = new BoardData();

            foreach (var line in lines)
            {
                if (line.Contains("="))
                {
                    // metadata
                    var splitted = line.Split('=');
                    var left = splitted[0].Trim().ToLower();
                    var right = splitted[1].Trim();

                    switch (left)
                    {
                        case "name":
                            boardData.name = right;
                            break;
                        case "size":
                            var side = int.Parse(right);
                            boardData.side = side;
                            boardData.data = new int[boardData.arraySize, boardData.arraySize];
                            break;
                        case "players":
                            boardData.players = right.Split(',');
                            break;
                    }
                }
                else
                {
                    // marble placement
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var splitted = line.Split(' ');
                    foreach (var marble in splitted)
                    {
                        var trimmed = marble.Trim();
                        var match = playerMarbleNotation.Match(trimmed);
                        var player = int.Parse(match.Groups[1].Value);
                        var position = AxialCoord.FromNotation(match.Groups[2].Value, boardData.arraySize);
                        Debug.Log(position);
                        boardData.SetAt(position, player);
                    }
                }
            }

            return boardData;
        }
    }
}