using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mqtt;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ControlPlane
{
    public class Log
    {

        public async static void  DebugCallback(string condition, string stacktrace, int type)
        {
            await PlaneSingleton.SendMessage($"Oculus: {type} {condition}");
        }

        public static async void UnityConsole(string str)
        {
            await PlaneSingleton.SendMessage($"Oculus: {str}");
            //var dto = DateTimeOffset.Now;

            //var logRec = new MainMessages
            //{
            //    LogMessage = new LogMessages
            //    {
            //        LogType = LogTypes.Console,
            //        Message = str,
            //        StackTrace = String.Empty
            //    }
            //};
            // await EventPlane.StartMessageMesh.SendMessage(logRec);
        }
    }
    public class PlaneSingleton
    {
        public PlaneSingleton? PlaneSingletonInstance = null;

        public string DeviceModel { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string DeviceUniqueIdentifier { get; set; }
        public string LoadedDeviceName { get; set; }
        public Thread singleThread { get; private set; }
        public bool KeepGoing { get; private set; }
        public bool Started { get; private set; }

        public async static void Test(string str)
        {
            await SendMessage($"Report from Occulus: {str}");
            //foreach (var v in values)
            //    await SendMessage($"Value: v");

            //var vars = Environment.GetEnvironmentVariables();       // .net 1.0 collection

            //foreach (DictionaryEntry e in vars)
            //    await SendMessage($"Env: {e.Key}={e.Value}");
        }

        public void Start()
        {
            if (PlaneSingletonInstance != null)
                Console.WriteLine("PlaneSingletonInstance already constructed");
            else
                PlaneSingletonInstance = new PlaneSingleton();

            if (Started)
            {
                Console.WriteLine("PlaneSingleton already started");
                return;
            }

            singleThread = new Thread(PlaneSingletonInstance.Process);
            KeepGoing = true;
            Started = true;
            singleThread.Start();
        }

        public void Stop()
        {
            KeepGoing = false;
        }

        private async void Process(object obj)
        { }

        //public static async Task<bool> Publish(string channel, string value)
        //{
        //    var factory = new MqttFactory();
        //    var mqttClient = factory.CreateMqttClient();

            //    if (mqttClient.IsConnected == false)
            //    {
            //        Debug.WriteLine("publishing failed");
            //        return false;
            //    }

            //    var message = new MqttApplicationMessageBuilder()
            //            .WithTopic(channel)
            //            .WithPayload(value)
            //            .WithRetainFlag()
            //            .Build();
            //    await mqttClient.PublishAsync(message);
            //    return true;
            //}


            ////connect to mqtt

        private static MQTTnet.Client.IMqttClient MqttClient = null;

        public static async Task Connect()
        {
            var factory = new MqttFactory();
            MqttClient = factory.CreateMqttClient();

            string clientId = Guid.NewGuid().ToString();

            string mqttURI = "10.0.52.35";
            string mqttUser = "thor";
            string mqttPassword = "pass";
            int mqttPort = 1883;
            
            bool mqttSecure = false;
            var messageBuilder = new MqttClientOptionsBuilder()
              .WithClientId(clientId)
              .WithCredentials(mqttUser, mqttPassword)
              .WithTcpServer(mqttURI, mqttPort)
              .WithCleanSession();
            var options = mqttSecure
              ? messageBuilder
                .WithTls()
                .Build()
              : messageBuilder
                .Build();

            Debug.WriteLine("MQTT: connecting");
            await MqttClient.ConnectAsync(options, CancellationToken.None);
            Debug.WriteLine("MQTT: connected");

           // MqttClient = mqttClient;
        }

        private static List<string> _msgFifo = new List<string>();

        public async static Task SendMessage(string msg)
        {
            if (msg != null)
                lock (_msgFifo)
                    _msgFifo.Add(msg);

            while (true)
            {
                string post;
                lock (_msgFifo)
                    if (_msgFifo.Count > 0)
                        post = _msgFifo[0];
                    else break;

                if (!await RawSendMessage(post)) break;

                lock (_msgFifo)
                    _msgFifo.RemoveAt(0);
            }
        }

        public static async Task<bool> RawSendMessage(string msg)
        {
            if (MqttClient == null)
                await Connect();

            if (MqttClient == null)
            {
                //Console.WriteLine("Mqtt Client not created yet?");
                return false;
            }
            if (!MqttClient.IsConnected) {
                //Console.WriteLine("Mqtt Client not connected yet");
                return false;
            }

            var message = new MqttApplicationMessageBuilder()
                   //here it does send the message
                   .WithTopic("Logging")
                   .WithPayload(msg)
                   .Build();
            await MqttClient.PublishAsync(message);

            return true;
        }
    }
}
