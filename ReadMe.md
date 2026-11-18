MyScheduler: A C# Console App for Planning Tasks Around Your Fixed Schedule
MyScheduler

A C# Console Application that allows users to efficiently manage their time using fixed schedules and flexible tasks. This application will calculate your free time and automatically assign your tasks depending on their priority and deadlines.

Members:

Cataring, Brent Aldrich V.

Aljama, Nikyle Nate

Hernandez, Charles Benedict

Objectives:

To record both scheduled and flexible tasks inside one console application.

Calculate free time during a 24-Hour schedule including overnight.

Automatically schedule tasks depending on its priority (High/Medium/Low) and deadlines

Display missed tasks in a report.

Display your schedule through a weekly/daily calendar.

Language Used:

C# (.NET Console Application)

Features:

Add Schedule

This allows users to add a fixed task like classes or sleeping. No overlapping will occur and can also be used for overnight schedules (e.g. 22:00-06:00).

Manage Schedules

This allows users to view their list of fixed schedules. Users can also delete any errors.

Add Activity

This allows users to add flexible tasks. These tasks can be set to different priority levels, hours per day and deadlines (optional).

Manage Activities

This allows users to view their list of tasks. Users can also mark tasks as Done or delete them.

Auto Scheduler

This will automatically add your pending activities to your free time. Long tasks will be split into smaller chunks so they fit perfectly.

Calendar

This displays your schedule in weekly format. Users can also go to specific days.

Missed Report

If you have missed any tasks this will tell you. This will also show exactly how many hours were missed.

How to Compile and Run

Ensure that .NET SDK is installed on your computer.

Open terminal/command prompt.

Go into your src folder.

Run:

dotnet run

You will be asked to enter the current date (YYYY-MM-DD). Once entered you can enter your schedules/activities via the main menu.