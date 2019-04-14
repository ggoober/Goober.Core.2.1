using System;
using System.Threading;
using System.Threading.Tasks;

namespace Goober.Core.Utils
{
    public static class AsyncUtils
    {
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            var taskFactory = new
                TaskFactory(CancellationToken.None,
                        TaskCreationOptions.None,
                        TaskContinuationOptions.None,
                        TaskScheduler.Default);

            return taskFactory.StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }


        public static void RunSync(Func<Task> func)
        {
            var taskFactory = new
                TaskFactory(CancellationToken.None,
                        TaskCreationOptions.None,
                        TaskContinuationOptions.None,
                        TaskScheduler.Default);

            taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }
    }
}
