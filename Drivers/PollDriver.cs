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
    public class PollPartDriver : ContentPartDriver<PollPart>
    {
        private readonly IPollService _pollService;

        public PollPartDriver(IPollService pollService)
        {
            _pollService = pollService;
        }


        protected override DriverResult Display(PollPart part, string displayType, dynamic shapeHelper)
        {
            return Combined(
                ContentShape("Parts_Poll",
                    () => shapeHelper.Parts_Poll(
                    Id: part.Id,
                    Question: part.Question,
                    OpenDateUtc: part.OpenDateUtc,
                    CloseDateUtc: part.CloseDateUtc,
                    MaxVotes: part.MaxVotes,
                    IsShown: part.IsShown,
                    IsOpen: !_pollService.IsPollClosed(part.Id),
                    IsMusicPoll: _pollService.IsMusicPoll(part.Id),
                    ShowVotingUI: _pollService.ShowVotingUI(part.Id),
                    IsMusicPollInTestMode: _pollService.IsMusicPollInTestMode(part.Id),
                    Choices: _pollService.GetChoices(part.Id).ToList())),
                ContentShape("Parts_Poll_Admin_Summary",
                    () => shapeHelper.Parts_Poll_Admin_Summary(
                        Question: part.Question,
                        ChoiceNum: _pollService.GetChoices(part.Id).Count()
                        )),
                ContentShape("Parts_Poll_Result",
                    () => shapeHelper.Parts_Poll_Result(
                        Question: part.Question,
                        Choices: _pollService.GetPollWithResult(part.Id).Choices,
                        Shown: part.IsShown
                        ))
                );

        }

        // GET
        protected override DriverResult Editor(PollPart part, dynamic shapeHelper)
        {
            if (part == null)
                throw new ArgumentNullException("part");
            var choices = _pollService
                            .GetChoices(part.Id)
                            .Select(c => new ChoiceEntry { Choice = c, Action = "Alter" })
                            .ToList();
            var model = new PollViewModel(part, choices);
            return ContentShape("Parts_Poll_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Poll",
                    Model: model,
                    Prefix: Prefix));
        }

        // POST        
        protected override DriverResult Editor(PollPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater == null)
                throw new ArgumentNullException("updater");
            if (part == null)
                throw new ArgumentNullException("part");

            var model = new PollViewModel();
            model.Choices = new List<ChoiceEntry>();
            updater.TryUpdateModel(model, Prefix, null, null);
            _pollService.EditPoll(part, model);

            return Editor(part, shapeHelper);
        }

        // Importing content
        protected override void Importing(PollPart part, ImportContentContext context)
        {
            context.ImportAttribute(part.PartDefinition.Name, "Question", question => part.Question = question);
            context.ImportAttribute(part.PartDefinition.Name, "OpenDateUtc", openDateUtc => part.OpenDateUtc = DateTime.Parse(openDateUtc));
            context.ImportAttribute(part.PartDefinition.Name, "CloseDateUtc", closeDateUtc => part.CloseDateUtc = DateTime.Parse(closeDateUtc));
            context.ImportAttribute(part.PartDefinition.Name, "MaxVotes", maxVotes => part.MaxVotes = Convert.ToInt32(maxVotes));
            context.ImportAttribute(part.PartDefinition.Name, "IsShown", isShown => part.IsShown = Convert.ToBoolean(isShown));
            context.ImportAttribute(part.PartDefinition.Name, "IsMusicPoll", isMusicPoll => part.IsMusicPoll = Convert.ToBoolean(isMusicPoll));
            context.ImportAttribute(part.PartDefinition.Name, "ShowVotingUI", showVotingUI => part.ShowVotingUI = Convert.ToBoolean(showVotingUI));
            context.ImportAttribute(part.PartDefinition.Name, "IsMusicPollInTestMode", isMusicPollInTestMode => part.IsMusicPollInTestMode = Convert.ToBoolean(isMusicPollInTestMode));
        }

        // Exporting content
        protected override void Exporting(PollPart part, ExportContentContext context)
        {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Question", part.Question);
            context.Element(part.PartDefinition.Name).SetAttributeValue("OpenDateUtc", part.OpenDateUtc);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CloseDateUtc", part.CloseDateUtc);
            context.Element(part.PartDefinition.Name).SetAttributeValue("MaxVotes", part.MaxVotes);
            context.Element(part.PartDefinition.Name).SetAttributeValue("IsShown", part.IsShown);
            context.Element(part.PartDefinition.Name).SetAttributeValue("IsMusicPoll", part.IsMusicPoll);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ShowVotingUI", part.ShowVotingUI);
            context.Element(part.PartDefinition.Name).SetAttributeValue("IsMusicPollInTestMode", part.IsMusicPollInTestMode);
        }

        // TODO: Exp / Imp af votes (participant) liste for hver poll, eller vote repo til db
    }
}