using PanopticonEventSceneEntities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SceneManagement.EntityModel
{
    public class EntityClassInfo
    {
        public Guid ManufacturerId { get; protected set; }
        public Guid WorldId { get; set; }
        public Guid EntityClassId { get; set; }
    }

    public abstract class EntityBehavior :MonoBehaviour
    {       
        public EntityClassInfo? EntityClassInfo { get; protected set; }  
        public Guid SceneId { get; set; }       // What scene are we in
        public Guid EntityInstanceId { get; set; }  // What entity instance are we
        public DateTimeOffset InstanceBirth { get;set; }

        protected void Update()
        {

        }

        private Transform? lastTransform = null;

        protected void UpdateTransform(Transform transform)
        {
            if (lastTransform == null)
            {       
                transformDirty(transform);
                return;
            }

            if (!transformEq(lastTransform, transform))
                transformDirty(transform);
        }

        // Make into an extension method
        private bool transformEq(Transform lastTransform, Transform transform)
        {
            // lastTransform.Equals(transform);

            if (lastTransform.position != transform.position) return false;
            if (lastTransform.rotation != transform.rotation) return false;
            if (lastTransform.localScale != transform.localScale) return false;

            return true;
        }

        private EntityChanged? EntityDelta = null;

        private void transformDirty(Transform transform)
        {
            if (EntityClassInfo == null) return;

            var ticks = (ulong)DateTime.Now.Ticks;

            if (EntityDelta == null)
                EntityDelta = new EntityChanged // Do the constants just once
                {
                    EntityClass = EntityClassInfo.EntityClassId.ToUuid(),
                    InstanceId = EntityInstanceId.ToUuid(),
                    InternalId = Guid.Empty.ToUuid(),
                    Reporter = Guid.Empty.ToUuid(),

                    PrivateType = PrivatePayloadTypes.NoPayload, // By default

                    LastMoveTick = 0,
                    LastTick = 0,
                };

            EntityDelta.Position = CvtVec3(transform.position);
            EntityDelta.Rotation = CvtAng(transform.rotation);
            EntityDelta.Scale = CvtVec3(transform.localScale);

            EntityDelta.LastTick = 0;
            EntityDelta.LastMoveTick = ticks;         

            // I suspect we don't want to broadcast deltas for every update, but
            // we do need to figure out how often to do it.

            lastTransform = transform;
        }

        private PanopticonEventSceneEntities.Vector3 CvtVec3(UnityEngine.Vector3 vec)
        {
            var ret = new PanopticonEventSceneEntities.Vector3()
            {
                X = vec.x,
                Y = vec.y,
                Z = vec.z
            };

            return ret;
        }

        private PanopticonEventSceneEntities.Quaternion CvtAng(UnityEngine.Quaternion ang)
        {
            var ret = new PanopticonEventSceneEntities.Quaternion()
            {
                X = ang.x,
                Y = ang.y,
                Z = ang.z,
                W = ang.w
            };

            return ret;
        }
    }
}
