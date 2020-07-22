using System;
using System.Collections.Generic;
using System.Threading;

namespace Pixyz.Utils  {

    public sealed class ThreadQueue : IDisposable {

        private volatile bool _stopped = false;
        public bool stopped { get { return _stopped; } }

        private volatile bool _isRunning = false;
        public bool isRunning { get { return _isRunning; } }

        private Queue<Action> actionPool = new Queue<Action>();
        private Thread thread;
        private AutoResetEvent waitHandle = new AutoResetEvent(false);

        public ThreadQueue(string _name = "ThreadQueue") {
            ThreadStart threadStart = new ThreadStart(loop);
            thread = new Thread(threadStart) { Name = _name };
            thread.Priority = ThreadPriority.Normal;
            thread.IsBackground = true;
        }

        private static ThreadQueue _Instance;
        public static ThreadQueue Instance {
            get {
                if (_Instance == null) {
                    _Instance = new ThreadQueue("Instance");
                    _Instance.start();
                }
                return _Instance;
            }
        }

        /// <summary>
        /// Starts the ThreadQueue
        /// </summary>
        public void start() {

            if (stopped)
                throw new Exception("Can't start a threadQueue that has been stopped. Maybe you should use pause(bool) instead.");

            if (isRunning)
                return;

            _isRunning = true;
            thread.Start();
        }

        /// <summary>
        /// Stop 
        /// </summary>
        public void stop() {

            _stopped = true;

            if (!thread.IsAlive)
                return;

            thread.Abort();
            _isRunning = false;
        }

        /// <summary>
        /// Will pause the ThreadQueue. Any running action will be completed before pausing.
        /// </summary>
        /// <param name="pause"></param>
        public void pause(bool pause) {
            _isRunning = !pause;
        }

        private void loop() {
            while (isRunning) {
                if (actionPool.Count == 0) {
                    waitHandle.WaitOne();
                } else {
                    Action action = actionPool.Dequeue();
                    if (action != null)
                        action.Invoke();
                }
            }
        }

        public void addAction(Action action) {
            actionPool.Enqueue(action);
            waitHandle.Set();
        }

        public void clear() {
            actionPool.Clear();
        }

        public void Dispose() {
            stop();
            GC.SuppressFinalize(this);
        }
    }
}
