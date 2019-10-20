using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualMachineManager.Services
{
    public class Logger
    {
        public static string Indent { get; private set; } = "";
        private static event Action<string> output;

        private static Stack<DateTime> timeStack = new Stack<DateTime>();

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
            output?.Invoke($"{Indent}{sectionName}\n");
            IncreaseIndent();
        }

        public static void LogMessage(string message)
        {
            output?.Invoke($"{Indent}{message}\n");
        }

        public static void StartAction(string name)
        {
            timeStack.Push(DateTime.Now);
            output?.Invoke($"{Indent}{name}...");
        }

        public static void EndAction()
        {
            var time = DateTime.Now - timeStack.Pop();
            output?.Invoke($"{time.Milliseconds}ms\n");
        }

        public static void EndProccess(string sectionName, string result = "done")
        {
            var time = DateTime.Now - timeStack.Pop();
            DecreaseIndent();
            output?.Invoke($"{Indent}{sectionName} - {result} |{time.Milliseconds}ms|\n");
        }
        #endregion

        #region private methods
        private static void IncreaseIndent()
        {
            Indent += '\t';
        }

        private static void DecreaseIndent()
        {
            Indent = Indent.Substring(0, Indent.Length - 1);
        }
        #endregion
    }
}
