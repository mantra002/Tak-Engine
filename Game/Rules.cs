using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tak_Engine.Game
{
    internal static class Rules
    {
        public static int GetNumberOfStones(int size)
        {
            switch (size)
            {
                case 4:
                    return 15;
                    break;
                case 5:
                    return 21;
                    break;
                case 6:
                    return 30;
                    break;
                case 7:
                    return 40;
                    break;
                case 8: 
                    return 50;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), "Size must be between 4 and 8.");
                    break;
            }
        }

        public static int GetNumberOfCapstones(int size)
        {
            switch (size)
            {
                case 4:
                    return 0;
                    break;
                case 5:
                    return 1;
                    break;
                case 6:
                    return 1;
                    break;
                case 7:
                    return 2;
                    break;
                case 8:
                    return 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), "Size must be between 4 and 8.");
                    break;
            }
        }
    }
}
