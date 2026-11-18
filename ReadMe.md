# MyScheduler: A C# Console App for Planning Tasks Around Your Fixed Schedule

## 📝 Project Description
MyScheduler is a C# console application that helps students and workers manage their time. It allows users to input their fixed schedules (like classes and sleep) and flexible tasks (like homework). The system automatically calculates the user's free time and slots the tasks into those gaps.

## 🎯 Objectives
* Calculate free time by subtracting fixed schedules from a 24-hour day.
* Automatically schedule flexible tasks based on priority (High to Low) and deadlines.
* Save and load user data locally so information is not lost when the app closes.
* Display a weekly and daily calendar view.

## 👨‍💻 Group Members
* Cataring, Brent Aldrich V.
* Aljama, Nikyle Nate
* Hernandez, Charles Benedict

## 💻 Programming Language Used
* **C#** (.NET SDK) 
* (No databases or external libraries were used).

## ✨ System Features
* **Add Fixed Schedules:** Input classes, shifts, or sleep. Supports overnight schedules and prevents overlapping times.
* **Add Activities:** Input flexible tasks with deadlines, daily hours required, and priority levels.
* **Auto-Scheduling:** Automatically places tasks in the earliest available free time.
* **Deadline Report:** Shows exactly which days and how many hours a task failed to schedule.
* **File I/O (Auto-Save):** Automatically saves and loads data using local text files.
* **Safe Exit:** Prevents accidental closing (Ctrl+C). The app only exits when you choose `[0] Exit`.
* **Auto-Date:** Automatically detects the computer's current system date.

## 🚀 Instructions on How to Compile and Run

**Requirements:**
* A computer with [.NET SDK](https://dotnet.microsoft.com/download) installed.

**Steps to Run:**
1. Open your terminal or command prompt.
2. Navigate to the folder containing `Program.cs`.
3. Compile the application by typing:
dotnet build
4. Run the application by typing:
dotnet run