using Orchard.ContentManagement;
using System;
using System.ComponentModel.DataAnnotations;

namespace Panmedia.SongVoting.Models
{
    public class PollPart : ContentPart<PollPartRecord>
    {
        [Required]
        public string Question
        {
            get { return Retrieve(x => x.Question); }
            set { Store(x => x.Question, value); }
        }              

        [Required]        
        public DateTime? OpenDateUtc
        {
            get { return Retrieve(x => x.OpenDateUtc); }
            set { Store(x => x.OpenDateUtc, value); }
        }

        [Required]        
        public DateTime? CloseDateUtc
        {
            get { return Retrieve(x => x.CloseDateUtc); }
            set { Store(x => x.CloseDateUtc, value); }
        }

        [Required]
        public int MaxVotes
        {
            get { return Retrieve(x => x.MaxVotes); }
            set { Store(x => x.MaxVotes, value); }
        }

        [Required]
        public bool IsShown
        {
            get { return Retrieve(x => x.Shown); }
            set { Store(x => x.Shown, value); }
        }

        [Required]
        public bool IsMusicPoll
        {
            get { return Retrieve(x => x.MusicPoll); }
            set { Store(x => x.MusicPoll, value); }
        }

        [Required]
        public bool ShowVotingUI
        {
            get { return Retrieve(x => x.ShowVotingUI); }
            set { Store(x => x.ShowVotingUI, value); }
        }

        [Required]
        public bool IsMusicPollInTestMode
        {
            get { return Retrieve(x => x.MusicPollInTestMode); }
            set { Store(x => x.MusicPollInTestMode, value); }
        }
    }
}