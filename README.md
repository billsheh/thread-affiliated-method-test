This utility function allows you to test a method requiring running within one thread. For example, for a method in a view model, it is better tested simulating the runtime enviroment, i.e., the method will be executed in UI thread.

The basic usage is rather simple, to test the following method, excepted from Stephen Toub's blog, http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx

        static async Task PrintThreadInfo()
        {
            //
            var d = new Dictionary<int, int>();
            for (int i = 0; i < 10000; i++)
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                int count;
                d[id] = d.TryGetValue(id, out count) ? count + 1 : 1;

                await Task.Yield();
            }
            foreach (var pair in d) Console.WriteLine(pair);
        }
        
        [Test]
        public void PrintThreadInfo_Test()
        {
            Console.WriteLine("Testing thread Id {0}", Thread.CurrentThread.ManagedThreadId);

            TestingUtility.RunThreadAffiliatedMethod(() => PrintThreadInfo());
           
        }
        
It can also work with exception test, shown below:

        [Test]
        [ExpectedException("System.Exception", ExpectedMessage = "Test exception")]
        public void Exception_Test()
        {
            
            TestingUtility.RunThreadAffiliatedMethod(async () => { throw new Exception("Test exception"); });
        }

Internals

There are other ways to write such kind of testing utility, like using thread affiliated scheduler[1] or DispatcherSynchronizationContext[2], but those are not easy to understand. Here I am following the very basic approach: create a message pump(i.e., Dispatcher) and pump message(method to be tested) into the message loop. Once the method is executed, exit the message loop(Dispatcher). Turn on the second debug flag, you will see some useful thread exectuing information.

 [1] Stephen Toub - http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx 
 [2] Custom Scheduler - http://www.journeyofcode.com/custom-taskscheduler-sta-threads/
