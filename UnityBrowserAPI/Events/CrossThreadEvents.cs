using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace UnityBrowserAPI.Events
{
    public delegate void DispatchProcess(CrossThreadEvent message); // Love to do generic  delegate, but not possible

    public class CrossThreadMessageSettings
    {
        public Type MessageType { get; private set; }
        public int MessageId { get; private set; }

        public CrossThreadEventServicer Servicer { get; private set; }
        public int ServicerId { get; private set; }

        public DispatchProcess Processor { get; private set; }

        public CrossThreadMessageSettings(CrossThreadEventServicer servicer, Type messageType, DispatchProcess process)
        {
            Servicer = servicer;
            ServicerId = Servicer.GetType().GetHashCode();

            MessageType = messageType;
            MessageId = MessageType.GetType().GetHashCode();

            Processor = process;
        }

    }

    public static class CrossThreadSingleton
    {

        // Wanted to use a priority queue here, but not in netstandard2.1, so we can abstract that later
        // I'll keep priorities
        private static Dictionary<int, Queue<CrossThreadEvent>> EventQueues;  // Key is servicer thread id
        private static object EventQueues_Lock = new object();      // Global Mutex
        // Idea behind priority tie-breaker was it would be 32 bit integer which would reset if the queue gets to zero

        private static Dictionary<int, CrossThreadMessageSettings> MessageTypes;
        private static object MessageTypes_Lock = new object();     // Global Mutex

        public static Dictionary<int, int> PerServicerHighWater { get; private set; }
        public static int Highwater { get; private set; }

        // We will have queues that need to run this way 
        public static int UnityThread { get; private set; }
        // How many to run in an update() -- might need finer grain someday
        public static int UnityMaxSlurp { get; private set; }

        static CrossThreadSingleton()
        {
            EventQueues = new Dictionary<int, Queue<CrossThreadEvent>>();
            MessageTypes = new Dictionary<int, CrossThreadMessageSettings>();
            PerServicerHighWater = new Dictionary<int, int>();
        }

        static void RegisterUnitySide(int maxSlurp)
        {
            UnityThread = Thread.CurrentThread.ManagedThreadId;
            UnityMaxSlurp = maxSlurp;
        }

        static void RegisterMessage(CrossThreadMessageSettings messageInfo)
        {
            lock (MessageTypes_Lock)
            {
                if (MessageTypes.ContainsKey(messageInfo.MessageId))
                    throw new ArgumentException("Duplicated Message Registry", "messageInfo");

                MessageTypes[messageInfo.MessageId] = messageInfo;
            }
        }

        public static void Enqueue(CrossThreadEvent inEvent)
        {
            var evi = inEvent.GetType().GetHashCode();

            CrossThreadMessageSettings msg;
            lock (MessageTypes_Lock)
            {
                if (!MessageTypes.ContainsKey(evi))
                    throw new ArgumentException("MessageType not registered", "inEvent");

                msg = MessageTypes[evi];
            }

            var servicer = msg.Servicer;
            var id = servicer.ServicerThreadId;

            int queueSize = 0;

            lock (EventQueues_Lock)
            {
                if (!EventQueues.ContainsKey(id))
                    EventQueues.Add(id, new Queue<CrossThreadEvent>());     // Initialize if needed

                // Again, wish this was a priority queue - maybe we can embed a sortedDictionary here
                EventQueues[id].Enqueue(inEvent);

                queueSize = EventQueues.Count;      // Start congestion mapping
            }

            if (queueSize > Highwater)
                Highwater = queueSize;

            if (!PerServicerHighWater.ContainsKey(id) || PerServicerHighWater[id] < queueSize)
                PerServicerHighWater[id] = queueSize;
        }

        public static bool Dispatch()
        {
            var servicerThreadId = Thread.CurrentThread.ManagedThreadId;

            CrossThreadEvent message;

            lock (EventQueues_Lock)
            {
                if (!EventQueues.ContainsKey(servicerThreadId)) return false;   // Nothing enqueued

                var queue = EventQueues[servicerThreadId];
                if (queue.Count < 1) return false;

                message = queue.Dequeue();

                // We're assuming fire/forget - if we need commit/rollback, we need something else
            }

            var recId = message.GetType().GetHashCode();

            var servicer = MessageTypes[recId];
            servicer.Processor(message);

            return true;    // There was a message
        }

    }

    /// <summary>
    /// Series of messages with one Servicer (assumed to be on one thread)
    /// </summary>

    public abstract class CrossThreadEventServicer
    {

        public int ServicerThreadId { get; private set; }
        // Not implemented yet, but act like it is
        //public long BasePriority { get; private set; }
        public uint PriorityTieBreaker { get; private set; }
        //public Type MessageType { get; private set; }
        public int EventTypeKey { get; private set; }

        public CrossThreadEventServicer()
        {
            ServicerThreadId = Thread.CurrentThread.ManagedThreadId;
            EventTypeKey = GetType().GetHashCode();
        }

        public int AddMessageType<T>(int basePriority = 0)
        {
            var mt = typeof(T).GetHashCode();

            return mt;
        }
    }

    /// <summary>
    /// Series of messages, intended for one servicer above
    /// </summary>
    public abstract class CrossThreadEvent
    {

    }
}
