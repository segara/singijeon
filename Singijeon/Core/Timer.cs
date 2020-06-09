using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Singijeon.Core
{
    public class TimerJob
    {
        Thread taskWorker;
        Task task;
        bool play = true;
        bool pause = false;
        public void Stop()
        {
            play = false;
            taskWorker.Interrupt();
        }
        public void SetPause(bool _pause)
        {
            pause = _pause;
        }
        public void SetTask(Task _task)
        {
            task = _task;
    }
        public void StartWork(int delay, Action func)
        {
            taskWorker = new Thread(delegate ()
            {
                while (play)
                {
                    try
                    {
                        if (!pause)
                        {
                            Task requestItemInfoTask = new Task(() =>
                            {
                                func.Invoke();

                            });
                            requestItemInfoTask.Start();
                        }
                          
                        Thread.Sleep(delay); //기본 실행 주기

                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                }
            });
            taskWorker.IsBackground = true;
            taskWorker.Start();
        }
    }
}
