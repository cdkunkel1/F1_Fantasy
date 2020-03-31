using System;
using System.Collections.Generic;
using System.Text;

namespace F1_Fantasy
{
    class PlayerReports
    {
        Player[] players;
        private string[] questions;
        private int[] questionNumbers;
        private string dashLine = "-----------------------------------------";

        //Constructor that passes in the players array, questions array, and the question numbers array
        public PlayerReports(Player[] players, string[] questions, int[] questionNumbers)
        {
            this.players = players;
            this.questions = questions;
            this.questionNumbers = questionNumbers;
        }
        //Prints out the questions along with the points earned by each player
        public void PrintAllScores()
        {
            for (int x = 0; x < questionNumbers.Length; x++)
            {
                Console.WriteLine(dashLine);
                Console.WriteLine(questions[x]);
                Console.WriteLine(dashLine);
                for (int i = 0; i < Player.GetCount(); i++)
                {
                    Console.Write(players[i].GetName() + ": " + players[i].GetPointsByQuestion(x) + "\n");
                }
            }
        }
    }
}
