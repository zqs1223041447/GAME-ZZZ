using UnityEngine;

namespace Game.Runtime.Core
{
    public static class GameLog
    {
        public static void Info(string tag, string message)
        {
            Debug.Log(Format(tag, message));
        }

        public static void Warn(string tag, string message)
        {
            Debug.LogWarning(Format(tag, message));
        }

        public static void Error(string tag, string message)
        {
            Debug.LogError(Format(tag, message));
        }

        static string Format(string tag, string message)
        {
            return "[" + tag + "] " + message;
        }
    }
}
