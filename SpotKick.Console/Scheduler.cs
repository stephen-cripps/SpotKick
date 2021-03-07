using System;
using System.IO;
using System.Linq;
using Microsoft.Win32.TaskScheduler;

namespace SpotKick.ConsoleApp
{
    class Scheduler
    {
        readonly TaskService taskService;
        const string taskName = "SpotKick Schedule";

        public Scheduler()
        {
            taskService= new TaskService();
        }

        /// <summary>
        /// Registers a daily schedule
        /// </summary>
        /// <param name="dailyInterval">run every n days</param>
        /// <param name="time">hour in the day at which to run</param>
        public void ScheduleOn(short dailyInterval, int time)
        {
            var task = taskService.NewTask();
            task.Actions.Add(new ExecAction("Spotkick.Console.exe", workingDirectory: Directory.GetCurrentDirectory()));

            //Run as administrator
            task.Principal.RunLevel = TaskRunLevel.Highest;
            
            //If scheduled time was missed, run when next available
            task.Settings.StartWhenAvailable = true;

            var trigger = new DailyTrigger(dailyInterval) {StartBoundary = DateTime.Today + TimeSpan.FromHours(time)};
            task.Triggers.Add(trigger);

            taskService.RootFolder.RegisterTaskDefinition(taskName, task);
        }

        /// <summary>
        /// Removes the daily schedule
        /// </summary>
        public void ScheduleOff()
        {
            using var ts = new TaskService();
            var tasks = ts.AllTasks.Where(t => t.Path.Contains("Schedule test task"));

            foreach (var task in tasks.Select(t => t))
            {
                task.Definition.Triggers.Clear();
                task.Definition.Principal.RunLevel = TaskRunLevel.Highest;
                task.RegisterChanges();
            }
        }
    }
}
