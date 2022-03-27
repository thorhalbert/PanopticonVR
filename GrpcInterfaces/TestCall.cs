using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace GrpcInterfaces
{
    public class TestCall
    {
        public static void Try()
        {
            //var option = new GrpcChannelOptions { Credentials = ChannelCredentials.Insecure };
            //var channel = new GrpcChannel("10.0.52.245:10080", option);

            var channel = new Channel("10.0.52.245:10080", ChannelCredentials.Insecure);
            var client = new PanopticonService.Greeter.GreeterClient(channel);

            // YOUR CODE GOES HERE

            var task = client.SayHelloAsync(new PanopticonService.HelloRequest { Name = "testname" });
            var answer = task.GetAwaiter().GetResult();

            //Console.WriteLine($"SayHello Returns: {answer.Message}");

            channel.ShutdownAsync().Wait();
        }
    }
}
