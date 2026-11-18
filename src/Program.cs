using System;
using System.IO;

namespace MyScheduler
{
    class Program
    {
        // Max limits
        const int MAX_RECORDS = 20;
        const int MAX_BLOCKS = 50;
        const int MAX_PLACEMENTS = 300;

        // --- FIXED SCHEDULES DATA ---
        static string[] schedName = new string[MAX_RECORDS];
        static int[] schedDay = new int[MAX_RECORDS];
        static int[] schedStart = new int[MAX_RECORDS];
        static int[] schedEnd = new int[MAX_RECORDS];
        static int schedCount = 0;

        // --- ACTIVITIES DATA ---
        static string[] actName = new string[MAX_RECORDS];
        static bool[] actHasDeadline = new bool[MAX_RECORDS];
        static DateTime[] actDeadline = new DateTime[MAX_RECORDS];
        static bool[,] actDays = new bool[MAX_RECORDS, 8]; // 1 to 7
        static double[] actHours = new double[MAX_RECORDS];
        static int[] actPriority = new int[MAX_RECORDS]; // 1=High, 2=Med, 3=Low
        static bool[] actDone = new bool[MAX_RECORDS];
        static int actCount = 0;

        // --- PLACEMENT DATA ---
        static int[] placeAct = new int[MAX_PLACEMENTS];
        static int[] placeDay = new int[MAX_PLACEMENTS];
        static int[] placeStart = new int[MAX_PLACEMENTS];
        static int[] placeEnd = new int[MAX_PLACEMENTS];
        static int placeCount = 0;
        static double[,] placedHours = new double[MAX_RECORDS, 7];

        // --- FREE BLOCKS DATA ---
        static int[] freeStart = new int[MAX_BLOCKS];
        static int[] freeEnd = new int[MAX_BLOCKS];
        static int freeCount = 0;

        // --- TIME DATA ---
        static DateTime baseDate = DateTime.Today;
        static DateTime currentWeekStart = DateTime.Today;

        // --- FILE PATHS ---
        static string schedFile = "schedules.txt";
        static string actFile = "activities.txt";

        static void Main()
        {
            // Keep the console open: Ctrl+C will NOT close the program.
            // The program only exits when you choose option [0].
            Console.CancelKeyPress += (sender, e) => { e.Cancel = true; };

            try
            {
                LoadData();
            }
            catch (Exception)
            {
                // A damaged data file should not close the program.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  !! Some saved data could not be read. Starting with what was loaded.");
                Console.ResetColor();
                Pause();
            }

            Console.Title = "MyScheduler";

            bool running = true;
            while (running)
            {
                try
                {
                    Console.Clear();

                    // UI Header
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("============================================================");
                    Console.WriteLine("            MyScheduler - Activity Planner");
                    Console.WriteLine("============================================================");
                    Console.ResetColor();
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  [1]  Add Fixed Schedule");
                    Console.WriteLine("  [2]  View Fixed Schedules");
                    Console.WriteLine("  [3]  Add Activity");
                    Console.WriteLine("  [4]  View Activities");
                    Console.WriteLine("  [5]  View Calendar");
                    Console.WriteLine("  [0]  Exit");
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.WriteLine("------------------------------------------------------------");

                    int doneCount = 0;
                    for (int i = 0; i < actCount; i++)
                    {
                        if (actDone[i]) doneCount++;
                    }

                    Console.WriteLine("  Schedules: " + schedCount + "/" + MAX_RECORDS + "   Activities: " + actCount + " (" + (actCount - doneCount) + " pending, " + doneCount + " done)");
                    Console.WriteLine("------------------------------------------------------------");
                    Console.Write("  Select option [0-5]: ");

                    string choice = Console.ReadLine();
                    if (choice == null) choice = "";
                    choice = choice.Trim();

                    if (choice == "1") AddFixedSchedule();
                    else if (choice == "2") ViewFixedSchedules();
                    else if (choice == "3") AddActivity();
                    else if (choice == "4") ViewActivities();
                    else if (choice == "5") ViewCalendar();
                    else if (choice == "0")
                    {
                        SaveData();
                        running = false;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("  !! Invalid option. Please choose 0-5.");
                        Console.ResetColor();
                        Pause();
                    }
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  !! An unexpected error occurred. Returning to main menu.");
                    Console.ResetColor();
                    Pause();
                }
            }
        }

        static void Pause()
        {
            Console.WriteLine("  Press ENTER to continue . . .");
            Console.ReadLine();
        }

        // =========================================================
        //  SAVE & LOAD
        // =========================================================
        static void SaveData()
        {
            using (StreamWriter sw = new StreamWriter(schedFile))
            {
                for (int i = 0; i < schedCount; i++)
                {
                    sw.WriteLine(schedName[i] + "|" + schedDay[i] + "|" + schedStart[i] + "|" + schedEnd[i]);
                }
            }

            using (StreamWriter sw = new StreamWriter(actFile))
            {
                for (int i = 0; i < actCount; i++)
                {
                    string daysStr = "";
                    for (int d = 1; d <= 7; d++) { daysStr += actDays[i, d] ? "1" : "0"; }
                    sw.WriteLine(actName[i] + "|" + actHasDeadline[i] + "|" + actDeadline[i].Ticks + "|" + daysStr + "|" + actHours[i] + "|" + actPriority[i] + "|" + actDone[i]);
                }
            }
        }

        static void LoadData()
        {
            if (File.Exists(schedFile))
            {
                string[] lines = File.ReadAllLines(schedFile);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == 4 && schedCount < MAX_RECORDS)
                    {
                        schedName[schedCount] = parts[0];
                        schedDay[schedCount] = int.Parse(parts[1]);
                        schedStart[schedCount] = int.Parse(parts[2]);
                        schedEnd[schedCount] = int.Parse(parts[3]);
                        schedCount++;
                    }
                }
            }

            if (File.Exists(actFile))
            {
                string[] lines = File.ReadAllLines(actFile);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == 7 && actCount < MAX_RECORDS)
                    {
                        actName[actCount] = parts[0];
                        actHasDeadline[actCount] = bool.Parse(parts[1]);
                        actDeadline[actCount] = new DateTime(long.Parse(parts[2]));
                        string daysStr = parts[3];
                        for (int d = 1; d <= 7; d++) { actDays[actCount, d] = daysStr[d - 1] == '1'; }
                        actHours[actCount] = double.Parse(parts[4]);
                        actPriority[actCount] = int.Parse(parts[5]);
                        actDone[actCount] = bool.Parse(parts[6]);
                        actCount++;
                    }
                }
            }
        }

        // =========================================================
        //  HELPER METHODS (Time and Days)
        // =========================================================
        static string GetDayName(int day)
        {
            if (day == 1) return "MON";
            if (day == 2) return "TUE";
            if (day == 3) return "WED";
            if (day == 4) return "THU";
            if (day == 5) return "FRI";
            if (day == 6) return "SAT";
            if (day == 7) return "SUN";
            return "???";
        }

        static int GetDayNumber(DateTime date)
        {
            int d = (int)date.DayOfWeek;
            if (d == 0) d = 7;
            return d;
        }

        static string TimeToString(int minutes)
        {
            int h = minutes / 60;
            int m = minutes % 60;
            return h.ToString("00") + ":" + m.ToString("00");
        }

        // Time Parser
        static bool TryParseTime(string input, out int totalMinutes)
        {
            totalMinutes = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;

            input = input.Trim().ToUpper().Replace(" ", "");

            bool isPM = input.EndsWith("PM");
            bool isAM = input.EndsWith("AM");

            if (isPM || isAM) { input = input.Substring(0, input.Length - 2); }

            int h, m = 0;
            if (input.Contains(":"))
            {
                string[] parts = input.Split(':');
                if (parts.Length != 2) return false;
                if (!int.TryParse(parts[0], out h)) return false;
                if (!int.TryParse(parts[1], out m)) return false;
            }
            else
            {
                if (!int.TryParse(input, out h)) return false;
            }

            if (h < 0 || m < 0 || m > 59) return false;

            if (isPM && h < 12) h += 12;
            if (isAM && h == 12) h = 0;
            if (h < 0 || h > 23) return false;

            totalMinutes = (h * 60) + m;
            return true;
        }

        // Flexible Duration Parser (Accepts 1.5, 1:30, 1h 30m)
        static bool TryParseDuration(string input, out double totalHours)
        {
            totalHours = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;
            input = input.Trim().ToLower();

            // 1. Try decimal format (e.g., "1.5")
            if (double.TryParse(input, out double d)) { totalHours = d; return true; }

            // 2. Try H:MM format (e.g., "1:30")
            if (input.Contains(":"))
            {
                string[] parts = input.Split(':');
                if (parts.Length == 2 && double.TryParse(parts[0], out double h) && double.TryParse(parts[1], out double m))
                {
                    totalHours = h + (m / 60.0);
                    return true;
                }
            }

            // 3. Try word format (e.g., "1h 30m", "1 hour 30 mins")
            input = input.Replace("hours", "").Replace("hour", "").Replace("hrs", "").Replace("hr", "")
                         .Replace("minutes", "").Replace("minute", "").Replace("mins", "").Replace("min", "");

            double h_val = 0, m_val = 0;
            bool hasH = false, hasM = false;

            if (input.Contains("h"))
            {
                string[] parts = input.Split('h');
                if (double.TryParse(parts[0].Trim(), out h_val)) hasH = true;
                input = parts.Length > 1 ? parts[1] : "";
            }

            if (input.Contains("m"))
            {
                string[] parts = input.Split('m');
                if (double.TryParse(parts[0].Trim(), out m_val)) hasM = true;
            }

            if (hasH || hasM)
            {
                totalHours = h_val + (m_val / 60.0);
                return true;
            }

            return false;
        }

        static string FormatDuration(double totalHours)
        {
            int h = (int)totalHours;
            int m = (int)Math.Round((totalHours - h) * 60);

            if (h > 0 && m > 0) return h + "h " + m + "m";
            if (h > 0) return h + "h";
            return m + "m";
        }

        static string GetCode(string name)
        {
            string upper = name.ToUpper();
            if (upper.Length > 5) return upper.Substring(0, 5);
            return upper;
        }

        static string GetPriorityName(int p)
        {
            if (p == 1) return "HIGH";
            if (p == 2) return "MED";
            return "LOW";
        }

        // =========================================================
        //  FEATURE 1: ADD FIXED SCHEDULE
        // =========================================================
        static void AddFixedSchedule()
        {
            if (schedCount >= MAX_RECORDS) { Console.Clear(); Console.WriteLine("  !! Schedule list is full."); Pause(); return; }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("============================================================");
            Console.WriteLine("  ADD FIXED SCHEDULE  (1 of 3)");
            Console.WriteLine("============================================================");
            Console.ResetColor();
            Console.WriteLine();

            string name = "";
            while (true)
            {
                Console.Write("  Schedule name (or 'B' to back): ");
                name = Console.ReadLine();
                if (name != null && name.Trim().ToUpper() == "B") return;
                if (name != null && name.Trim() != "") break;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  !! Name cannot be empty.");
                Console.ResetColor();
            }
            name = name.Trim();

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("============================================================");
            Console.WriteLine("  ADD FIXED SCHEDULE  (2 of 3)");
            Console.WriteLine("============================================================");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  Name: " + name);
            Console.WriteLine();
            Console.WriteLine("  [1] MON [2] TUE [3] WED [4] THU [5] FRI [6] SAT [7] SUN");
            Console.WriteLine();

            int[] selectedDays = new int[7];
            int dayCount = 0;

            while (true)
            {
                Console.Write("  Enter days (e.g. 1,3,5) (or 'B' to back): ");
                string input = Console.ReadLine();
                if (input != null && input.Trim().ToUpper() == "B") return;

                if (input == null || input.Trim() == "") { Console.WriteLine("  !! Please select at least one day."); continue; }

                selectedDays = new int[7];
                dayCount = 0;
                bool error = false;

                string[] parts = input.Split(',');
                for (int i = 0; i < parts.Length; i++)
                {
                    int d;
                    if (!int.TryParse(parts[i].Trim(), out d) || d < 1 || d > 7)
                    {
                        Console.WriteLine("  !! Invalid day: " + parts[i]);
                        error = true;
                        break;
                    }

                    bool duplicate = false;
                    for (int j = 0; j < dayCount; j++) { if (selectedDays[j] == d) duplicate = true; }
                    if (duplicate) { Console.WriteLine("  !! Day " + d + " entered twice."); error = true; break; }

                    selectedDays[dayCount] = d;
                    dayCount++;
                }

                if (!error) break;
            }

            for (int i = 0; i < dayCount - 1; i++)
            {
                for (int j = 0; j < dayCount - i - 1; j++)
                {
                    if (selectedDays[j] > selectedDays[j + 1])
                    {
                        int temp = selectedDays[j];
                        selectedDays[j] = selectedDays[j + 1];
                        selectedDays[j + 1] = temp;
                    }
                }
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("============================================================");
            Console.WriteLine("  ADD FIXED SCHEDULE  (3 of 3)");
            Console.WriteLine("============================================================");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  Name: " + name);
            Console.Write("  Days: ");
            for (int i = 0; i < dayCount; i++) { Console.Write(GetDayName(selectedDays[i]) + " "); }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("  [A] Same time for all days");
            Console.WriteLine("  [E] Different time for each day");
            Console.Write("  Choice [A/E] (or 'B' to back): ");

            string mode = Console.ReadLine();
            if (mode != null && mode.Trim().ToUpper() == "B") return;
            if (mode == null) mode = "A";
            mode = mode.Trim().ToUpper();
            if (mode != "A" && mode != "E") mode = "A";

            int[] startTimes = new int[7];
            int[] endTimes = new int[7];

            if (mode == "A")
            {
                int st, en;
                while (true)
                {
                    Console.Write("  Start time (e.g., 8 AM, 1:00 PM, 13:00) (or 'B' to back): ");
                    string startIn = Console.ReadLine();
                    if (startIn != null && startIn.Trim().ToUpper() == "B") return;
                    if (TryParseTime(startIn, out st)) break;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  !! Invalid format. Try 8 AM or 13:00.");
                    Console.ResetColor();
                }

                while (true)
                {
                    Console.Write("  End time (e.g., 10 AM, 3:00 PM, 15:00) (or 'B' to back): ");
                    string endIn = Console.ReadLine();
                    if (endIn != null && endIn.Trim().ToUpper() == "B") return;
                    if (TryParseTime(endIn, out en)) break;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  !! Invalid format.");
                    Console.ResetColor();
                }

                if (st == en) { Console.WriteLine("  !! Start and end must differ."); Pause(); return; }

                if (en < st)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine();
                    Console.WriteLine("  !! Note: End time is earlier than start time.");
                    Console.WriteLine("     This is an OVERNIGHT schedule.");
                    Console.ResetColor();
                    Console.Write("  [C] Confirm   [B] Back: ");
                    string conf = Console.ReadLine();
                    if (conf == null || conf.Trim().ToUpper() != "C") return;
                }

                for (int i = 0; i < dayCount; i++) { startTimes[i] = st; endTimes[i] = en; }
            }
            else
            {
                for (int i = 0; i < dayCount; i++)
                {
                    Console.WriteLine();
                    Console.WriteLine("  " + GetDayName(selectedDays[i]));

                    int st, en;
                    while (true)
                    {
                        Console.Write("    Start time (e.g., 8 AM, 1:00 PM) (or 'B' to back): ");
                        string startIn = Console.ReadLine();
                        if (startIn != null && startIn.Trim().ToUpper() == "B") return;
                        if (TryParseTime(startIn, out st)) break;
                        Console.WriteLine("    !! Invalid format.");
                    }

                    while (true)
                    {
                        Console.Write("    End time (e.g., 10 AM, 3:00 PM) (or 'B' to back): ");
                        string endIn = Console.ReadLine();
                        if (endIn != null && endIn.Trim().ToUpper() == "B") return;
                        if (TryParseTime(endIn, out en)) break;
                        Console.WriteLine("    !! Invalid format.");
                    }

                    if (st == en) { Console.WriteLine("  !! Start and end must differ."); Pause(); return; }
                    startTimes[i] = st;
                    endTimes[i] = en;
                }
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("============================================================");
            Console.WriteLine("  PREVIEW");
            Console.WriteLine("============================================================");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("  " + name);
            for (int i = 0; i < dayCount; i++)
            {
                Console.WriteLine("    " + GetDayName(selectedDays[i]) + "  " + TimeToString(startTimes[i]) + " - " + TimeToString(endTimes[i]));
            }
            Console.WriteLine();
            Console.Write("  [S] Save   [B] Back: ");
            string saveChoice = Console.ReadLine();
            if (saveChoice == null || saveChoice.Trim().ToUpper() != "S") return;

            // Check overlaps
            for (int i = 0; i < dayCount; i++)
            {
                for (int j = 0; j < schedCount; j++)
                {
                    if (schedDay[j] == selectedDays[i])
                    {
                        int s1 = startTimes[i], e1 = endTimes[i];
                        int s2 = schedStart[j], e2 = schedEnd[j];
                        bool overlap = false;
                        if (s1 < e1 && s2 < e2) { if (s1 < e2 && s2 < e1) overlap = true; }
                        else if (s1 > e1 && s2 < e2) { if (s1 < e2 || s2 < e1) overlap = true; }
                        else if (s1 < e1 && s2 > e2) { if (s1 < e2 || s2 < e1) overlap = true; }
                        else if (s1 > e1 && s2 > e2) { overlap = true; }

                        if (overlap)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("  !! CONFLICT on " + GetDayName(selectedDays[i]) + " with " + schedName[j]);
                            Console.ResetColor();
                            Pause();
                            return;
                        }
                    }
                }
            }

            for (int i = 0; i < dayCount; i++)
            {
                schedName[schedCount] = name;
                schedDay[schedCount] = selectedDays[i];
                schedStart[schedCount] = startTimes[i];
                schedEnd[schedCount] = endTimes[i];
                schedCount++;
            }

            SaveData();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("  >> Successfully added " + name);
            Console.ResetColor();
            Pause();
        }

        // =========================================================
        //  FEATURE 2: VIEW FIXED SCHEDULES
        // =========================================================
        static void ViewFixedSchedules()
        {
            bool viewing = true;
            while (viewing)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("============================================================");
                Console.WriteLine("  MY FIXED SCHEDULES                          (" + schedCount + " of " + MAX_RECORDS + ")");
                Console.WriteLine("============================================================");
                Console.ResetColor();
                Console.WriteLine();

                if (schedCount == 0) { Console.WriteLine("  No schedules added yet."); }
                else
                {
                    Console.WriteLine("  #   NAME              DAY    TIME");
                    Console.WriteLine("  --  ----------------  -----  -------------");
                    for (int i = 0; i < schedCount; i++)
                    {
                        string overnight = "";
                        if (schedEnd[i] < schedStart[i]) overnight = " (Overnight)";
                        Console.WriteLine("  " + (i + 1) + ".  " + schedName[i].PadRight(16) + "  " + GetDayName(schedDay[i]) + "    " + TimeToString(schedStart[i]) + " - " + TimeToString(schedEnd[i]) + overnight);
                    }
                }

                Console.WriteLine();
                Console.WriteLine("  [D] Delete a schedule");
                Console.WriteLine("  [B] Back to menu");
                Console.Write("  Choice: ");
                string choice = Console.ReadLine();
                if (choice == null) choice = "";
                choice = choice.Trim().ToUpper();

                if (choice == "B" || choice == "") viewing = false;
                else if (choice == "D")
                {
                    if (schedCount == 0) { Pause(); continue; }
                    Console.Write("  Delete which # (or 'B' to back): ");
                    string numStr = Console.ReadLine();
                    if (numStr != null && numStr.Trim().ToUpper() == "B") continue;

                    int num;
                    if (int.TryParse(numStr, out num) && num >= 1 && num <= schedCount)
                    {
                        for (int i = num - 1; i < schedCount - 1; i++)
                        {
                            schedName[i] = schedName[i + 1];
                            schedDay[i] = schedDay[i + 1];
                            schedStart[i] = schedStart[i + 1];
                            schedEnd[i] = schedEnd[i + 1];
                        }
                        schedCount--;
                        SaveData();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("  >> Deleted successfully.");
                        Console.ResetColor();
                    }
                    else { Console.WriteLine("  !! Invalid number."); }
                    Pause();
                }
            }
        }

        // =========================================================
        //  FEATURE 3: ADD ACTIVITY
        // =========================================================
        static void AddActivity()
        {
            if (actCount >= MAX_RECORDS) { Console.Clear(); Console.WriteLine("  !! Activity list is full."); Pause(); return; }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("============================================================");
            Console.WriteLine("  ADD ACTIVITY");
            Console.WriteLine("============================================================");
            Console.ResetColor();
            Console.WriteLine();

            string name = "";
            while (true)
            {
                Console.Write("  Activity name (or 'B' to back): ");
                name = Console.ReadLine();
                if (name != null && name.Trim().ToUpper() == "B") return;
                if (name != null && name.Trim() != "") break;
                Console.WriteLine("  !! Name cannot be empty.");
            }
            name = name.Trim();

            bool hasDeadline = false;
            DateTime deadline = DateTime.Now;

            Console.WriteLine();
            Console.WriteLine("  Deadline (Press ENTER to skip)");
            Console.Write("    Date YYYY-MM-DD (or 'B' to back): ");
            string dateStr = Console.ReadLine();
            if (dateStr != null && dateStr.Trim().ToUpper() == "B") return;

            if (dateStr != null && dateStr.Trim() != "")
            {
                DateTime d;
                if (!DateTime.TryParse(dateStr, out d)) { Console.WriteLine("  !! Invalid date format. Canceled."); Pause(); return; }

                int timeMins;
                while (true)
                {
                    Console.Write("    Time (e.g., 11:59 PM, 23:59) (or 'B' to back): ");
                    string timeStr = Console.ReadLine();
                    if (timeStr != null && timeStr.Trim().ToUpper() == "B") return;
                    if (TryParseTime(timeStr, out timeMins)) break;
                    Console.WriteLine("  !! Invalid time.");
                }

                deadline = d.Date.AddMinutes(timeMins);
                hasDeadline = true;
            }

            Console.WriteLine();
            Console.WriteLine("  Which days?");
            Console.WriteLine("  [A] Every day");
            Console.WriteLine("  [S] Specific days");
            Console.Write("  Choice [A/S] (or 'B' to back): ");
            string dayChoice = Console.ReadLine();
            if (dayChoice == null) dayChoice = "";
            dayChoice = dayChoice.Trim().ToUpper();
            if (dayChoice == "B") return;

            bool[] days = new bool[8];
            bool hasDays = false;

            if (dayChoice == "A")
            {
                for (int i = 1; i <= 7; i++) days[i] = true;
                hasDays = true;
            }
            else if (dayChoice == "S")
            {
                Console.Write("  Enter days (e.g. 1,3,5) (or 'B' to back): ");
                string dayStr = Console.ReadLine();
                if (dayStr != null && dayStr.Trim().ToUpper() == "B") return;
                if (dayStr == null || dayStr.Trim() == "") { Console.WriteLine("  !! No days selected. Canceled."); Pause(); return; }

                string[] parts = dayStr.Split(',');
                for (int i = 0; i < parts.Length; i++)
                {
                    int d;
                    if (int.TryParse(parts[i].Trim(), out d) && d >= 1 && d <= 7) { days[d] = true; hasDays = true; }
                }
            }

            if (!hasDays) { Console.WriteLine("  !! No days selected. Canceled."); Pause(); return; }

            double hrs = 0;
            while (true)
            {
                Console.Write("  Duration per day (e.g., 1.5, 1:30, 1h 30m) (or 'B' to back): ");
                string hrStr = Console.ReadLine();
                if (hrStr != null && hrStr.Trim().ToUpper() == "B") return;

                if (TryParseDuration(hrStr, out hrs))
                {
                    if (hrs >= 0.5 && hrs <= 8) break;
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  !! Must be between 30 mins and 8 hours.");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine("  Priority:");
            Console.WriteLine("    [1] High  [2] Medium  [3] Low");
            Console.Write("  Choice [1/2/3] (or 'B' to back): ");
            string pStr = Console.ReadLine();
            if (pStr != null && pStr.Trim().ToUpper() == "B") return;

            int prio = 2;
            if (pStr != null)
            {
                pStr = pStr.Trim();
                if (pStr == "1") prio = 1;
                else if (pStr == "3") prio = 3;
            }

            Console.WriteLine();
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("  Summary: " + name);
            if (hasDeadline) Console.WriteLine("  Due: " + deadline.ToString("MMM dd, yyyy HH:mm"));
            else Console.WriteLine("  Due: No deadline (Flexible)");
            Console.WriteLine("  Duration/day: " + FormatDuration(hrs));
            Console.WriteLine("  Priority: " + GetPriorityName(prio));
            Console.WriteLine();
            Console.Write("  [S] Save   [B] Back: ");
            string saveChoice = Console.ReadLine();
            if (saveChoice == null || saveChoice.Trim().ToUpper() != "S") return;

            actName[actCount] = name;
            actHasDeadline[actCount] = hasDeadline;
            actDeadline[actCount] = deadline;
            for (int d = 1; d <= 7; d++) actDays[actCount, d] = days[d];
            actHours[actCount] = hrs;
            actPriority[actCount] = prio;
            actDone[actCount] = false;
            actCount++;

            SaveData();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("  >> Successfully added " + name);
            Console.ResetColor();
            Pause();
        }

        // =========================================================
        //  FEATURE 4: VIEW ACTIVITIES
        // =========================================================
        static void ViewActivities()
        {
            bool viewing = true;
            while (viewing)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("============================================================");
                Console.WriteLine("  MY ACTIVITIES                               (" + actCount + " of " + MAX_RECORDS + ")");
                Console.WriteLine("============================================================");
                Console.ResetColor();
                Console.WriteLine();

                if (actCount == 0) { Console.WriteLine("  No activities added yet."); }
                else
                {
                    Console.WriteLine("  #   DONE   PRIO   ACTIVITY             DURATION");
                    Console.WriteLine("  --  -----  -----  -------------------  ---------");
                    for (int i = 0; i < actCount; i++)
                    {
                        string mark = actDone[i] ? "[x]" : "[ ]";
                        Console.WriteLine("  " + (i + 1) + ".  " + mark + "    " + GetPriorityName(actPriority[i]) + "   " + actName[i].PadRight(20) + " " + FormatDuration(actHours[i]));
                    }
                }

                Console.WriteLine();
                Console.WriteLine("  [T] Toggle Done   [D] Delete   [B] Back");
                Console.Write("  Choice: ");
                string choice = Console.ReadLine();
                if (choice == null) choice = "";
                choice = choice.Trim().ToUpper();

                if (choice == "B" || choice == "") viewing = false;
                else if (choice == "T")
                {
                    if (actCount == 0) { Pause(); continue; }
                    Console.Write("  Toggle which # (or 'B' to back): ");
                    string numStr = Console.ReadLine();
                    if (numStr != null && numStr.Trim().ToUpper() == "B") continue;

                    int num;
                    if (int.TryParse(numStr, out num) && num >= 1 && num <= actCount)
                    {
                        actDone[num - 1] = !actDone[num - 1];
                        SaveData();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("  >> Updated status.");
                        Console.ResetColor();
                    }
                    else Console.WriteLine("  !! Invalid number.");
                    Pause();
                }
                else if (choice == "D")
                {
                    if (actCount == 0) { Pause(); continue; }
                    Console.Write("  Delete which # (or 'B' to back): ");
                    string numStr = Console.ReadLine();
                    if (numStr != null && numStr.Trim().ToUpper() == "B") continue;

                    int num;
                    if (int.TryParse(numStr, out num) && num >= 1 && num <= actCount)
                    {
                        for (int i = num - 1; i < actCount - 1; i++)
                        {
                            actName[i] = actName[i + 1];
                            actHasDeadline[i] = actHasDeadline[i + 1];
                            actDeadline[i] = actDeadline[i + 1];
                            for (int d = 1; d <= 7; d++) actDays[i, d] = actDays[i + 1, d];
                            actHours[i] = actHours[i + 1];
                            actPriority[i] = actPriority[i + 1];
                            actDone[i] = actDone[i + 1];
                        }
                        actCount--;
                        SaveData();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("  >> Deleted successfully.");
                        Console.ResetColor();
                    }
                    else Console.WriteLine("  !! Invalid number.");
                    Pause();
                }
            }
        }

        // =========================================================
        //  FEATURE 5: VIEW CALENDAR
        // =========================================================
        static void RemoveBusyTime(int start, int end)
        {
            int[] newStart = new int[MAX_BLOCKS];
            int[] newEnd = new int[MAX_BLOCKS];
            int newCount = 0;

            for (int i = 0; i < freeCount; i++)
            {
                int s = freeStart[i];
                int e = freeEnd[i];

                if (e <= start || s >= end)
                {
                    newStart[newCount] = s;
                    newEnd[newCount] = e;
                    newCount++;
                }
                else
                {
                    if (s < start) { newStart[newCount] = s; newEnd[newCount] = start; newCount++; }
                    if (e > end) { newStart[newCount] = end; newEnd[newCount] = e; newCount++; }
                }
            }

            freeCount = newCount;
            for (int i = 0; i < freeCount; i++) { freeStart[i] = newStart[i]; freeEnd[i] = newEnd[i]; }
        }

        static void ComputeFreeTime(int dayOfWeek)
        {
            freeCount = 1;
            freeStart[0] = 0;
            freeEnd[0] = 1440;

            for (int i = 0; i < schedCount; i++)
            {
                if (schedDay[i] == dayOfWeek)
                {
                    if (schedStart[i] < schedEnd[i]) RemoveBusyTime(schedStart[i], schedEnd[i]);
                    else RemoveBusyTime(schedStart[i], 1440);
                }
            }

            int prevDay = dayOfWeek - 1;
            if (prevDay == 0) prevDay = 7;

            for (int i = 0; i < schedCount; i++)
            {
                if (schedDay[i] == prevDay && schedStart[i] > schedEnd[i]) RemoveBusyTime(0, schedEnd[i]);
            }
        }

        static void GeneratePlan(DateTime weekStart)
        {
            placeCount = 0;
            for (int i = 0; i < MAX_RECORDS; i++)
            {
                for (int j = 0; j < 7; j++) placedHours[i, j] = 0;
            }

            int[] order = new int[MAX_RECORDS];
            int orderCount = 0;

            for (int i = 0; i < actCount; i++)
            {
                if (!actDone[i]) { order[orderCount] = i; orderCount++; }
            }

            for (int i = 0; i < orderCount - 1; i++)
            {
                for (int j = 0; j < orderCount - i - 1; j++)
                {
                    int a = order[j];
                    int b = order[j + 1];
                    bool swap = false;

                    if (actPriority[a] > actPriority[b]) swap = true;
                    else if (actPriority[a] == actPriority[b])
                    {
                        if (actHasDeadline[a] && actHasDeadline[b]) { if (actDeadline[a] > actDeadline[b]) swap = true; }
                        else if (actHasDeadline[a] && !actHasDeadline[b]) { swap = false; }
                        else if (!actHasDeadline[a] && actHasDeadline[b]) { swap = true; }
                    }

                    if (swap) { order[j] = b; order[j + 1] = a; }
                }
            }

            for (int d = 0; d < 7; d++)
            {
                DateTime currentDate = weekStart.AddDays(d);
                int dow = GetDayNumber(currentDate);
                ComputeFreeTime(dow);

                for (int o = 0; o < orderCount; o++)
                {
                    int ai = order[o];
                    if (!actDays[ai, dow]) continue;

                    int dayLimit = 1440;
                    if (actHasDeadline[ai])
                    {
                        if (currentDate.Date > actDeadline[ai].Date) continue;
                        if (currentDate.Date == actDeadline[ai].Date)
                        {
                            dayLimit = (actDeadline[ai].Hour * 60) + actDeadline[ai].Minute;
                        }
                    }

                    double needMinutes = actHours[ai] * 60.0;

                    for (int f = 0; f < freeCount; f++)
                    {
                        if (needMinutes <= 0) break;
                        int bEnd = freeEnd[f];
                        if (bEnd > dayLimit) bEnd = dayLimit;
                        int avail = bEnd - freeStart[f];
                        if (avail <= 0) continue;

                        int take = (int)needMinutes;
                        if (take > avail) take = avail;

                        if (take > 0)
                        {
                            placeAct[placeCount] = ai;
                            placeDay[placeCount] = d;
                            placeStart[placeCount] = freeStart[f];
                            placeEnd[placeCount] = freeStart[f] + take;
                            placeCount++;

                            placedHours[ai, d] = placedHours[ai, d] + (take / 60.0);
                            needMinutes = needMinutes - take;
                            freeStart[f] = freeStart[f] + take;
                        }
                    }
                }
            }
        }

        // Shows pending activities that have a deadline, with days remaining.
        // Hidden completely when there is nothing to show.
        static void ShowDeadlineList()
        {
            // Collect pending activities that have a deadline
            int[] list = new int[MAX_RECORDS];
            int count = 0;
            for (int i = 0; i < actCount; i++)
            {
                if (!actDone[i] && actHasDeadline[i])
                {
                    list[count] = i;
                    count++;
                }
            }

            // Nothing to show -> hide the whole section
            if (count == 0) return;

            // Sort by earliest deadline first
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = 0; j < count - i - 1; j++)
                {
                    if (actDeadline[list[j]] > actDeadline[list[j + 1]])
                    {
                        int temp = list[j];
                        list[j] = list[j + 1];
                        list[j + 1] = temp;
                    }
                }
            }

            Console.WriteLine("============================================================");
            Console.WriteLine("  DEADLINES");
            Console.WriteLine("============================================================");
            Console.WriteLine();

            for (int k = 0; k < count; k++)
            {
                int ai = list[k];
                int daysLeft = (int)(actDeadline[ai].Date - baseDate.Date).TotalDays;

                string status;
                if (daysLeft < 0) { status = "OVERDUE by " + (-daysLeft) + (daysLeft == -1 ? " day" : " days"); Console.ForegroundColor = ConsoleColor.Red; }
                else if (daysLeft == 0) { status = "DUE TODAY"; Console.ForegroundColor = ConsoleColor.Red; }
                else if (daysLeft <= 3) { status = daysLeft + (daysLeft == 1 ? " day left" : " days left"); Console.ForegroundColor = ConsoleColor.Yellow; }
                else { status = daysLeft + " days left"; Console.ForegroundColor = ConsoleColor.Green; }

                Console.WriteLine("  " + (k + 1) + ". " + actName[ai].PadRight(20) + " " + actDeadline[ai].ToString("MMM dd, yyyy HH:mm") + "   " + status);
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        static void ViewCalendar()
        {
            int weekOffset = 0;
            bool viewing = true;
            while (viewing)
            {
                // Refresh "today" so it stays correct if the program was left open past midnight
                baseDate = DateTime.Today;

                DateTime displayStart = currentWeekStart.AddDays(weekOffset * 7);
                GeneratePlan(displayStart);

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("============================================================");
                Console.WriteLine("  MY CALENDAR   " + displayStart.ToString("MMM dd, yyyy") + " - " + displayStart.AddDays(6).ToString("MMM dd, yyyy"));
                Console.WriteLine("============================================================");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("  Priority: High first, then earliest deadline.");
                Console.WriteLine("  Window: 00:00 - 24:00");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  [A] <- Prev Week    [D] -> Next Week    [B] Back to menu");
                Console.ResetColor();
                Console.WriteLine("============================================================");

                Console.Write("        ");
                for (int d = 0; d < 7; d++) { Console.Write(GetDayName(GetDayNumber(displayStart.AddDays(d))).PadRight(7)); }
                Console.WriteLine();

                Console.Write("        ");
                for (int d = 0; d < 7; d++)
                {
                    DateTime date = displayStart.AddDays(d);
                    string dateStr = date.ToString("MM/dd");
                    if (date.Date == baseDate.Date) { Console.ForegroundColor = ConsoleColor.Green; dateStr = "(Today)"; Console.ResetColor(); }
                    Console.Write(dateStr.PadRight(7));
                }
                Console.WriteLine();

                for (int h = 0; h < 24; h++)
                {
                    int minute = h * 60;
                    Console.Write(" " + h.ToString("00") + ":00  ");
                    for (int d = 0; d < 7; d++) { Console.Write(GetCellText(d, displayStart.AddDays(d), minute).PadRight(7)); }
                    Console.WriteLine();
                }
                Console.WriteLine();

                bool hasAnything = (schedCount > 0 || actCount > 0);
                if (hasAnything)
                {
                    Console.WriteLine("  LEGEND");
                    string[] printedNames = new string[MAX_RECORDS * 2];
                    int printedCount = 0;

                    for (int i = 0; i < schedCount; i++)
                    {
                        bool alreadyPrinted = false;
                        for (int j = 0; j < printedCount; j++) { if (printedNames[j] == schedName[i]) { alreadyPrinted = true; break; } }
                        if (!alreadyPrinted) { printedNames[printedCount] = schedName[i]; printedCount++; Console.WriteLine("   " + GetCode(schedName[i]).PadRight(6) + " - " + schedName[i]); }
                    }

                    for (int i = 0; i < actCount; i++)
                    {
                        bool alreadyPrinted = false;
                        for (int j = 0; j < printedCount; j++) { if (printedNames[j] == actName[i]) { alreadyPrinted = true; break; } }
                        if (!alreadyPrinted) { printedNames[printedCount] = actName[i]; printedCount++; Console.WriteLine("   " + GetCode(actName[i]).PadRight(6) + " - " + actName[i]); }
                    }
                    Console.WriteLine("   .      - Free");
                    Console.WriteLine();
                }

                ShowDeadlineList();

                Console.WriteLine("============================================================");

                int missedCount = 0;
                string reportText = "";

                for (int i = 0; i < actCount; i++)
                {
                    if (actDone[i]) continue;

                    int totalDays = 0, placedDays = 0;
                    double totalMissingHours = 0;
                    string missedDaysDetails = "";

                    for (int d = 0; d < 7; d++)
                    {
                        DateTime date = displayStart.AddDays(d);
                        int dow = GetDayNumber(date);

                        if (!actDays[i, dow]) continue;
                        if (actHasDeadline[i] && date.Date > actDeadline[i].Date) continue;

                        totalDays++;
                        if (placedHours[i, d] >= actHours[i] - 0.01) placedDays++;
                        else
                        {
                            double missingToday = actHours[i] - placedHours[i, d];
                            totalMissingHours += missingToday;
                            if (missedDaysDetails != "") missedDaysDetails += ", ";
                            missedDaysDetails += GetDayName(dow) + " (" + FormatDuration(missingToday) + " short)";
                        }
                    }

                    if (missedDaysDetails != "")
                    {
                        missedCount++;
                        reportText = reportText + "  " + missedCount + ". " + actName[i] + " [" + GetPriorityName(actPriority[i]) + "]\n";
                        reportText = reportText + "     Missed: " + missedDaysDetails + "\n";
                        reportText = reportText + "     Total Short: " + FormatDuration(totalMissingHours) + " (" + placedDays + " of " + totalDays + " days placed)\n\n";
                    }
                }

                if (missedCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  NOT FULLY SCHEDULED");
                    Console.WriteLine("============================================================");
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.Write(reportText);
                    Console.WriteLine("============================================================");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  All pending activities were fully scheduled.");
                    Console.ResetColor();
                    Console.WriteLine("============================================================");
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  [A] Prev Week   [D] Next Week   [1] Day view   [B] Back");
                Console.ResetColor();
                Console.Write("  Choice: ");

                string choice = Console.ReadLine();
                if (choice == null) choice = "";
                choice = choice.Trim().ToUpper();

                if (choice == "B") viewing = false;
                else if (choice == "A")
                {
                    if (weekOffset > -520) weekOffset--;
                    else { Console.WriteLine("  !! Reached the 10-year past limit."); Pause(); }
                }
                else if (choice == "D")
                {
                    if (weekOffset < 520) weekOffset++;
                    else { Console.WriteLine("  !! Reached the 10-year future limit."); Pause(); }
                }
                else if (choice == "1")
                {
                    Console.Write("  Enter exact Date (YYYY-MM-DD) (or 'B' to back): ");
                    string dayStr = Console.ReadLine();
                    if (dayStr != null && dayStr.Trim().ToUpper() == "B") continue;

                    DateTime targetDate;
                    if (DateTime.TryParse(dayStr, out targetDate))
                    {
                        int targetDow = GetDayNumber(targetDate);
                        DateTime targetWeekStart = targetDate.AddDays(-(targetDow - 1));
                        GeneratePlan(targetWeekStart);
                        ViewDay(targetWeekStart, targetDow - 1);
                    }
                    else { Console.WriteLine("  !! Invalid date format. Please use YYYY-MM-DD."); Pause(); }
                }
            }
        }

        static string GetCellText(int dayIndex, DateTime date, int minute)
        {
            int dow = GetDayNumber(date);

            for (int i = 0; i < schedCount; i++)
            {
                if (schedDay[i] == dow)
                {
                    if (schedStart[i] < schedEnd[i]) { if (schedStart[i] <= minute && schedEnd[i] > minute) return GetCode(schedName[i]); }
                    else { if (schedStart[i] <= minute) return GetCode(schedName[i]); }
                }
            }

            int prevDay = dow - 1;
            if (prevDay == 0) prevDay = 7;
            for (int i = 0; i < schedCount; i++)
            {
                if (schedDay[i] == prevDay && schedStart[i] > schedEnd[i] && schedEnd[i] > minute) return GetCode(schedName[i]);
            }

            for (int p = 0; p < placeCount; p++)
            {
                if (placeDay[p] == dayIndex && placeStart[p] <= minute && placeEnd[p] > minute) return GetCode(actName[placeAct[p]]);
            }

            return ".";
        }

        static void ViewDay(DateTime weekStart, int d)
        {
            bool viewing = true;
            while (viewing)
            {
                DateTime date = weekStart.AddDays(d);
                int dow = GetDayNumber(date);

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("============================================================");
                Console.WriteLine("  " + GetDayName(dow) + " - " + date.ToString("MMM dd, yyyy"));
                Console.WriteLine("============================================================");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("  TIME    ACTIVITY");
                Console.WriteLine("  ------  --------------------------------------------");

                for (int h = 0; h < 24; h++)
                {
                    int minute = h * 60;
                    string name = "";
                    int startMin = 0, endMin = 0;
                    bool isAct = false;

                    for (int i = 0; i < schedCount; i++)
                    {
                        if (schedDay[i] == dow)
                        {
                            if (schedStart[i] < schedEnd[i]) { if (schedStart[i] <= minute && schedEnd[i] > minute) { name = schedName[i]; startMin = schedStart[i]; endMin = schedEnd[i]; break; } }
                            else { if (schedStart[i] <= minute) { name = schedName[i]; startMin = schedStart[i]; endMin = 1440; break; } }
                        }
                    }

                    if (name == "")
                    {
                        int prevDay = dow - 1;
                        if (prevDay == 0) prevDay = 7;
                        for (int i = 0; i < schedCount; i++)
                        {
                            if (schedDay[i] == prevDay && schedStart[i] > schedEnd[i] && schedEnd[i] > minute) { name = schedName[i]; startMin = 0; endMin = schedEnd[i]; break; }
                        }
                    }

                    if (name == "")
                    {
                        for (int p = 0; p < placeCount; p++)
                        {
                            if (placeDay[p] == d && placeStart[p] <= minute && placeEnd[p] > minute) { name = actName[placeAct[p]]; startMin = placeStart[p]; endMin = placeEnd[p]; isAct = true; break; }
                        }
                    }

                    if (name == "") { Console.WriteLine("  " + h.ToString("00") + ":00   free"); }
                    else
                    {
                        string prefix = isAct ? "[ ] " : "";
                        string fullName = prefix + name;
                        if (startMin >= minute && startMin < minute + 60) Console.WriteLine("  " + h.ToString("00") + ":00   " + fullName.PadRight(25) + " (" + TimeToString(startMin) + "-" + TimeToString(endMin) + ")");
                        else Console.WriteLine("  " + h.ToString("00") + ":00   " + fullName.PadRight(25) + " (same block)");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("============================================================");

                int missedCount = 0;
                string missedText = "";

                for (int i = 0; i < actCount; i++)
                {
                    if (actDone[i]) continue;
                    if (!actDays[i, dow]) continue;
                    if (actHasDeadline[i] && date.Date > actDeadline[i].Date) continue;

                    if (placedHours[i, d] < actHours[i] - 0.01)
                    {
                        missedCount++;
                        double shortfall = actHours[i] - placedHours[i, d];
                        missedText = missedText + "  " + missedCount + ". " + actName[i] + " [" + GetPriorityName(actPriority[i]) + "]\n";
                        missedText = missedText + "     Needs " + FormatDuration(actHours[i]) + ", but only got " + FormatDuration(placedHours[i, d]) + ". (" + FormatDuration(shortfall) + " short)\n";
                    }
                }

                if (missedCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  NOT SCHEDULED TODAY");
                    Console.WriteLine("============================================================");
                    Console.ResetColor();
                    Console.Write(missedText);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  No activities were missed today.");
                    Console.ResetColor();
                }

                Console.WriteLine("============================================================");
                Console.WriteLine("  [B] Back");
                Console.Write("  Choice: ");

                string choice = Console.ReadLine();
                if (choice == null) choice = "";
                if (choice.Trim().ToUpper() == "B" || choice.Trim() == "") viewing = false;
            }
        }
    }
}