using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_2019_3_OR_NEWER
using UnityEngine.LowLevel;
#else
using UnityEngine.Experimental.LowLevel;
#endif

#pragma warning disable CS1998

namespace Pixyz.Utils
{
    public static class Dispatcher
    {
        #region Yield Instructions

        public abstract class YieldInstruction
        {
            public virtual async Task<bool> Dispatch(IEnumerator enumerator)
            {
                return true;
            }
        }

        #region Go Main Thread
        public class GoMainThreadDispatch : YieldInstruction
        {
            public override async Task<bool> Dispatch(IEnumerator enumerator)
            {
                lock (pendingActions) {
                    pendingActions.Add(new DelayedAction(() => MoveNext(enumerator)));
                }
                return true;
            }
        }
        public static GoMainThreadDispatch GoMainThread()
        {
            return new GoMainThreadDispatch();
        }
        #endregion

        #region Delay Frames
        public class DelayFramesDispatch : YieldInstruction
        {
            private int frames;
            public DelayFramesDispatch(int frames)
            {
                this.frames = frames;
            }
            public override async Task<bool> Dispatch(IEnumerator enumerator)
            {
                lock (pendingActions) {
                    pendingActions.Add(new DelayedAction(() => MoveNext(enumerator), frames));
                }
                return true;
            }
        }
        public static DelayFramesDispatch DelayFrames(int frames)
        {
            return new DelayFramesDispatch(frames);
        }
        #endregion

        #region Go Thread Pool
        public class GoThreadPoolDispatch : YieldInstruction
        {
            public override async Task<bool> Dispatch(IEnumerator enumerator)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback((o) => MoveNext(enumerator)));
                return true;
            }
        }
        public static GoThreadPoolDispatch GoThreadPool()
        {
            return new GoThreadPoolDispatch();
        }
        #endregion

        #region Wait Seconds
        public class WaitForSecondsDispatch : YieldInstruction
        {
            internal readonly float seconds;
            internal WaitForSecondsDispatch(float seconds)
            {
                this.seconds = seconds;
            }
            public override async Task<bool> Dispatch(IEnumerator enumerator)
            {
                await Task.Delay(Mathf.RoundToInt(((WaitForSecondsDispatch)enumerator.Current).seconds * 1000));
                MoveNext(enumerator);
                return true;
            }
        }
        public static WaitForSecondsDispatch WaitForSeconds(float seconds)
        {
            return new WaitForSecondsDispatch(seconds);
        }
        #endregion

        #region Sleep Seconds
        public class SleepForSecondsDispatch : YieldInstruction
        {
            internal readonly float seconds;
            internal SleepForSecondsDispatch(float seconds)
            {
                this.seconds = seconds;
            }
            public override async Task<bool> Dispatch(IEnumerator enumerator)
            {
                Thread.Sleep(Mathf.RoundToInt(((SleepForSecondsDispatch)enumerator.Current).seconds * 1000));
                MoveNext(enumerator);
                return true;
            }
        }
        public static SleepForSecondsDispatch SleepForSeconds(float seconds)
        {
            return new SleepForSecondsDispatch(seconds);
        }
        #endregion

        #endregion

        static Dispatcher()
        {
            Initialize();
        }

        public static event VoidHandler OnUpdate;

        private static void Initialize()
        {
            // Connect to game loop
            PlayerLoopSystem playerLoop = PlayerLoop.GetDefaultPlayerLoop();
            var newSystemList = new PlayerLoopSystem[playerLoop.subSystemList.Length + 1];
            Array.Copy(playerLoop.subSystemList, 0, newSystemList, 1, playerLoop.subSystemList.Length);
            newSystemList[0] = new PlayerLoopSystem { type = typeof(PlayerLoopSystem.UpdateFunction), updateDelegate = () => Update() };
            playerLoop.subSystemList = newSystemList;
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        public struct DelayedAction
        {
            public Action action;
            public int framesDelay;
            public DelayedAction(Action action, int framesDelay = 0)
            {
                this.action = action;
                this.framesDelay = framesDelay;
            }
        }

        /// <summary>
        /// Pending actions to dispatch in the main thread.
        /// </summary>
        private static List<DelayedAction> pendingActions = new List<DelayedAction>();

        /// <summary>
        /// Update callback, connected to editor loop and game loop, in order to be able to dispatch
        /// instructions in the main thread in editor or play mode.
        /// </summary>
        public static void Update()
        {
            OnUpdate?.Invoke();

            lock (pendingActions) {
                for (int i = 0; i < pendingActions.Count; i++) {
                    if (pendingActions[i].framesDelay <= 0) {
                        pendingActions[i].action?.Invoke();
                        pendingActions.RemoveAt(i);
                        i--;
                    } else {
                        pendingActions[i] = new DelayedAction(pendingActions[i].action, pendingActions[i].framesDelay - 1);
                    }
                }
            }
        }

        /// <summary>
        /// Starts a coroutine.
        /// Similar to Unity's coroutine, except that it can switch threads.
        /// Yield instructions to use are all static methods in the Dispatcher class.
        /// </summary>
        /// <param name="enumerable"></param>
        public static void StartCoroutine(IEnumerator enumerable)
        {
            MoveNext(enumerable);
        }

        /// <summary>
        /// Move to the next IEnumerator yield instruction.
        /// </summary>
        /// <param name="enumerator"></param>
        private static async void MoveNext(IEnumerator enumerator)
        {
            if (enumerator.MoveNext()) {
                var actionDispatcher = enumerator.Current as YieldInstruction;
                if (actionDispatcher != null) {
                    await actionDispatcher.Dispatch(enumerator);
                } else {
                    // It will still do MoveNext, but will not await for this yield
                    //Debug.LogWarning($"Yield '{enumerator.Current}' not recognized");
                }
            } else {
                return;
            }
        }
    }
}