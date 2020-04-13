using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace F1_Fantasy
{
    class Program
    {
        static void Main(string[] args)
        {
            //Declare Variables
            int[] f1Scoring = { 25, 18, 15, 12, 10, 8, 6, 4, 2, 1 };
            int[] potentialPoints = { 100, 65, 25, 25, 25, 25, 100, 25, 30, 25, 25, 0, 25, 40 };
            
            int[] position = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

            int driverCount = 0;
            int constructorCount = 0;
            int raceCount = 0;
            string sql = "";
            string conString = @"Data Source = (LocalDB)\Formula_1; Initial Catalog = Formula_1; Integrated Security = True"; //This is the connection string for the F1 database
            int scoringStyle = 1; //Will be used to indicate a different type of scoring when using the Ranking() method
            Boolean exit = false;
            int menuChoice = 0;
            string rankingType = "";

            var url = "https://ergast.com/api/f1/current/driverStandings.json"; //URL link for the WDC Rankings API
            var RootObject = _download_serialized_json_data<RootObject>(url); //Deserialize the json data into objects
            List<DriverStanding> driverStandings = new List<DriverStanding>(); //Create a new list that will make it easier to access the API data
            foreach(DriverStanding standing in RootObject.MRData.StandingsTable.StandingsLists[0].DriverStandings) //Populate the list with each value found in driver standings
            {
                driverStandings.Add(RootObject.MRData.StandingsTable.StandingsLists[0].DriverStandings[driverCount]);
                driverCount++; //Keep track of how many elements were added to the list
            }

            url = "https://ergast.com/api/f1/current/constructorStandings.json"; //URL link for the constructor standings API
            RootObject = _download_serialized_json_data<RootObject>(url); //Deserialize the json data into objects
            List<ConstructorStanding> constructorStandings = new List<ConstructorStanding>(); //Create a new list that will make it easier to access the API data
            foreach (ConstructorStanding standing in RootObject.MRData.StandingsTable.StandingsLists[0].ConstructorStandings) //Populate the list with each value found in driver standings
            {
                constructorStandings.Add(RootObject.MRData.StandingsTable.StandingsLists[0].ConstructorStandings[constructorCount]);
                constructorCount++; //Keep track of how many elements were added to the list
            }

            url = "https://ergast.com/api/f1/current.json"; //URL link for the race schedule API
            RootObject = _download_serialized_json_data<RootObject>(url); //Deserialize the json data into objects
            List<Race> raceSchedule = new List<Race>(); //Create a new list that will make it easier to access the API data
            foreach (Race race in RootObject.MRData.RaceTable.Races) //Populate the list with each value found in driver standings
            {
                raceSchedule.Add(RootObject.MRData.RaceTable.Races[raceCount]);
                raceCount++; //Keep track of how many elements were added to the list
            }

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
            Questions question = new Questions();
            


            sql = @"SELECT * FROM [Formula_1].[dbo].[WDC Rankings]"; //SQL statement to select data for Driver Rankings. Potential points is 100
            Rankings(cnn, players, sql, scoringStyle, question.GetQuestionNumber(0));

            sql = @"SELECT * FROM [Formula_1].[dbo].[Constructor Rankings]"; //SQL statement to select data for constructor rankings. Potential points is 65
            Rankings(cnn, players, sql, scoringStyle, question.GetQuestionNumber(1));

            sql = @"SELECT * FROM [Formula_1].[dbo].[Fastest Pit Stop]"; //SQL statement to select data for fastest pit stop. Potential points is 25
            SingleSelection(cnn, players, sql, f1Scoring, question.GetQuestionNumber(2));

            sql = @"SELECT * FROM [Formula_1].[dbo].[Most DOD]"; //SQL statement to select data for most driver of the days. Potential points is 25
            SingleSelection(cnn, players, sql, f1Scoring, question.GetQuestionNumber(3));

            sql = @"SELECT * FROM [Formula_1].[dbo].[Safety/VSC]"; //SQL statement to select data for number of safety cars and VSC's. Potential points is 25
            ClosestSelection(cnn, players, sql, f1Scoring, question.GetQuestionNumber(4));

            sql = @"SELECT * FROM [Formula_1].[dbo].[Dominant TM]"; //SQL statement to select data for the most dominant teammate. Potential points is 25
            SingleSelection(cnn, players, sql, f1Scoring, question.GetQuestionNumber(5));

            sql = @"SELECT * FROM [Formula_1].[dbo].[Podium Drivers]"; //SQL statement to select data for which drivers got a podium. Potential points is 100
            scoringStyle = 2; //Indicates a different type of scoring
            Rankings(cnn, players, sql, scoringStyle, question.GetQuestionNumber(6));

            sql = @"SELECT * FROM [Formula_1].[dbo].[Most Penalized T]"; //SQL statement to select data for the most penalized team. Potential points is 25
            SingleSelection(cnn, players, sql, f1Scoring, question.GetQuestionNumber(7));

            scoringStyle = 3;
            sql = @"SELECT * FROM [Formula_1].[dbo].[After Six Races]"; //SQL statement to select data for the WDC rankings after six races. Potential points is 30
            Rankings(cnn, players, sql, scoringStyle, question.GetQuestionNumber(8));

            sql = @"SELECT * FROM [Formula_1].[dbo].[Fewest Laps]"; //SQL statement to select data on the driver with the least amount of laps completed. Potential points is 25
            SingleSelection(cnn, players, sql, f1Scoring, question.GetQuestionNumber(9));

            sql = @"SELECT * FROM [Formula_1].[dbo].[Unbroken Lead]"; //SQL statement to select data on which race the WDC was won at. Potential points is 25
            ClosestSelection(cnn, players, sql, f1Scoring, question.GetQuestionNumber(10));

            sql = @"SELECT * FROM [Formula_1].[dbo].[Race Retirements]"; //SQL statement to select data on the number of retirements per race. Potential points to gain is 0
            scoringStyle = 1;
            CheckIfPicked(cnn, players, sql, scoringStyle, question.GetQuestionNumber(11));

            sql = @"SELECT * FROM [Formula_1].[dbo].[Random Events]"; //SQL statement to select data on the number of random events that occurred. Potential points is 25
            ClosestSelection(cnn, players, sql, f1Scoring, question.GetQuestionNumber(12));

            sql = @"SELECT * FROM [Formula_1].[dbo].[Y/N Scenarios]"; //SQL statement to select data on whether or not certain events happened. Potential points is 40
            scoringStyle = 2;
            CheckIfPicked(cnn, players, sql, scoringStyle, question.GetQuestionNumber(13)); 


            PlayerReports playerReport = new PlayerReports(players, question.GetQuestionNameArray(), question.GetQuestionNumberArray());

            while (exit == false) //Continues until the user chooses to quit
            {
                F1Car(); //Displays an F1 Car
                DisplayMenu(); //Displays a user with menu options to the user
                menuChoice = GetUserInput();
                switch(menuChoice) //Checks the user's menu choice
                {
                    case 1: 
                        DisplayPoints(players); //Displays current number of points by player
                        break;
                    case 2: 
                        playerReport.PrintAllScores(); //Prints all the questions and number of points given by each
                        break;
                    case 3:
                        DisplayDriverRankings(driverStandings, driverCount);
                        break;
                    case 4:
                        DisplayConstructorRankings(constructorStandings, constructorCount);
                        break;
                    case 5:
                        DisplayRaceSchedule(raceSchedule, raceCount);
                        break;
                    case 6: 
                        exit = ExitMessage();
                        break;
                }
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                Console.Clear();
            }
            CloseConnection(cnn);
        }
        //Method that will pull the Json data from the API and deserialize it
        private static T _download_serialized_json_data<T>(string url) where T : new()
        {
            using (var w = new WebClient())
            {
                var json_data = string.Empty;
                // attempt to download JSON data as a string
                try
                {
                    json_data = w.DownloadString(url);
                }
                catch (Exception) { }
                // if string with JSON data is not empty, deserialize it to class and return its instance 
                return !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<T>(json_data) : new T();
            }
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
        
        public static void Rankings(SqlConnection cnn, Player[] players, string sql, int scoringStyle, int questionNumber)
        {
            int[] answers = new int[30]; //These are set at 30 to account for any new drivers or teams
            string nullChecker = "";
            Boolean stop = false;
            int[] result = new int[30];
            int count = 0;
            for (int x = 0; x < Player.GetCount(); x++)
            {
                count = 0;
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
                result[count] = -1;
                //Close the connection
                dataReader.Close();
                command.Dispose();
                if (stop == false && scoringStyle == 1) //Will not add points if the results values are null
                {
                    result[count] = -1; //This will be the sentinel value
                    AddPoints(answers, result, players[x], questionNumber);
                }
                else if (stop == false && scoringStyle == 2) //This is a different scoring style based on the question
                {
                    result[count] = -1;
                    ChangePoints(answers, result, players[x], questionNumber);
                }
                else if (stop == false && scoringStyle == 3) //Different scoring style
                {
                    result[count] = -1;
                    AddPointsV2(answers, result, players[x], questionNumber);

                }
                
            }
        }
        //This method will be used to calculate points from the fastest pit stop question
        public static void SingleSelection(SqlConnection cnn, Player[] players, string sql, int[] f1Scoring, int questionNumber)
        {
            string[] results = new string[30]; //Stores the actual results
            string[] answers = new string[20]; //Stores the player guesses
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
                        for (int x = 0; x < Player.GetCount(); x++)
                        {
                            answers[x] = dataReader.GetValue(x).ToString();
                        }
                    }
                    results[count] = dataReader.GetValue(3).ToString(); //Every row's results will be stored in this array
                    count++; //Increments to account for a new row
                }
            }
            results[count] = "stop"; //Sentinel Value

            if (stop == false)
            {
                for (int x = 0; x < Player.GetCount(); x++)
                {
                    AddPoints(answers[x], results, f1Scoring, players[x], questionNumber); //Adds points for each player
                }
            }
            //Close the connection
            dataReader.Close();
            command.Dispose();
        }

        public static void ClosestSelection(SqlConnection cnn, Player[] players, string sql, int[] f1Scoring, int questionNumber)
        {
            int[] answers = new int[20];
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
                    for (int x = 0; x < Player.GetCount(); x++)
                    {
                        answers[x] = dataReader.GetInt32(x);
                        players[x].SetAnswer(answers[x]);
                    }
                    result = dataReader.GetInt32(Player.GetCount()); //The results column will be located at the Player.GetCount() number
                }
            }

            if (stop == false)
            {
                GetDistance(players, result); //Finds how far the player's were from the results
                SortAnswers(f1Scoring, players, questionNumber);
            }

            //Close the connection
            dataReader.Close();
            command.Dispose();
        }
        //This method will calculate how many points the players lose during the three races they selected for retirements
        public static void CheckIfPicked(SqlConnection cnn, Player[] players, string sql, int scoringStyle, int questionNumber)
        {
            int[] answers = new int[20];
            int result = 0;
            string nullChecker = "";
            SqlCommand command = new SqlCommand(sql, cnn); //Executes the sql command to return the table
            SqlDataReader dataReader = command.ExecuteReader(); //Begin to read the table
            Boolean stop = false;

            while (dataReader.Read() && stop == false) //Reads the next line, stops if the results column is null
            {
                nullChecker = dataReader.GetValue(4).ToString(); //Checks the value in the results column to make sure it is not null
                stop = CheckIfNull(nullChecker); //stop will return true if the value is null

                if (stop == false)
                {           
                    for(int x = 0; x < Player.GetCount(); x++)
                    {
                        answers[x] = dataReader.GetInt32(x + 1); //Checks to see if the player chose a race, indicated by a 1
                    }
                    result = dataReader.GetInt32(Player.GetCount() + 1); //Stores the number of retirements for a race

                    if (scoringStyle == 1)
                    {
                        for (int x = 0; x < Player.GetCount(); x++)
                        {
                            if (answers[x] == 1)
                            {
                                SubtractPoints(result, players[x], questionNumber); //They lose five points for every retirement in their chose race
                            }
                        }
                    }
                    else if (scoringStyle == 2)
                    {
                        for (int x = 0; x < Player.GetCount(); x++)
                        {
                            CheckEvent(result, players[x], answers[x], questionNumber); //Checks to see if the event happened or not and whether the player picked it
                        }
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
        public static void AddPoints(string answer, string[] results, int[] f1Scoring, Player player, int questionNumber)
        {
            int count = 0;
            while (results[count] != "stop") //Loops through all 10 results
            {
                if (answer == results[count]) //When the player's answer matches the results, they gain points
                {
                    player.UpdatePoints(f1Scoring[count]); //Points are updated with F1 scoring style, meaning 25 for first, 18 for second, etc.
                    player.SetPointsByQuestion(f1Scoring[count], questionNumber);
                }
                count++;
            }
        }
        //This method will check values within a range and add points
        public static void AddPoints(int[] answers, int[] results, Player player, int questionNumber)
        {
            int count = 0;
            int pointsTracker = 0; //This will be used to keep track of the total points earned by the player on this question
            const int CORRECT = 5; //Player gains 5 points if they are exact
            const int SLIGHTLY_OFF = 2; //Player gains 2 points if they are within 2
            while (results[count] != -1) //Loops through all the results. -1 is the sentinel value
            {
                if (answers[count] == results[count]) //When the player's answer matches the results, they gain points
                {
                    player.UpdatePoints(CORRECT); //If they are exact, the player will gain five points
                    pointsTracker += CORRECT;
                }
                else if ((results[count] - 2) < answers[count] && answers[count] < (results[count] + 2)) //If the player was within 2, they gain 2 points
                {
                    player.UpdatePoints(SLIGHTLY_OFF);
                    pointsTracker += SLIGHTLY_OFF;
                }
                count++; //Increment the count
            }
            player.SetPointsByQuestion(pointsTracker, questionNumber);
        }
        //This method will check to see if a driver was guessed or not
        public static void ChangePoints(int[] answers, int[] results, Player player, int questionNumber)
        {
            int count = 0;
            const int CORRECT = 5; //Player gains 5 points if they are exact
            const int INCORRECT = -3; //Player gains 2 points if they are within 2
            while (results[count] != -1) //Loops through all the results. -1 is the sentinel value
            {
                if (answers[count] == results[count]) //When the player's answer matches the results, they gain points
                {
                    player.UpdatePoints(CORRECT); //If the player guessed right, they will gain five points
                    player.SetPointsByQuestion(CORRECT, questionNumber); //Assigns the number of points earned by this question
                }
                else
                {
                    player.UpdatePoints(INCORRECT); //If the player guessed wrong or missed one, they lose three points
                    player.SetPointsByQuestion(INCORRECT, questionNumber); //Assigns the number of points earned by this question
                }
                count++; //Increment the count
            }
        }
        //Adds a specific number of points for a player
        public static void AddPoints(int f1Scoring, Player player, int questionNumber)
        {
            player.UpdatePoints(f1Scoring);
            player.SetPointsByQuestion(f1Scoring, questionNumber); //Assigns the number of points earned by this question
        }
        //This method will check values within a range and add points
        public static void AddPointsV2(int[] answers, int[] results, Player player, int questionNumber)
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
                    player.SetPointsByQuestion(CORRECT, questionNumber); //Assigns the number of points earned by this question
                }
                else if ((results[count] - 1) < answers[count] && answers[count] < (results[count] + 1)) //If the player was within 2, they gain 2 points
                {
                    player.UpdatePoints(ONE_OFF);
                    player.SetPointsByQuestion(ONE_OFF, questionNumber); //Assigns the number of points earned by this question
                }
                else if ((results[count] - 2) < answers[count] && answers[count] < (results[count] + 2)) //If the player was within 2, they gain 2 points
                {
                    player.UpdatePoints(TWO_OFF);
                    player.SetPointsByQuestion(TWO_OFF, questionNumber); //Assigns the number of points earned by this question
                }
                count++; //Increment the count
            }
        }
        //This method will subtract points from the player for every retirement in a selected race
        public static void SubtractPoints(int result, Player player, int questionNumber)
        {
            int pointsLost = (result * -5);
            player.UpdatePoints(pointsLost);
            player.SetPointsByQuestion(pointsLost, questionNumber); //Assigns the number of points earned by this question
        }
        //This method will check to see if the user correctly guessed true or false for events and will add points as a result
        public static void CheckEvent(int results, Player player, int answer, int questionNumber)
        {
            const int CORRECT_TRUE = 5;
            const int CORRECT_FALSE = 3;
            const int INCORRECT = -3;
            
            if (results == 1 && results == answer) //If the user correctly guessed true, then they will gain 5 points
            {
                player.UpdatePoints(CORRECT_TRUE);
                player.SetPointsByQuestion(CORRECT_TRUE, questionNumber); //Assigns the number of points earned by this question
            }
            else if (results == 0 && results == answer) //If the user correctly guessed false, then they will gain 3 points
            {
                player.UpdatePoints(CORRECT_FALSE);
                player.SetPointsByQuestion(CORRECT_FALSE, questionNumber); //Assigns the number of points earned by this question
            }
            else //If the user guessed incorrectly, then they lose three points
            {
                player.UpdatePoints(INCORRECT);
                player.SetPointsByQuestion(INCORRECT, questionNumber); //Assigns the number of points earned by this question
            }
        }
        //This method will return the player's answers as their distances from the answer
        public static void GetDistance(Player[] players, int result)
        {
            for (int x = 0; x < Player.GetCount(); x++) 
            {
                players[x].SetAnswer(Math.Abs(result - players[x].GetAnswer())); //Sets the answer array to how far away the guess was from the result
            }
        }
        //Sorts the player's answers from closest to furthest then gives them points
        public static void SortAnswers(int[] f1Scoring, Player[] players, int questionNumber)
        {
            int points = 0;

            for(int x = 0; x < Player.GetCount(); x++)
            {
                players[x].SetPosition(x); //This will asign defaul positioning for the players 
            }
            //The positions will then be sorted
            for (int x = 0; x < Player.GetCount() - 1; x++) //Loops through all the players and gives them positions starting at 0
            {
                int min = x;

                for (int j = x + 1; j < Player.GetCount(); j++)
                {
                    if (players[j].GetAnswer() < players[min].GetAnswer())
                    {
                        min = j;
                    }
                }

                if (min != x)
                {
                    SwapPositions(players, min, x); //Swaps positions to sort from least to greatest
                }
                for (int i = 0; i < Player.GetCount(); i++) //Checks to see if any players chose the same answer
                {
                    if (players[x].GetAnswer() == players[i].GetAnswer()) //If players chose the same answer, they will be put in the same position
                    {
                        players[i].SetPosition(players[x].GetPosition());
                    }
                }
            }
            //This will loop through the player's answers and assign them points based on how far their answer was
            for (int x = 0; x < Player.GetCount(); x++)
            {
                points = f1Scoring[players[x].GetPosition()];
                AddPoints(points, players[x], questionNumber);
            }            
        }
        //Swaps two numbers
        public static void SwapPositions(Player[] players, int x, int y)
        {
            int temp = players[x].GetPosition();
            players[x].SetPosition(players[y].GetPosition());
            players[y].SetPosition(temp);
            
        }
        //This will simply output the scores for each player
        public static void DisplayPoints(Player[] players)
        {
            Console.WriteLine("Here are the current points for each player:\n");
            for (int x = 0; x < Player.GetCount(); x++)
            {
                Console.WriteLine(players[x].ToString());
            }
        }
        //Displays the current standings for the drivers
        public static void DisplayDriverRankings(List<DriverStanding> standings, int count)
        {
            Console.WriteLine("Position\t" + "Driver".PadRight(15) + "\tPoints".PadRight(8) + "\tWins");
            for (int x = 0; x < count; x++)
            {
                Console.WriteLine(standings[x].position.PadRight(8) + "\t" + standings[x].Driver.familyName.PadRight(15) + "\t" + standings[x].points.PadRight(8) + standings[x].wins);
            }
        }
        //Displays the current standings for the constructors
        public static void DisplayConstructorRankings(List<ConstructorStanding> standings, int count)
        {
            Console.WriteLine("Position\t" + "Constructor".PadRight(15) + "\tPoints".PadRight(8) + "\tWins");
            for (int x = 0; x < count; x++)
            {
                Console.WriteLine(standings[x].position.PadRight(8) + "\t" + standings[x].Constructor.name.PadRight(15) + "\t" + standings[x].points.PadRight(8) + standings[x].wins);
            }
        }
        //Displays the schedule for the season
        public static void DisplayRaceSchedule(List<Race> schedule, int count)
        {
            Console.WriteLine("Round\t" + "Race".PadRight(30) + "\tDate".PadRight(10) + "\tTrack Name");
            for (int x = 0; x < count; x++)
            {
                Console.WriteLine(schedule[x].round.PadRight(5) + "\t" + schedule[x].raceName.PadRight(30) + "\t" + schedule[x].date.PadRight(10) + "\t" + schedule[x].Circuit.circuitName);
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
        //This will display a menu of application options for the user
        public static void DisplayMenu()
        {
            Console.WriteLine("It's the final few seconds before the lights go out, and the race to the podium begins...\n");
            Console.WriteLine("Fantasy F1");
            Console.WriteLine("Please enter an option from the menu\n");
            Console.WriteLine("1. Current Points");
            Console.WriteLine("2. Points Earned by Question");
            Console.WriteLine("3. Current WDC Standings");
            Console.WriteLine("4. Current Constructor Standings");
            Console.WriteLine("5. Race Schedule");
            Console.WriteLine("6. Exit");
        }
        //Gets the user input for the main menu
        public static int GetUserInput()
        {
            string userInputString = "";
            int userInput = 0;

            userInputString = Console.ReadLine();

            while (userInputString != "1" && userInputString != "2" && userInputString != "3" && userInputString != "4" && userInputString != "5" && userInputString != "6") //Ensures that user enters a number from the menu
            {
                Console.WriteLine("Please enter a number from the menu");
                userInputString = Console.ReadLine();
            }
            while (!int.TryParse(userInputString, out userInput)) //If the string can't be parsed to a int, then it will repeat and display an error message
            {
                Console.WriteLine("Please enter a number from the menu\n");
                userInputString = Console.ReadLine();
            }
            Console.Clear();
            return userInput;
        }

        public static Boolean ExitMessage()
        {
            Boolean exit = true;
            Console.WriteLine("Thanks for playing!\n");
            return exit;
        }
    }

    public class Driver
    {
        public string driverId { get; set; }
        public string permanentNumber { get; set; }
        public string code { get; set; }
        public string url { get; set; }
        public string givenName { get; set; }
        public string familyName { get; set; }
        public string dateOfBirth { get; set; }
        public string nationality { get; set; }
    }

    public class Constructor
    {
        public string constructorId { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public string nationality { get; set; }
    }

    public class DriverStanding
    {
        public string position { get; set; }
        public string positionText { get; set; }
        public string points { get; set; }
        public string wins { get; set; }
        public Driver Driver { get; set; }
        public List<Constructor> Constructors { get; set; }
    }

    public class ConstructorStanding
    {
        public string position { get; set; }
        public string positionText { get; set; }
        public string points { get; set; }
        public string wins { get; set; }
        public Constructor Constructor { get; set; }
    }

    public class StandingsList
    {
        public string season { get; set; }
        public string round { get; set; }
        public List<ConstructorStanding> ConstructorStandings { get; set; }
        public List<DriverStanding> DriverStandings { get; set; }
    }

    public class StandingsTable
    {
        public string season { get; set; }
        public List<StandingsList> StandingsLists { get; set; }
    }

    public class Location
    {
        public string lat { get; set; }
        public string @long { get; set; }
        public string locality { get; set; }
        public string country { get; set; }
    }

    public class Circuit
    {
        public string circuitId { get; set; }
        public string url { get; set; }
        public string circuitName { get; set; }
        public Location Location { get; set; }
    }

    public class Race
    {
        public string season { get; set; }
        public string round { get; set; }
        public string url { get; set; }
        public string raceName { get; set; }
        public Circuit Circuit { get; set; }
        public string date { get; set; }
        public string time { get; set; }
    }

    public class RaceTable
    {
        public string season { get; set; }
        public List<Race> Races { get; set; }
    }

    public class MRData
    {
        public string xmlns { get; set; }
        public string series { get; set; }
        public string url { get; set; }
        public string limit { get; set; }
        public string offset { get; set; }
        public string total { get; set; }
        public StandingsTable StandingsTable { get; set; }
        public RaceTable RaceTable { get; set; }
    }

    public class RootObject
    {
        public MRData MRData { get; set; }
    }
}
