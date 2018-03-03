using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public static class Logger
    {
        private static string _lastAction;
        private static string _indent = "";
        private static event Action<string> output;

        private static Stack<DateTime> timeStack = new Stack<DateTime>();

        #region private methods
        private static void IncreaseIndent()
        {
            _indent += '\t';
        }

        private static void DecreaseIndent()
        {
            _indent = _indent.Substring(0, _indent.Length - 1);
        }
        #endregion

        #region public methods
        public static void RegisterOutputChannels(params Action<string>[] channels)
        {
            foreach (var channel in channels)
            {
                output += channel;
            }
        }

        public static void StartProcess(string sectionName)
        {
            timeStack.Push(DateTime.Now);
            output($"{_indent}{sectionName}\n");
            IncreaseIndent();
        }

        public static void LogMessage(string message)
        {
            output($"{_indent}{message}\n");
        }

        public static void StartAction(string name)
        {
            _lastAction = name;
            timeStack.Push(DateTime.Now);
            output($"{_indent}{name}...");
        }

        public static void EndAction()
        {
            var time = DateTime.Now - timeStack.Pop();
            output($"{time.Milliseconds}ms\n");
        }

        public static void EndProccess(string sectionName, string result = "done")
        {
            var time = DateTime.Now - timeStack.Pop();
            DecreaseIndent();
            output($"{_indent}{sectionName} - {result} |{time.Milliseconds}ms|\n");
        }
        #endregion
    }
}
