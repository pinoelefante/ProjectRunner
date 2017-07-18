using ProjectRunner.ServerAPI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectRunner.Views.Converters
{
    public class IntEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = System.Convert.ToInt32(value);
            var par = System.Convert.ToInt32(parameter);
            return val == par;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NotBoolean : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
    public class IntGreaterThan : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var param = System.Convert.ToInt32(parameter);
            var val = System.Convert.ToInt32(value);
            return val > param;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ItemTappedEventArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var eventArgs = value as ItemTappedEventArgs;
            if (eventArgs == null)
                throw new ArgumentException("Expected TappedEventArgs as value", "value");

            return eventArgs.Item;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "true_image.png" : "false_image.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TimeDifferenceString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime time = (DateTime)value;
            TimeSpan diff = DateTime.Now.Subtract(time);
            if(diff.TotalSeconds > 0)
            {
                return "";
            }
            return $"{Math.Abs(diff.Days)}d {Math.Abs(diff.Hours).ToString("D2")}h {Math.Abs(diff.Minutes).ToString("D2")}m";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class EditModeIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (bool)value;
            return v ? "unlocked.png" : "locked.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class UnixTimestampConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (System.Convert.ToInt64(value))/1000d;
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(v);
            return date.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ChatMessageUsername : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ChatMessage message = value as ChatMessage;
            if (message == null)
                return "???";
            if (message.SentBy == null)
                return $"USER_{message.UserId}";
            return message.SentBy.Username;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SportNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Sports sport = (Sports)value;
            switch (sport)
            {
                case Sports.BICYCLE:
                    return "Bicycle";
                case Sports.FOOTBALL:
                    return "Football";
                case Sports.RUNNING:
                    return "Running";
                case Sports.TENNIS:
                    return "Tennis";
            }
            return "Sport Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ActivityJoinedPlayerCalculator : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Activity activity = value as Activity;
            if (activity == null)
                return 0;
            var joined = activity.JoinedPlayers + activity.GuestUsers;
            if (!activity.OrganizerMode)
                joined++;
            return joined;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ActivityStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (ActivityStatus)value;
            switch(status)
            {
                case ActivityStatus.CANCELLED:
                    return "Cancelled";
                case ActivityStatus.DELETED:
                    return "Deleted";
                case ActivityStatus.ENDED:
                    return "Ended";
                case ActivityStatus.PENDING:
                    return "Pending";
                case ActivityStatus.STARTED:
                    return "Started";
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SportMaxPlayerEditable : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sport = (Sports)value;
            switch(sport)
            {
                case Sports.BICYCLE:
                case Sports.RUNNING:
                    return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SportWithTeam : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Activity act = value as Activity;
            switch (act?.Sport)
            {
                case Sports.FOOTBALL:
                    return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IsRoadActivityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is RoadActivity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IsTeamActivityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is TeamActivity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DateTimeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dateTime = (DateTime)value;
            string field = parameter == null ? "All" : parameter.ToString();
            switch(field)
            {
                case "All":
                    return dateTime.ToString();
                case "Date":
                    return dateTime.ToString("d");
                case "Time":
                    return dateTime.ToString("t");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class UserImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int sex = System.Convert.ToInt32(value);
            return sex == 0 ? "male.png" : "female.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
