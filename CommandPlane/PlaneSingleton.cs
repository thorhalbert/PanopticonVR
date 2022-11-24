using Grpc.Core;
//using MQTTnet;
//using MQTTnet.Client;
using ProtobufRepo;
//using PanopticonAPIs;
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

        public async static Task  DebugCallback(string condition, string stacktrace, int type)
        {
            if (PlaneSingleton.PlaneSingletonInstance == null) return;
         //   await PlaneSingleton.PlaneSingletonInstance.SendMessage($"Oculus: {type} {condition} {stacktrace}");
        }

        public static async Task UnityConsole(string str)
        {
            if (PlaneSingleton.PlaneSingletonInstance == null) return;
          //  await PlaneSingleton.PlaneSingletonInstance.SendMessage($"Oculus: {str}");
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

        public Guid InstanceId { get; set; } = Guid.NewGuid();
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
        public string Kafka_Initial { get; private set; }

        public string Panopticon_Server { get; private set; }
        public int Panopticon_Port { get; private set; }

        public Thread singleThread { get; private set; }
        public bool KeepGoing { get; private set; }
        public bool Started { get; private set; }

        public async static void Test(string str)
        {
          //  await SendMessage($"Report from Occulus: {str}");
            //foreach (var v in values)
            //    await SendMessage($"Value: v");

            //var vars = Environment.GetEnvironmentVariables();       // .net 1.0 collection

            //foreach (DictionaryEntry e in vars)
            //    await SendMessage($"Env: {e.Key}={e.Value}");
        }

        public async Task Start()
        {
            Console.WriteLine("PlantSingleton starting up");

            if (PlaneSingletonInstance != null)
                Debug.WriteLine("PlaneSingletonInstance already constructed");
            else
                PlaneSingletonInstance = this;

            if (Started)
            {
                Debug.WriteLine("PlaneSingleton already started");
                return;
            }

            LoadConfig();

            //await Connect();
       
            //singleThread = new Thread(PlaneSingletonInstance.Process);
            //KeepGoing = true;
            //Started = true;
          
            //await RawSendMessage("Test Startup!");

            //await RawSendMessage($"DeviceName {DeviceName}");
            //await RawSendMessage($"DeviceType {DeviceType}");
            //await RawSendMessage($"DeviceUniqueIdentifier {DeviceUniqueIdentifier}");
            //await RawSendMessage($"LoadedDeviceName {LoadedDeviceName}");

            //await RawSendMessage($"Raw: {rawString}");

            //await RawSendMessage(rawString);

            //await RawSendMessage($"Broker: {PlaneSingleton.PlaneSingletonInstance.MQTT_Broker}");
            //await RawSendMessage($"User: {PlaneSingleton.PlaneSingletonInstance.MQTT_User}");
            ////await RawSendMessage($"Password: {PlaneSingleton.PlaneSingletonInstance.MQTT_Password}");
            //await RawSendMessage($"Port: {PlaneSingleton.PlaneSingletonInstance.MQTT_Port}");

            singleThread.Start();
        }

        public async Task Stop()
        {
            KeepGoing = false;
        }

        private bool ExitThread = false;
        // This runs in the new thread (when it exits the thread ends)
        private void Process()
        {
            while (true)
            {
                try
                {
                    Debug.WriteLine($"Start Ping");
                    doPing().Wait();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in thread: ");
                    Log.DebugCallback(ex.Message, ex.StackTrace, 0).Wait();
                }
                finally
                {
                    Debug.WriteLine($"End Ping");
                    Thread.Sleep(60000);
                }            
            }
        }

        static int iter = 1;

        private async Task doPing()
        {
            if (PlaneSingleton.PlaneSingletonInstance == null) return;

            var channel = new Channel(Panopticon_Server, Panopticon_Port, ChannelCredentials.Insecure);
        
            var ping = new PanopticonService.DoPing.DoPingClient(channel);
            var result = ping.Ping(new PanopticonService.PingRequest
            {
                UniqueDeviceId = PlaneSingleton.PlaneSingletonInstance.DeviceUniqueIdentifier,
                InstanceId = PlaneSingleton.PlaneSingletonInstance.InstanceId.GetUuid(),
                Iteration = iter++,
                Current = DateTimeOffset.Now.GetDTO(),
            });

            // These can change dynamically (server could hand us off)
            Panopticon_Server = result.PanopticonServer;
            Panopticon_Port = result.PanopticonPort;
            Kafka_Broker = result.KafkaBroker;
            Kafka_Schema = result.KafkaSchema;

          //  await RawSendMessage($"(Browser) Result: {result.ServerName} {result.InstanceId} {result.Iteration} {result.Current.GetDate()}");
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

            string setR(string s, string def = null)
            {
                if (vars.ContainsKey(s))
                    return vars[s];

                return def;
            }

            this.MQTT_Broker = setR("MQTT_Broker");
            this.MQTT_Port = Convert.ToInt32(setR("MQTT_Port", "1883"));
            this.MQTT_User = setR("MQTT_User");
            this.MQTT_Password = setR("MQTT_Password");

            this.Kafka_Broker = setR("Kafka_Broker");
            this.Kafka_Schema = setR("Kafka_Schema");
            this.Kafka_Initial = setR("Kafka_Initial");

            this.Panopticon_Server = setR("Panopticon_Server", "10.0.52.35");
            this.Panopticon_Port = Convert.ToInt32(setR("Panopticon_Port", "10080"));
   
            //Debug.WriteLine(responseFromServer);
        }


        //public MQTTnet.Client.IMqttClient MqttClient { get; private set; }

        //public async Task Connect()
        //{
        //    Console.WriteLine("MqttClient starting up");
        //    MqttClient = await GetNewClient();

        //    if (MqttClient.IsConnected)
        //         Debug.WriteLine("MQTT: connected and up");
        //}

        //public async Task<MQTTnet.Client.IMqttClient> GetNewClient()
        //{
        //   var mqttClient = SetupMQTT(out var options);
        //    if (mqttClient == null)
        //    {
        //        Debug.WriteLine($"Can't Open MQTT");
        //        return null;
        //    }

        //    //Debug.WriteLine("MQTT: connecting");
        //    await mqttClient.ConnectAsync(options, CancellationToken.None);
        //    //Debug.WriteLine("MQTT: connected");

        //    return mqttClient;
        //}

        //private MQTTnet.Client.IMqttClient? SetupMQTT(out MqttClientOptions options)
        //{

        //    var factory = new MqttFactory();
        //    var MqttClient = factory.CreateMqttClient();

        //    string clientId = Guid.NewGuid().ToString();

        //    if (PlaneSingleton.PlaneSingletonInstance == null)
        //    {
        //        options = new MqttClientOptions();
        //        return null;
        //    }

        //    var mqttURI = PlaneSingleton.PlaneSingletonInstance.MQTT_Broker;
        //    var mqttUser = PlaneSingleton.PlaneSingletonInstance.MQTT_User;
        //    var mqttPassword = PlaneSingleton.PlaneSingletonInstance.MQTT_Password;
        //    var mqttPort = PlaneSingleton.PlaneSingletonInstance.MQTT_Port;

        //    var mqttSecure = false;
        //    var messageBuilder = new MqttClientOptionsBuilder()
        //      .WithClientId(clientId)
        //      .WithCredentials(mqttUser, mqttPassword)
        //      .WithTcpServer(mqttURI, mqttPort)
        //      .WithCleanSession();
        //    options = mqttSecure
        //      ? messageBuilder
        //        .WithTls()
        //        .Build()
        //      : messageBuilder
        //        .Build();

        //    return MqttClient;
        //}

        //private static List<string> _msgFifo = new List<string>();

        //public async Task SendMessage(string msg)
        //{
        //    await RawSendMessage(msg);
        //    return;

        //    if (msg != null)
        //        lock (_msgFifo)
        //            _msgFifo.Add(msg);

        //    while (true)
        //    {
        //        string post=null;
        //        lock (_msgFifo)
        //        {
        //            post = null;
        //            if (_msgFifo.Count > 0)
        //                post = _msgFifo[0];
        //            else break;
        //        }

        //        if (!await RawSendMessage(post)) break;

        //        lock (_msgFifo)
        //            if (post!=null)
        //            _msgFifo.RemoveAt(0);
        //    }
        //}

        //public async Task<bool> RawSendMessage(string msg)
        //{
        //    if (MqttClient == null)
        //        await Connect();

        //    if (MqttClient == null)
        //    {
        //        //Debug.WriteLine("Mqtt Client not created yet?");
        //        return false;
        //    }
        //    if (!MqttClient.IsConnected) {
        //        //Debug.WriteLine("Mqtt Client not connected yet");
        //        return false;
        //    }

        //    var message = new MqttApplicationMessageBuilder()
        //           //here it does send the message
        //           .WithTopic("Logging")
        //           .WithPayload(msg)
        //           .Build();
        //    await MqttClient.PublishAsync(message);

        //    return true;
        //}
    }
}
