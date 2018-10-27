using Panmedia.SongVoting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Panmedia.SongVoting.ViewModels
{
    public class AdminIndexViewModel
    {
        public IList<PollEntry> Polls { get; set; }
        public PollIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PollEntry
    {
        public PollPartRecord Poll { get; set; }
        public int NumberOfChoices { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PollIndexOptions
    {
        public PollIndexFilter Filter { get; set; }
        public PollIndexBulkAction BulkAction { get; set; }
    }

    public enum PollIndexBulkAction
    {
        None,
        Close,
        Open,
        Delete
    }

    public enum PollIndexFilter
    {
        All,
        Closed,
        Open
    }
}