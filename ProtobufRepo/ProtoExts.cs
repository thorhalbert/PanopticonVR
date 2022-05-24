using PanopticonService;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProtobufRepo
{
    public static class ProtoExts
    {
        public static Uuid GetUuid(this Guid g)
        {
            return new Uuid { Uuid_ = g.ToString() };
        }

        public static DTO GetDTO(this DateTimeOffset t)
        {
            return new DTO
            {
                Ticks = t.Ticks,
                Offset = t.Offset.Seconds
            };
        }

        public static DateTimeOffset GetDate(this DTO d)
        {
            return new DateTimeOffset(d.Ticks, new TimeSpan(0, 0, d.Offset));
        }
    }

    
}
