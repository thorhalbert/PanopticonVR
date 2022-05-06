using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.Rpc;
using MQTTnet.Protocol;
using PanopticonAPIs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;

namespace ControlPlane
{
    internal class RPCWrapper
    {
        public static async Task<HelloReply> Call_Ping(HelloRequest request)
        {
            HelloReply retVal = null;
            var mqttFactory = new MqttFactory();

            // The RPC client is an addon for the existing client. So we need a regular client
            // which is wrapped later.

            using var mqttClient = PlaneSingleton.MqttClient;

            using (var mqttRpcClient = mqttFactory.CreateMqttRpcClient(mqttClient))
            {
                // Access to a fully featured application message is not supported for RCP calls!
                // The method will throw an exception when the response was not received in time.
                var ret = await mqttRpcClient.ExecuteAsync(
                    TimeSpan.FromSeconds(10),
                    "ping",
                    request.ToByteArray(),
                    MqttQualityOfServiceLevel.AtMostOnce);

                retVal = HelloReply.Parser.ParseFrom(ret);
            }

            return retVal;
        }
    }
}
