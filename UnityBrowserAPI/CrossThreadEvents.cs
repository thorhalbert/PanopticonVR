using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace UnityBrowserAPI
{
    public delegate void DispatchProcess(CrossThreadEvent message);

    public class CrossThreadMessageSettings
    {
        public Type MessageType { get; private set; }
        public int MessageId { get; private set; }

        public CrossThreadEventType Servicer { get; private set; }
        public int ServicerId { get; private set; }

        public DispatchProcess Processor { get; private set; }

        public CrossThreadMessageSettings(CrossThreadEventType servicer, Type messageType, DispatchProcess process)
        {
            Servicer = servicer;
            ServicerId = Servicer.GetType().GetHashCode();

            MessageType = messageType;
            MessageId = MessageType.GetType().GetHashCode();

            Processor = process;
        }

    }

    public static class CrossThreadSingleton {

        // Wanted to use a priority queue here, but not in netstandard2.1, so we can abstract that later
        // I'll keep priorities
        private static Dictionary<int, Queue<CrossThreadEvent>> EventQueues;  // Key is servicer thread id
        private static object EventQueues_Lock = new object();
        // Idea behind priority tie-breaker was it would be 32 bit integer which would reset if the queue gets to zero

        private static Dictionary<int, CrossThreadMessageSettings> MessageTypes;
        private static object MessageTypes_Lock = new object();

        static CrossThreadSingleton()
        {
            EventQueues = new Dictionary<int, Queue<CrossThreadEvent>>();
            MessageTypes = new Dictionary<int, CrossThreadMessageSettings>();
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


        public static void Enqueue(CrossThreadEvent inEvent){
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
                    EventQueues.Add(id, new Queue<CrossThreadEvent>());

                // Again, wish this was a priority queue - maybe we can embed a sortedDictionary here
                EventQueues[id].Enqueue(inEvent);

                queueSize = EventQueues.Count;      // Start congestion mapping
            }                
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
            }

            var recId = message.GetType().GetHashCode();

            var servicer = MessageTypes[recId];
            servicer.Processor(message);

            return true;    // There was a message
        }

    }



    public abstract class CrossThreadEventType
    {

        public int ServicerThreadId { get; private set; }
        // Not implemented yet, but act like it is
        //public long BasePriority { get; private set; }
        public UInt32 PriorityTieBreaker { get; private set; }
        //public Type MessageType { get; private set; }
        public int EventTypeKey { get; private set; }

        public CrossThreadEventType()
        {
            ServicerThreadId = Thread.CurrentThread.ManagedThreadId;
            EventTypeKey = this.GetType().GetHashCode();
        }

        public int AddMessageType<T>(int basePriority)
        {
            var mt = typeof(T).GetHashCode();

            return mt;
        }
    }

    public abstract class CrossThreadEvent { 

    }
}
