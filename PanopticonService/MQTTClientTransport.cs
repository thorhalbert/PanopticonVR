using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.AspNetCore;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PanopticonService
{
    public class MQTTClientTransport : IHostedService
    {
        private readonly ManagedMqttClientOptions options;
        private ManagedMqttClient mqttClient;
        private Dictionary<string, RPCHandler> Routes { get; } = new Dictionary<string, RPCHandler>();

        public MQTTClientTransport()
        {
            // Exposed to docker - we should do some fancy dependancy injection stuff here
            var clientid = Environment.GetEnvironmentVariable("MQTT_CLIENTID");
            var server = Environment.GetEnvironmentVariable("MQTT_BROKER");
            var port = Convert.ToInt32(Environment.GetEnvironmentVariable("MQTT_PORT"));
            var username = Environment.GetEnvironmentVariable("MQTT_USER");
            var password = Environment.GetEnvironmentVariable("MQTT_PASSWORD");

            // Setup and start a managed MQTT client.
            options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(
                new MqttClientOptionsBuilder()
                    .WithClientId(clientid)
                    .WithTcpServer(server, port)
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .WithCredentials(username, password)
                    //.WithAuthentication("huh? method?", Array.Empty<byte>())
                    .Build())
                .Build();

            SetupRoutes();
        }

        private void SetupRoutes()
        {
    
          //  RPCHandler.Setup(Routes, "ping", do_ping);
        }

        //public async static Task<Byte[]> do_ping(Byte[] rawReq)
        //{
        //    var request = HelloRequest.Parser.ParseFrom(rawReq);

        //    Console.WriteLine($"Got Ping: {request.Name}");

        //    var response = new HelloReply() { Message = $"Hello! {request.Name}" };

        //    return response.ToByteArray();
        //}

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            mqttClient = new MqttFactory().CreateManagedMqttClient();

            var topics = new MqttTopicFilterBuilder().WithTopic("#").Build();
            //var topics = new MqttTopicFilterBuilder().WithTopic("MQTTnet.RPC/+/ping").Build();

            await mqttClient.SubscribeAsync(topics.Topic);
            mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;

            await mqttClient.StartAsync(options);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (mqttClient == null) return;
            await mqttClient.StopAsync();
        }

        private async Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {

            var tag = arg.Tag;
            var client = arg.ClientId;

            var topic = arg.ApplicationMessage.Topic;
            var typ = arg.ApplicationMessage.ContentType;
            var msg = arg.ApplicationMessage.ConvertPayloadToString();  

            // Beginnings of a router

            var method = topic[(topic.LastIndexOf('/') + 1)..].ToLower();

            if (method == "response") return;       // Don't respond to our own response
            if (method == "logging")
            {
                Console.WriteLine($"Logging: {msg}");
                return;
            }

            if (!Routes.ContainsKey(method))
            {
                Console.WriteLine($"Can't find route to topic {topic}");
                return;
            }

            Console.WriteLine($"Topic: {topic} Tag: {tag} Client: {client} Type: {typ} Msg: {msg}");

            var route = Routes[method];

            var pay = arg.ApplicationMessage.Payload;

            var result = await route.FuncDelegate(pay);

            var respTopic = topic + "/response";

            Console.WriteLine($"Return to {respTopic}, {result.Length} bytes");
            Console.WriteLine($"Dump: {BitConverter.ToString(result)}");

            var message = new MqttApplicationMessageBuilder()
                   //here it does send the message
                   .WithTopic(respTopic)
                   .WithPayload(result)
                   .Build();

            await mqttClient.EnqueueAsync(message);

            return;
        }

        internal class RPCHandler
        {
            public string Method { get; private set; }
            public Func<byte[], Task<byte[]>> FuncDelegate { get; private set; }

            internal static void Setup(Dictionary<string, RPCHandler> routes, string v, Func<byte[], Task<byte[]>> do_ping)
            {
                var rpc = new RPCHandler
                {
                    Method = v.ToLower(),
                    FuncDelegate = do_ping
                };

                routes.Add(v, rpc);
            }
        }
    }
}
