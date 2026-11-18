using System;

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

        // --- PLACEMENT DATA (For the generated calendar) ---
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

        static void Main()
        {
            Console.Title = "MyScheduler";

            Console.Clear();
            Console.WriteLine("============================================================");
            Console.WriteLine("  MyScheduler - Personal Schedule & Activity Planner");
            Console.WriteLine("============================================================");
            Console.WriteLine();
            
            while (true)
            {
                Console.Write("  Enter Current Date (YYYY-MM-DD): ");
                string dateInput = Console.ReadLine();
                
                DateTime parsedDate;
                if (DateTime.TryParse(dateInput, out parsedDate))
                {
                    baseDate = parsedDate.Date;
                    break;
                }
                else
                {
                    Console.WriteLine("  !! Invalid date format. Please try again.");
                }
            }

            // Automatically find the Monday of the inputted week
            int dayOfWeek = GetDayNumber(baseDate);
            currentWeekStart = baseDate.AddDays(-(dayOfWeek - 1));

            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("============================================================");
                Console.WriteLine("  MyScheduler - Personal Schedule & Activity Planner");
                Console.WriteLine("============================================================");
                Console.WriteLine();
                Console.WriteLine("  [1]  Add Fixed Schedule");
                Console.WriteLine("  [2]  View Fixed Schedules");
                Console.WriteLine("  [3]  Add Activity");
                Console.WriteLine("  [4]  View Activities");
                Console.WriteLine("  [5]  View Calendar");
                Console.WriteLine("  [0]  Exit");
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
                if (choice == "1") AddFixedSchedule();
                else if (choice == "2") ViewFixedSchedules();
                else if (choice == "3") AddActivity();
                else if (choice == "4") ViewActivities();
                else if (choice == "5") ViewCalendar();
                else if (choice == "0") running = false;
                else
                {
                    Console.WriteLine("  !! Invalid option. Please choose 0-5.");
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
            int d = (int)date.DayOfWeek; // 0=Sunday
            if (d == 0) d = 7; // Make Sunday 7
            return d;
        }

        static string TimeToString(int minutes)
        {
            int h = minutes / 60;
            int m = minutes % 60;
            return h.ToString("00") + ":" + m.ToString("00");
        }

        static bool IsValidTime(string timeStr)
        {
            if (timeStr == null || timeStr.Length != 5) return false;
            if (timeStr[2] != ':') return false;
            
            int h, m;
            if (!int.TryParse(timeStr.Substring(0, 2), out h)) return false;
            if (!int.TryParse(timeStr.Substring(3, 2), out m)) return false;
            
            if (h < 0 || h > 23) return false;
            if (m < 0 || m > 59) return false;
            
            return true;
        }

        static int TimeToInt(string timeStr)
        {
            int h = int.Parse(timeStr.Substring(0, 2));
            int m = int.Parse(timeStr.Substring(3, 2));
            return (h * 60) + m;
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
            if (schedCount >= MAX_RECORDS)
            {
                Console.Clear();
                Console.WriteLine("  !! Schedule list is full.");
                Pause();
                return;
            }

            Console.Clear();
            Console.WriteLine("============================================================");
            Console.WriteLine("  ADD FIXED SCHEDULE  (1 of 3)");
            Console.WriteLine("============================================================");
            Console.WriteLine();

            string name = "";
            while (true)
            {
                Console.Write("  Schedule name (or 'B' to back): ");
                name = Console.ReadLine();
                if (name != null && name.Trim().ToUpper() == "B") return;
                if (name != null && name.Trim() != "") break;
                Console.WriteLine("  !! Name cannot be empty.");
            }
            name = name.Trim();

            Console.Clear();
            Console.WriteLine("============================================================");
            Console.WriteLine("  ADD FIXED SCHEDULE  (2 of 3)");
            Console.WriteLine("============================================================");
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
                
                if (input == null || input.Trim() == "")
                {
                    Console.WriteLine("  !! Please select at least one day.");
                    continue;
                }

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
                    for (int j = 0; j < dayCount; j++)
                    {
                        if (selectedDays[j] == d) duplicate = true;
                    }
                    
                    if (duplicate)
                    {
                        Console.WriteLine("  !! Day " + d + " entered twice.");
                        error = true;
                        break;
                    }
                    
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
            Console.WriteLine("============================================================");
            Console.WriteLine("  ADD FIXED SCHEDULE  (3 of 3)");
            Console.WriteLine("============================================================");
            Console.WriteLine();
            Console.WriteLine("  Name: " + name);
            Console.Write("  Days: ");
            for (int i = 0; i < dayCount; i++)
            {
                Console.Write(GetDayName(selectedDays[i]) + " ");
            }
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
                string startIn = "";
                while (true)
                {
                    Console.Write("  Start time HH:MM (or 'B' to back): ");
                    startIn = Console.ReadLine();
                    if (startIn != null && startIn.Trim().ToUpper() == "B") return;
                    if (IsValidTime(startIn)) break;
                    Console.WriteLine("  !! Invalid time. Use HH:MM.");
                }

                string endIn = "";
                while (true)
                {
                    Console.Write("  End time HH:MM (or 'B' to back): ");
                    endIn = Console.ReadLine();
                    if (endIn != null && endIn.Trim().ToUpper() == "B") return;
                    if (IsValidTime(endIn)) break;
                    Console.WriteLine("  !! Invalid time. Use HH:MM.");
                }

                int st = TimeToInt(startIn);
                int en = TimeToInt(endIn);

                if (st == en)
                {
                    Console.WriteLine("  !! Start and end must differ.");
                    Pause();
                    return;
                }

                if (en < st)
                {
                    Console.WriteLine();
                    Console.WriteLine("  !! Note: End time is earlier than start time.");
                    Console.WriteLine("     This is an OVERNIGHT schedule.");
                    Console.Write("  [C] Confirm   [B] Back: ");
                    string conf = Console.ReadLine();
                    if (conf == null || conf.Trim().ToUpper() != "C") return;
                }

                for (int i = 0; i < dayCount; i++)
                {
                    startTimes[i] = st;
                    endTimes[i] = en;
                }
            }
            else
            {
                for (int i = 0; i < dayCount; i++)
                {
                    Console.WriteLine();
                    Console.WriteLine("  " + GetDayName(selectedDays[i]));
                    
                    string startIn = "";
                    while (true)
                    {
                        Console.Write("    Start time HH:MM (or 'B' to back): ");
                        startIn = Console.ReadLine();
                        if (startIn != null && startIn.Trim().ToUpper() == "B") return;
                        if (IsValidTime(startIn)) break;
                        Console.WriteLine("    !! Invalid time.");
                    }

                    string endIn = "";
                    while (true)
                    {
                        Console.Write("    End time HH:MM (or 'B' to back): ");
                        endIn = Console.ReadLine();
                        if (endIn != null && endIn.Trim().ToUpper() == "B") return;
                        if (IsValidTime(endIn)) break;
                        Console.WriteLine("    !! Invalid time.");
                    }

                    startTimes[i] = TimeToInt(startIn);
                    endTimes[i] = TimeToInt(endIn);

                    if (startTimes[i] == endTimes[i])
                    {
                        Console.WriteLine("  !! Start and end must differ.");
                        Pause();
                        return;
                    }
                }
            }

            Console.Clear();
            Console.WriteLine("============================================================");
            Console.WriteLine("  PREVIEW");
            Console.WriteLine("============================================================");
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

            for (int i = 0; i < dayCount; i++)
            {
                for (int j = 0; j < schedCount; j++)
                {
                    if (schedDay[j] == selectedDays[i])
                    {
                        int s1 = startTimes[i];
                        int e1 = endTimes[i];
                        int s2 = schedStart[j];
                        int e2 = schedEnd[j];

                        bool overlap = false;
                        if (s1 < e1 && s2 < e2) { if (s1 < e2 && s2 < e1) overlap = true; }
                        else if (s1 > e1 && s2 < e2) { if (s1 < e2 || s2 < e1) overlap = true; }
                        else if (s1 < e1 && s2 > e2) { if (s1 < e2 || s2 < e1) overlap = true; }
                        else if (s1 > e1 && s2 > e2) { overlap = true; }

                        if (overlap)
                        {
                            Console.WriteLine("  !! CONFLICT on " + GetDayName(selectedDays[i]) + " with " + schedName[j]);
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

            Console.WriteLine();
            Console.WriteLine("  >> Successfully added " + name);
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
                Console.WriteLine("============================================================");
                Console.WriteLine("  MY FIXED SCHEDULES                          (" + schedCount + " of " + MAX_RECORDS + ")");
                Console.WriteLine("============================================================");
                Console.WriteLine();

                if (schedCount == 0)
                {
                    Console.WriteLine("  No schedules added yet.");
                }
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
                        Console.WriteLine("  >> Deleted successfully.");
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
            Console.WriteLine("============================================================");
            Console.WriteLine("  ADD ACTIVITY");
            Console.WriteLine("============================================================");
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
                if (!DateTime.TryParse(dateStr, out d))
                {
                    Console.WriteLine("  !! Invalid date format. Canceled.");
                    Pause();
                    return;
                }

                string timeStr = "";
                while (true)
                {
                    Console.Write("    Time HH:MM (or 'B' to back): ");
                    timeStr = Console.ReadLine();
                    if (timeStr != null && timeStr.Trim().ToUpper() == "B") return;
                    if (IsValidTime(timeStr)) break;
                    Console.WriteLine("  !! Invalid time.");
                }

                int mins = TimeToInt(timeStr);
                deadline = d.Date.AddMinutes(mins);
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
                    if (int.TryParse(parts[i].Trim(), out d) && d >= 1 && d <= 7)
                    {
                        days[d] = true;
                        hasDays = true;
                    }
                }
            }
            
            if (!hasDays) { Console.WriteLine("  !! No days selected. Canceled."); Pause(); return; }

            double hrs = 0;
            while (true)
            {
                Console.Write("  Hours per day (or 'B' to back): ");
                string hrStr = Console.ReadLine();
                if (hrStr != null && hrStr.Trim().ToUpper() == "B") return;
                
                if (double.TryParse(hrStr, out hrs))
                {
                    if (hrs >= 0.5 && hrs <= 8)
                    {
                        double doubled = hrs * 2;
                        if (doubled == (int)doubled) break;
                    }
                }
                Console.WriteLine("  !! Must be 0.5 to 8, in 0.5 steps.");
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
            Console.WriteLine("  Hours/day: " + hrs);
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

            Console.WriteLine();
            Console.WriteLine("  >> Successfully added " + name);
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
                Console.WriteLine("============================================================");
                Console.WriteLine("  MY ACTIVITIES                               (" + actCount + " of " + MAX_RECORDS + ")");
                Console.WriteLine("============================================================");
                Console.WriteLine();

                if (actCount == 0)
                {
                    Console.WriteLine("  No activities added yet.");
                }
                else
                {
                    Console.WriteLine("  #   DONE   PRIO   ACTIVITY             HOURS/DAY");
                    Console.WriteLine("  --  -----  -----  -------------------  ---------");
                    for (int i = 0; i < actCount; i++)
                    {
                        string mark = "[ ]";
                        if (actDone[i]) mark = "[x]";
                        
                        Console.WriteLine("  " + (i + 1) + ".  " + mark + "    " + GetPriorityName(actPriority[i]) + "   " + actName[i].PadRight(20) + " " + actHours[i]);
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
                        Console.WriteLine("  >> Updated status.");
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
                        Console.WriteLine("  >> Deleted successfully.");
                    }
                    else Console.WriteLine("  !! Invalid number.");
                    Pause();
                }
            }
        }

        // =========================================================
        //  FEATURE 5: VIEW CALENDAR (Logic & Display)
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
            for (int i = 0; i < freeCount; i++)
            {
                freeStart[i] = newStart[i];
                freeEnd[i] = newEnd[i];
            }
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
                if (!actDone[i])
                {
                    order[orderCount] = i;
                    orderCount++;
                }
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

        static void ViewCalendar()
        {
            int weekOffset = 0; 
            
            bool viewing = true;
            while (viewing)
            {
                DateTime displayStart = currentWeekStart.AddDays(weekOffset * 7);
                GeneratePlan(displayStart);

                Console.Clear();
                Console.WriteLine("============================================================");
                Console.WriteLine("  MY CALENDAR   " + displayStart.ToString("MMM dd, yyyy") + " - " + displayStart.AddDays(6).ToString("MMM dd, yyyy"));
                Console.WriteLine("============================================================");
                Console.WriteLine();
                Console.WriteLine("  Priority: High first, then earliest deadline.");
                Console.WriteLine("  Window: 00:00 - 24:00");
                Console.WriteLine();
                Console.WriteLine("  [A] <- Prev Week    [D] -> Next Week    [B] Back to menu");
                Console.WriteLine("============================================================");
                
                Console.Write("        ");
                for (int d = 0; d < 7; d++)
                {
                    DateTime date = displayStart.AddDays(d);
                    Console.Write(GetDayName(GetDayNumber(date)).PadRight(7));
                }
                Console.WriteLine();

                Console.Write("        ");
                for (int d = 0; d < 7; d++)
                {
                    DateTime date = displayStart.AddDays(d);
                    string dateStr = date.ToString("MM/dd");
                    if (date.Date == baseDate.Date) dateStr = "(Today)";
                    Console.Write(dateStr.PadRight(7));
                }
                Console.WriteLine();

                for (int h = 0; h < 24; h++)
                {
                    int minute = h * 60;
                    Console.Write(" " + h.ToString("00") + ":00  ");
                    
                    for (int d = 0; d < 7; d++)
                    {
                        string cellText = GetCellText(d, displayStart.AddDays(d), minute);
                        Console.Write(cellText.PadRight(7));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();

                bool hasAnything = false;
                for (int i = 0; i < schedCount; i++) hasAnything = true;
                for (int i = 0; i < actCount; i++) hasAnything = true;

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

                Console.WriteLine("============================================================");
                
                int missedCount = 0;
                string reportText = "";

                for (int i = 0; i < actCount; i++)
                {
                    if (actDone[i]) continue;

                    int totalDays = 0;
                    int placedDays = 0;
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
                            missedDaysDetails += GetDayName(dow) + " (" + missingToday.ToString("0.0") + " hrs short)";
                        }
                    }

                    if (missedDaysDetails != "")
                    {
                        missedCount++;
                        reportText = reportText + "  " + missedCount + ". " + actName[i] + " [" + GetPriorityName(actPriority[i]) + "]\n";
                        reportText = reportText + "     Missed: " + missedDaysDetails + "\n";
                        reportText = reportText + "     Total Short: " + totalMissingHours.ToString("0.0") + " hrs (" + placedDays + " of " + totalDays + " days placed)\n\n";
                    }
                }

                if (missedCount > 0)
                {
                    Console.WriteLine("  NOT FULLY SCHEDULED");
                    Console.WriteLine("============================================================");
                    Console.WriteLine();
                    Console.Write(reportText);
                    Console.WriteLine("============================================================");
                }
                else
                {
                    Console.WriteLine("  All pending activities were fully scheduled.");
                    Console.WriteLine("============================================================");
                }

                Console.WriteLine();
                Console.WriteLine("  [A] Prev Week   [D] Next Week   [1] Day view   [B] Back");
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
                    else
                    {
                        Console.WriteLine("  !! Invalid date format. Please use YYYY-MM-DD.");
                        Pause();
                    }
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
                Console.WriteLine("============================================================");
                Console.WriteLine("  " + GetDayName(dow) + " - " + date.ToString("MMM dd, yyyy"));
                Console.WriteLine("============================================================");
                Console.WriteLine();
                Console.WriteLine("  TIME    ACTIVITY");
                Console.WriteLine("  ------  --------------------------------------------");

                for (int h = 0; h < 24; h++)
                {
                    int minute = h * 60;
                    string name = "";
                    int startMin = 0;
                    int endMin = 0;
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

                    if (name == "")
                    {
                        Console.WriteLine("  " + h.ToString("00") + ":00   free");
                    }
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
                        missedText = missedText + "     Needs " + actHours[i] + " hrs, but only got " + placedHours[i, d].ToString("0.0") + " hrs. (" + shortfall.ToString("0.0") + " hrs short)\n";
                    }
                }

                if (missedCount > 0)
                {
                    Console.WriteLine("  NOT SCHEDULED TODAY");
                    Console.WriteLine("============================================================");
                    Console.Write(missedText);
                }
                else
                {
                    Console.WriteLine("  No activities were missed today.");
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