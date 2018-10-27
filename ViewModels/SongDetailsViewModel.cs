using Panmedia.SongVoting.Models;
using System;
using System.Collections.Generic;

namespace Panmedia.SongVoting.ViewModels
{
    public class SongDetailsViewModel
    {
        public string SongTitleAndAuthor { get; set; }
        public PollChoiceRecord Choice { get; set; }
        
        public IList<SongVoteEntry> VotesByUser { get; set; }
        // public IList<VoteEntry> AnonymVotesByChoices { get; set; }
        // public IList<VoteEntry> UserVotesByChoices { get; set; }
    }

    public class SongVoteEntry
    {   
        public string UserFullName { get; set; }
        public IList<PollVoteRecord> NumberOfVotesByUser { get; set; }
    }
}