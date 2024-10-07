using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityMultiPlayer.Common;

namespace UnityMultiPlayer.ThreadManagement
{

    public class ThreadController : UnitySingleton<ThreadController>
    {
        private List<Thread> _threads = new List<Thread>();

        public void StartNewThread(ThreadStart task)
        {
            Thread newThread = new Thread(task);
            _threads.Add(newThread);
            newThread.Start();
        }


        private void OnApplicationQuit()
        {
            foreach (var thread in _threads)
            {
                try
                {
                    if (thread != null && thread.IsAlive)
                    {
                        thread.Abort();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }

            Debug.Log("All threads stopped.");
        }

    }

}