using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tak_Engine
{
    internal static class MoveGeneration
    {
        public static List<Move> GenerateValidMoves(Board b)
        {
            Types.Player currentPlayer = b.GetCurrentPlayer();
            List<Move> validMoves = new List<Move>();
            // Generate moves for placing pieces
            for (int x = 0; x < b.Size; x++)
            {
                for (int y = 0; y < b.Size; y++)
                {
                    if (b.GetCell(x, y).IsEmpty())
                    {
                        if (b.Stones[(int)currentPlayer] > 0)
                        {
                            validMoves.Add(new Move(Types.Move.Place, new Piece(Types.Piece.Flat, currentPlayer), x, y));
                            validMoves.Add(new Move(Types.Move.Place, new Piece(Types.Piece.Standing, currentPlayer), x, y));
                        }
                        if(b.Capstones[(int)currentPlayer] > 0)
                        {
                            validMoves.Add(new Move(Types.Move.Place, new Piece(Types.Piece.Capstone, currentPlayer), x, y));
                        }
                    }
                }
            }
            // Generate moves pieces
            for(int x = 0; x < b.Size; x++)
            {
                for (int y = 0; y < b.Size; y++)
                {
                    BoardCell cell = b.GetCell(x, y);
                    if (!cell.IsEmpty())
                    {
                        Piece topPiece = cell.TopPiece;
                        if (topPiece.Player == currentPlayer)
                        {
                            // Move the top piece to adjacent cells
                            if (x+1 < b.Size)
                            {
                                validMoves.Add(new Move(Types.Move.PieceMove, topPiece, x, y, x + 1, y));
                            }
                            if (x - 1 > 0)
                            {
                                validMoves.Add(new Move(Types.Move.PieceMove, topPiece, x, y, x - 1, y));
                            }
                            if (y + 1 < b.Size)
                            {
                                validMoves.Add(new Move(Types.Move.PieceMove, topPiece, x, y, x, y + 1));
                            }
                            if (y - 1 > 0)
                            {
                                validMoves.Add(new Move(Types.Move.PieceMove, topPiece, x, y, x, y - 1));
                            }
                        }
                    }
                }
            }
            return validMoves;
        }
    }
}
