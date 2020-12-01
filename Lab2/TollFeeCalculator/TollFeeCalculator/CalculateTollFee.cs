﻿using System;
using System.IO;

namespace TollFeeCalculator
{
    class Program
    {
        static void Main()
        {
            string inputFilePath = "../../../../testData.txt";
            Run(Environment.CurrentDirectory + inputFilePath);
        }

        static void Run(string path) 
        {
            string inputData = File.ReadAllText(path);
            string[] datesCSV = inputData.Split(", ");
            DateTime[] dates = new DateTime[datesCSV.Length-1]; //bugg
            for(int i = 0; i < dates.Length; i++) {
                dates[i] = DateTime.Parse(datesCSV[i]);
            }
            Console.Write("The total fee for the inputfile is: " + TotalFeeCost(dates));
        }

        static int TotalFeeCost(DateTime[] dates) 
        {
            int fee = 0;
            int multiPassageIntervalInMinutes = 60;
            DateTime initialInvervalDate = dates[0];
            foreach (var date in dates)
            {
                int differenceInMinutes = (date - initialInvervalDate).Minutes;
                if(differenceInMinutes > multiPassageIntervalInMinutes) {
                    fee += TollFeePass(date);
                    initialInvervalDate = date;
                    Console.WriteLine($"fee for {date} is {fee}" );
                } else {
                    fee += Math.Max(TollFeePass(date), TollFeePass(initialInvervalDate));
                    Console.WriteLine($"fee for (else) {date.Hour}:{date.Minute} is {fee}");
                }
            }
            return Math.Max(fee, 60); //bugg
        }

        static int TollFeePass(DateTime d)
        {
            if (free(d)) return 0;
            int hour = d.Hour;
            int minute = d.Minute;
            if (hour == 6 && minute >= 0 && minute <= 29) return 8;
            else if (hour == 6 && minute >= 30 && minute <= 59) return 13;
            else if (hour == 7 && minute >= 0 && minute <= 59) return 18;
            else if (hour == 8 && minute >= 0 && minute <= 29) return 13;
            else if (hour >= 8 && hour <= 14 && minute >= 30 && minute <= 59) return 8;
            else if (hour == 15 && minute >= 0 && minute <= 29) return 13;
            else if (hour == 15 && minute >= 0 || hour == 16 && minute <= 59) return 18;
            else if (hour == 17 && minute >= 0 && minute <= 59) return 13;
            else if (hour == 18 && minute >= 0 && minute <= 29) return 8;
            else return 0;
        }
        //Gets free dates
        static bool free(DateTime day) {
        return (int)day.DayOfWeek == 5 || (int)day.DayOfWeek == 6 || day.Month == 7;
        }
    }
}
