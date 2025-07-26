using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tak_Engine.Game
{

    public class PathFinder
    {
        public static List<ulong> GenerateRoads(int size)
        {
            var results = new List<ulong>();

            for (int col = 0; col < size; col++)
            {
                bool[,] visited = new bool[size, size];
                RoadSearch(0, col, visited, results, -1, -1);
            }

            return results;
        }
        private static ulong ConvertBoolArrayToULong(bool[,] grid)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);
            int totalBits = rows * cols;

            if (totalBits > 64)
                throw new ArgumentException("2D array is too large to fit in a ulong (max 64 bits)");

            ulong result = 0;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    result <<= 1; // shift left to make space for next bit
                    if (grid[r, c])
                        result |= 1; // set bit if true
                }
            }

            // Optionally, shift remaining bits if you want MSB aligned
            result <<= (64 - totalBits); // align to top bits if needed

            return result;
        }
        private static void RoadSearch(int row, int col, bool[,] visited, List<ulong> results, int prevRow, int prevCol)
        {
            int rows = visited.GetLength(0);
            int cols = visited.GetLength(1);
            ulong roadBitBoard, rotatedBitBoard;
            if (row < 0 || row >= rows || col < 0 || col >= cols)
                return;
            if (visited[row, col])
                return;

            // Check if adding this would put us adjacent to any already visited cell (other than the direct connection)
            if (BoarderingPathFilter(row, col, visited, prevRow, prevCol))
                return;

            visited[row, col] = true;

            if (row == rows - 1)
            {
                if (IsValidPath(visited))
                {
                    roadBitBoard = ConvertBoolArrayToULong((bool[,])visited.Clone());
                    rotatedBitBoard = ConvertBoolArrayToULong(Rotate90Degress((bool[,])visited.Clone()));
                    results.Add(roadBitBoard);
                    results.Add(rotatedBitBoard);
                }
            }
            else
            {
                RoadSearch(row + 1, col, visited, results, row, col); // Down
                RoadSearch(row - 1, col, visited, results, row, col); // Up
                RoadSearch(row, col + 1, visited, results, row, col); // Right
                RoadSearch(row, col - 1, visited, results, row, col); // Left
            }

            visited[row, col] = false; 
        }
        private static T[,] Rotate90Degress<T>(T[,] array)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            T[,] rotated = new T[cols, rows];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    rotated[c, rows - 1 - r] = array[r, c];
                }
            }

            return rotated;

        }

        private static bool BoarderingPathFilter(int row, int col, bool[,] visited, int prevRow, int prevCol)
        {
            int[] dR = { -1, 1, 0, 0 };
            int[] dC = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int nr = row + dR[i];
                int nc = col + dC[i];

                if (nr == prevRow && nc == prevCol)
                    continue;

                if (nr >= 0 && nr < visited.GetLength(0) && nc >= 0 && nc < visited.GetLength(1))
                {
                    if (visited[nr, nc])
                        return true;
                }
            }

            return false;
        }

        private static bool IsValidPath(bool[,] visited)
        {
            int rows = visited.GetLength(0);
            int cols = visited.GetLength(1);

            int startMarks = 0;
            int endMarks = 0;

            for (int c = 0; c < cols; c++)
            {
                if (visited[0, c]) startMarks++;
                if (visited[rows - 1, c]) endMarks++;
            }

            return startMarks == 1 && endMarks == 1;
        }

        public static void PrintPaths(List<bool[,]> paths)
        {
            int count = 1;
            foreach (var path in paths)
            {
                Console.WriteLine($"Path {count++}:");
                int rows = path.GetLength(0);
                int cols = path.GetLength(1);

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                        Console.Write(path[r, c] ? "1 " : "0 ");
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }
    }
}
