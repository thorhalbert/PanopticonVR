using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.AspNetCore;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using System;
using System.Threading.Tasks;

namespace PanopticonService
{
    public  class MQTTClientTransport
    {
        private readonly ManagedMqttClientOptions options;

        public MQTTClientTransport()
        {
            // Setup and start a managed MQTT client.
            options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId("Client1")
                    .WithTcpServer("broker.hivemq.com", 1883)
                    .WithProtocolVersion(MqttProtocolVersion.V500)
                    .WithAuthentication("huh? method?", Array.Empty<byte>())
                    .Build())
                .Build();

          
        }

        public async void Start()
        {
            var mqttClient = new MqttFactory().CreateManagedMqttClient();

            var topics = new MqttTopicFilterBuilder().WithTopic("MQTTnet.RPC/+/ping").Build();
            
            await mqttClient.SubscribeAsync(topics.Topic);
            mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
            await mqttClient.StartAsync(options);
        }

        private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            var msg = arg.ApplicationMessage;
            var tag = arg.Tag;
            var client = arg.ClientId;

            return null;
        }
    }

}
   