using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace AlarmLibrary
{
    public static class BackgroundTaskHelper
    {
        public static bool TryRegisterCompletedHandler(string taskName, BackgroundTaskCompletedEventHandler action)
        {
            var taskRegistragion = BackgroundTaskRegistration.AllTasks.Values.SingleOrDefault(r => r.Name == taskName);
            if (taskRegistragion == null) return false;
            taskRegistragion.Completed += action;
            return true;
        }

        public static bool IsTaskRegistered(string taskName)
        {
            var taskRegistered = false;
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    taskRegistered = true;
                    break;
                }
            }
            return taskRegistered;
        }

        public async static Task<bool> RegisterTask(string taskName, string taskAssemblyName,
            params IBackgroundTrigger[] triggers)
        {
            return await RegisterTask(taskName, taskAssemblyName, null, triggers);
        }

        public async static Task<bool> RegisterTask(string taskName, string taskAssemblyName,
            BackgroundTaskCompletedEventHandler completedHandler, params IBackgroundTrigger[] triggers)
        {
            var taskRegistered = IsTaskRegistered(taskName);

            if (taskRegistered) return false; // already registered so this registration was not successfull.

            //required call
            var access = await BackgroundExecutionManager.RequestAccessAsync();

            //abort if access isn't granted
            if (access == BackgroundAccessStatus.DeniedByUser || access == BackgroundAccessStatus.DeniedBySystemPolicy)
            {
                return false;
            }

            var builder = new BackgroundTaskBuilder();

            builder.Name = taskName;
            builder.TaskEntryPoint = taskAssemblyName + "." + taskName;

            foreach (var trigger in triggers)
            {
                builder.SetTrigger(trigger);
            }

            var r = builder.Register();

            if (completedHandler != null)
            {
                r.Completed += completedHandler;
            }

            return true;
        }
    }
}
