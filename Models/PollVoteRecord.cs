using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Panmedia.SongVoting.Models
{
    public class PollVoteRecord
    {
        public virtual int Id { get; set; }
        public virtual PollPartRecord PollPartRecord { get; set; }
        public virtual PollChoiceRecord PollChoiceRecord { get; set; }
        public virtual string Username { get; set; }
        public virtual string Fingerprint { get; set; }
        public virtual DateTime VoteDateUtc { get; set; }
    }
}