using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;

namespace F1_Fantasy
{
    class Program
    {
        static void Main(string[] args)
        {
            //Declare Variables
            int[] f1Scoring = { 25, 18, 15, 12, 10, 8, 6, 4, 2, 1 };
            string sql = "";
            string conString = @"Data Source = (LocalDB)\Formula_1; Initial Catalog = Formula_1; Integrated Security = True"; //This is the connection string for the F1 database

            SqlConnection cnn = new SqlConnection(conString); //Create a SQL Connection object to connect to the F1 Database
            OpenConnection(cnn); //Open the connection to the database

            //Use a Player class to keep track of names and points
            Player player1 = new Player("Nathan");
            Player player2 = new Player("Ben");
            Player player3 = new Player("Cory");

            sql = @"SELECT * FROM [Formula_1].[dbo].[Fastest Pit Stop]"; //SQL statement to select data for fastest pit stop
            FastestPitStop(cnn, player1, player2, player3, sql, f1Scoring);




            CloseConnection(cnn);
            Console.ReadKey();
        }
        //This method will open a connection to the SQL server
        public static void OpenConnection(SqlConnection cnn)
        {
            cnn.Open();
        }
        //This method will close the connection to the SQL server
        public static void CloseConnection(SqlConnection cnn)
        {
            cnn.Close();
        }
        //This method will be used to calculate points from the fastest pit stop question
        public static void FastestPitStop(SqlConnection cnn, Player player1, Player player2, Player player3, string sql, int[] f1Scoring)
        {
            string[] results = new string[10]; //Stores the actual results
            string[] answers = new string[3]; //Stores the player guesses
            SqlCommand command = new SqlCommand(sql, cnn); //Executes the sql command to return the table
            SqlDataReader dataReader = command.ExecuteReader(); //Begin to read the table
            string nullChecker = "";
            int count = 0;
            Boolean stop = false;
            dataReader.Read(); //Read the first line
            do
            {
                nullChecker = dataReader.GetValue(3).ToString(); //Checks the value in the results column to make sure it is not null
                if (string.IsNullOrEmpty(nullChecker)) //If the results column is null, then no points will be added or subtracted
                {
                    Console.WriteLine("Awaiting results");
                    stop = true; //Indicates that the loop should stop due to a null value
                }
                else //This occurs if there are results to read
                {
                    if (count == 0) //This will establish the players' answers in the first row
                    {
                        answers[0] = dataReader.GetValue(0).ToString();
                        answers[1] = dataReader.GetValue(1).ToString();
                        answers[2] = dataReader.GetValue(2).ToString();
                    }
                    results[count] = dataReader.GetValue(3).ToString(); //Every row's results will be stored in this array
                    count++; //Increments to account for a new row
                }
            } while (dataReader.Read() && stop == false); //Reads the next line, stops if the results column is null
                
            for (int x = 0; x < 10; x++) //Loops through all 10 results
                {
                if (answers[0] == results[x]) //When the player's answer matches the results, they gain points
                {
                    player1.UpdatePoints(f1Scoring[x]); //Points are updated with F1 scoring style, meaning 25 for first, 18 for second, etc.
                }
                if (answers[1] == results[x]) //Checks Player 2's answer
                {
                    player2.UpdatePoints(f1Scoring[x]);
                }
                if (answers[2] == results[x]) //Checks Player 3's answer
                {
                    player3.UpdatePoints(f1Scoring[x]);
                }
            }
        }
    }
}
