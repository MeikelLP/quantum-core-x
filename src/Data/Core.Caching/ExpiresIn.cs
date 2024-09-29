namespace QuantumCore.Caching;

public class ExpiresIn
{
    public static TimeSpan OneSecond => TimeSpan.FromSeconds(1);
    public static TimeSpan ThirtySeconds => TimeSpan.FromSeconds(30);
    
    public static TimeSpan OneMinute => TimeSpan.FromMinutes(1);
    public static TimeSpan FiveMinutes => TimeSpan.FromMinutes(5);
    public static TimeSpan TenMinutes => TimeSpan.FromMinutes(10);
    public static TimeSpan FifteenMinutes => TimeSpan.FromMinutes(15);
    public static TimeSpan ThirtyMinutes => TimeSpan.FromMinutes(30);
    
    public static TimeSpan OneHour => TimeSpan.FromHours(1);
    public static TimeSpan TwelveHours => TimeSpan.FromHours(12);
    public static TimeSpan OneDay => TimeSpan.FromDays(1);
    public static TimeSpan OneWeek => TimeSpan.FromDays(7);
    
    public static TimeSpan OneMonth => TimeSpan.FromDays(30);
    public static TimeSpan ThreeMonths => TimeSpan.FromDays(90);
    public static TimeSpan SixMonths => TimeSpan.FromDays(180);
    public static TimeSpan OneYear => TimeSpan.FromDays(365);
}