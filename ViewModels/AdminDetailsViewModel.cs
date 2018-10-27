using Panmedia.SongVoting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Panmedia.SongVoting.ViewModels
{
    public class AdminDetailsViewModel
    {
        public string Question { get; set; }
        public DateTime Open { get; set; }
        public DateTime Close { get; set; }
        public int MaxVotes { get; set; }
        public IList<PollResultEntry> ResultByChoices { get; set; }
        public IList<VoteEntry> VotesByChoices { get; set; }
        public IList<VoteEntry> AnonymVotesByChoices { get; set; }
        public IList<VoteEntry> UserVotesByChoices { get; set; }
    }

    public class VoteEntry
    {
        public PollChoiceRecord Choice { get; set; }
        public bool IsAnonymous { get; set; }
        public IDictionary<DateTime, int> Results { get; set; }
    }
}