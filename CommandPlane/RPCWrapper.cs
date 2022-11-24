//using MQTTnet;
//using MQTTnet.Client;
//using MQTTnet.Extensions.Rpc;
//using MQTTnet.Protocol;
//using PanopticonAPIs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
//using Google.Protobuf;
using System.Diagnostics;

namespace ControlPlane
{
    internal class RPCWrapper
    {
        //public static async Task<HelloReply> Call_Ping(HelloRequest request)
        //{
        //    try
        //    {
        //        if ( PlaneSingleton.PlaneSingletonInstance==null) return null;

        //        HelloReply retVal = null;
        //        var mqttFactory = new MqttFactory();

        //        // The RPC client is an addon for the existing client. So we need a regular client
        //        // which is wrapped later.

        //        // We may want to store this one for future use - though we might should check to see if it's still connected
        //        using var mqttClient = await PlaneSingleton.PlaneSingletonInstance.GetNewClient();

        //        using (var mqttRpcClient = mqttFactory.CreateMqttRpcClient(mqttClient))
        //        {
        //            // Access to a fully featured application message is not supported for RCP calls!
        //            // The method will throw an exception when the response was not received in time.
        //            Debug.WriteLine($"Send Ping Out");

        //            var ret = await mqttRpcClient.ExecuteAsync(
        //                TimeSpan.FromSeconds(10),
        //                "ping",
        //                request.ToByteArray(),
        //                MqttQualityOfServiceLevel.AtMostOnce);

        //            Debug.WriteLine($"Ping Returns {ret.Length} bytes");

        //            Debug.WriteLine($"Dump: {BitConverter.ToString(ret)}");


        //            try
        //            {
        //                retVal = HelloReply.Parser.ParseFrom(ret);
        //            }
        //            catch (Exception ex)
        //            {
        //                Debug.WriteLine($"Protobuf Fail: {ex.Message}");
        //                return null;
        //            }
        //        }

        //        return retVal;
        //    }
        //    catch(Exception ex)
        //    {
        //        Debug.WriteLine($"Error in Ping: ");
        //        await Log.DebugCallback(ex.Message, ex.StackTrace, 0);              
        //    }

        //    return null;
        //}
    }
}
