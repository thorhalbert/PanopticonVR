using System;

namespace UnityBrowserAPI.Events.Entities
{
    public abstract class ReporterBase : CrossThreadEvent
    {
        private static int _Sequence = 1;

        public DateTimeOffset ReportTime { get; private set; }
        public int ReportSequence { get; private set; }

        protected ReporterBase()
        {
            ReportSequence = _Sequence++;
            ReportTime = DateTimeOffset.Now;
        }
    }
}
