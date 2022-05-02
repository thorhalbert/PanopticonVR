using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.AspNetCore;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PanopticonService
{
    public class MQTTClientTransport : IHostedService
    {
        private readonly ManagedMqttClientOptions options;
        private ManagedMqttClient mqttClient;

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
        }

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

        private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            var msg = arg.ApplicationMessage;
            var tag = arg.Tag;
            var client = arg.ClientId;

            Console.WriteLine($"Msg: {msg} Tag: {tag} Client: {client}");

            return null;
        }
    }
}
