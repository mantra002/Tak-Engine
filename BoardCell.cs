using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tak_Engine
{
    internal class BoardCell
    {
        private List<Piece> pieces;

        public Piece TopPiece
        {
            get
            {
                if (IsEmpty())
                {
                    throw new InvalidOperationException("Cannot access the top piece of an empty cell.");
                }
                return pieces.Last();
            }
        }

        public BoardCell()
        {
            pieces = new List<Piece>();
        }

        public bool IsEmpty()
        {
            return pieces.Count == 0;
        }
        public Piece[] GetPieces()
        {
            if (IsEmpty())
            {
                return Array.Empty<Piece>();
            }
            return pieces.ToArray();
        }

        public void AddPiece(Piece piece)
        {
            if (piece == null)
            {
                throw new ArgumentNullException(nameof(piece), "Cannot add a null piece.");
            }
            pieces.Add(piece);
        }

        public Piece[] RemovePiece()
        {
            return RemovePiece(1);
        }
        public Piece[] RemovePiece(int numberOfPieces)
        {
            if (numberOfPieces <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfPieces), "Number of pieces to remove must be greater than zero.");
            }
            if (IsEmpty())
            {
                throw new InvalidOperationException("Cannot remove a piece from an empty cell.");
            }
            if(pieces.Count() < numberOfPieces)
            {
                throw new InvalidOperationException("Not enough pieces to remove.");
            }
            Piece[] removedPieces = new Piece[numberOfPieces];

            removedPieces = pieces.GetRange(pieces.Count - numberOfPieces, numberOfPieces).ToArray();
            pieces.RemoveRange(pieces.Count - numberOfPieces, numberOfPieces);
            return removedPieces;
        }
       
    }
}
