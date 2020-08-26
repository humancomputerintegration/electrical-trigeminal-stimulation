using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pixyz.Utils  {

    public static class Profiling {

        public static void Time(string name, Action action) {
            Stopwatch time = Stopwatch.StartNew();
            action.Invoke();
            time.Stop();
            UnityEngine.Debug.Log($"<color=#8000ff>{name} done in {time.Elapsed.FormatNicely()}</color>");
        }

        private static Dictionary<string, Stopwatch> _Stopwatches = new Dictionary<string, Stopwatch>();

        public static void Start(string key) {
            if (!_Stopwatches.ContainsKey(key))
                _Stopwatches.Add(key, Stopwatch.StartNew());
            else {
                _Stopwatches[key] = Stopwatch.StartNew();
            }
        }

        public static TimeSpan End(string key) {
            if (!_Stopwatches.ContainsKey(key))
                return TimeSpan.MinValue;
            Stopwatch sw = _Stopwatches[key];
            sw.Stop();
            _Stopwatches.Remove(key);
            return sw.Elapsed;
        }

        public static void EndAndPrint(string key) {
            TimeSpan time = End(key);
            UnityEngine.Debug.Log($"<color=#8000ff>{key} done in {time.FormatNicely()}</color>");
        }
    }

    public class Profiler {

        private Stopwatch _timer;
        private Stopwatch _refer;
        private readonly string name;
        private int _iterations;

        public Profiler(string name) {
            this.name = name;
        }

        public void start() {
            if (_timer == null) {
                _timer = Stopwatch.StartNew();
                _refer = Stopwatch.StartNew();
            } else {
                _timer.Start();
                _refer.Start();
            }
        }

        public void end() {
            _timer.Stop();
            _iterations++;
        }

        public override string ToString() {
            if (_timer == null) {
                return $"Profiled '{name}' has never ran";
            }
            return $"{Math.Round(100d * _timer.ElapsedTicks / _refer.ElapsedTicks, 2)}% of time, ~{1000d * _timer.ElapsedMilliseconds / _iterations}μs, x{_iterations}";
        }

        public void log() {
            UnityEngine.Debug.Log(ToString());
        }
    }
}