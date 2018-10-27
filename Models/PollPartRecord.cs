using Orchard.ContentManagement.Records;
using System;

namespace Panmedia.SongVoting.Models
{
    public class PollPartRecord : ContentPartRecord
    {
        public virtual string Question { get; set; }
        public virtual DateTime? OpenDateUtc { get; set; }
        public virtual DateTime? CloseDateUtc { get; set; }
        public virtual int MaxVotes { get; set; }
        public virtual bool Shown { get; set; }
        public virtual bool MusicPoll { get; set; }
        public virtual bool ShowVotingUI { get; set; }

        public virtual bool MusicPollInTestMode { get; set; }

    }
}