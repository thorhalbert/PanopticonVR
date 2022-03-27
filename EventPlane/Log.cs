using PanopticonEventPlaneOperations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventPlane
{
    public class Log
    {
        public static async void UnityConsole(string str)
        {
            var dto = DateTimeOffset.Now;
         
            var logRec = new MainMessages
            {
                LogMessage = new LogMessages
                {
                    LogType = LogTypes.Console,
                    Message = str,
                    StackTrace = String.Empty
                }
            };
            await EventPlane.StartMessageMesh.SendMessage(logRec);
        }
    }
}
