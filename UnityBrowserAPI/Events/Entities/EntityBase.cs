using System;
using System.Numerics;

namespace UnityBrowserAPI.Events.Entities
{
    /// <summary>
    /// Ultimately the idea here is to look something like a video compression codec
    /// where we compute a vector (possibly affected by gravity physics) where the receivers
    /// can compute the current position, then publish an absolute position/fix occasionally if the
    /// estimated value exceeds an error tolerance, like a keyframe.   Initially we will spray all of the
    /// movement data uncompressed, so we can get a feel for it to come up with a wait to fair a curve
    /// to estimate the vector.   We do want this data to have as little lag as possible.
    /// 
    /// We may also have problems with competing physics engines, or possibily bad actors operating
    /// as the browser.   So, imagine 10,000 browsers watching a baseball game or a billiards tournament.
    /// Everyone must agree on where everything is.
    /// 
    /// If a tree falls in a virtual forest and nobody is viewing the scene, then did it fall?   Answer is no.
    /// </summary>
    public abstract class EntityBase : ReporterBase
    {
        public Guid ManufacturerId { get; set; }
        public Guid EntityClass { get; set; }

        public Guid InstanceId { get; set; }

        public Vector3 Position { get; set;}
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public byte[]? PrivateBlob { get; set; }     
    }
}
