using Pixyz.Utils;
using UnityEditor;
using UnityEngine;

namespace Pixyz.Editor {

    public class ProgressBar {

        private static ProgressBar _Instance;

        private VoidHandler cancelCallback;

        private float latestProgress = 0f;
        private float progress = 0f;

        private string latestMessage = null;
        private string message = "";

        private float timeSinceLastRepaint;

        private bool closed = false;
        private string title;

        public ProgressBar(VoidHandler cancelCallback, string title = "Processing...") {
            this.cancelCallback = cancelCallback;
            this.title = title;

            Dispatcher.OnUpdate += OnEditorUpdate;

            // Avoids 2 overlapping progress bars (cancels the previous one)
            if (_Instance != null && !_Instance.closed) {
                _Instance.SetProgress(1, null);
                _Instance = null;
            }

            _Instance = this;
        }

        public void SetProgress(float progress, string message) {
            this.progress = progress;
            this.message = message;
        }

        private void Close() {
            closed = true;
            EditorUtility.ClearProgressBar();
            Dispatcher.OnUpdate -= OnEditorUpdate;
        }

        private void OnEditorUpdate() {

            if (closed)
                return;

            timeSinceLastRepaint += Time.deltaTime;

            if (latestProgress != progress
             || latestMessage != message
             || timeSinceLastRepaint > 1f) {
                Repaint();
            }
        }

        private void Repaint() {

            timeSinceLastRepaint = 0f;

            latestProgress = progress;
            latestMessage = message;

            if (progress >= 1f) {
                Close();
                return;
            }

            if (cancelCallback != null) {
                if (EditorUtility.DisplayCancelableProgressBar(title, $"{message} {progress * 100}%", progress)) {
                    cancelCallback.Invoke();
                }
            } else {
                EditorUtility.DisplayProgressBar(title, $"{message} {progress * 100}%", progress);
            }
        }
    }

#if UNITY_2020_1_OR_NEWER
    public class BackgroundProgressBar
    {
        private VoidHandler cancelCallback;

        private float latestProgress = 0f;
        private float progress = 0f;

        private string latestMessage = null;
        private string message = "";

        private float timeSinceLastRepaint;

        private bool closed = false;
        private string title;

        private int progressID = -1;

        public BackgroundProgressBar(VoidHandler cancelCallback, string title = "Processing...")
        {
            this.cancelCallback = cancelCallback;
            this.title = title;

            progressID = Progress.Start(title);
            if (cancelCallback != null)
                Progress.RegisterCancelCallback(progressID, () => { cancelCallback.Invoke(); return true; });

            Dispatcher.OnUpdate += OnEditorUpdate;
        }

        public void SetProgress(float progress, string message)
        {
            this.progress = progress;
            this.message = message;
        }

        private void Close()
        {
            closed = true;

            if (Progress.Exists(progressID))
                Progress.Finish(progressID);

            progressID = -1;

            Dispatcher.OnUpdate -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (closed)
                return;

            timeSinceLastRepaint += Time.deltaTime;

            if (latestProgress != progress
             || latestMessage != message
             || timeSinceLastRepaint > 1f)
            {
                Repaint();
            }
        }

        private void Repaint()
        {
            timeSinceLastRepaint = 0f;

            latestProgress = progress;
            latestMessage = message;

            if (progress >= 1f)
            {
                Close();
                return;
            }

            Progress.Report(progressID, progress, $"{message} {progress * 100}%");
        }
    }
#endif
}