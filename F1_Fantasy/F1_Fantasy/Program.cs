using System;

namespace F1_Fantasy
{
    class Program
    {
        static void Main(string[] args)
        {
            Player player1 = new Player("Nathan");
            Player player2 = new Player("Ben");
            Player player3 = new Player("Cory");

            DisplayPoints(player1.GetName(), player2.GetName(), player3.GetName());
        }


        public static void DisplayPoints(string player1, string player2, string player3)
        {
            Console.WriteLine("Here are the points currently held by each player:\n");
            Console.WriteLine(player1 + ": " + currentPoints1);
            Console.WriteLine
        }
    }
}
