using Panmedia.SongVoting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Panmedia.SongVoting.ViewModels
{
    public class PollIndexViewModel
    {
        public IEnumerable<PollPart> Polls { get; set; }
        public bool ShowEnterSongBtn { get; set; }
    }
}