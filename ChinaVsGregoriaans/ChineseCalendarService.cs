using System.Globalization;

namespace ChinaVsGregoriaans;

// Chinese lunisolar date; Month is 1..12, IsLeap marks a leap month
public sealed record ChineseDate(int Year, int Month, bool IsLeap, int Day, int Stem, int Branch);

// Conversion between the Gregorian and the Chinese lunisolar calendar
public static class ChineseCalendarService
{
    private static readonly ChineseLunisolarCalendar Cal = new();

    // Offset to the traditional Huangdi era year
    private const int HuangdiOffset = 2697;

    private static readonly string[] StemsPinyin = ["Jiǎ", "Yǐ", "Bǐng", "Dīng", "Wù", "Jǐ", "Gēng", "Xīn", "Rén", "Guǐ"];
    private static readonly string[] StemsHan = ["甲", "乙", "丙", "丁", "戊", "己", "庚", "辛", "壬", "癸"];
    private static readonly string[] BranchesPinyin = ["Zǐ", "Chǒu", "Yín", "Mǎo", "Chén", "Sì", "Wǔ", "Wèi", "Shēn", "Yǒu", "Xū", "Hài"];
    private static readonly string[] BranchesHan = ["子", "丑", "寅", "卯", "辰", "巳", "午", "未", "申", "酉", "戌", "亥"];
    private static readonly string[] AnimalsNl = ["Rat", "Os", "Tijger", "Konijn", "Draak", "Slang", "Paard", "Geit", "Aap", "Haan", "Hond", "Varken"];
    private static readonly string[] AnimalsEmoji = ["🐀", "🐂", "🐅", "🐇", "🐉", "🐍", "🐎", "🐐", "🐒", "🐓", "🐕", "🐖"];
    private static readonly string[] ElementsNl = ["Hout", "Vuur", "Aarde", "Metaal", "Water"];
    private static readonly string[] MonthsHan = ["正", "二", "三", "四", "五", "六", "七", "八", "九", "十", "冬", "腊"];
    private static readonly string[] DigitsHan = ["", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十"];
    private static readonly string[] MonthsNl = ["januari", "februari", "maart", "april", "mei", "juni", "juli", "augustus", "september", "oktober", "november", "december"];
    private static readonly string[] DaysNl = ["zondag", "maandag", "dinsdag", "woensdag", "donderdag", "vrijdag", "zaterdag"];

    public static DateTime MinDate => Cal.MinSupportedDateTime.Date;
    public static DateTime MaxDate => Cal.MaxSupportedDateTime.Date;
    public static int MinYear => Cal.GetYear(MinDate);
    public static int MaxYear => Cal.GetYear(MaxDate);

    public static ChineseDate FromGregorian(DateTime date)
    {
        int year = Cal.GetYear(date);
        int monthIndex = Cal.GetMonth(date);
        int leap = Cal.GetLeapMonth(year);
        bool isLeap = leap > 0 && monthIndex == leap;
        int month = leap > 0 && monthIndex >= leap ? monthIndex - 1 : monthIndex;
        int sexagenary = Cal.GetSexagenaryYear(date);
        return new ChineseDate(year, month, isLeap, Cal.GetDayOfMonth(date),
            Cal.GetCelestialStem(sexagenary), Cal.GetTerrestrialBranch(sexagenary));
    }

    // Null when the Chinese date does not exist
    public static DateTime? ToGregorian(int year, int month, bool isLeap, int day)
    {
        int index = MonthIndex(year, month, isLeap);
        if (index == 0 || day < 1 || day > Cal.GetDaysInMonth(year, index)) return null;
        return Cal.ToDateTime(year, index, day, 0, 0, 0, 0);
    }

    // 0 when the month does not exist
    public static int DaysInMonth(int year, int month, bool isLeap)
    {
        int index = MonthIndex(year, month, isLeap);
        return index == 0 ? 0 : Cal.GetDaysInMonth(year, index);
    }

    // Month number that has a leap copy this year, 0 if none
    public static int LeapMonthOf(int year)
    {
        if (year < MinYear || year > MaxYear) return 0;
        int leap = Cal.GetLeapMonth(year);
        return leap > 0 ? leap - 1 : 0;
    }

    public static DateTime NewYear(int year) => Cal.ToDateTime(year, 1, 1, 0, 0, 0, 0);

    // Lunar birthday in the given Chinese year; leap month falls back to the regular month, day 30 to 29
    public static DateTime? LunarAnniversary(ChineseDate birth, int year)
    {
        if (year < MinYear || year > MaxYear) return null;
        bool leap = birth.IsLeap && LeapMonthOf(year) == birth.Month;
        int days = DaysInMonth(year, birth.Month, leap);
        return ToGregorian(year, birth.Month, leap, Math.Min(birth.Day, days));
    }

    // All lunar birthdays that fall within a Gregorian year (usually one)
    public static List<DateTime> LunarAnniversariesIn(ChineseDate birth, int gregorianYear) =>
        new[] { gregorianYear - 1, gregorianYear }
            .Select(y => LunarAnniversary(birth, y))
            .Where(d => d.HasValue && d.Value.Year == gregorianYear)
            .Select(d => d!.Value)
            .ToList();

    // Calendar month index (1..13), 0 when invalid
    private static int MonthIndex(int year, int month, bool isLeap)
    {
        if (year < MinYear || year > MaxYear || month < 1 || month > 12) return 0;
        int leap = Cal.GetLeapMonth(year);
        if (isLeap && leap != month + 1) return 0;
        return leap > 0 && (isLeap || month >= leap) ? month + 1 : month;
    }

    public static string StemBranchHan(ChineseDate d) => StemsHan[d.Stem - 1] + BranchesHan[d.Branch - 1];
    public static string StemBranchPinyin(ChineseDate d) => $"{StemsPinyin[d.Stem - 1]}-{BranchesPinyin[d.Branch - 1]}";
    public static string Animal(int branch) => AnimalsNl[branch - 1];
    public static string AnimalEmoji(int branch) => AnimalsEmoji[branch - 1];
    public static string Element(int stem) => ElementsNl[(stem - 1) / 2];
    public static string YinYang(int stem) => stem % 2 == 1 ? "Yang" : "Yin";
    public static int HuangdiYear(int year) => year + HuangdiOffset;

    public static string MonthHan(int month, bool isLeap) => (isLeap ? "闰" : "") + MonthsHan[month - 1] + "月";

    public static string DayHan(int day) => day switch
    {
        <= 10 => "初" + DigitsHan[day],
        < 20 => "十" + DigitsHan[day - 10],
        20 => "二十",
        < 30 => "廿" + DigitsHan[day - 20],
        _ => "三十"
    };

    // Dutch formatting without depending on ICU culture data
    public static string FormatGregorian(DateTime d, bool weekday = true) =>
        (weekday ? DaysNl[(int)d.DayOfWeek] + " " : "") + $"{d.Day} {MonthsNl[d.Month - 1]} {d.Year}";
}
