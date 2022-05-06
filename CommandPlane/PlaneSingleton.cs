using MQTTnet;
using MQTTnet.Client;
using PanopticonAPIs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
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
        public static PlaneSingleton? PlaneSingletonInstance = null;

        public string DeviceModel { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string DeviceUniqueIdentifier { get; set; }
        public string LoadedDeviceName { get; set; }

        private string rawString;

        public string MQTT_Broker { get; private set; }
        public int MQTT_Port { get; private set; }
        public string MQTT_User { get; private set; }
        public string MQTT_Password { get; private set; }

        public string Kafka_Broker { get; private set; }
        public string Kafka_Schema { get; private set; }

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

        public async void Start()
        {
            if (PlaneSingletonInstance != null)
                Console.WriteLine("PlaneSingletonInstance already constructed");
            else
                PlaneSingletonInstance = this;

            if (Started)
            {
                Console.WriteLine("PlaneSingleton already started");
                return;
            }

            LoadConfig();

            singleThread = new Thread(PlaneSingletonInstance.Process);
            KeepGoing = true;
            Started = true;
            singleThread.Start();

            await RawSendMessage("Test Startup!");

            await RawSendMessage($"DeviceName {DeviceName}");
            await RawSendMessage($"DeviceType {DeviceType}");
            await RawSendMessage($"DeviceUniqueIdentifier {DeviceUniqueIdentifier}");
            await RawSendMessage($"LoadedDeviceName {LoadedDeviceName}");

            await RawSendMessage($"Raw: {rawString}");

            //await RawSendMessage(rawString);

            await RawSendMessage($"Broker: {PlaneSingleton.PlaneSingletonInstance.MQTT_Broker}");
            await RawSendMessage($"User: {PlaneSingleton.PlaneSingletonInstance.MQTT_User}");
            await RawSendMessage($"Password: {PlaneSingleton.PlaneSingletonInstance.MQTT_Password}");
            await RawSendMessage($"Port: {PlaneSingleton.PlaneSingletonInstance.MQTT_Port}");
        }

        public void Stop()
        {
            KeepGoing = false;
        }

        // This runs in the new thread (when it exits the thread ends)
        private async void Process()
        {
            while (true)
            {
                try
                {
                    doPing();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in thread: {ex.Message}");
                }

                Thread.Sleep(30000);
            }
        }

        private void doPing()
        {
            var req = new HelloRequest()
            {
                Name = "oculus"
            };

            var ret = RPCWrapper.Call_Ping(req);
            Console.WriteLine($"Result: {ret.Result}");
        }

        private void LoadConfig()
        {
            var request = WebRequest.Create($"http://10.0.52.35/{DeviceUniqueIdentifier}.dat");
            var response = request.GetResponse();
            var dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);
            var responseFromServer = reader.ReadToEnd();
            rawString = responseFromServer;

            var vars = new Dictionary<string, string>();
            var delims = new[] { '\r', '\n' };
            var spls = responseFromServer.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            //var spls = responseFromServer.Split('\n');
            foreach (var s in spls)
                if (!String.IsNullOrWhiteSpace(s))
                {
                    var ps = s.Split('/');
                    vars.Add(ps[0].Trim(), ps[1].Trim());
                }

            string setR(string s)
            {
                if (vars.ContainsKey(s))
                    return vars[s];

                return null;
            }

            this.MQTT_Broker = setR("MQTT_Broker");
            var prt = setR("MQTT_Port");
            this.MQTT_Port = 1883;
            if (!String.IsNullOrWhiteSpace(prt))
                this.MQTT_Port = Convert.ToInt32(prt);
            this.MQTT_User = setR("MQTT_User");
            this.MQTT_Password = setR("MQTT_Password");

            //Console.WriteLine(responseFromServer);
        }

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

        public static MQTTnet.Client.IMqttClient MqttClient { get; private set; }

        public static async Task Connect()
        {
            MqttClient = SetupMQTT(out var options);
            if (MqttClient == null)
            {
                Console.WriteLine($"Can't Open MQTT");
                return;
            }

            Debug.WriteLine("MQTT: connecting");
            await MqttClient.ConnectAsync(options, CancellationToken.None);
            Debug.WriteLine("MQTT: connected");
        }

        private static MQTTnet.Client.IMqttClient? SetupMQTT(out MqttClientOptions options)
        {

            var factory = new MqttFactory();
            var MqttClient = factory.CreateMqttClient();

            string clientId = Guid.NewGuid().ToString();

            if (PlaneSingleton.PlaneSingletonInstance == null)
            {
                options = new MqttClientOptions();
                return null;
            }

            var mqttURI = PlaneSingleton.PlaneSingletonInstance.MQTT_Broker;
            var mqttUser = PlaneSingleton.PlaneSingletonInstance.MQTT_User;
            var mqttPassword = PlaneSingleton.PlaneSingletonInstance.MQTT_Password;
            var mqttPort = PlaneSingleton.PlaneSingletonInstance.MQTT_Port;

            var mqttSecure = false;
            var messageBuilder = new MqttClientOptionsBuilder()
              .WithClientId(clientId)
              .WithCredentials(mqttUser, mqttPassword)
              .WithTcpServer(mqttURI, mqttPort)
              .WithCleanSession();
            options = mqttSecure
              ? messageBuilder
                .WithTls()
                .Build()
              : messageBuilder
                .Build();

            return MqttClient;
        }

        private static List<string> _msgFifo = new List<string>();

        public async static Task SendMessage(string msg)
        {
            //await RawSendMessage(msg);
            //return;

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
