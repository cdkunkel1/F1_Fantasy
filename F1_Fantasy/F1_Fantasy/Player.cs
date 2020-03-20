using System;
using System.Collections.Generic;
using System.Text;

namespace F1_Fantasy
{
    class Player
    {
        private string name = ""; //Keeps track of the player's name
        private int points = 0; //Keeps track of how many points the player has

        public Player(string name)
        {
            this.name = name;
        }
        //This is a getter for the name
        public string GetName()
        {
            return name;
        }
        //This is a getter for the points
        public int GetPoints()
        {
            return points;
        }
        //This is a setter for the name
        public void SetName(string name)
        {
            this.name = name;
        }
        //This is a setter for the points
        public void SetPoints(int points)
        {
            this.points = points;
        }
        //This will be used to update the points a player has
        public int UpdatePoints(int changeInPoints)
        {
            points += changeInPoints;
            return points;
        }
    }
}
