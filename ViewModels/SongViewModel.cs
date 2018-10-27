using Panmedia.SongVoting.Models;
using Panmedia.SongVoting.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Panmedia.SongVoting.ViewModels
{
    public class SongViewModel
    {
        public SongViewModel()
        {
            // nothin' to see here
        }

        public SongViewModel(SongPart part)
        {
            if (part == null)
                throw new ArgumentNullException("part");
            
            SongTitle = part.SongTitle == null ? "" : part.SongTitle;
            SongAuthor = part.SongAuthor == null ? "" : part.SongAuthor;
            SongUrl = part.SongUrl;            
            SongThumbnailUrl = part.SongThumbnailUrl;
            SongDescription = part.SongDescription;
            SongCategory = part.SongCategory.ToString();
            MusicPolls = null;           
            SelectedPollId = 0;
            PollChoiceRecord_Id = part.PollChoiceRecord_Id;
            SongPartRecordId = part.ContentItem.Id;
            SongEditorUserName = part.SongEditorUserName;
            SongEditorUserItemId = part.SongEditorUserItemId;
        }

        public string SongTitle { get; set; }
        public string SongAuthor { get; set; }
        public string SongUrl { get; set; }
        public string SongThumbnailUrl { get; set; }
        public string SongDescription { get; set; }

        public string SongCategory { get; set; }        

        // List of open music polls for Poll choice dropdown
        public SelectList MusicPolls { get; set; }  
                
        public int SelectedPollId { get; set; }            

        public int PollChoiceRecord_Id { get; set; }
        public int SongPartRecordId { get; set; }
        public bool Shown { get; set; }

        public string SongEditorUserName { get; set; }
        public int SongEditorUserItemId { get; set; }
    }
}