using System;
namespace ReportBuilderAjax.Web
{
    public partial class Schedule
    {
        #region Enums
        public enum Recurrence
        {
            Once = 1,
            DailyRepeatAfterInterval = 2,
            DailyDaysOfWeek = 3,
            Weekly = 4,
            MonthlyDaysOfWeek = 5,
            MonthlyCalendarDays = 6,
            QuarterlyDaysOfWeek = 7,
            QuarterlyCalendarDays = 8,
            YearlyDaysOfWeek = 9,
            YearlyCalendarDays = 10
        }

        [Flags]
        public enum DaysOfWeek
        {
            None = 0,
            Sunday = 1,
            Monday = 2,
            Tuesday = 4,
            Wednesday = 8,
            Thursday = 16,
            Friday = 32,
            Saturday = 64
        }

        public enum WeekOfMonth
        {
            None = 0,
            First = 1,
            Second = 2,
            Third = 3,
            Fourth = 4,
            Last = 5
        }

        [Flags]
        public enum MonthsOfYear
        {
            None = 0,
            January = 1,
            February = 2,
            March = 4,
            April = 8,
            May = 16,
            June = 32,
            July = 64,
            August = 128,
            September = 256,
            October = 512,
            November = 1024,
            December = 2048
        }

        public enum Priority
        {
            Normal = 1,
            Low = 2,
            High = 3
        }

        #endregion

        public string RecurrenceDescription
        {
            get { return getRecurrenceDescription(); }
        }

        private string getRecurrenceDescription()
        {
            string recurrence = string.Empty;

            switch ((Recurrence)this.RecurrenceTypeID)
            {
                case Recurrence.Once:
                    recurrence = "Once";
                    break;
                case Recurrence.DailyDaysOfWeek:
                    recurrence = "Every " + getDaysOfWeekDescription();
                    break;
                case Recurrence.DailyRepeatAfterInterval:
                    recurrence = "Every " + RepeatAfterInterval.ToString() + " days";
                    break;
                case Recurrence.Weekly:
                    recurrence = "Every " + getDaysOfWeekDescription() + " of every " + RepeatAfterInterval.ToString() + " weeks";
                    break;
                case Recurrence.MonthlyDaysOfWeek:
                    recurrence = string.Format("The {0} {1} of {2}", ((WeekOfMonth)WeekOfMonthTypeID).ToString(), getDaysOfWeekDescription(), getMonthsOfYearDescription());
                    break;
                case Recurrence.MonthlyCalendarDays:
                    recurrence = string.Format("On day(s) {0} of {1}", CalendarDays, getMonthsOfYearDescription());
                    break;
                case Recurrence.QuarterlyDaysOfWeek:
                    recurrence = string.Format("Quarterly on the {0} {1} of Jan, Apr, Jul, and Oct", ((WeekOfMonth)WeekOfMonthTypeID).ToString().ToLower(), getDayOfWeekDescription());
                    break;
                case Recurrence.QuarterlyCalendarDays:
                    recurrence = string.Format("Quarterly on Jan {0}, Apr {0}, Jul {0}, and Oct {0}", CalendarDays);
                    break;
                case Recurrence.YearlyDaysOfWeek:
                    recurrence = string.Format("Yearly on the {0} {1} of {2}", ((WeekOfMonth)WeekOfMonthTypeID).ToString().ToLower(), getDayOfWeekDescription(), getMonthOfYearDescription());
                    break;
                case Recurrence.YearlyCalendarDays:
                    recurrence = string.Format("Yearly on {0} {1}", getMonthOfYearDescription(), CalendarDays);
                    break;
            }

            return recurrence;
        }

        private string getDaysOfWeekDescription()
        {
            string daysDesc = string.Empty;
            DaysOfWeek days = (DaysOfWeek)DaysOfWeekBitmask;

            if (IsEveryWeekDay)
            {
                daysDesc = "week day";
                return daysDesc;
            }
            else
            {
                if (days.HasFlag(DaysOfWeek.Sunday)) daysDesc += " Sun,";
                if (days.HasFlag(DaysOfWeek.Monday)) daysDesc += " Mon,";
                if (days.HasFlag(DaysOfWeek.Tuesday)) daysDesc += " Tue,";
                if (days.HasFlag(DaysOfWeek.Wednesday)) daysDesc += " Wed,";
                if (days.HasFlag(DaysOfWeek.Thursday)) daysDesc += " Thu,";
                if (days.HasFlag(DaysOfWeek.Friday)) daysDesc += " Fri,";
                if (days.HasFlag(DaysOfWeek.Saturday)) daysDesc += " Sat,";
            }

            // remove leading space and trailing comma
            return daysDesc.Trim().Substring(0, daysDesc.Length - 2);
        }

        private string getDayOfWeekDescription()
        {
            DaysOfWeek days = (DaysOfWeek)DaysOfWeekBitmask;
            if (days.HasFlag(DaysOfWeek.Sunday)) return "Sun";
            if (days.HasFlag(DaysOfWeek.Monday)) return "Mon";
            if (days.HasFlag(DaysOfWeek.Tuesday)) return "Tue";
            if (days.HasFlag(DaysOfWeek.Wednesday)) return "Wed";
            if (days.HasFlag(DaysOfWeek.Thursday)) return "Thu";
            if (days.HasFlag(DaysOfWeek.Friday)) return "Fri";
            if (days.HasFlag(DaysOfWeek.Saturday)) return "Sat";

            return "";
        }

        private string getMonthsOfYearDescription()
        {
            string monthsDesc = string.Empty;
            MonthsOfYear months = (MonthsOfYear)MonthsOfYearBitmask;

            if (months.HasFlag(MonthsOfYear.January) &&
                months.HasFlag(MonthsOfYear.February) &&
                months.HasFlag(MonthsOfYear.March) &&
                months.HasFlag(MonthsOfYear.April) &&
                months.HasFlag(MonthsOfYear.May) &&
                months.HasFlag(MonthsOfYear.June) &&
                months.HasFlag(MonthsOfYear.July) &&
                months.HasFlag(MonthsOfYear.August) &&
                months.HasFlag(MonthsOfYear.September) &&
                months.HasFlag(MonthsOfYear.October) &&
                months.HasFlag(MonthsOfYear.November) &&
                months.HasFlag(MonthsOfYear.December))
            {
                monthsDesc = "every month";
                return monthsDesc;
            }
            else
            {
                if (months.HasFlag(MonthsOfYear.January)) monthsDesc += " Jan,";
                if (months.HasFlag(MonthsOfYear.February)) monthsDesc += " Feb,";
                if (months.HasFlag(MonthsOfYear.March)) monthsDesc += " Mar,";
                if (months.HasFlag(MonthsOfYear.April)) monthsDesc += " Apr,";
                if (months.HasFlag(MonthsOfYear.May)) monthsDesc += " May,";
                if (months.HasFlag(MonthsOfYear.June)) monthsDesc += " Jun,";
                if (months.HasFlag(MonthsOfYear.July)) monthsDesc += " Jul,";
                if (months.HasFlag(MonthsOfYear.August)) monthsDesc += " Aug,";
                if (months.HasFlag(MonthsOfYear.September)) monthsDesc += " Sep,";
                if (months.HasFlag(MonthsOfYear.October)) monthsDesc += " Oct,";
                if (months.HasFlag(MonthsOfYear.November)) monthsDesc += " Nov,";
                if (months.HasFlag(MonthsOfYear.December)) monthsDesc += " Dec,";
            }

            // remove leading space and trailing comma
            return monthsDesc.Trim().Substring(0, monthsDesc.Length - 2);
        }

        private string getMonthOfYearDescription()
        {
            MonthsOfYear months = (MonthsOfYear)MonthsOfYearBitmask;
            if (months.HasFlag(MonthsOfYear.January)) return "Jan";
            if (months.HasFlag(MonthsOfYear.February)) return "Feb";
            if (months.HasFlag(MonthsOfYear.March)) return "Mar";
            if (months.HasFlag(MonthsOfYear.April)) return "Apr";
            if (months.HasFlag(MonthsOfYear.May)) return "May";
            if (months.HasFlag(MonthsOfYear.June)) return "Jun";
            if (months.HasFlag(MonthsOfYear.July)) return "Jul";
            if (months.HasFlag(MonthsOfYear.August)) return "Aug";
            if (months.HasFlag(MonthsOfYear.September)) return "Sep";
            if (months.HasFlag(MonthsOfYear.October)) return "Oct";
            if (months.HasFlag(MonthsOfYear.November)) return "Nov";
            if (months.HasFlag(MonthsOfYear.December)) return "Dec";
        
            return "";
        }

        public bool IsEveryWeekDay
        {
            get 
            {
                if (DaysOfWeekBitmask.HasValue)
                {
                    DaysOfWeek days = (DaysOfWeek)DaysOfWeekBitmask;
                    if (days.HasFlag(DaysOfWeek.Monday) &&
                        days.HasFlag(DaysOfWeek.Tuesday) &&
                        days.HasFlag(DaysOfWeek.Wednesday) &&
                        days.HasFlag(DaysOfWeek.Thursday) &&
                        days.HasFlag(DaysOfWeek.Friday) &&
                        !days.HasFlag(DaysOfWeek.Saturday) &&
                        !days.HasFlag(DaysOfWeek.Sunday))
                    {
                        return true;
                    }
                }
                
                return false; 
            }
        }

        public Priority PriorityType
        {
            get { return (Priority)this.PriorityTypeID; }
        }

        public Recurrence RecurrenceType
        {
            get { return (Recurrence)this.RecurrenceTypeID; }
        }

        public WeekOfMonth WeekOfMonthType
        {
            get { return (WeekOfMonth)this.WeekOfMonthTypeID; }
        }
    }
}