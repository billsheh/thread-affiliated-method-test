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
        public async void PrintThreadInfo_ThreadPool_Test()
        {
            await Task.Run(async () =>
            {
                await PrintThreadInfo();
            });
        }

The method above will output one line in a thread affiliated enviroment but it will output multiple lines in a thread pool enviroment.

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

What it means to testing

This simple utility can be used to test all UI such as a dialog, View Models, etc which must be running in a dedicated thread. You will find the testing quite complex if you refers to the PRISM reference implementations. With this utility you can write testing code like this:

        [TestMethod]
        public void OpenFolderDialog_Test()
        {
            TestingUtility.RunThreadAffiliatedMethod(async () =>
            {
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                dialog.Title = "Test dialog";
                CommonFileDialogResult result = dialog.ShowDialog();
                if (result != CommonFileDialogResult.Ok)
                    return;
                else
                    Trace.WriteLine(dialog.FileName);
            });
        }


Internals

There are other ways to write such kind of testing utility, like using thread affiliated scheduler[1] or DispatcherSynchronizationContext[2], but those are not easy to understand. Here I am following the very basic approach: create a message pump(i.e., Dispatcher) and pump message(method to be tested) into the message loop. Once the method is executed, exit the message loop(Dispatcher). Turn on the second debug flag, you will see some useful thread exectuing information.

        [1] Stephen Toub - http://blogs.msdn.com/b/pfxteam/archive/2012/01/20/10259049.aspx 
        [2] Custom Scheduler - http://www.journeyofcode.com/custom-taskscheduler-sta-threads/
