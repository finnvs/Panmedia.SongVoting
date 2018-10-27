using Panmedia.SongVoting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Panmedia.SongVoting.ViewModels
{
    public class PollResultViewModel
    {
        public string Question { get; set; }
        public bool Shown { get; set; }
        public IList<PollResultEntry> Choices { get; set; }
    }

    public class PollResultEntry
    {
        public PollChoiceRecord Choice { get; set; }
        public PollResultRecord Result { get; set; }
    }
}