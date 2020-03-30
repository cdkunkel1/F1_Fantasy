using System;
using System.Collections.Generic;
using System.Text;

namespace F1_Fantasy
{
    class PlayerReports
    {
        Player[] players;

        public PlayerReports(Player[] temp)
        {
            this.players = temp;
        }

        public void PrintAllScores()
        {
            for (int x = 0; x < Player.GetCount(); x++)
            {
                Console.WriteLine(players[x].GetPointsByQuestion())
            }
        }
    }
}
