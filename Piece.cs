using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tak_Engine
{
    internal class Piece
    {
        public Types.Piece PieceType { get; set; }
        public Types.Player Player { get; set; }

        public Piece(Types.Piece cellType, Types.Player player)
        {
            PieceType = cellType;
            Player = player;
        }
    }
}
