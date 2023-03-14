using System.Text;
using Domain;
using GameBrain;

namespace ConsoleUI;

public static class UI
{
    public static void DrawGameBoard(EGameSquareState[][] board, CheckersBrain brain, int? x1, int? y1, bool nextMoveByBlack, CheckersOption options)
    {
        PrintLetters(board[0].GetLength(0));
        Console.WriteLine();
        
        for (var i = 0; i < board.GetLength(0); i++)
        {
            var numbersOnTheLeftAreWritten = false;
            PrintNumbers(board.GetLength(0),i, numbersOnTheLeftAreWritten);
            numbersOnTheLeftAreWritten = true;
            for (var j = 0; j < board[0].GetLength(0); j++)
            {
                var text = "";
                if (board[i][j].Equals(EGameSquareState.Light))
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    text = "   ";
                }
                if (board[i][j].Equals(EGameSquareState.Dark))
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    text = "   ";
                }
                if (board[i][j].Equals(EGameSquareState.WhitePiece))
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Red;
                    text = " \u2B24 ";
                }
                if (board[i][j].Equals(EGameSquareState.BlackPiece))
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Blue;
                    text = " \u2B24 ";
                }
                if (board[i][j].Equals(EGameSquareState.WhiteKing))
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    text = " \u2B24 ";
                }
                if (board[i][j].Equals(EGameSquareState.BlackKing))
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    text = " \u2B24 ";
                }

                if (x1 !=null && y1 != null)
                {
                    var x1NotNull = x1 ?? default(int);
                    var y1NotNull = y1 ?? default(int);
                    if (nextMoveByBlack)
                    {
                        if (brain.IsMovePossible(x1NotNull,y1NotNull,i,j,nextMoveByBlack,options,false))
                        {
                            Console.BackgroundColor = ConsoleColor.Cyan;
                        }
                    }

                    if (nextMoveByBlack == false)
                    {
                        if (brain.IsMovePossible(x1NotNull,y1NotNull,i,j,nextMoveByBlack,options,false))
                        {
                            Console.BackgroundColor = ConsoleColor.Magenta;
                        }
                    }

                }

                
                Console.OutputEncoding = Encoding.Unicode;
                Console.Write(text);
                Console.ResetColor();
            }
            PrintNumbers(board.GetLength(0),i,numbersOnTheLeftAreWritten);
            Console.WriteLine();
            
        }
        PrintLetters(board[0].GetLength(0));
        Console.WriteLine();
    }

    private static void PrintLetters(int boardWidth)
    {
        string alpha = "ABCDEFGHIJKLMNOPQRSTUVQXYZ";
        Console.Write("    ");
        for (int i =0; i < 26 && i < boardWidth; ++i)
        {  
            Console.Write(" " + alpha[i]+ " ");
        }
    }

    private static void PrintNumbers(int boardLength, int currentLine, bool numbersOnTheLeftAreWritten)
    {
        var number = boardLength - currentLine;
        if (numbersOnTheLeftAreWritten == false)
        {
            Console.Write("{0,4}", number + " ");
        }
        else
        {
            Console.Write("{0,-4}", " " + number );
        }
        
    }
    
}