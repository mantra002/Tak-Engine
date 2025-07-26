using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


using Tak_Engine.Game;

namespace Tak_Engine.Engine
{
    internal class TranspositionTable
    {
        Position[] tt;
        internal uint TableSizeInMb;
        readonly ulong TableSizeInPositions;
        ulong TtEntries = 0;
        public double PercentFull { get { return TtEntries / (double)TableSizeInPositions; } }

        internal TranspositionTable(uint sizeInMb = 64)
        {
            TableSizeInMb = sizeInMb;
            TableSizeInPositions = (ulong)sizeInMb * 1000000 / (ulong)(Position.GetSize());
            tt = new Position[TableSizeInPositions];
#if DEBUG
            Console.WriteLine($"Intializing Transposition Table with {TableSizeInMb} mb of space / {TableSizeInPositions} positions");
#endif 
        }

        internal void ClearTable()
        {
            tt = new Position[TableSizeInPositions];
        }
        private int GetTTIndex(ulong hashKey)
        {
            return (int)(hashKey % TableSizeInPositions);
        }

        internal Position LookupPosition(ulong hashKey)
        {
            Position p = (Position)tt[GetTTIndex(hashKey)];
            if (p != null && p.HashKey == hashKey)
            {
                //Console.WriteLine($"Retrived position {hashKey} successfully!");
                return p; }
            return null;
        }
        internal void AddPosition(ulong key, int score, Move movePlayed, byte depth, byte plyFromRoot, NodeType nt)
        {
            //Console.WriteLine($"Saving position with key {key} at index {GetTTIndex(key)}");
            Position p = new Position(key, score, movePlayed, depth, plyFromRoot, nt);
            tt[GetTTIndex(key)] = p;
            TtEntries++;
        }
        public enum NodeType
        {
            Exact,
            Beta,
            Alpha
        }
    
        [StructLayout(LayoutKind.Sequential)]
        internal class Position
        {
            public readonly ulong HashKey;
            public int Score { get; set; }
            public readonly Move MovePlayed;
            public readonly byte Depth;
            public readonly NodeType NType;


            internal Position(ulong hk, int score, Move movePlayed, byte depth, byte plyFromRoot, NodeType nt)
            {
                this.HashKey = hk;
                this.MovePlayed = movePlayed;
                this.Depth = depth;
                this.NType = nt;
                this.Score = score;
            }
            public static int GetSize()
            {
                return System.Runtime.InteropServices.Marshal.SizeOf<Position>();
            }
        }
    }

}

