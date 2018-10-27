using Panmedia.SongVoting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Panmedia.SongVoting.ViewModels
{
    public class SongUrlViewModel
    {
        // For admin view edit of specific song data
        public string SongTitle { get; set; }
        public string SongAuthor { get; set; }
        public string SongUrl { get; set; }       
        public string SongDescription { get; set; }
        public int SongPartRecordId { get; set; }
        
    }
}