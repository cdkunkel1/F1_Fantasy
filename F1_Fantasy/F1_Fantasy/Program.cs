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
            string conString = @"Data Source = (LocalDB)\Formula_1; Initial Catalog = Formula_1; Integrated Security = True"; //This is the connection string for the F1 database
            SqlConnection cnn = new SqlConnection(conString); //Create a SQL Connection object to connect to the F1 Database
            OpenConnection(cnn); //Open the connection to the database

            Player player1 = new Player("Nathan");
            Player player2 = new Player("Ben");
            Player player3 = new Player("Cory");

            DisplayPoints(player1.GetName(), player2.GetName(), player3.GetName());
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

        public static void DisplayPoints(string player1, string player2, string player3)
        {
            Console.WriteLine("Here are the points currently held by each player:\n");
            Console.WriteLine(player1 + ": " + currentPoints1);
            Console.WriteLine
        }
    }
}
