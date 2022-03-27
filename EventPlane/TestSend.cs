//using KafkaNet;
//using KafkaNet.Model;
//using KafkaNet.Protocol;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace EventPlane
//{
//    public class TestSend
//    {
      
//        public static void Send()
//        {
//            var options = new KafkaOptions(new Uri("http://10.0.52.208:9092"));
//            var router = new BrokerRouter(options);

//            using (Producer client = new Producer(router))
//            {
//                // Let's keep the key regulated so we don't have to tombstone a bunch of crap later
//                var msg = new Message("body message", new Guid("0f75f72c-a474-11ec-a1d0-b7018a187ce4").ToString());
//                client.SendMessageAsync("TestScene1", new[] { msg }).Wait();
//            }
//        }
//    }
//}
