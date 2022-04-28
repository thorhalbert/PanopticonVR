// See https://aka.ms/new-console-template for more information

//using EventPlane;

Console.WriteLine("Hello, World!");

ControlPlane.PlaneSingleton.Test("Test");


// We can test here

//var thr = new EventPlane.StartPlaneThread();
//thr.Start();

//Console.WriteLine("Start Message Plane");

//var plane = new StartMessageMesh();
//plane.Start();

//Console.WriteLine("Plane returns, send message");
//Thread.Sleep(3000);

//Log.UnityConsole("Test text logging");

while (true)
    Thread.Sleep(10000);