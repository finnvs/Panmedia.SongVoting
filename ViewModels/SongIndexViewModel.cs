using Panmedia.SongVoting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Panmedia.SongVoting.ViewModels
{
    public class SongIndexViewModel
    {
        public IList<SongEntry> Songs { get; set; }
        public SongIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class SongEntry
    {
        public SongPartRecord Song { get; set; }        
        public bool IsChecked { get; set; }
    }

    public class SongIndexOptions
    {
        public SongIndexFilter Filter { get; set; }
        public SongIndexBulkAction BulkAction { get; set; }
    }

    public enum SongIndexBulkAction
    {
        None,
        Close,
        Open,
        Delete
    }

    public enum SongIndexFilter
    {
        All,
        Closed,
        Open
    }
}