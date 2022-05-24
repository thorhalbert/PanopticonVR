using System;
using System.Collections.Generic;
using System.Text;

namespace SceneManagement.Avatar
{
    public class VRAvatar : FirstPersonAvatar
    {  // Start is called before the first frame update
        protected override void AvatarStart()
        {
            base.AvatarStart();
        }

        // Update is called once per frame
        protected override void AvatarUpdate()
        {
            base.AvatarUpdate();

            base.UpdateTransform(this.transform);
        }
    }
}
