// See https://aka.ms/new-console-template for more information
using Tak_Engine;

Board board = new Board(5);
board.SetupBoard("[TPS \"x5/x5/x5/x5/x5 1 1\"]", 5);

Console.WriteLine("Initial Board Setup: " + board.ToString());
board.SetupBoard("[TPS \"x3,12,2S/x,22S,22C,11,21/121,212,12,1121C,1212S/21S,1,21,211S,12S/x,21S,2,x2 1 26\"]", 5);
Console.WriteLine("After Setup: " + board.ToString());
List<List<int>> t =  MoveGeneration.GenerateUniqueParts(5);
Console.WriteLine("Unique Parts for 5: " + string.Join(", ", t.Select(x => "[" + string.Join(", ", x) + "]")));