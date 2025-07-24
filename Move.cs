using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tak_Engine
{
    internal class Move
    {
        public Types.Move MoveType { get; set; }
        public Piece Piece { get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int EndX { get; set; }            
        public int EndY { get; set; }
        public List<int> NumberOfDropedPieces { get; set; } 

        public Move(Types.Move moveType, Piece piece, int startX, int startY)
        {
            if(moveType != Types.Move.Place)
            {
                throw new ArgumentException("These arguments are only valid for a place move.", nameof(moveType));
            }
            MoveType = moveType;
            Piece = piece ?? throw new ArgumentNullException(nameof(piece), "Piece cannot be null.");
            StartX = startX;
            StartY = startY;
            EndX = -1; // Default value indicating no end position
            EndY = -1; // Default value indicating no end position
            NumberOfDropedPieces = new List<int>(); // Default value indicating no dropped pieces
        }
        public Move(Types.Move moveType, Piece piece, int startX, int startY, int endX, int endY)
        {
            if (moveType != Types.Move.PieceMove)
            {
                throw new ArgumentException("These arguments are only valid for a piece move.", nameof(moveType));
            }
            if (startX < 0 || startY < 0 || endX < 0 || endY < 0)
            {
                throw new ArgumentOutOfRangeException("Coordinates must be non-negative.");
            }
            MoveType = moveType;
            Piece = piece ?? throw new ArgumentNullException(nameof(piece), "Piece cannot be null.");
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
            if(NumberOfDropedPieces == null) NumberOfDropedPieces = new List<int>() { 1 };
        }
        public Move(Types.Move moveType, Piece piece, int startX, int startY, int endX, int endY, List<int> numberOfDroppedPieces) : this(moveType, piece, startX, startY, endX, endY)
        {
            NumberOfDropedPieces = numberOfDroppedPieces ?? throw new ArgumentNullException(nameof(numberOfDroppedPieces), "Number of dropped pieces cannot be null.");
        }
    }
}
