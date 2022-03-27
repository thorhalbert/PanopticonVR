//using KafkaNet;
//using KafkaNet.Common;
//using KafkaNet.Model;
//using KafkaNet.Protocol;
//using Misakai.Kafka;
using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using Kafka.Public;
//using Kafka.Public.Loggers;

namespace EventPlane
{
    public class StartPlaneThread
    {
        public static StartPlaneThread? PlaneSingleton { get; private set; }
        public IProducer<string, string>? producer { get; private set; }

        private static bool Started = false;
        private static Thread? singleThread = null;
        private static bool KeepGoing = false;

        private static string SceneProcessor = "TestScene1";
        private const string broker = "10.0.52.35:9092";

        // Should be able to call this again after a Stop()
        public void Start()
        {
            if (PlaneSingleton != null)
                Console.WriteLine("PlaneSingleton already constructed");
            else
                PlaneSingleton = new StartPlaneThread();

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

            var config = new ProducerConfig { BootstrapServers = broker, EnableDeliveryReports=false };

            // If serializers are not specified, default serializers from
            // `Confluent.Kafka.Serializers` will be automatically used where
            // available. Note: by default strings are encoded as UTF8.
            producer = new ProducerBuilder<string, string>(config).Build();

            await ProduceTest("startup", "EventPlane Startup");

            await doAllConsumeAsync();
        }

        private async Task ProduceTest(string key, string value)
        {
            if (producer == null) return;

            try
            {
                var dr = await producer.ProduceAsync(SceneProcessor, new Message<string, string> { Value = value, Key = key }); ;
                Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<string, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            }
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

            using (var c = new ConsumerBuilder<string, string>(conf).Build())
            {
                c.Subscribe(SceneProcessor);

                CancellationTokenSource cts = new CancellationTokenSource();
                //Console.CancelKeyPress += (_, e) => {
                //    e.Cancel = true; // prevent the process from terminating.
                //    cts.Cancel();
                //};

                try
                {
                    while (true)
                    {
                        try
                        {
                            var cr = c.Consume(cts.Token);
                            var key = cr.Message.Key;
                            var value = cr.Message.Value;
                            Console.WriteLine($"Consumed message '{key}/{value}' at: '{cr.TopicPartitionOffset}'.");

                            if (!string.IsNullOrEmpty(key) && key == "test")
                            {
                                await ProduceTest("browser", $"Bounce: {value}");
                            }
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
        }

        //private async void Process_kafka_net()
        //{
        //    // This probably is also part of the startup -- different processors for each scene, that or totally different docker

        //    var options = new KafkaOptions(new Uri($"http://{broker}")) { Log = new kLogger() };

        //    var router = new BrokerRouter(options);

        //    sendMessage(router, "Test message");

        //    //await Task.Run(() =>
        //    //{
        //        var consumer = new Consumer(new ConsumerOptions(SceneProcessor, router) { Log = new kLogger() });

        //        // Consume returns a blocking IEnumerable (ie: never ending stream)
        //        foreach (var message in consumer.Consume())
        //        {
        //            try
        //            {
        //                doConsume(router, message);
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"Got error {ex.Message}");
        //            }

        //            //if (!KeepGoing) break;
        //        }

        //        Console.WriteLine("[Consume Loop Exits]");


        //    while (KeepGoing)
        //        Thread.Sleep(1000);
        //}

        //private static void Process_kafka_sharp()
        //{
        //var cluster = new ClusterClient(new Configuration
        //{
        //    Seeds = broker,
        //    ClientId = Guid.NewGuid().ToString(),
        //    Compatibility = Compatibility.V0_11_0,
        //    ErrorStrategy = ErrorStrategy.Discard,

        //}, new kLogger());

        //cluster.Produce(SceneProcessor,
        //    Encoding.ASCII.GetBytes("browser"),
        //    Encoding.ASCII.GetBytes("test from browser"));



        //// cluster.Messages.Where(kr => kr.Topic == capturedTopic)//.Sample(TimeSpan.FromMilliseconds(15))
        ////   .Subscribe(kr => Console.WriteLine("{0}/{1} {2}: {3}", kr.Topic, kr.Partition, kr.Offset, kr.Value as string));

        //cluster.MessageReceived += kafkaRecord =>
        //{
        //    try
        //    {
        //        //var key = kafkaRecord.Key;
        //        //var value = kafkaRecord.Value;

        //        Console.WriteLine("MessageReceived got message");

        //        var key = Encoding.ASCII.GetString((byte[])kafkaRecord.Key);
        //        var value = Encoding.UTF8.GetString((byte[])kafkaRecord.Value);
        //        Console.WriteLine($"Got message {value}");
        //        if (key != null && key == "test")
        //            cluster.Produce(SceneProcessor, Encoding.ASCII.GetBytes("browser"),
        //                Encoding.ASCII.GetBytes($"Return {value}"));
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Got error {ex.Message}");
        //    }
        //};

        //cluster.Subscribe("SceneConsumerGroup1",
        //    new[] { SceneProcessor },
        //    new ConsumerGroupConfiguration
        //    {
        //        SessionTimeoutMs = 30000,
        //        RebalanceTimeoutMs = 20000,
        //        DefaultOffsetToReadFrom = Offset.Latest,
        //        AutoCommitEveryMs = 5000
        //    });

        //cluster.Messages.Subscribe(kr => Console.WriteLine($"Message: { kr.Topic}/{kr.Partition} {kr.Offset}: {Encoding.UTF8.GetString((byte[])kr.Value)}"));
        //}

  

        //private void doConsume(BrokerRouter router, Message message)
        //{
        //    try
        //    {
        //        var key = Encoding.ASCII.GetString(message.Key);
        //        var value = Encoding.ASCII.GetString(message.Value);
        //        Console.WriteLine($"Response: {key}/{value}");
        //        if (key == "test")  // We don't want to be responding to our own message
        //                            sendMessage(router, $"Bounce: {value}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error consuming message: {ex.Message}");
        //    }
        //}

        //private void sendMessage(BrokerRouter router, string msgVal)
        //{
        //    using (Producer client = new Producer(router))
        //    {
        //        // Let's keep the key regulated so we don't have to tombstone a bunch of crap later
        //        var msg = new Message(msgVal, new Guid("9722e3d6-a499-11ec-90b3-ffc5787a376b").ToString());
        //        client.SendMessageAsync(SceneProcessor, new[] { msg }).Wait();
        //    }
        //}
    }
}
