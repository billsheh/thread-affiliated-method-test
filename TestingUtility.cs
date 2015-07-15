    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    
    public static class TestingUtility
    {
        public static void RunThreadAffiniatedAction(Func<Task> taskFunc, bool outputDebugInfo = false)
        {

            Dispatcher dispatcher = null;
            DispatcherSynchronizationContext uiSyncCtx = null;
            ManualResetEvent dispatcherReadyEvent = new ManualResetEvent(false);


            TaskScheduler actionScheduler = null;
            Exception actionException = null;

            Thread actionThread = new Thread(new ThreadStart(() =>
            {
                dispatcher = Dispatcher.CurrentDispatcher;
                uiSyncCtx = new DispatcherSynchronizationContext(dispatcher);

                SynchronizationContext.SetSynchronizationContext(uiSyncCtx);

                dispatcherReadyEvent.Set();

                actionScheduler = TaskScheduler.FromCurrentSynchronizationContext();

                if (outputDebugInfo)
                    Console.WriteLine("Starting action thread dispatcher...");

                Dispatcher.Run();

                if (outputDebugInfo)
                    Console.WriteLine("Exit action thread dispatcher");
            }));
            actionThread.Start();

            if (outputDebugInfo)
                Console.WriteLine("Action thread Id {0}", actionThread.ManagedThreadId);

            dispatcherReadyEvent.WaitOne();


            Func<Task> threadAction = async () =>
            {

                await taskFunc().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        actionException = t.Exception;
                    }
                    else if (t.IsCanceled)
                    {
                        actionException = new OperationCanceledException("task was cancelled");
                    }

                    if (outputDebugInfo)
                        Console.WriteLine("Shutting down action thread...");
                    dispatcher.InvokeShutdown();
                });



            };

            if (outputDebugInfo)
                Console.WriteLine("Starting to execute action...");
            dispatcher.InvokeAsync(threadAction);


            if (outputDebugInfo)
                Console.WriteLine("Wait for the action thread to finish...");
            actionThread.Join();

            if (outputDebugInfo)
                Console.WriteLine("Action thread finished");

            if (actionException != null)
            {
                var aggreException = actionException as AggregateException;
                if (aggreException != null)
                {
                    if (aggreException.InnerExceptions.Count() == 1)
                    {
                        throw aggreException.InnerExceptions[0];

                    }
                    else
                    throw aggreException.Flatten();

                }else
                {
                    throw actionException;
                }
            }

        }
    }
