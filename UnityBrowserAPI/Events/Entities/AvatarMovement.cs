using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace UnityBrowserAPI.Events.Entities
{

    public abstract class AvatarMovement : EntityBase
    {
        public Vector3 CameraPosition { get; set; }
        public Vector3 CameraRotation { get; set; }
        public Vector3 CameraScale { get; set; }
    }

    public class AvatarMovementIngress : AvatarMovement
    { }

    public class AvatarMovementEgress : AvatarMovement 
    { }
}
