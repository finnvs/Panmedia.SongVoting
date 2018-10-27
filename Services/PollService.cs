using Panmedia.SongVoting.Models;
using Panmedia.SongVoting.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Core.Title.Models;

namespace Panmedia.SongVoting.Services
{
    public class PollService : IPollService
    {
        private readonly IRepository<PollPartRecord> _pollRepository;
        private readonly IRepository<PollChoiceRecord> _choiceRepository;
        private readonly IRepository<PollResultRecord> _resultRepository;
        private readonly IRepository<PollVoteRecord> _voteRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;

        public PollService(
            IRepository<PollPartRecord> pollRepository,
            IRepository<PollChoiceRecord> choiceRepository,
            IRepository<PollResultRecord> resultRepository,
            IRepository<PollVoteRecord> voteRepository,
            IOrchardServices orchardServices,
            IContentManager contentManager)
        {
            _pollRepository = pollRepository;
            _choiceRepository = choiceRepository;
            _resultRepository = resultRepository;
            _orchardServices = orchardServices;
            _voteRepository = voteRepository;
            _contentManager = contentManager;
        }

        public PollPartRecord GetPoll(int id)
        {
            return _pollRepository.Get(id);
        }

        public PollPartRecord GetPollFromChoiceId (int id)
        {
            var pollChoice = _choiceRepository.Fetch(c => c.Id == id).FirstOrDefault();
            if (pollChoice != null)
            {
                return _pollRepository.Get(pollChoice.PollPartRecord.Id);
            }
            else
            { 
                throw new ArgumentNullException("part");
            }
        }

        public IQueryable<PollChoiceRecord> GetChoices(int pollId)
        {
            PollPartRecord poll = GetPoll(pollId);
            var iQueryableChoices = _choiceRepository.Fetch(c => c.PollPartRecord.Id == pollId).OrderBy(c => c.Id).AsQueryable<PollChoiceRecord>();

            // If poll is a moosic poll, return a randomized list of choices here, rather than the ordered list. pollId !=0 is checked
            // for the case of a new poll create from the dashboard, where the poll id will be zero, and poll will be null.
            if (poll != null) { 
                if (pollId !=0 && poll.MusicPoll)
                {
                    // randomize once per username
                    var currentUser = _orchardServices.WorkContext.CurrentUser;
                    string user = currentUser == null ? "Anonymous" : currentUser.UserName;                   
                    var random = new Random(user.GetHashCode());
                    // randomize every time method is called
                    // var random = new Random();                   
                    var shuffledChoices = iQueryableChoices.OrderBy(x => random.Next());
                    return shuffledChoices;
                }
            }
            return iQueryableChoices;
        }

        public IQueryable<PollChoiceRecord> GetOrderedChoices(int pollId)
        {
            return _choiceRepository.Fetch(c => c.PollPartRecord.Id == pollId).OrderBy(c => c.Id).AsQueryable<PollChoiceRecord>();
        }


        public void CreatePoll(PollPart poll)
        {
            if (poll == null)
                throw new ArgumentNullException("poll");
            var p = new PollPartRecord
            {
                ContentItemRecord = poll.Record.ContentItemRecord,
                Question = poll.Question,
                OpenDateUtc = poll.OpenDateUtc,
                CloseDateUtc = poll.CloseDateUtc,
                MaxVotes = poll.MaxVotes,
                Shown = poll.IsShown,
                MusicPoll = poll.IsMusicPoll,
                ShowVotingUI = poll.ShowVotingUI
            };
            _pollRepository.Create(p);
        }


        public IContentQuery<PollPart, PollPartRecord> GetPolls()
        {
            return _orchardServices.ContentManager
                .Query<PollPart, PollPartRecord>();
        }


        public void UpdatePoll(PollPartRecord poll)
        {
            _pollRepository.Update(poll);
        }


        public void CreateChoice(PollChoiceRecord choice)
        {
            _choiceRepository.Create(choice);
            _resultRepository.Create(new PollResultRecord
            {
                Count=0,
                PollChoiceRecord=choice
            });
        }


        public void UpdateChoice(PollChoiceRecord choice)
        {
            _choiceRepository.Update(choice);
        }
        

        public void DeleteChoice(PollChoiceRecord choice)
        {
            var results = _resultRepository.Fetch(r => r.PollChoiceRecord.Id == choice.Id);
            var votes = _voteRepository.Fetch(v => v.PollChoiceRecord.Id == choice.Id);
            foreach (var r in results)
                _resultRepository.Delete(r);
            foreach (var v in votes)
                _voteRepository.Delete(v);
            _choiceRepository.Delete(choice);
        }


        public IContentQuery<PollPart, PollPartRecord> GetPolls(bool isShown)
        {
            return _orchardServices.ContentManager
                .Query<PollPart, PollPartRecord>()
                .Where(poll => poll.Shown == isShown);
        }

        
        public IContentQuery<PollPart, PollPartRecord> GetPolls(bool isShown, bool isOpen = true)
        {
            if (isOpen == true)
            {
                return _orchardServices.ContentManager
                    .Query<PollPart, PollPartRecord>()
                    .Where(poll => poll.Shown == isShown && DateTime.Now < poll.CloseDateUtc);
            }
            else if (isOpen == false)
            {
                return _orchardServices.ContentManager
                    .Query<PollPart, PollPartRecord>()
                    .Where(poll => poll.Shown == isShown && DateTime.Now < poll.OpenDateUtc && DateTime.Now > poll.CloseDateUtc);
            }
            else
            {
                return _orchardServices.ContentManager
                    .Query<PollPart, PollPartRecord>()
                    .Where(poll => poll.Shown == isShown);
            }
        }

        public IEnumerable<PollPart> GetClosedPolls(bool isShown, bool isPollClosed)
        {
            var closedPolls = new List<PollPart>();
            var allShownPolls = _orchardServices.ContentManager
                .Query<PollPart, PollPartRecord>()
                .Where(poll => poll.Shown == isShown).List();

            //var allShownPolls = _pollRepository.Fetch(poll => poll.Shown == isShown).AsQueryable();

            foreach (PollPart p in allShownPolls)
            {
                if (isPollClosed == IsPollClosed(p.Id))
                {
                    if(DateTime.Now > p.Record.CloseDateUtc)
                        closedPolls.Add(p);
                }
            }
            return closedPolls;
        }
             


        public void OpenPoll(int id)
        {
            var poll = _pollRepository.Get(id);
            poll.Shown = true;
            _pollRepository.Update(poll);
        }

        public void ClosePoll(int id)
        {
            var poll = _pollRepository.Get(id);
            poll.Shown = false;
            _pollRepository.Update(poll);
        }


        public void DeletePoll(int id)
        {
            _orchardServices.ContentManager.Remove(_orchardServices.ContentManager.Get(id));
            _pollRepository.Delete(_pollRepository.Get(id));
        }

        public PollResultRecord GetResult(int choiceId)
        {
            return _resultRepository.Get(r=>r.PollChoiceRecord.Id == choiceId);
        }


        public PollResultViewModel GetPollWithResult(int id)
        {
            PollPartRecord poll = GetPoll(id);
            PollResultViewModel model = new PollResultViewModel
            {
                Question = poll.Question,
                Shown = poll.Shown,
                Choices = GetChoices(id).Select(c => new PollResultEntry { Choice = c, Result = GetResult(c.Id) }).ToList()
            };
            return model;
        }


        public bool IsPollClosed(int pollId)
        {
            var poll = GetPoll(pollId);
            return DateTime.Now < poll.OpenDateUtc || DateTime.Now > poll.CloseDateUtc;
        }

        // Is it a music poll?
        public bool IsMusicPoll(int pollId)
        {
            var poll = GetPoll(pollId);
            return poll.MusicPoll;
        }

        // Is it time to show the voting UI?
        public bool ShowVotingUI(int pollId)
        {
            var poll = GetPoll(pollId);
            return poll.ShowVotingUI;
        }

        // Is the Music Poll in Test mode (allowing more than one song to be nominated by the same user)?
        public bool IsMusicPollInTestMode(int pollId)
        {
            var poll = GetPoll(pollId);
            return poll.MusicPollInTestMode;
        }

        // Do we have a duplicate vote already in the repo for the same poll and the same user?
        public bool IsVoteDuplicate(int pollId, string userName, int pollChoiceRecordId)
        {
            var votes = _voteRepository.Fetch(v => v.PollPartRecord.Id == pollId)
                .Where(v => v.Username == userName && v.PollChoiceRecord.Id == pollChoiceRecordId);
            if(votes.Count() > 0)
            {
                return true;
            }
            return false;
        }

        public string GetPollTitle(int pollId)
        {
            var pollContentItem = _contentManager.Get(pollId);
            var pollTitle = pollContentItem.As<TitlePart>().Title;
            return pollTitle;
        }

        public IQueryable<PollVoteRecord> GetVotes(int pollId)
        {
            return _voteRepository.Fetch(v => v.PollPartRecord.Id == pollId).AsQueryable();
        }

        public IQueryable<PollVoteRecord> GetSongVotes(int pollId, int pollChoiceRecordId)
        {
            // return _voteRepository.Fetch(v => v.PollPartRecord.Id == pollId && v.PollChoiceRecord.Id == pollChoiceRecordId).AsQueryable();
            return _voteRepository.Fetch(v => v.PollChoiceRecord.Id == pollChoiceRecordId).AsQueryable();
        }

        public PollResultRecord GetSongResult(int pollChoiceRecordId)
        {            
            return _resultRepository.Get(r => r.PollChoiceRecord.Id == pollChoiceRecordId);
        }

        public IQueryable<PollVoteRecord> GetUserVotes(int pollId, string userName)
        {
            return _voteRepository.Fetch(v => v.PollPartRecord.Id == pollId).Where(v => v.Username == userName).AsQueryable();
        }

        public IQueryable<PollVoteRecord> GetUserVotesForSong(int pollId, string userName, int pollChoiceRecordId)
        {
            return _voteRepository.Fetch(v => v.PollPartRecord.Id == pollId).Where(v => v.Username == userName && v.PollChoiceRecord.Id == pollChoiceRecordId).AsQueryable();
        }

        public void EditPoll(PollPart part, PollViewModel model)
        {
            if (part == null)
                throw new ArgumentNullException("part");
            if (model == null)
                throw new ArgumentNullException("model");
                        
            var poll = new PollPartRecord
            {
                Id = part.Record.ContentItemRecord.Id,

                Question = model.Question,
                OpenDateUtc = model.Open,
                CloseDateUtc = model.Close,
                MaxVotes = model.MaxVotes,
                Shown = model.Shown,
                MusicPoll = model.MusicPoll,
                ShowVotingUI = model.ShowVotingUI,

                ContentItemRecord = part.Record.ContentItemRecord
            };
            UpdatePoll(poll);

            foreach (var entry in model.Choices)
            {
                switch (entry.Action)
                {
                    case "Alter":
                        var choice = new PollChoiceRecord
                        {
                            Id = entry.Choice.Id,
                            Answer = entry.Choice.Answer,
                            PollPartRecord = part.Record                            
                        };
                        UpdateChoice(choice);
                        break;
                    case "Create":
                        if (entry.Choice.Answer != null && entry.Choice.Answer.Length > 0)
                        {
                            choice = new PollChoiceRecord
                            {
                                Answer = entry.Choice.Answer,
                                PollPartRecord = part.Record
                                
                            };
                            CreateChoice(choice);
                        }
                        break;
                }
            }

            var choices = GetChoices(poll.Id).ToList();
            var newChoices = model.Choices.Select(c => choices.FirstOrDefault(x => x.Id == c.Choice.Id || x.Answer == c.Choice.Answer));
            var toDelete = choices.Except(newChoices);
            foreach (var choice in toDelete)
                DeleteChoice(choice);
        }
    }
}