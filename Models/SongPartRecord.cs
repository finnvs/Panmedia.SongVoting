using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System;
using System.Web.Mvc;

namespace Panmedia.SongVoting.Models
{
    public class SongPartRecord : ContentPartRecord
    {
        public virtual string SongTitle { get; set; }
        public virtual string SongAuthor { get; set; }                
        public virtual string SongUrl { get; set; }
        public virtual string SongThumbnailUrl { get; set; }
        public virtual string SongDescription { get; set; }
        public virtual SongCategory SongCategory { get; set; }
        public virtual string SongEditorUserName { get; set; }
        public virtual int SongEditorUserItemId { get; set; }
        public virtual DateTime? CreatedDateUtc { get; set; }
        public virtual DateTime? SubmittedDateUtc { get; set; }
        public virtual int Votes { get; set; }
        public virtual bool Shown { get; set; }        
        public virtual int PollChoiceRecord_Id { get; set; }       
    }
}