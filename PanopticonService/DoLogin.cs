using Grpc.Core;
using ProtobufRepo;
using System;
using System.Threading.Tasks;

namespace PanopticonService
{
    public class DoLogin :PanopticonService.DoPing.DoPingBase
    {
        static int iterate = 0;
        static Guid instanceId = Guid.NewGuid();

        public override Task<PingReply> Ping(PingRequest request, ServerCallContext context)
        {
            Console.WriteLine($"(Server) {request.UniqueDeviceId} {request.InstanceId} {request.Iteration} {request.Current.GetDate()}");

            var ret = new PingReply
            {
                ServerName = Environment.MachineName,
                InstanceId = instanceId.GetUuid(),
                Iteration = iterate++,
                Current = DateTimeOffset.Now.GetDTO(),

                PanopticonServer = "10.0.52.35",
                PanopticonPort = 10080,
                KafkaBroker = "10.0.52.35:9092",
                KafkaSchema = "http://10.0.52.35:8081",
            };

            return Task.FromResult(ret);
        }
    }
}
