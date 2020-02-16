using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Singijeon
{
    class RequestTrDataManager
    {
        private static RequestTrDataManager requestTrDataManagerInstance;

        Queue<Task> requestTaskQueue = new Queue<Task>();

        Thread taskWorker;
        public int REQUEST_DELAY = 610;
        //public int REQUEST_DELAY = 3000;

        private RequestTrDataManager()
        {
            taskWorker = new Thread(delegate()
            {
                while(true)
                {
                    try
                    {
                        while (requestTaskQueue.Count > 0)
                        {
                            requestTaskQueue.Dequeue().RunSynchronously();
                            Thread.Sleep(REQUEST_DELAY);
                        }
                        Thread.Sleep(100); //기본 실행 주기

                    }catch(Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                }
            });
        }
        public static RequestTrDataManager GetInstance()
        {
            if(requestTrDataManagerInstance == null)
            {
                requestTrDataManagerInstance = new RequestTrDataManager();
            }
            return requestTrDataManagerInstance;
        }
        public void Run()
        {
            taskWorker.Start();
        }
        public void RequestTrData(Task task)
        {
            requestTaskQueue.Enqueue(task);
        }
    }
}
