using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tak_Engine.Game
{
    internal static class Types
    {
        public enum Piece
        {
            Flat,
            Standing,
            Capstone
        }
        public enum  Player
        {
            White,
            Black
        }
        public enum Move
        {
            Place,
            PieceMove
        }
        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }
        public enum GameOver
        {
            NotOver,
            WhiteRoad,
            BlackRoad,
            WhiteStones,
            BlackStones,
            Tie
        }
    }
}
