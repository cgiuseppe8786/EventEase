using System.Globalization;

namespace EventEase.Helpers;

/// <summary>
/// Italian date formatting that does not depend on the browser culture
/// (Blazor WebAssembly loads only the ICU data of the user's locale).
/// </summary>
public static class ItalianDate
{
    private static readonly string[] Months =
    [
        "gennaio", "febbraio", "marzo", "aprile", "maggio", "giugno",
        "luglio", "agosto", "settembre", "ottobre", "novembre", "dicembre"
    ];

    private static readonly string[] Days =
    [
        "domenica", "lunedì", "martedì", "mercoledì", "giovedì", "venerdì", "sabato"
    ];

    /// <summary>"nov"</summary>
    public static string ShortMonth(DateTime date) => Months[date.Month - 1][..3];

    /// <summary>"20 nov 2026"</summary>
    public static string Short(DateTime date) => $"{date.Day} {ShortMonth(date)} {date.Year}";

    /// <summary>"20 nov 2026, 09:30"</summary>
    public static string ShortWithTime(DateTime date) => $"{Short(date)}, {Time(date)}";

    /// <summary>"venerdì 20 novembre 2026, ore 09:30"</summary>
    public static string Long(DateTime date) =>
        $"{Days[(int)date.DayOfWeek]} {date.Day} {Months[date.Month - 1]} {date.Year}, ore {Time(date)}";

    /// <summary>"09:30"</summary>
    public static string Time(DateTime date) => date.ToString("HH:mm", CultureInfo.InvariantCulture);
}
