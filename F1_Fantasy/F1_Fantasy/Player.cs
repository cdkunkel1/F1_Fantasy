using System;
using System.Collections.Generic;
using System.Text;

namespace F1_Fantasy
{
    class Player
    {
        private string name = ""; //Keeps track of the player's name
        private int points = 0; //Keeps track of how many points the player has
        private int answer = 0;
        private int position = 0;
        private int[] pointsByQuestion = new int[14];
        private static int count = 0;

        public Player(string name)
        {
            this.name = name;
            this.points = 0;
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
        //This is a getter for a player's answer
        public int GetAnswer()
        {
            return answer;
        }
        //This is a getter for the position
        public int GetPosition()
        {
            return position;
        }

        public int GetPointsByQuestion(int x)
        {
            return pointsByQuestion[x];
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
        //This is a setter for a player's answer
        public void SetAnswer(int answer)
        {
            this.answer = answer;
        }
        //This is a setter for the position
        public void SetPosition(int position)
        {
            this.position = position;
        }

        public void SetPointsByQuestion(int points, int x)
        {
            this.pointsByQuestion[x] = points;
        }
        //This will be used to update the points a player has
        public int UpdatePoints(int changeInPoints)
        {
            points += changeInPoints;
            return points;
        }

        public static int GetCount()
        {
            return count;
        }

        public static void SetCount(int temp)
        {
            count = temp;
        }

        public static void IncCount()
        {
            count++;
        }
        //This method will return the values as a string
        public string ToString()
        {
            return name + ": " + points + "\n";
        }
    }
}
