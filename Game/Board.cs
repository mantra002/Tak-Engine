using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tak_Engine.Game.Types;

namespace Tak_Engine.Game
{
    internal class Board
    {
        public int MoveNumber { get; set; }
        public int Size { get; set; }
        public BoardCell[] GameBoard;
        public Stack<Move> MoveStack;
        public ulong[] RoadBitboard { get { return CalculateBitboards(); } }
        public int[] SquareCounts { get { return CalculateSquareCounts(); } } // Square count for each player
        public int[] Stones { get; set; } = new int[2]; // Stone count for each player
        public int[] Capstones { get; set; } = new int[2]; // Capstone count for each player
        public Types.GameOver GameOverStatus { get; set; } = Types.GameOver.NotOver;
        public bool InTak
        {
            get
            {
                // Check if any player has a Tak
                return false;
            }
        }
        public ulong Hash
        {
            get {
                return CalculateHash();
            }
            
        }
        public Board() : this(5) { } // Default size is 5x5
        public Board(int size) {
            initBoard(size);
        }

        private void initBoard(int size)
        {
            if (size < 4 || size > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Board sizes of 4x4 to 8x8 are suupported");
            }
            Size = size;
            MoveNumber = 0;
            Stones[(int)Player.White] = Rules.GetNumberOfStones(size);
            Stones[(int)Player.Black] = Rules.GetNumberOfStones(size);
            Capstones[(int)Player.White] = Rules.GetNumberOfCapstones(size);
            Capstones[(int)Player.Black] = Rules.GetNumberOfCapstones(size);
            GameBoard = new BoardCell[size * size];
            MoveStack = new Stack<Move>();
            GameOverStatus = Types.GameOver.NotOver;
            for (int i = 0; i < size * size; i++)
            {
                GameBoard[i] = new BoardCell();
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
        private ulong[] CalculateBitboards()
        {
            ulong[] bitboards = new ulong[2]; // 0 for White, 1 for Black
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    int index = GetIndex(x, y);
                    if (!GameBoard[index].IsEmpty())
                    {
                        if (GameBoard[index].TopPiece.Player == Player.White)
                        {
                            bitboards[(int)Player.White] |= (1UL << (x + y * Size));
                        }
                        else
                        {
                            bitboards[(int)Player.Black] |= (1UL << (x + y * Size));
                        }
                    }
                }
            }
            return bitboards;
        }
        private int[] CalculateSquareCounts()
        {
            int[] squareCounts = new int[2]; // 0 for White, 1 for Black
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    int index = GetIndex(x, y);
                    if (!GameBoard[index].IsEmpty())
                    {
                        Piece piece = GameBoard[index].TopPiece;

                        if (piece.Player == Player.White)
                        {
                            squareCounts[(int)Player.White]++;
                        }
                        else
                        {
                            squareCounts[(int)Player.Black]++;
                        }
                        
                    }
                }
            }
            return squareCounts;
        }
        private ulong CalculateHash()
        {
            ulong hash = 0;
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    int index = GetIndex(x, y);
                    if (!GameBoard[index].IsEmpty())
                    {
                        foreach (Piece piece in GameBoard[index].GetPieces())
                        {
                            hash ^= (ulong)(piece.PieceType.GetHashCode() + piece.Player.GetHashCode() + (x * Size + y));
                        }
                    }
                }
            }
            return hash;
        }
        public void PlacePieces(int x, int y, Piece[] pieces)
        {
            if (pieces == null || pieces.Length == 0)
            {
                throw new ArgumentException("Pieces cannot be null or empty.", nameof(pieces));
            }
            foreach (Piece piece in pieces)
            {
                PlacePiece(x, y, piece);
            }
        }
        public void PlacePiece(int x, int y, Types.Piece cellType, Player player)
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
        public Types.GameOver GameOver()
        {
            // Check if any player has a Road
            if(MoveGeneration.HasValidRoad(RoadBitboard[(int)GetCurrentPlayer()], Size))
            {
                if(GetCurrentPlayer() == Player.White)
                {
                    return Types.GameOver.WhiteRoad; // White has a road
                }
                else
                {
                    return Types.GameOver.BlackRoad; // Black has a road
                } 
            }
            // Check if all pieces are placed
            if (Stones[(int)Player.White] == 0 && Stones[(int)Player.Black] == 0 &&
                Capstones[(int)Player.White] == 0 && Capstones[(int)Player.Black] == 0)
            {
                int score = SquareCounts[0] - SquareCounts[1]; 
                if (score > 0)
                {
                    return Types.GameOver.WhiteStones; // White has more squares
                }
                else if (score < 0)
                {
                    return Types.GameOver.BlackStones; // Black has more squares
                }
                else
                {
                    return Types.GameOver.Tie; // Tie
                }
            }
            return Types.GameOver.NotOver;
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
                    if (move.Piece.Player == Player.White)
                    {
                        Capstones[(int)Player.White]--;
                    }
                    else
                    {
                        Capstones[(int)Player.Black]--;
                    }
                }
                else
                {
                    if (move.Piece.Player == Player.White)
                    {
                        Stones[(int)Player.White]--;
                    }
                    else
                    {
                        Stones[(int)Player.Black]--;
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
                Direction direction = GetMoveDirection(move);
                int piecesDropped = 0;
                for(int i = 0; i < move.NumberOfDropedPieces.Count; i++)
                {
                    switch (direction)
                    {
                        case Direction.Left:
                            PlacePieces(move.StartX-i, move.StartY, piecesToMove.Skip(piecesDropped).Take(move.NumberOfDropedPieces[i]).ToArray());
                            piecesDropped += move.NumberOfDropedPieces[i];
                            break;
                        case Direction.Right:
                            PlacePieces(move.StartX + i, move.StartY, piecesToMove.Skip(piecesDropped).Take(move.NumberOfDropedPieces[i]).ToArray());
                            piecesDropped += move.NumberOfDropedPieces[i];
                            break;
                        case Direction.Up:
                            PlacePieces(move.StartX, move.StartY-i, piecesToMove.Skip(piecesDropped).Take(move.NumberOfDropedPieces[i]).ToArray());
                            piecesDropped += move.NumberOfDropedPieces[i];
                            break;
                        case Direction.Down:
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
            MoveStack.Push(move);
            MoveNumber++;
            GameOverStatus = GameOver();
        }
        public override string ToString()
        {
            int consecutiveX;
            StringBuilder sb = new StringBuilder();
            sb.Append("[TPS \"");
            for(int y = 0; y < Size; y++)
            {
                consecutiveX = 0;
                if (y > 0) sb.Append("/");
                for (int x = 0; x < Size; x++)
                {
                    BoardCell cell = GetCell(x, y);
                    if (cell.IsEmpty())
                    {
                        consecutiveX++;
                        if (x == Size - 1)
                        {
                            if (consecutiveX == 1) sb.Append("x");
                            else sb.Append($"x{consecutiveX}");
                            if (x < Size -1) sb.Append(",");
                        }
                    }
                    else if (consecutiveX > 0)
                    {
                        if (consecutiveX == 1) sb.Append("x");
                        else sb.Append($"x{consecutiveX}");
                        if (x < Size - 1) sb.Append(",");
                        consecutiveX = 0;
                        x--; // Decrement x to stay on the same cell after appending
                    }
                    else
                    {
                        List<Piece> pieces = cell.GetPieces().ToList();
                        for (int i = 0; i < pieces.Count; i++)
                        {
                            if (pieces[i].PieceType == Types.Piece.Capstone)
                            {
                                sb.Append(pieces[i].Player == Player.White ? "1C" : "2C");
                            }
                            else if (pieces[i].PieceType == Types.Piece.Standing)
                            {
                                sb.Append(pieces[i].Player == Player.White ? "1S" : "2S");
                            }
                            else
                            {
                                sb.Append(pieces[i].Player == Player.White ? "1" : "2");
                            }
                        }
                        if (x < Size - 1) sb.Append(",");
                    }
                }
            }
            int tpsMoveNumber = MoveNumber / 2;
            int playerMoveNumber = MoveNumber % 2 + 1;
            sb.Append($" {playerMoveNumber} {tpsMoveNumber}\"]");
            return sb.ToString();
        }
        public void SetupBoard(string tps, int boardSize)
        {
            initBoard(boardSize);
            tps = tps.Trim(new char[3] { '[', ']', ' ' });
            if (string.IsNullOrEmpty(tps))
            {
                throw new ArgumentException("TPS string cannot be null or empty.", nameof(tps));
            }
            string[] parts = tps.Split('"');
            if (parts[0].Trim() != "TPS")
            {
                throw new ArgumentException("TPS string must start with 'TPS'.", nameof(tps));
            }
           if(parts.Length < 3)
            {
                throw new ArgumentException("TPS string must contain at least three parts.", nameof(tps));
            } 

            parts = parts[1].Split(' ');
            string[] rows = parts[0].Split('/');

            int TpsMoveNumber = int.Parse(parts[2]);
            MoveNumber = TpsMoveNumber*2 + (int.Parse(parts[1]) - 1);
            int currentX = 0;
            for (int y = 0; y < rows.Length; y++)
            {
                currentX = 0;
                if (string.IsNullOrEmpty(rows[y]))
                {
                    throw new ArgumentException("Row cannot be null or empty.", nameof(tps));
                }
                string[] cells = rows[y].Split(',');
                for (int x = 0; x < cells.Length; x++)
                {
                    string cell = cells[x].Trim().ToLower();
                    if (cell.Length == 0)
                    {
                        throw new ArgumentException("Cell cannot be empty.", nameof(tps));
                    }
                    if (cell[0]=='x')
                    {
                        if (cell.Length > 1) currentX += int.Parse(cell.Substring(1))-1;
                    }
                    else
                    {
                        List<Piece> pieces = new List<Piece>();
                        for (int i = 0; i < cell.Length; i++)
                        {
                            Player player = Player.White;
                            Piece piece ;
                            if (cell[i]=='2')
                            {
                                player = Player.Black;
                            }
                            if(cell.Length > i+1 && cell[i+1] == 's')
                            {
                                piece = new Piece(Types.Piece.Standing, player);
                                i++;
                            }
                            else if (cell.Length > i + 1 && cell[i + 1] == 'c')
                            {
                                piece = new Piece(Types.Piece.Capstone, player);
                                i++;
                            }
                            else
                            {
                                piece = new Piece(Types.Piece.Flat, player);
                            }
                            pieces.Add(piece);
                        }
                        PlacePieces(currentX, y, pieces.ToArray());
                    }
                    currentX += 1;
                }
            }
        }   
        public void UnMakeMove()
        {
            Move move = MoveStack.Pop();
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move), "Move cannot be null.");
            }
            if (move.MoveType == Types.Move.Place)
            {
                RemovePiece(move.StartX, move.StartY);
                if (move.Piece.PieceType == Types.Piece.Capstone)
                {
                    if (move.Piece.Player == Player.White)
                    {
                        Capstones[(int)Player.White]++;
                    }
                    else
                    {
                        Capstones[(int)Player.Black]++;
                    }
                }
                else
                {
                    if (move.Piece.Player == Player.White)
                    {
                        Stones[(int)Player.White]++;
                    }
                    else
                    {
                        Stones[(int)Player.Black]++;
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
                Direction direction = GetMoveDirection(move);
                List<Piece> pieceStack = new List<Piece>();
                for (int i = move.NumberOfDropedPieces.Count-1; i > 0 ; i--)
                {
                    switch (direction)
                    {
                        case Direction.Left:
                            foreach (Piece p in RemovePieces(move.EndX + i, move.StartY, move.NumberOfDropedPieces[i]))
                            {
                                pieceStack.Add(p);
                            }
                            break;
                        case Direction.Right:
                            foreach (Piece p in RemovePieces(move.EndX - i, move.StartY, move.NumberOfDropedPieces[i]))
                            {
                                pieceStack.Add(p);
                            }
                            break;
                        case Direction.Up:
                            foreach (Piece p in RemovePieces(move.StartX, move.EndY+i, move.NumberOfDropedPieces[i]))
                            {
                                pieceStack.Add(p);
                            }
                            break;
                        case Direction.Down:
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
            GameOverStatus = Types.GameOver.NotOver;
        }

        public Player GetCurrentPlayer()
        {
            if (MoveNumber % 2 == 0)
            {
                return Player.White;
            }
            else
            {
                return Player.Black;
            }
        }

        private Direction GetMoveDirection(Move move)
        {
            if (move.StartX - move.EndX > 0)
            {
                return Direction.Left;
            }
            else if (move.StartX - move.EndX < 0)
            {
                return Direction.Right;
            }
            else if (move.StartY - move.EndY > 0)
            {
                return Direction.Up;
            }
            else if (move.StartY - move.EndY < 0)
            {
                return Direction.Down;
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
