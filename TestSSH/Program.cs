// See https://aka.ms/new-console-template for more information
using Renci.SshNet;

Console.WriteLine("Hello, World!");


var ci = new ConnectionInfo("house.thorsbrain.com", "neoworlds",
    new PrivateKeyAuthenticationMethod("neoworlds", new PrivateKeyFile(@"C:\cygwin64\home\thor\.creds\neoworlds.pri")));


using (var client = new SshClient(ci))
{
    client.HostKeyReceived += (sender, e) =>
    {
        Console.WriteLine(Convert.ToHexString(e.FingerPrint));
    
    };
    client.Connect();
}