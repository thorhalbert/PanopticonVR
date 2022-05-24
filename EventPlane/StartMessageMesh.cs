
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using PanopticonEventPlaneOperations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace EventPlane
{
    public class StartMessageMesh
    {
        public static StartMessageMesh? PlaneSingleton { get; private set; }
        public IProducer<string, MainMessages>? producer { get; private set; }
        internal ProtobufSerializer<MainMessages>? mainMessageSerializer { get; private set; }

        private static bool Started = false;
        private static Thread? singleThread = null;
        private static bool KeepGoing = false;

        //private static string SceneProcessor = "TestScene1_msgs";
        //private const string broker = "10.0.52.35:9092";
        //private const string schemaUrl = "http://10.0.52.35:8081";

        private static string SceneProcessor  { get { return ControlPlane.PlaneSingleton.PlaneSingletonInstance.Kafka_Initial;    }}
        private static string broker { get { return ControlPlane.PlaneSingleton.PlaneSingletonInstance.Kafka_Broker; } }
        private static  string schemaUrl { get { return ControlPlane.PlaneSingleton.PlaneSingletonInstance.Kafka_Schema; } }

        private int sequence = 0;

        public Guid SessionGuid { get; private set; }
        internal ProtobufDeserializer<MainMessages>? mainMessageDeserializer { get; private set; }

        // Should be able to call this again after a Stop()
        public void Start()
        {
            if (PlaneSingleton != null)
                Console.WriteLine("PlaneSingleton already constructed");
            else
            {
                PlaneSingleton = this;
                SessionGuid = Guid.NewGuid();
            }

            if (Started)
            {
                Console.WriteLine("PlaneSingleton already started");
                return;
            }

            singleThread = new Thread(PlaneSingleton.Process);
            KeepGoing = true;
            Started = true;
            singleThread.Start();
        }

        public void Stop()
        {
            KeepGoing = false;
            
            // Need to stop the other thread
        }

        public void SwitchScenes()
        {
        }

        private async void Process(object obj)
        {
            try
            {
                Console.WriteLine($"Start new thread to process messages for {SceneProcessor}");

                await confluentkafka();

                while (KeepGoing)
                    Thread.Sleep(1000);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Got error in thread: {ex.Message} {ex.StackTrace}");
            }

            Console.WriteLine($"End thread to process messages for {SceneProcessor}");

            Started = false;
        }


        private async Task confluentkafka()
        {
            // You get an IL2CPP fail if you set EnableDeliveryReports to true
            // I started to write a static delegate that this would fire, but I don't know what we
            // need from that yet.   It's most assures us a message does arrive on the broker.
            // This really isn't transactional yet, we may not need that.

            // Unity might fix this for us in IL2CPP eventually.

     
            var serSchemaConf = new SchemaRegistryConfig {Url=schemaUrl,  };
            var serSchema = new CachedSchemaRegistryClient(serSchemaConf);
      
            var proser = new ProtobufSerializerConfig { AutoRegisterSchemas = true };
            mainMessageSerializer = new ProtobufSerializer<MainMessages>(serSchema, proser);

            var prodes = new ProtobufDeserializerConfig();
            mainMessageDeserializer = new ProtobufDeserializer<MainMessages>(prodes);

            var config = new ProducerConfig { BootstrapServers = broker, EnableDeliveryReports = false };

            // If serializers are not specified, default serializers from
            // `Confluent.Kafka.Serializers` will be automatically used where
            // available. Note: by default strings are encoded as UTF8.
            producer = new ProducerBuilder<string, MainMessages>(config)
                .SetValueSerializer(mainMessageSerializer)           
                .Build();
            
            //await ProduceTest("startup", "EventPlane Startup");

            await doAllConsumeAsync();
        }

        public async static Task SendMessage(MainMessages message)
        {
            if (PlaneSingleton == null) return;
            if (PlaneSingleton.producer == null) return;

            //var servicePool = new System.Collections.Concurrent.ConcurrentQueue<MainMessages>();
            //servicePool.Enqueue(message);

            var now = DateTimeOffset.Now;

            message.TimeTicks = now.Ticks;
            message.OffsetTicks = now.Offset.Ticks;

            message.SessionSequence = PlaneSingleton.sequence++;

            var dr = await PlaneSingleton.producer.ProduceAsync(SceneProcessor,
                new Message<string, MainMessages> { Value = message, Key = PlaneSingleton.SessionGuid.ToString() });
            Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
        }

        private async Task doAllConsumeAsync()
        {
            var conf = new ConsumerConfig
            {
                GroupId = "TaskConsumerGroup",
                BootstrapServers = broker,
                // Note: The AutoOffsetReset property determines the start offset in the event
                // there are not yet any committed offsets for the consumer group for the
                // topic/partitions of interest. By default, offsets are committed
                // automatically, so in this example, consumption will only start from the
                // earliest message in the topic 'my-topic' the first time you run the program.
                AutoOffsetReset = AutoOffsetReset.Latest
            };

            using var c = new ConsumerBuilder<string, MainMessages>(conf)
                .SetValueDeserializer(mainMessageDeserializer.AsSyncOverAsync())
                               .Build();

            c.Subscribe(SceneProcessor);

            CancellationTokenSource cts = new CancellationTokenSource();
            //Console.CancelKeyPress += (_, e) => {
            //    e.Cancel = true; // prevent the process from terminating.
            //    cts.Cancel();
            //};

            try
            {
                while (KeepGoing)
                {
                    try
                    {
                        var cr = c.Consume(cts.Token);
                        var key = cr.Message.Key;
                        var value = cr.Message.Value;

                        //  if (key == SessionGuid) return;  // Don't need to see our own messages

                        await ProcessMessage(cr, key, value);
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Error occured: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                c.Close();
            }
        }

        private async Task ProcessMessage(ConsumeResult<string, MainMessages> cr, string key, MainMessages value)
        {
            Console.WriteLine($"Consumed message '{key}/{value}' at: '{cr.TopicPartitionOffset}'.");
        }
    }
}
