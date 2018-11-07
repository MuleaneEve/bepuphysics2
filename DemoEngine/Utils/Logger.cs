using System;

namespace DemoEngine.Utils
{
    public static class Logger
    {
#if DEBUG
        public const bool IsDebug = true;
#else
        public const bool IsDebug = false;
#endif

        public static void Log(object sender, string message, params object[] objs)
        {
            var text = "";
            if (objs != null)
                foreach (var o in objs)
                    text += (o ?? "<null>") + Environment.NewLine;

            var s = sender == null ? "" : sender.GetType().Name + " - ";
            var msg = DateTime.Now.ToString("u") + ": " + s + message + Environment.NewLine + text;
            Console.Write(msg);
            if (!Console.IsOutputRedirected) // This app has an actual Console window, so log a copy into the debugger (if it is attached)
                System.Diagnostics.Debug.Write(msg);
        }
    }
}