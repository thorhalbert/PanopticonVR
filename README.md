# PanopticonVR
Panopticon Browser Smart VR Objects and Acceleration

This is the .net visual studio support side for the Panopticon browser, which is a sketch of the ultimate VRWorlds browser and federated VR ecosystem, basically a VR operating system.

The primary protocols I'm supporting is based on Kafka messaging, and GRPC (though these have been quite a challenge in unity3d).
Ultimately I also need to be able to support webassembly, though this isn't going to directly happen on the browser side for a while, since as unity steps up to support .netstandard2.1, Wasmtime has bumped their version up to later .netcore (6 probably). 
It will be a while before unity supports .net 6.   In the mean time, the webassembly parts of the project and the sandboxing will be done here in this support library.
The support and acceleration pieces will be done via kafka and grpc in docker.  

I recently got an oculus quest 2, and so even more challenging to get everything running over in android.  It might even make sense to keep the heavy sandbox pieces over in docker.
That will make it easier to deploy as much horsepower as necessary to support browser operations.

The 'Panopticon' part of the browser comes from me wanted to do a subset of the VRWorlds concept to make a fully capable work and programming/development environment.
One of the big parts of this handling display of remote frame buffers.   This will run the gamut of vt100 emulators to vnc/rdp frames to remote monitoring of systems and cloud environments.

Oculus now supports real keyboards in VR (though they're way backordered and I can't yet get one), so this should actually make this kind of environment tenable.
