using System;

public class TimeManager
{
    public static System.DateTime GetCurrentTime()
    {
        return System.DateTime.Now;
    }
    /// <summary>
    /// Tính số giây giữa 2 mốc thời gian
    /// </summary>
    public static double GetSecondsBetween(DateTime oldTime, DateTime newTime)
    {
        return (newTime - oldTime).TotalSeconds;
    }
    /// <summary>
    /// Kiểm tra đã đủ thời gian refill chưa
    /// </summary>
    public static bool HasEnoughTimePassedForRefill(DateTime lastTime, double refillSeconds)
    {
        DateTime currentTime = GetCurrentTime();
        return GetSecondsBetween(lastTime, currentTime) >= refillSeconds;
    }

    public static bool HasDayPassed(DateTime oldTime, DateTime currentTime)
    {
        DateTime replaceOldTime = new DateTime(oldTime.Year, oldTime.Month, oldTime.Day);
        DateTime replaceCurrentTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day);

        if (replaceOldTime < replaceCurrentTime)
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// Lấy ngày bắt đầu của tuần chứa 'time'. Mặc định tuần bắt đầu vào thứ Hai.
    /// </summary>
    public static DateTime GetStartOfWeek(DateTime time, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        // Sử dụng time.Date để loại bỏ phần giờ, phút, giây
        System.DateTime date = time.Date; 
        // Tính toán số ngày cần trừ đi để về đầu tuần
        int diff = (7 + (int)date.DayOfWeek - (int)startOfWeek) % 7;
        return date.AddDays(-diff);
    }

    public static DateTime GetStartOfDay(System.DateTime time)
    {
        return new  DateTime(time.Year, time.Month, time.Day);
    }

    public static long CalculateTime(DateTime oldTime, DateTime newTime)
    {
        TimeSpan diff = newTime - oldTime;
        long result = diff.Days * 24 * 60 * 60 +  diff.Hours * 60 * 60 + diff.Minutes * 60 + diff.Seconds;
        return result;
    }
}