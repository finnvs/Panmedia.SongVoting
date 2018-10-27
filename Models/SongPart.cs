using Orchard.ContentManagement;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Panmedia.SongVoting.Models
{
    public class SongPart : ContentPart<SongPartRecord>
    {   
        [Required(ErrorMessage="Sangens titel skal indtastes")] 
        public string SongTitle
        {
            get { return Retrieve(x => x.SongTitle); }
            set { Store(x => x.SongTitle, value); }
        }
        [Required(ErrorMessage = "Sangens forfatter(e) skal indtastes")]
        public string SongAuthor
        {
            get { return Retrieve(x => x.SongAuthor); }
            set { Store(x => x.SongAuthor, value); }
        }
        // [Url]
        [Required(ErrorMessage = "Indtast venligst en URL af formen: 'http://min-sang-url-her'")]
        [AllowHtml]
        [StringLength(2000, ErrorMessage = "Maximum chars 2000")] // sat unlimited i migrations / record klassen
        [DataType(DataType.Html)]
        public string SongUrl
        {
            get { return Retrieve(x => x.SongUrl); }
            set { Store(x => x.SongUrl, value); }
        }
        [Url]
        public string SongThumbnailUrl
        {
            get { return Retrieve(x => x.SongThumbnailUrl); }
            set { Store(x => x.SongThumbnailUrl, value); }
        }
        public string SongDescription
        {
            get { return Retrieve(x => x.SongDescription); }
            set { Store(x => x.SongDescription, value); }
        }
        
        // Enum written to db as a string
        // [Required(ErrorMessage ="Vælg venligst en kategori for din sang - Original, Cowrite?")]
        public SongCategory SongCategory
        {
            get { return Retrieve(x => x.SongCategory); }
            set { Store(x => x.SongCategory, value); }
        }

        public string SongEditorUserName
        {
            get { return Retrieve(x => x.SongEditorUserName); }
            set { Store(x => x.SongEditorUserName, value); }
        }

        public int SongEditorUserItemId
        {
            get { return Retrieve(x => x.SongEditorUserItemId); }
            set { Store(x => x.SongEditorUserItemId, value); }
        }
        public DateTime? CreatedDateUtc
        {
            get { return Retrieve(x => x.CreatedDateUtc); }
            set { Store(x => x.CreatedDateUtc, value); }
        }
        public DateTime? SubmittedDateUtc
        {
            get { return Retrieve(x => x.SubmittedDateUtc); }
            set { Store(x => x.SubmittedDateUtc, value); }
        }
        public int Votes
        {
            get { return Retrieve(x => x.Votes); }
            set { Store(x => x.Votes, value); }
        }
        public bool Shown
        {
            get { return Retrieve(x => x.Shown); }
            set { Store(x => x.Shown, value); }
        }

        [Required(ErrorMessage = "Vælg venligst en afsteming")]
        public int PollChoiceRecord_Id
        {
            get { return Retrieve(x => x.PollChoiceRecord_Id); }
            set { Store(x => x.PollChoiceRecord_Id, value); }
        }                
    }

    // Int reference is necessary for storing in db as string
    public enum SongCategory : int
    {
        Original = 1,
        Cowrite = 2,
        IntOriginal = 3,
        IntCowrite = 4
    }


}