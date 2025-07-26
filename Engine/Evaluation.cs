using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tak_Engine.Engine
{
    using Tak_Engine.Game;

    static internal class Evaluation
    {
        static public int Evaluate(Board b)
        {
            if(b.GetCurrentPlayer() == Types.Player.White)
            {
                return b.SquareCounts[0] - b.SquareCounts[1];
            }
            else
            {
                return b.SquareCounts[1] - b.SquareCounts[0];
            }
            return 0;
        }

    }
}
