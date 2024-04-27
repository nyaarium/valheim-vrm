using System;
using UnityEngine;

namespace ValheimVRM
{
    public class TimeTracker
    {
        private static DateTime startTime;
        private static DateTime lastAccessTime;
        private static bool isStartTimeSet = false;

        public static void SetStartTime()
        {
            startTime = DateTime.UtcNow;
            lastAccessTime = DateTime.UtcNow;
            isStartTimeSet = true;
        }

        public static string GetElapsedTime()
        {
            if (!isStartTimeSet)
            {
                Debug.LogError("Start time not set. Call SetStartTime() before getting elapsed time.");
                return "Error: Start time not set.";
            }

            DateTime currentTime = DateTime.UtcNow;
            TimeSpan timeSinceLastAccess = currentTime - lastAccessTime;
            TimeSpan totalTimeElapsed = currentTime - startTime;
        
            lastAccessTime = currentTime; 

            string timeSinceLastAccessFormatted = $"{Math.Round(timeSinceLastAccess.TotalMilliseconds, 2)}ms";
            string totalTimeElapsedFormatted = $"{Math.Round(totalTimeElapsed.TotalSeconds, 2)}s";

            return $" Time From Last: {timeSinceLastAccessFormatted} | Total Time Elapsed: {totalTimeElapsedFormatted}";
        }
    }
}