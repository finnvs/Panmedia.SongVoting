using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Panmedia.SongVoting.Models
{
    
    public class PollChoiceRecord
    {
        public virtual int Id { get; set; }
        public virtual string Answer { get; set; }
        public virtual PollPartRecord PollPartRecord { get; set; }

     }
}