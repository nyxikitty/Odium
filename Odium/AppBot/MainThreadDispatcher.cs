using System;
using System.Collections.Generic;
using MelonLoader;

namespace Odium.Threadding
{
    public static class MainThreadDispatcher
    {
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();
        private static readonly object _lock = new object();
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;

            MelonEvents.OnGUI.Subscribe(ProcessQueue, 0);
            _initialized = true;
        }

        private static void ProcessQueue()
        {
            lock (_lock)
            {
                while (_executionQueue.Count > 0)
                {
                    try
                    {
                        _executionQueue.Dequeue().Invoke();
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"Error executing main thread action: {ex}");
                    }
                }
            }
        }

        public static void Enqueue(Action action)
        {
            if (action == null) return;

            lock (_lock)
            {
                _executionQueue.Enqueue(action);
            }
        }

        public static void EnqueueCoroutine(System.Collections.IEnumerator coroutine)
        {
            Enqueue(() => MelonCoroutines.Start(coroutine));
        }
    }
}