using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tak_Engine.Types;

namespace Tak_Engine
{
    internal class Board
    {
        public int MoveNumber { get; set; }
        public int Size { get; set; }
        public BoardCell[] GameBoard;
        public int[] Stones { get; set; } = new int[2]; // Stone count for each player
        public int[] Capstones { get; set; } = new int[2]; // Capstone count for each player

        public Board() : this(5) // Default size is 5x5
        {
        }

        public Board(int size) {
            if(size < 4 || size > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Board sizes of 4x4 to 8x8 are suupported");
            }
            this.Size = size;
            this.MoveNumber = 0;
            this.Stones[(int)Types.Player.White] = Rules.GetNumberOfStones(size);
            this.Stones[(int)Types.Player.Black] = Rules.GetNumberOfStones(size);
            this.Capstones[(int)Types.Player.White] = Rules.GetNumberOfCapstones(size);
            this.Capstones[(int)Types.Player.Black] = Rules.GetNumberOfCapstones(size);
            this.GameBoard = new BoardCell[size * size];
            for (int i = 0; i< size * size; i++) {
                this.GameBoard[i] = new BoardCell();
            }
        }
        public BoardCell GetCell(int x, int y)
        {
            int index = GetIndex(x, y);
            if (index < 0 || index >= GameBoard.Length)
            {
                throw new ArgumentOutOfRangeException("Coordinates are out of bounds.");
            }
            return GameBoard[index];
        }
        public void PlacePieces(int x, int y, Piece[] pieces)
        {
            if (pieces == null || pieces.Length == 0)
            {
                throw new ArgumentException("Pieces cannot be null or empty.", nameof(pieces));
            }
            foreach (Piece piece in pieces.Reverse())
            {
                PlacePiece(x, y, piece);
            }
        }
        public void PlacePiece(int x, int y, Types.Piece cellType, Types.Player player)
        {
            Piece p = new Piece(cellType, player);
            PlacePiece(x, y, p);
        }
        public void PlacePiece(int x, int y, Piece piece)
        {
            int index = GetIndex(x, y);
            GameBoard[index].AddPiece(piece);
        }

        public Piece[] RemovePiece(int x, int y)
        {
            return RemovePieces(x, y, 1);
        }
        public Piece[] RemovePieces(int x, int y, int numberOfPieces)
        {
            int index = GetIndex(x, y);
            if (GameBoard[index].IsEmpty())
            {
                throw new InvalidOperationException("Cannot remove a piece from an empty cell.");
            }
            return GameBoard[index].RemovePiece(numberOfPieces);
        }

        public void MakeMove(Move move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move), "Move cannot be null.");
            }
            //If the move is a piece move, we just need to add the placed peice to the board.
            if (move.MoveType == Types.Move.Place)
            {
                PlacePiece(move.StartX, move.StartY, move.Piece.PieceType, move.Piece.Player);
                if(move.Piece.PieceType == Types.Piece.Capstone)
                {
                    if (move.Piece.Player == Types.Player.White)
                    {
                        Capstones[(int)Types.Player.White]--;
                    }
                    else
                    {
                        Capstones[(int)Types.Player.Black]--;
                    }
                }
                else
                {
                    if (move.Piece.Player == Types.Player.White)
                    {
                        Stones[(int)Types.Player.White]--;
                    }
                    else
                    {
                        Stones[(int)Types.Player.Black]--;
                    }
                }
            }
            //If we're moving a piece, it could be a stack dropping seveal pieces, or a single piece. 
            else if (move.MoveType == Types.Move.PieceMove)
            {
                int startIndex = GetIndex(move.StartX, move.StartY);
                int endIndex = GetIndex(move.EndX, move.EndY);
                if (GameBoard[startIndex].IsEmpty())
                {
                    throw new InvalidOperationException("Cannot move from an empty cell.");
                }
                if (GameBoard[startIndex].GetPieces().Last().PieceType != move.Piece.PieceType ||
                    GameBoard[startIndex].GetPieces().Last().Player != move.Piece.Player)
                {
                    throw new InvalidOperationException("Mismatch between move piece information and board state.");
                }
                if(move.NumberOfDropedPieces.Sum() > 5)
                {
                    throw new ArgumentOutOfRangeException(nameof(move.NumberOfDropedPieces), "Cannot drop more than 5 pieces in a single move.");
                }
                //Pick up the pieces from the start cell.
                Piece[] piecesToMove = GameBoard[startIndex].RemovePiece(move.NumberOfDropedPieces.Sum());
                Types.Direction direction = GetMoveDirection(move);
                int piecesDropped = 0;
                for(int i = 0; i < move.NumberOfDropedPieces.Count; i++)
                {
                    switch (direction)
                    {
                        case Types.Direction.Left:
                            PlacePieces(move.StartX-i, move.StartY, piecesToMove.Skip(piecesDropped).Take(move.NumberOfDropedPieces[i]).ToArray());
                            piecesDropped += move.NumberOfDropedPieces[i];
                            break;
                        case Types.Direction.Right:
                            PlacePieces(move.StartX + i, move.StartY, piecesToMove.Skip(piecesDropped).Take(move.NumberOfDropedPieces[i]).ToArray());
                            piecesDropped += move.NumberOfDropedPieces[i];
                            break;
                        case Types.Direction.Up:
                            PlacePieces(move.StartX, move.StartY-i, piecesToMove.Skip(piecesDropped).Take(move.NumberOfDropedPieces[i]).ToArray());
                            piecesDropped += move.NumberOfDropedPieces[i];
                            break;
                        case Types.Direction.Down:
                            PlacePieces(move.StartX, move.StartY+i, piecesToMove.Skip(piecesDropped).Take(move.NumberOfDropedPieces[i]).ToArray());
                            piecesDropped += move.NumberOfDropedPieces[i];
                            break;
                        default:
                            throw new ArgumentException("Invalid move direction.", nameof(move.MoveType));
                    }
                }
            }
            else
            {
                throw new ArgumentException("Invalid move type.", nameof(move.MoveType));
            }
            MoveNumber++;
        }
        public void UnMakeMove(Move move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move), "Move cannot be null.");
            }
            if (move.MoveType == Types.Move.Place)
            {
                RemovePiece(move.StartX, move.StartY);
                if (move.Piece.PieceType == Types.Piece.Capstone)
                {
                    if (move.Piece.Player == Types.Player.White)
                    {
                        Capstones[(int)Types.Player.White]++;
                    }
                    else
                    {
                        Capstones[(int)Types.Player.Black]++;
                    }
                }
                else
                {
                    if (move.Piece.Player == Types.Player.White)
                    {
                        Stones[(int)Types.Player.White]++;
                    }
                    else
                    {
                        Stones[(int)Types.Player.Black]++;
                    }
                }
            }
            else if (move.MoveType == Types.Move.PieceMove)
            {
                int startIndex = GetIndex(move.StartX, move.StartY);
                int endIndex = GetIndex(move.EndX, move.EndY);
                if (GameBoard[startIndex].IsEmpty())
                {
                    throw new InvalidOperationException("Cannot move from an empty cell.");
                }
                if (GameBoard[startIndex].GetPieces().Last().PieceType != move.Piece.PieceType ||
                    GameBoard[startIndex].GetPieces().Last().Player != move.Piece.Player)
                {
                    throw new InvalidOperationException("Mismatch between move piece information and board state.");
                }
                Types.Direction direction = GetMoveDirection(move);
                List<Piece> pieceStack = new List<Piece>();
                for (int i = move.NumberOfDropedPieces.Count-1; i > 0 ; i--)
                {
                    switch (direction)
                    {
                        case Types.Direction.Left:
                            foreach (Piece p in RemovePieces(move.EndX + i, move.StartY, move.NumberOfDropedPieces[i]))
                            {
                                pieceStack.Add(p);
                            }
                            break;
                        case Types.Direction.Right:
                            foreach (Piece p in RemovePieces(move.EndX - i, move.StartY, move.NumberOfDropedPieces[i]))
                            {
                                pieceStack.Add(p);
                            }
                            break;
                        case Types.Direction.Up:
                            foreach (Piece p in RemovePieces(move.StartX, move.EndY+i, move.NumberOfDropedPieces[i]))
                            {
                                pieceStack.Add(p);
                            }
                            break;
                        case Types.Direction.Down:
                            foreach (Piece p in RemovePieces(move.StartX, move.EndY-i, move.NumberOfDropedPieces[i]))
                            {
                                pieceStack.Add(p);
                            }
                            break;
                        default:
                            throw new ArgumentException("Invalid move direction.", nameof(move.MoveType));
                    }
                }
                if(pieceStack.Count != move.NumberOfDropedPieces.Sum())
                {
                    throw new InvalidOperationException("Mismatch in number of pieces to unmake.");
                }
                PlacePieces(move.StartX, move.StartY, pieceStack.ToArray());
            }
            else
            {
                throw new ArgumentException("Invalid move type.", nameof(move.MoveType));
            }
            MoveNumber--;
        }

        public Types.Player GetCurrentPlayer()
        {
            if (MoveNumber % 2 == 0)
            {
                return Types.Player.White;
            }
            else
            {
                return Types.Player.Black;
            }
        }

        private Types.Direction GetMoveDirection(Move move)
        {
            if (move.StartX - move.EndX > 0)
            {
                return Types.Direction.Left;
            }
            else if (move.StartX - move.EndX < 0)
            {
                return Types.Direction.Right;
            }
            else if (move.StartY - move.EndY > 0)
            {
                return Types.Direction.Up;
            }
            else if (move.StartY - move.EndY < 0)
            {
                return Types.Direction.Down;
            }
            else
            {
                throw new ArgumentException("Start and end coordinates are the same, no movement direction.");
            }
        }

        private int GetIndex(int x, int y)
        {
            if (x < 0 || x >= Size || y < 0 || y >= Size)
            {
                throw new ArgumentOutOfRangeException("Coordinates are out of bounds.");
            }
            return y * Size + x;
        }
    }
}
