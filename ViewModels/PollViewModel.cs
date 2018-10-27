using Panmedia.SongVoting.Models;
using System;
using System.Collections.Generic;

namespace Panmedia.SongVoting.ViewModels
{
    public class PollViewModel
    {
        public PollViewModel()
        {
            Choices = new List<ChoiceEntry>();
        }
        public PollViewModel(PollPart part, IList<ChoiceEntry> choices)
        {
            if (part == null)
                throw new ArgumentNullException("part");
            if (choices == null)
                throw new ArgumentNullException("choices");
            bool isNew = part.Record.ContentItemRecord == null;           
            Question = part.Record.Question == null ? "" : part.Record.Question;            
            Open = part.Record.OpenDateUtc == null ? DateTime.Now : part.Record.OpenDateUtc.Value;
            Close = part.Record.CloseDateUtc == null ? DateTime.Now.AddDays(14) : part.Record.CloseDateUtc.Value;
            MaxVotes = part.Record.MaxVotes;            
            Shown = isNew ? true : part.Record.Shown;            
            MusicPoll = part.Record.MusicPoll;
            ShowVotingUI = part.Record.ShowVotingUI;
            MusicPollInTestMode = part.Record.MusicPollInTestMode;
            Choices = choices == null ? new List<ChoiceEntry>() : choices;                     
        }
        public string Question { get; set; }
        public DateTime Open { get; set; }
        public DateTime Close { get; set; }
        public int MaxVotes { get; set; }
        public bool Shown { get; set; }
        public bool MusicPoll { get; set; }
        public bool ShowVotingUI { get; set; }
        public bool MusicPollInTestMode { get; set; }
        public IList<ChoiceEntry> Choices { get; set; }
    }

    public class ChoiceEntry
    {
        public PollChoiceRecord Choice { get; set; }
        public SongPartRecord Song { get; set; }
        public string Action { get; set; }
    }
}