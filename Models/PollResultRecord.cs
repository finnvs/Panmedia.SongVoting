using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Panmedia.SongVoting.Models
{
    public class PollResultRecord
    {
        public virtual int Id { get; set; }
        public virtual PollChoiceRecord PollChoiceRecord { get; set; }
        public virtual int Count { get; set; }
    }
}