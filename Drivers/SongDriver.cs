using Panmedia.SongVoting.Models;
using Panmedia.SongVoting.Services;
using Panmedia.SongVoting.ViewModels;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Panmedia.SongVoting.Drivers
{
    [ComVisible(false)]
    public class SongPartDriver : ContentPartDriver<SongPart>
    {
        private readonly ISongService _songService;

        public SongPartDriver(ISongService songService)
        {
            _songService = songService;
        }


        protected override DriverResult Display(SongPart part, string displayType, dynamic shapeHelper)
        {
            return Combined(
                ContentShape("Parts_Song",
                    () => shapeHelper.Parts_Song(
                    Id: part.Id,
                    SongTitle: part.SongTitle,
                    SongAuthor: part.SongAuthor,
                    SongUrl: part.SongUrl,
                    SongDescription: part.SongDescription,
                    SongCategory: part.SongCategory,                    
                    MusicPolls: _songService.MusicPolls())),
                ContentShape("Parts_Song_Admin_Summary",
                    () => shapeHelper.Parts_Song_Admin_Summary(
                        SongTitle: part.SongTitle                        
                        ))
                 );
        }

        // GET
        protected override DriverResult Editor(SongPart part, dynamic shapeHelper)
        {
            if (part == null)
                throw new ArgumentNullException("part");

            var model = new SongViewModel(part);
            model.MusicPolls = _songService.MusicPolls();
            return ContentShape("Parts_Song_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Song",
                    Model: model,
                    Prefix: Prefix));
        }

        // POST        
        protected override DriverResult Editor(SongPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater == null)
                throw new ArgumentNullException("updater");
            if (part == null)
                throw new ArgumentNullException("part");

            var model = new SongViewModel();
            updater.TryUpdateModel(model, Prefix, null, null);
            // updater.TryUpdateModel(part, Prefix, null, null);

            _songService.EditSong(part, model);

            return Editor(part, shapeHelper);
        }

        // Importing content
        protected override void Importing(SongPart part, ImportContentContext context)
        {
            context.ImportAttribute(part.PartDefinition.Name, "SongTitle", songTitle => part.SongTitle = songTitle);
            context.ImportAttribute(part.PartDefinition.Name, "SongAuthor", songAuthor => part.SongAuthor = songAuthor);
            context.ImportAttribute(part.PartDefinition.Name, "SongUrl", songUrl => part.SongUrl = songUrl);
            context.ImportAttribute(part.PartDefinition.Name, "SongThumbnailUrl", songThumbnailUrl => part.SongThumbnailUrl = songThumbnailUrl);
            context.ImportAttribute(part.PartDefinition.Name, "SongDescription", songDescription => part.SongDescription = songDescription);
            context.ImportAttribute(part.PartDefinition.Name, "SongCategory",
                songCategory => part.SongCategory = (SongCategory)Enum.Parse(typeof(SongCategory), songCategory));
            // alternative, but with no null values for attr accepted at import:
            //part.SongCategory = (SongCategory)Enum.Parse(typeof(SongCategory), context.Attribute(part.PartDefinition.Name, "SongCategory"));
            context.ImportAttribute(part.PartDefinition.Name, "SongEditorUserName", songEditorUserName => part.SongEditorUserName = songEditorUserName);
            context.ImportAttribute(part.PartDefinition.Name, "SongEditorUserItemId",
                songEditorUserItemId => part.SongEditorUserItemId = Convert.ToInt32(songEditorUserItemId));
            context.ImportAttribute(part.PartDefinition.Name, "CreatedDateUtc", createdDateUtc => part.CreatedDateUtc = DateTime.Parse(createdDateUtc));
            context.ImportAttribute(part.PartDefinition.Name, "SubmittedDateUtc", submittedDateUtc => part.SubmittedDateUtc = DateTime.Parse(submittedDateUtc));
            context.ImportAttribute(part.PartDefinition.Name, "Votes", votes => part.Votes = Convert.ToInt32(votes));
            context.ImportAttribute(part.PartDefinition.Name, "Shown", shown => part.Shown = Convert.ToBoolean(shown));
            context.ImportAttribute(part.PartDefinition.Name, "PollChoiceRecord_Id", pollChoiceRecord_Id => part.PollChoiceRecord_Id = Convert.ToInt32(pollChoiceRecord_Id));
        }

        // Exporting content
        protected override void Exporting(SongPart part, ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("SongTitle", part.SongTitle);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SongAuthor", part.SongAuthor);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SongUrl", part.SongUrl);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SongThumbnailUrl", part.SongThumbnailUrl);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SongDescription", part.SongDescription);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SongCategory", part.SongCategory); // .ToString() ?
            context.Element(part.PartDefinition.Name).SetAttributeValue("SongEditorUserName", part.SongEditorUserName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SongEditorUserItemId", part.SongEditorUserItemId);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CreatedDateUtc", part.CreatedDateUtc);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SubmittedDateUtc", part.SubmittedDateUtc);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Votes", part.Votes);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Shown", part.Shown);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PollChoiceRecord_Id", part.PollChoiceRecord_Id);
        }
    }
}

