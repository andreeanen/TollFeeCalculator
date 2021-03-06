﻿using System;
using System.IO;
using System.Linq;

namespace TollFeeCalculator
{
    public class Program
    {        
        public static void Main()
        {
            Program program = new Program();
            string inputFilePath = "../../../../testData.txt"; 
            program.Run(Environment.CurrentDirectory + inputFilePath);
        }
      
        public void Run(string path)
        {
            var program = new Program();
            string inputData = program.ReadFileData(new FileReader(), path);
            DateTime[] dates = Parse(inputData);
            DateTime[] datesFromSameDay = GetDatesFromSameDay(dates);
            int totalDailyFee = CalculateTotalFee(datesFromSameDay);
            Console.Write("The total fee for the inputfile is: " + TotalToPay(totalDailyFee));
        }

        // 1. Bug if path is invalid
        public string ReadFileData(IFileReader fileReader, string path)
        {
            try
            {
                return fileReader.Read(path);
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException();
            }
        }

        // 2. Bug if file contains different days
        public DateTime[] GetDatesFromSameDay(DateTime[] dates)
        {
            var initialDate = dates[0];
            var datesFromSameDay = new DateTime[dates.Length];
            datesFromSameDay = dates.Where(d => d.Date == initialDate.Date).OrderBy(d => d.Date).ToArray();
            return datesFromSameDay;
        }

        public DateTime[] Parse(string inputData)
        {
            string[] datesCSV = inputData.Split(','); //3. Bug empty space after comma
            DateTime[] dates = new DateTime[datesCSV.Length]; //4. Bug wrong array length
            for (int i = 0; i < dates.Length; i++)
            {
                dates[i] = ConvertStringToDateTime(datesCSV[i]);
            }
            return dates;
        }

        // 5. Bug when date string has invalid format
        public DateTime ConvertStringToDateTime(string date)
        {
            try
            {
                return DateTime.Parse(date);
            }
            catch (FormatException)
            {
                throw new FormatException();
            }
        }

        // 6. Bug min not max
        public int TotalToPay(int calculatedDailyFee) 
        {
            const int maxDailyFee = 60;
            return Math.Min(calculatedDailyFee, maxDailyFee);
        }

        public int CalculateTotalFee(DateTime[] dates) 
        {
            int totalDailyFee = 0;
            DateTime intervalStart = dates[0];
            int intervalTotalFee = 0;
            int intervalHighestFee = 0;
            foreach (var date in dates)
            {
                var currentPassageFee = CalculateFeePerTimespan(date);
                if (IsNewInterval(intervalStart, date))
                {
                    totalDailyFee += intervalTotalFee;
                    intervalStart = date;              
                    intervalHighestFee = currentPassageFee;
                    intervalTotalFee = currentPassageFee;
                }
                else //7. Bug wrong fee calculation, compare with starting interval only
                {
                    intervalTotalFee = RecalculateIntervalFee(intervalTotalFee, intervalHighestFee, currentPassageFee);  
                    intervalHighestFee = GetHigher(intervalHighestFee, currentPassageFee);                    
                }
            }
            totalDailyFee += intervalTotalFee;
            return totalDailyFee;
        }

        public bool IsNewInterval(DateTime intervalStartTime, DateTime currentPassageTime)
        {
            const int multiPassageIntervalMinutes = 60;
            var isIntervalOver = (currentPassageTime - intervalStartTime).TotalMinutes > multiPassageIntervalMinutes; //8. Bug TotalMinutes method not Minutes
            var isFirstPassage = intervalStartTime == currentPassageTime;

            return isIntervalOver || isFirstPassage;
        }

       
        private int RecalculateIntervalFee(int totalIntervalFee, int highestFee, int currentFee)
        {
            var recalculatedIntervalFee = totalIntervalFee;
            if (currentFee > highestFee)
            {
                recalculatedIntervalFee = totalIntervalFee - highestFee + currentFee;
            }
            return recalculatedIntervalFee;
        }

        private int GetHigher(int intervalHighestFee, int currentPassageFee)
        {
            return Math.Max(intervalHighestFee, currentPassageFee);
        }

        private int CalculateFeePerTimespan(DateTime date)
        {
            TimeSpan timeOfDay = date.TimeOfDay;
            int feePerTimespan;
            feePerTimespan = (CheckFreeDates(date)) ? 0 : GetFeePerTimespan(timeOfDay);
            return feePerTimespan;
        }

        public int GetFeePerTimespan(TimeSpan timeOfDay)
        {
            int feePerTimespan;
            if (CheckIfTimeOfDayIsInTimespan(timeOfDay, new TimeSpan(6, 0, 0), new TimeSpan(6, 29, 0)))
                feePerTimespan = 8;
            else if (CheckIfTimeOfDayIsInTimespan(timeOfDay, new TimeSpan(6, 30, 0), new TimeSpan(6, 59, 0)))
                feePerTimespan = 13;
            else if (CheckIfTimeOfDayIsInTimespan(timeOfDay, new TimeSpan(7, 0, 0), new TimeSpan(7, 59, 0)))
                feePerTimespan = 18;
            else if (CheckIfTimeOfDayIsInTimespan(timeOfDay, new TimeSpan(8, 0, 0), new TimeSpan(8, 29, 0)))
                feePerTimespan = 13;
            else if (CheckIfTimeOfDayIsInTimespan(timeOfDay, new TimeSpan(8, 30, 0), new TimeSpan(14, 59, 0))) //9. Bug wrong value for minutes 0-29 for hours 8-14
                feePerTimespan = 8;
            else if (CheckIfTimeOfDayIsInTimespan(timeOfDay, new TimeSpan(15, 0, 0), new TimeSpan(15, 29, 0)))
                feePerTimespan = 13;
            else if (CheckIfTimeOfDayIsInTimespan(timeOfDay, new TimeSpan(15, 30, 0), new TimeSpan(16, 59, 0)))
                feePerTimespan = 18;
            else if (CheckIfTimeOfDayIsInTimespan(timeOfDay, new TimeSpan(17, 0, 0), new TimeSpan(17, 59, 0)))
                feePerTimespan = 13;
            else if (CheckIfTimeOfDayIsInTimespan(timeOfDay, new TimeSpan(18, 0, 0), new TimeSpan(18, 29, 0)))
                feePerTimespan = 8;
            else
                feePerTimespan = 0;
            return feePerTimespan;
        }

        private bool CheckIfTimeOfDayIsInTimespan(TimeSpan timeOfDay, TimeSpan startTime, TimeSpan endTime)
        {
            bool isTimeOfDayInTimespan = false;
            if(timeOfDay >= startTime && timeOfDay <= endTime)
            {
                isTimeOfDayInTimespan= true;
            }
            return isTimeOfDayInTimespan;
        }

        // 10. Bug Friday was free 
        // 11. Bug Sunday was not free
        public bool CheckFreeDates(DateTime date) 
        {
            int july = 7;
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday || date.Month == july; 
        }
    }
}
