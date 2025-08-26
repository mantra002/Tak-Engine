using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tak_Engine.Game
{
    internal static class MoveGeneration
    {
        private static List<List<int>>[] DropPieceTables;
        private static List<ulong>[] ValidRoads;
        static MoveGeneration()
        {
            ValidRoads = new List<ulong>[8];
            DropPieceTables = new List<List<int>>[8];
            for (int i = 0; i < 8; i++)
            {
                DropPieceTables[i] = GenerateUniqueParts(i+1);
                ValidRoads[i] = GenerateRoadsBitBoards(i + 1);
            }
        }
        static public bool HasValidRoad(ulong bitboard, int size)
        {
            foreach(ulong validRoad in ValidRoads[size - 1])
            {
                if ((bitboard & validRoad) == validRoad)
                {
                    return true;
                }
            }
            return false;
        }
          
        static private List<ulong> GenerateRoadsBitBoards(int size)
        {
            List<ulong> res = PathFinder.GenerateRoads(size); // Start from index 1
            //PathFinder.PrintPaths(res);
            return res;
        }
        
        static private List<List<int>> GenerateUniqueParts(int size)
        {
            List<List<int>> table = new List<List<int>>();
            List<List<int>> cleanedTable = new List<List<int>>();
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

                if (k < 0) break;

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
            for(int i = 0; i < table.Count; i++)
            {
                List<int> val = table[i];
                val.RemoveAll(x => x == 0);
                GeneratePermuation(val.Count, val, ref cleanedTable);
            }
            cleanedTable = cleanedTable.Distinct(new ListEqualsComparison()).ToList();
            return cleanedTable;
        }
        private class ListEqualsComparison : IEqualityComparer<List<int>>
        {
            public bool Equals(List<int>x, List<int>y)
            {
                if(x.Count != y.Count) return false;
                for (int i = 0; i < x.Count; i++)
                {
                    if (x[i] != y[i]) return false;
                }
                return true;
            }

            public int GetHashCode(List<int> obj)
            {
                int hash = 17;
                for(int i = 0; i < obj.Count; i++)
                {
                    hash = hash ^ obj[i].GetHashCode();
                }
                return hash;
            }
        }
        private static void GeneratePermuation<T>(int k, List<T> inputList, ref List<List<T>> result)
        {
            if (k == 1)
            {
                result.Add(new List<T>(inputList)); // Add a copy of the current permutation
                return;
            }

            for (int i = 0; i < k; i++)
            {
                GeneratePermuation(k - 1, inputList, ref result);

                // Swap elements based on k's parity (Heap's algorithm logic)
                if (k % 2 == 0)
                {
                    (inputList[i], inputList[k - 1]) = (inputList[k - 1], inputList[i]); // Swap A[i] and A[k-1]
                }
                else
                {
                    (inputList[0], inputList[k - 1]) = (inputList[k - 1], inputList[0]); // Swap A[0] and A[k-1]
                }
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
            BoardCell bc;
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
                            for(int i = 0; i < cell.GetPieces().Length; i++)
                            {
                                for(int j =0; j < DropPieceTables[i].Count; j++)
                                {
                                    if (x + 1 + i < b.Size)
                                    {
                                        bc = b.GetCell(x + 1 + i, y);
                                        if (bc.IsEmpty() || bc.TopPiece.PieceType == Types.Piece.Flat)
                                        {
                                            validMoves.Add(new Move(Types.Move.PieceMove, topPiece, x, y, x + 1 + i, y, DropPieceTables[i][j]));
                                        }
                                        else if(bc.TopPiece.PieceType == Types.Piece.Standing && topPiece.PieceType == Types.Piece.Capstone)
                                        {
                                            // If the top piece is a capstone, it can mover over a wall 
                                            validMoves.Add(new Move(Types.Move.PieceMove, topPiece, x, y, x + 1 + i, y, DropPieceTables[i][j]));
                                        }
                                    }
                                    if (x - 1 - i > 0)
                                    {
                                        bc = b.GetCell(x - 1 - i, y);
                                        if (bc.IsEmpty() || bc.TopPiece.PieceType == Types.Piece.Flat)
                                        {
                                            validMoves.Add(new Move(Types.Move.PieceMove, topPiece, x, y, x - 1 - i, y, DropPieceTables[i][j]));
                                        }
                                        else if (bc.TopPiece.PieceType == Types.Piece.Standing && topPiece.PieceType == Types.Piece.Capstone)
                                        {
                                            // If the top piece is a capstone, it can mover over a wall 
                                            validMoves.Add(new Move(Types.Move.PieceMove, topPiece, x, y, x - 1 - i, y, DropPieceTables[i][j]));
                                        }
                                    }
                                    if (y + 1 + i < b.Size)
                                    {
                                        bc = b.GetCell(x, y + 1 + i);
                                        if (bc.IsEmpty() || bc.TopPiece.PieceType == Types.Piece.Flat)
                                        {
                                            validMoves.Add(new Move(Types.Move.PieceMove, topPiece, x, y, x, y + 1 + i, DropPieceTables[i][j]));
                                        }
                                        else if (bc.TopPiece.PieceType == Types.Piece.Standing && topPiece.PieceType == Types.Piece.Capstone)
                                        {
                                            // If the top piece is a capstone, it can mover over a wall 
                                            validMoves.Add(new Move(Types.Move.PieceMove, topPiece, x, y, x, y + 1 + i, DropPieceTables[i][j]));
                                        }
                                    }
                                    if (y - 1 - i > 0)
                                    {
                                        bc = b.GetCell(x, y - 1 - i);
                                        if (bc.IsEmpty() || bc.TopPiece.PieceType == Types.Piece.Flat)
                                        {
                                            validMoves.Add(new Move(Types.Move.PieceMove, topPiece, x, y, x, y - 1 - i, DropPieceTables[i][j]));
                                        }
                                        else if (bc.TopPiece.PieceType == Types.Piece.Standing && topPiece.PieceType == Types.Piece.Capstone)
                                        {
                                            // If the top piece is a capstone, it can mover over a wall 
                                            validMoves.Add(new Move(Types.Move.PieceMove, topPiece, x, y, x, y - 1 - i, DropPieceTables[i][j]));
                                        }
                                    }
                                }
                            }
                            // Move the top piece to adjacent cells
                            
                        }
                    }
                }
            }
            return validMoves;
        }
    }
}
