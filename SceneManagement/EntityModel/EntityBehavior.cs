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
        public EntityClassInfo EntityClassInfo { get; protected set; }  
        public Guid SceneId { get; set; }       // What scene are we in
        public Guid EntityInstanceId { get; set; }  // What entity instance are we
        public DateTimeOffset InstanceBirth { get;set; }

        protected void Update()
        {

        }

    }
}
