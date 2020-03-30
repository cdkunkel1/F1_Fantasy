using System;
using System.Collections.Generic;
using System.Text;

namespace F1_Fantasy
{
    class Questions
    {
        private string[] questionName = { "WDC Rankings", "Constructor Rankings", "Fastest Pit Stop", "Most Driver of the Day awards",
                "Number of safety cars/virtual safety cars", "Most dominant teammate", "Guessing podium drivers", "Most penalized team",
                "WDC Rankings After Six Races", "Driver with Fewest Laps Completed", "WDC Unbroken Lead Race",
                "Race Retirements", "Random Events", "Yes/No Scenarios"};
        private int[] questionNumber = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };

        public string GetQuestionName(int number)
        {
            return questionName[number];
        }

        public int GetQuestionNumber(int number)
        {
            return questionNumber[number];
        }

        public void SetQuestionName(int number, string questionName)
        {
            this.questionName[number] = questionName;
        }

        public void SetQuestionNumber(int number, int questionNumber)
        {
            this.questionNumber[number] = questionNumber;
        }
    }
}
