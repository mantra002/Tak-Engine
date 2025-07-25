using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tak_Engine
{
    internal static class MoveGeneration
    {
        private static List<List<int>>[] DropPieceTables;

        static MoveGeneration()
        {
            DropPieceTables = new List<List<int>>[5];
            for (int i = 1; i < 6; i++)
            {
                DropPieceTables[i-1] = GenerateUniqueParts(i);
            }
        }
        static public List<List<int>> GenerateUniqueParts(int size)
        {
            List<List<int>> table = new List<List<int>>();
           int[] partition = new int[size];

            int k = 0;
            partition[0] = size;

            while(true)
            {
                table.Add(partition.ToList());
                int rightMostValue = 0;
                while(k >= 0 && partition[k] == 1)
                {
                    rightMostValue += partition[k];
                    k--;
                }

                if (k < 0) return table;

                partition[k]--;
                rightMostValue++;

                while(rightMostValue > partition[k])
                {
                    partition[k + 1] = partition[k];
                    rightMostValue = rightMostValue - partition[k];
                    k++;
                }

                partition[k + 1] = rightMostValue;
                k++;
            }
        }
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
