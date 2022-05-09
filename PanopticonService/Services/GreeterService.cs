using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PanopticonService
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReplyX> SayHello(HelloRequestX request, ServerCallContext context)
        {
            Console.WriteLine($"Client sent {request.Name} - {context.Host}, {context.Peer}");
            return Task.FromResult(new HelloReplyX
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
