using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tak_Engine.Game;


namespace Tak_Engine.Engine
{
    static internal class MoveOrdering
    {
        internal static void OrderMoves(Board b, TranspositionTable tt, List<Move> moves, bool UseSEE = false)
        {
            int score;

            foreach (Move m in moves)
            {
              /*  score = 0;
                thisPieceValue = Evaluation.GetPieceValue(m.Piece);

                if (m.PieceCaptured != 0)
                {
                    if (UseSEE)
                    {
                        score += StaticExchangeEvaluation(b, m, thisPieceValue);
                    }
                    else
                    {
                        score -= thisPieceValue;
                        score += Evaluation.GetPieceValue(m.PieceCaptured) * Evaluation.CaptureBonusMultiplier;
                    }
                }
                if (m.PromoteIntoPiece != 0)
                {
                    score += Evaluation.GetPieceValue(m.PromoteIntoPiece);
                }

                m.MoveScore = -score; //Sorting small = good*/
            }

            moves.Sort();
        }
    }
}
