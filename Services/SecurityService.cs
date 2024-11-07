using PFMS_MI04.Models;
using PFMS_MI04.Models.Security;

namespace PFMS_MI04.Services
{
    public static class SecurityService
    {
        private static readonly string[] validLogTypes = { "AUTH", "ACCESS", "API", "ERROR", "CHANGE", "SECURITY" };
        private static readonly int logPushDelay = 300000; // 5 Minutes
        private static readonly int logPushDelayTemp = 60000; // 1 Minute
        private static List<SecLog> secLogs = new List<SecLog>();
        private static SecLoggingService secLoggingService = new SecLoggingService();
        private static Thread loggerThread;
        private static bool threadOn = false;

        public static bool Log(string userID, string message, string logType, string atPage,
            [System.Runtime.CompilerServices.CallerMemberName] string sourceFunc = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            if (!threadOn)
            {
                StrComparator.println("Start Logger Thread");
                startLoggerService();
                threadOn = true;
            }

            bool recorded = false;
            if (validLogTypes.Contains(logType))
            {
                SecLog log = new SecLog(userID);
                string filename = Path.GetFileName(sourceFilePath);
                log.setMsg(message, atPage, sourceFunc, filename, logType);
                secLogs.Add(log);
                recorded = true;
                StrComparator.println(log.ToString());
            }
            else
            {
                throw new InvalidDataException("LogType is Invalid! | Inputted:[" + logType + "]");
            }
            return recorded;
        }

        // ----- Logger Thread ----- //

        private static void startLoggerService()
        {
            loggerThread = new Thread(new ThreadStart(pushCurrentLogs));
            loggerThread.Start();
        }

        private static void stopLoggerService()
        {
            threadOn = false;
            loggerThread.Join();
        }

        private static void pushCurrentLogs()
        {
            while (threadOn)
            {
                bool inserted = false;
                try
                {
                    Thread.Sleep(logPushDelay);
                    StrComparator.println("Attempt Log Upload");
                    if(secLogs.Count() > 0)
                    {
                        StrComparator.println("Uploading " + secLogs.Count() + " Logs");
                        inserted = secLoggingService.uploadSecLog(secLogs);
                        secLogs.Clear();
                    }
                }
                catch (ThreadAbortException taE)
                {
                    StrComparator.println("ThreadAbortException: " + taE.Message);
                }
                catch (ThreadInterruptedException tiE)
                {
                    StrComparator.println("ThreadInterruptedException: " + tiE.Message);
                }
            }
        }
    }
}
