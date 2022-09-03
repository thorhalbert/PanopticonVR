using System;
using System.Numerics;

namespace UnityBrowserAPI.Events.Entities
{
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
