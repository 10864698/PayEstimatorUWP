using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayEstimatorUWP
{
    public class PublicHolidays
    {
        public bool PublicHoliday { get; set; }
        public DateTime EasterSunday { get; set; }
        public DateTime QueensBirthday { get; set; }
        public DateTime LabourDay { get; set; }

        public PublicHolidays()
        {
            PublicHoliday = false;
        }

        public PublicHolidays(DateTimeOffset startDate)
        {
            PublicHoliday = false;

            // Calculate Easter Sunday, Queens Birthday & Labour Day
            EasterSunday = CalculateEasterSunday(startDate.LocalDateTime.Year);
            QueensBirthday = CalculateQueensBirthday(startDate.LocalDateTime.Year);
            LabourDay = CalculateLabourDay(startDate.LocalDateTime.Year);


            // New Years Day (1st January)
            if (startDate.LocalDateTime.Day == 1 && startDate.LocalDateTime.Month == 1)
            {
                PublicHoliday = true;
            }

            // New Years Day Extra Day
            else if (startDate.LocalDateTime.Day == 2 && startDate.LocalDateTime.Month == 1 && startDate.LocalDateTime.DayOfWeek == DayOfWeek.Monday)
            {
                PublicHoliday = true;
            }

            else if (startDate.LocalDateTime.Day == 3 && startDate.LocalDateTime.Month == 1 && startDate.LocalDateTime.DayOfWeek == DayOfWeek.Monday)
            {
                PublicHoliday = true;
            }

            // Australia Day (26th January)
            else if (startDate.LocalDateTime.Day == 26 && startDate.LocalDateTime.Month == 1 && (startDate.LocalDateTime.DayOfWeek != DayOfWeek.Saturday || startDate.LocalDateTime.DayOfWeek != DayOfWeek.Sunday))
            {
                PublicHoliday = true;
            }

            // Australia Day Subsitute
            else if (startDate.LocalDateTime.Day == 27 && startDate.LocalDateTime.Month == 1 && startDate.LocalDateTime.DayOfWeek == DayOfWeek.Monday)
            {
                PublicHoliday = true;
            }

            else if (startDate.LocalDateTime.Day == 28 && startDate.LocalDateTime.Month == 1 && startDate.LocalDateTime.DayOfWeek == DayOfWeek.Monday)
            {
                PublicHoliday = true;
            }

            // Easter Friday
            else if (startDate.LocalDateTime.Date == EasterSunday.AddDays(-2).Date)
            {
                PublicHoliday = true;
            }

            // Easter Sunday
            else if (startDate.LocalDateTime.Date == EasterSunday.Date)
            {
                PublicHoliday = true;
            }

            // Easter Monday
            if (startDate.LocalDateTime.Date == EasterSunday.AddDays(1).Date)
            {
                PublicHoliday = true;
            }

            // ANZAC Day (25th April)
            else if (startDate.LocalDateTime.Day == 25 && startDate.LocalDateTime.Month == 4)
            {
                PublicHoliday = true;
            }

            // Queens Birthday
            else if (startDate.LocalDateTime.Date == QueensBirthday.Date)
            {
                PublicHoliday = true;
            }

            // Labour Day 
            else if (startDate.LocalDateTime.Date == LabourDay.Date)
            {
                PublicHoliday = true;
            }

            // Christmas Day (25th Decmember)
            else if (startDate.LocalDateTime.Day == 25 && startDate.LocalDateTime.Month == 12)
            {
                PublicHoliday = true;
            }

            // Boxing Day (26th December)
            else if (startDate.LocalDateTime.Day == 26 && startDate.LocalDateTime.Month == 12)
            {
                PublicHoliday = true;
            }

            // Christmas Day Boxing Day Extra Day(s)
            else if (startDate.LocalDateTime.Day == 27 && startDate.LocalDateTime.Month == 12 && startDate.LocalDateTime.DayOfWeek == DayOfWeek.Monday)
            {
                PublicHoliday = true;
            }

            else if (startDate.LocalDateTime.Day == 28 && startDate.LocalDateTime.Month == 12 && (startDate.LocalDateTime.DayOfWeek == DayOfWeek.Monday || startDate.LocalDateTime.DayOfWeek == DayOfWeek.Tuesday))
            {
                PublicHoliday = true;
            }

            else
            {
                PublicHoliday = false;
            }
        }

        // https://stackoverflow.com/questions/2510383/how-can-i-calculate-what-date-good-friday-falls-on-given-a-year
        // https://www.codeproject.com/Articles/10860/Calculating-Christian-Holidays

        public DateTime CalculateEasterSunday(int year)
        {
            int day = 0;
            int month = 0;

            int g = year % 19;
            int c = year / 100;
            int h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25) + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) * (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            day = i - ((year + (int)(year / 4) + i + 2 - c + (int)(c / 4)) % 7) + 28;
            month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }

            return new DateTime(year, month, day);
        }

        public DateTime CalculateQueensBirthday(int year)
        {
            DateTime secondMondayInJune = new DateTime(year, 6, 1);

            while (secondMondayInJune.DayOfWeek != DayOfWeek.Monday)
            {
                secondMondayInJune = secondMondayInJune.AddDays(1);
            }

            secondMondayInJune = secondMondayInJune.AddDays(7);

            return secondMondayInJune;
        }

        public DateTime CalculateLabourDay(int year)
        {
            DateTime firstMondayInOctober = new DateTime(year, 10, 1);

            while (firstMondayInOctober.Date.DayOfWeek != DayOfWeek.Monday)
                firstMondayInOctober = firstMondayInOctober.AddDays(1);

            return firstMondayInOctober;
        }
    }
}