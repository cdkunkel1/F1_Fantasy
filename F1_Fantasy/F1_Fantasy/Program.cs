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
            int scoringStyle = 1; //Will be used to indicate a different type of scoring when using the Ranking() method

            SqlConnection cnn = new SqlConnection(conString); //Create a SQL Connection object to connect to the F1 Database
            OpenConnection(cnn); //Open the connection to the database

            //Use a Player class to keep track of names and points
            Player[] players = new Player[20];
            players[0] = new Player("Nathan");
            Player.IncCount();
            players[1] = new Player("Ben");
            Player.IncCount();
            players[2] = new Player("Cory");
            Player.IncCount(); //Count is now at three for the number of players

            sql = @"SELECT * FROM [Formula_1].[dbo].[WDC Rankings]"; //SQL statement to select data for Driver Rankings
            Rankings(cnn, players, sql, scoringStyle);

            sql = @"SELECT * FROM [Formula_1].[dbo].[Constructor Rankings]"; //SQL statement to select data for constructor rankings
            Rankings(cnn, players, sql, scoringStyle);

           /* sql = @"SELECT * FROM [Formula_1].[dbo].[Fastest Pit Stop]"; //SQL statement to select data for fastest pit stop
            SingleSelection(cnn, player1, player2, player3, sql, f1Scoring);

            sql = @"SELECT * FROM [Formula_1].[dbo].[Most DOD]"; //SQL statement to select data for most driver of the days
            SingleSelection(cnn, player1, player2, player3, sql, f1Scoring);

            sql = @"SELECT * FROM [Formula_1].[dbo].[Safety/VSC]"; //SQL statement to select data for number of safety cars and VSC's
            ClosestSelection(cnn, player1, player2, player3, sql, f1Scoring);

            sql = @"SELECT * FROM [Formula_1].[dbo].[Dominant TM]"; //SQL statement to select data for the most dominant teammate
            SingleSelection(cnn, player1, player2, player3, sql, f1Scoring);

            sql = @"SELECT * FROM [Formula_1].[dbo].[Podium Drivers]"; //SQL statement to select data for which drivers got a podium
            scoringStyle = 2; //Indicates a different type of scoring
            Rankings(cnn, player1, player2, player3, sql, scoringStyle);

            sql = @"SELECT * FROM [Formula_1].[dbo].[Most Penalized T]"; //SQL statement to select data for the most penalized team
            SingleSelection(cnn, player1, player2, player3, sql, f1Scoring);

            scoringStyle = 3;
            sql = @"SELECT * FROM [Formula_1].[dbo].[After Six Races]"; //SQL statement to select data for the WDC rankings after six races
            Rankings(cnn, player1, player2, player3, sql, scoringStyle);

            sql = @"SELECT * FROM [Formula_1].[dbo].[Fewest Laps]"; //SQL statement to select data on the driver with the least amount of laps completed
            SingleSelection(cnn, player1, player2, player3, sql, f1Scoring);

            sql = @"SELECT * FROM [Formula_1].[dbo].[Unbroken Lead]"; //SQL statement to select data on which race the WDC was won at
            ClosestSelection(cnn, player1, player2, player3, sql, f1Scoring);

            sql = @"SELECT * FROM [Formula_1].[dbo].[Race Retirements]"; //SQL statement to select data on the number of retirements per race
            scoringStyle = 1;
            CheckIfPicked(cnn, player1, player2, player3, sql, scoringStyle);

            sql = @"SELECT * FROM [Formula_1].[dbo].[Random Events]"; //SQL statement to select data on the number of random events that occurred
            ClosestSelection(cnn, player1, player2, player3, sql, f1Scoring);

            sql = @"SELECT * FROM [Formula_1].[dbo].[Y/N Scenarios]"; //SQL statement to select data on whether or not certain events happened
            scoringStyle = 2;
            CheckIfPicked(cnn, player1, player2, player3, sql, scoringStyle); */


            CloseConnection(cnn);

            F1Car(); //Displays an F1 Car
            DisplayPoints(players);
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
        
        public static void Rankings(SqlConnection cnn, Player[] players, string sql, int scoringStyle)
        {
            int[] answers = new int[30]; //These are set at 30 to account for any new drivers or teams
            string nullChecker = "";
            Boolean stop = false;
            int[] result = new int[30];
            int count = 0;
            for (int x = 0; x < Player.GetCount(); x++)
            {
                SqlCommand command = new SqlCommand(sql, cnn); //Executes the sql command to return the table
                SqlDataReader dataReader = command.ExecuteReader(); //Begin to read the table
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
                        answers[count] = dataReader.GetInt32(x + 1); //These are what the players picked
                    }
                    count++;
                }
                if (stop == false && scoringStyle == 1) //Will not add points if the results values are null
                {
                    result[count] = -1; //This will be the sentinel value
                    AddPoints(answers, result, players[x]);
                }
                else if (stop == false && scoringStyle == 2) //This is a different scoring style based on the question
                {
                    result[count] = -1;
                    ChangePoints(answers, result, players[x]);
                }
                else if (stop == false && scoringStyle == 3) //Different scoring style
                {
                    result[count] = -1;
                    AddPointsV2(answers, result, players[x]);

                }
                //Close the connection
                dataReader.Close();
                command.Dispose();
            }
        }
        //This method will be used to calculate points from the fastest pit stop question
        public static void SingleSelection(SqlConnection cnn, Player player1, Player player2, Player player3, string sql, int[] f1Scoring)
        {
            string[] results = new string[30]; //Stores the actual results
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
            results[count] = "stop"; //Sentinel Value

            if (stop == false)
            {
                AddPoints(answers[0], results, f1Scoring, player1); //Adds points for each player
                AddPoints(answers[1], results, f1Scoring, player2); //Adds points for each player
                AddPoints(answers[2], results, f1Scoring, player3); //Adds points for each player
            }
            //Close the connection
            dataReader.Close();
            command.Dispose();
        }

        public static void ClosestSelection(SqlConnection cnn, Player player1, Player player2, Player player3, string sql, int[] f1Scoring)
        {
            int[] answers = new int[3];
            int result = 0;
            string nullChecker = "";
            SqlCommand command = new SqlCommand(sql, cnn); //Executes the sql command to return the table
            SqlDataReader dataReader = command.ExecuteReader(); //Begin to read the table
            Boolean stop = false;

            while (dataReader.Read() && stop == false) //Reads the next line, stops if the results column is null
            {
              
                nullChecker = dataReader.GetValue(3).ToString(); //Checks the value in the results column to make sure it is not null
                stop = CheckIfNull(nullChecker); //stop will return true if the value is null
                
                if (stop == false) //This occurs if there are results to read
                {
                    answers[0] = dataReader.GetInt32(0);
                    answers[1] = dataReader.GetInt32(1);
                    answers[2] = dataReader.GetInt32(2);
                    result = dataReader.GetInt32(3); //Every row's results will be stored in this array
                }
            }

            if (stop == false)
            {
                GetDistance(answers, result); //Finds how far the player's were from the results
                SortAnswers(answers, f1Scoring, player1, player2, player3);
                AddPoints(answers[0], player1);
                AddPoints(answers[1], player2);
                AddPoints(answers[2], player3);
            }

            //Close the connection
            dataReader.Close();
            command.Dispose();
        }
        //This method will calculate how many points the players lose during the three races they selected for retirements
        public static void CheckIfPicked(SqlConnection cnn, Player player1, Player player2, Player player3, string sql, int scoringStyle)
        {
            int[] answers = new int[3];
            int result = 0;
            string nullChecker = "";
            SqlCommand command = new SqlCommand(sql, cnn); //Executes the sql command to return the table
            SqlDataReader dataReader = command.ExecuteReader(); //Begin to read the table
            Boolean stop = false;

            while (dataReader.Read() && stop == false) //Reads the next line, stops if the results column is null
            {
                if (scoringStyle == 1)
                {
                    nullChecker = dataReader.GetValue(5).ToString(); //Checks the value in the results column to make sure it is not null
                    stop = CheckIfNull(nullChecker); //stop will return true if the value is null
                }
                else if (scoringStyle == 2)
                {
                    nullChecker = dataReader.GetValue(4).ToString(); //Checks the value in the results column to make sure it is not null
                    stop = CheckIfNull(nullChecker); //stop will return true if the value is null
                }

                if (stop == false)
                {
                    if (scoringStyle == 1)
                    {
                        answers[0] = dataReader.GetInt32(2); //Checks to see if the player chose the race
                        answers[1] = dataReader.GetInt32(3);
                        answers[2] = dataReader.GetInt32(4);
                        result = dataReader.GetInt32(5);
                    }
                    else if (scoringStyle == 2) //Different columns must be read in the second scoring style
                    {
                        answers[0] = dataReader.GetInt32(1); //Checks to see if the player chose the race
                        answers[1] = dataReader.GetInt32(2);
                        answers[2] = dataReader.GetInt32(3);
                        result = dataReader.GetInt32(4);
                    }

                    if (scoringStyle == 1)
                    {


                        if (answers[0] == 1) //If the player chose the race
                        {
                            SubtractPoints(result, player1); //They lose five points for every retirement in their chose race
                        }
                        if (answers[1] == 1) //These will check to see if any of the players picked the race
                        {
                            SubtractPoints(result, player2);
                        }
                        if (answers[2] == 1)
                        {
                            SubtractPoints(result, player3);
                        }
                    }
                    else if (scoringStyle == 2)
                    {
                        CheckEvent(result, player1, answers[0]);
                        CheckEvent(result, player2, answers[1]);
                        CheckEvent(result, player3, answers[2]);
                    }
                }
            }
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
            int count = 0;
            while (results[count] != "stop") //Loops through all 10 results
            {
                if (answer == results[count]) //When the player's answer matches the results, they gain points
                {
                    player.UpdatePoints(f1Scoring[count]); //Points are updated with F1 scoring style, meaning 25 for first, 18 for second, etc.
                }
                count++;
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
        //This method will check to see if a driver was guessed or not
        public static void ChangePoints(int[] answers, int[] results, Player player)
        {
            int count = 0;
            const int CORRECT = 5; //Player gains 5 points if they are exact
            const int INCORRECT = -3; //Player gains 2 points if they are within 2
            while (results[count] != -1) //Loops through all the results. -1 is the sentinel value
            {
                if (answers[count] == results[count]) //When the player's answer matches the results, they gain points
                {
                    player.UpdatePoints(CORRECT); //If the player guessed right, they will gain five points
                }
                else
                {
                    player.UpdatePoints(INCORRECT); //If the player guessed wrong or missed one, they lose three points
                }
                count++; //Increment the count
            }
        }
        //Adds a specific number of points for a player
        public static void AddPoints(int f1Scoring, Player player)
        {
            player.UpdatePoints(f1Scoring);
        }
        //This method will check values within a range and add points
        public static void AddPointsV2(int[] answers, int[] results, Player player)
        {
            int count = 0;
            const int CORRECT = 5; //Player gains 5 points if they are exact
            const int ONE_OFF = 3; //Player gains 2 points if they are within 2
            const int TWO_OFF = 1;
            while (results[count] != -1) //Loops through all the results. -1 is the sentinel value
            {
                if (answers[count] == results[count]) //When the player's answer matches the results, they gain points
                {
                    player.UpdatePoints(CORRECT); //If they are exact, the player will gain five points
                }
                else if ((results[count] - 1) < answers[count] && answers[count] < (results[count] + 1)) //If the player was within 2, they gain 2 points
                {
                    player.UpdatePoints(ONE_OFF);
                }
                else if ((results[count] - 2) < answers[count] && answers[count] < (results[count] + 2)) //If the player was within 2, they gain 2 points
                {
                    player.UpdatePoints(TWO_OFF);
                }
                count++; //Increment the count
            }
        }
        //This method will subtract points from the player for every retirement in a selected race
        public static void SubtractPoints(int result, Player player)
        {
            int pointsLost = (result * -5);
            player.UpdatePoints(pointsLost);
        }
        //This method will check to see if the user correctly guessed true or false for events and will add points as a result
        public static void CheckEvent(int results, Player player, int answer)
        {
            const int CORRECT_TRUE = 5;
            const int CORRECT_FALSE = 3;
            const int INCORRECT = -3;
            
            if (results == 1 && results == answer) //If the user correctly guessed true, then they will gain 5 points
            {
                player.UpdatePoints(CORRECT_TRUE);
            }
            else if (results == 0 && results == answer) //If the user correctly guessed false, then they will gain 3 points
            {
                player.UpdatePoints(CORRECT_FALSE);
            }
            else //If the user guessed incorrectly, then they lose three points
            {
                player.UpdatePoints(INCORRECT);
            }
        }
        //This method will return the player's answers as their distances from the answer
        public static void GetDistance(int[] answer, int result)
        {
            for (int x = 0; x < answer.Length; x++) 
            {
                answer[x] = Math.Abs(result - answer[x]); //Sets the answer array to how far away the guess was from the result
            }
        }
        //Sorts the player's answers from closest to furthest then gives them points
        public static void SortAnswers(int[]answers, int[] f1Scoring, Player player1, Player player2, Player player3)
        {
            int a = answers[0];
            int b = answers[1];
            int c = answers[2];
            //These if statements will sort the numbers from least (a) to greatest (c)
            if (a > c)
            {
                Swap(ref a, ref c);
            }
            if (a > b)
            {
                Swap(ref a, ref b);
            }
            if (b > c)
            {
                Swap(ref b, ref c);
            }          
            //This will loop through the player's answers and assign them points based on how far their answer was
            for(int x = 0; x < 3; x++)
            {
                if (answers[x] == a) //If the player's answer matched the closest, then the will receive the most points
                {
                    answers[x] = f1Scoring[0]; 
                }
                else if (answers[x] == b) //If two players guess the same number, then it is possible for them to tie each other
                {
                    answers[x] = f1Scoring[1];
                }
                else if (answers[x] == c) //Note that if the first two players tie for first, the third player will receive third place points
                {
                    answers[x] = f1Scoring[2];
                }
            }            
        }
        //Swaps two numbers
        public static void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
            
        }
        //This will simply output the scores for each player
        public static void DisplayPoints(Player[] players)
        {
            for (int x = 0; x < Player.GetCount(); x++)
            {
                Console.WriteLine(players[x].ToString());
            }
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
