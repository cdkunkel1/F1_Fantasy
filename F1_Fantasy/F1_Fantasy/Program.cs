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

            sql = @"SELECT * FROM [Formula_1].[dbo].[WDC Rankings]"; //SQL statement to select data for Driver Rankings
            Rankings(cnn, player1, player2, player3, sql);

            sql = @"SELECT * FROM [Formula_1].[dbo].[Constructor Rankings]"; //SQL statement to select data for constructor rankings
            Rankings(cnn, player1, player2, player3, sql);

            sql = @"SELECT * FROM [Formula_1].[dbo].[Fastest Pit Stop]"; //SQL statement to select data for fastest pit stop
            FastestPitStop(cnn, player1, player2, player3, sql, f1Scoring);

            CloseConnection(cnn);

            F1Car(); //Displays an F1 Car
            DisplayPoints(player1, player2, player3);
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
        
        public static void Rankings(SqlConnection cnn, Player player1, Player player2, Player player3, string sql)
        {
            int[] answersP1 = new int[30]; //These are set at 30 to account for any new drivers or teams
            int[] answersP2 = new int[30];
            int[] answersP3 = new int[30];
            SqlCommand command = new SqlCommand(sql, cnn); //Executes the sql command to return the table
            SqlDataReader dataReader = command.ExecuteReader(); //Begin to read the table
            string nullChecker = "";
            Boolean stop = false;
            int[] result = new int[30];
            int count = 0;
            while (dataReader.Read() && stop == false) //Reads the next line, stops if the results column is null 
            {
                if (count == 0)
                {
                    nullChecker = dataReader.GetValue(4).ToString(); //Checks the value in the results column to make sure it is not null
                    stop = CheckIfNull(nullChecker); //stop will return true if the value is null
                }
                if (stop == false)
                {
                    result[count] = dataReader.GetInt32(4); //This is the actual result
                    answersP1[count] = dataReader.GetInt32(1); //These are what the players picked
                    answersP2[count] = dataReader.GetInt32(2);
                    answersP3[count] = dataReader.GetInt32(3);
                }
                count++;
            }
            if (stop == false) //Will not add points if the results values are null
            {
                result[count] = -1; //This will be the sentinel value
                AddPoints(answersP1, result, player1);
                AddPoints(answersP2, result, player2);
                AddPoints(answersP3, result, player3);
            }
            //Close the connection
            dataReader.Close();
            command.Dispose();
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
            while (dataReader.Read() && stop == false) //Reads the next line, stops if the results column is null
            {
                if (count == 0)
                {
                    nullChecker = dataReader.GetValue(3).ToString(); //Checks the value in the results column to make sure it is not null
                    stop = CheckIfNull(nullChecker); //stop will return true if the value is null
                }
                if (stop == false) //This occurs if there are results to read
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
            } 

            AddPoints(answers[0], results, f1Scoring, player1); //Adds points for each player
            AddPoints(answers[1], results, f1Scoring, player2); //Adds points for each player
            AddPoints(answers[2], results, f1Scoring, player3); //Adds points for each player
            //Close the connection
            dataReader.Close();
            command.Dispose();
        }
        //This method will check to see if a string value is null
        public static Boolean CheckIfNull(string nullChecker)
        {
            Boolean stop = false;
            if (string.IsNullOrEmpty(nullChecker)) //If the results column is null, then no points will be added or subtracted
            {
                stop = true; //Indicates that the loop should stop due to a null value
            }
            return stop;
        }
        //This method will loop through the results and add points for each player
        public static void AddPoints(string answer, string[] results, int[] f1Scoring, Player player)
        {
            for (int x = 0; x < 10; x++) //Loops through all 10 results
            {
                if (answer == results[x]) //When the player's answer matches the results, they gain points
                {
                    player.UpdatePoints(f1Scoring[x]); //Points are updated with F1 scoring style, meaning 25 for first, 18 for second, etc.
                }
            }
        }
        //This method will check values within a range and add points
        public static void AddPoints(int[] answers, int[] results, Player player)
        {
            int count = 0;
            const int CORRECT = 5; //Player gains 5 points if they are exact
            const int SLIGHTLY_OFF = 2; //Player gains 2 points if they are within 2
            while (results[count] != -1) //Loops through all the results. -1 is the sentinel value
            {
                if (answers[count] == results[count]) //When the player's answer matches the results, they gain points
                {
                    player.UpdatePoints(CORRECT); //If they are exact, the player will gain five points
                }
                else if ((results[count] - 2) < answers[count] && answers[count] < (results[count] + 2)) //If the player was within 2, they gain 2 points
                {
                    player.UpdatePoints(SLIGHTLY_OFF);
                }
                count++; //Increment the count
            }
        }
        //This will simply output the scores for each player
        public static void DisplayPoints(Player player1, Player player2, Player player3)
        {
            Console.WriteLine(player1.ToString());
            Console.WriteLine(player2.ToString());
            Console.WriteLine(player3.ToString());
        }
        //Just something I made that I thought was cool
        public static void F1Car()
        {
            Console.WriteLine(@"                                  __________________");
            Console.WriteLine(@"__________                     /                    |");
            Console.WriteLine(@"\         \                 /               /      |           _____");
            Console.WriteLine(@" \         \             /               /         |         /       \ ");
            Console.WriteLine(@"  \         \  _____  /                /           \      /             \ ");
            Console.WriteLine(@"    ___    ____                                     ---/                  \ _____   ____ ");
            Console.WriteLine(@"        /        \            ______________              \________________/     /        \   ");
            Console.WriteLine(@"       |          |                                                             |          | \ ");
            Console.WriteLine(@"       |          |                                                             |          |    \  _________ ");
            Console.WriteLine(@"        \        /  ___________________________________________________________  \        /  ___  |_________| ");
            Console.WriteLine(@"          ------                                                                   -------     ");
            Console.WriteLine("\n");
        }
    }
}
