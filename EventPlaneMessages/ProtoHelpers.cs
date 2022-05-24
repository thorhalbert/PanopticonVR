using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;

namespace PanopticonEventSceneEntities
{
    public static class ProtoHelpers
    {
        public static void SetGuid(this Uuid ing, Guid SetGuid)
        {
            // We'll deal with endian issues once we run into them - tired of guids as strings
            ing.UuidBytes = ByteString.CopyFrom(SetGuid.ToByteArray());
        }

        public static Uuid ToUuid(this Guid setGuid)
        {
            var ret = new Uuid();
            ret.SetGuid(setGuid);
            return ret;
        }
    }
}
