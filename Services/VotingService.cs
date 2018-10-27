using System;
using System.Collections.Generic;
using Orchard;
using Orchard.Data;
using Orchard.UI.Notify;
using Panmedia.SongVoting.Models;
using Orchard.Localization;

namespace Panmedia.SongVoting.Services
{
    public class VotingService : IVotingService
    {
        private readonly IRepository<PollPartRecord> _pollRepository;
        private readonly IRepository<PollVoteRecord> _voteRepository;
        private readonly IRepository<PollResultRecord> _resultRepository;
        private readonly IOrchardServices Services;


        public VotingService(IRepository<PollPartRecord> pollRepository,
            IRepository<PollVoteRecord> voteRepository,
            IRepository<PollResultRecord> resultRepository,
            IOrchardServices services
           )
        {
            _pollRepository = pollRepository;
            _voteRepository = voteRepository;
            _resultRepository = resultRepository;
            Services = services;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool CanVote(int pollId, string user, string fingerprint)
        {
            if (user == "Anonymous")
                // return _voteRepository.Count(v => v.PollPartRecord.Id == pollId && v.Fingerprint == fingerprint) == 0;
                return false;
            else
            {
                // if MaxVotes isn't reached for user, let user vote.
                PollPartRecord poll = _pollRepository.Get(pollId);
                if (_voteRepository.Count(v => v.PollPartRecord.Id == pollId && v.Username == user) < poll.MaxVotes)
                {
                    return true;
                }
                else
                    return false;

                // statement will return false if one vote is already given
                // return _voteRepository.Count(v => v.PollPartRecord.Id == pollId && v.Username == user) == 0;
            }
        }

        public void TryVote(int pollId, string user, string fingerprint, IEnumerable<PollChoiceRecord> choices)
        {
            if (choices == null)
                throw new ArgumentNullException("choices");
            PollPartRecord poll = _pollRepository.Get(pollId);

            //var existingVotesFromUser = _voteRepository.Fetch(exv => exv.PollPartRecord.Id == pollId && exv.Username == user);
            //foreach (var choice in choices)
            //{
            //    foreach(var exv in existingVotesFromUser)
            //    {
            //        if (choice.Id == exv.PollChoiceRecord.Id)
            //        {
            //            return;
            //        }
            //    }
            //}

            foreach (var choice in choices)
            {
                if (_voteRepository.Count(v => v.PollPartRecord.Id == pollId && v.Username == user) < poll.MaxVotes)
                {                                    
                   
                    PollVoteRecord vote = new PollVoteRecord
                    {
                        Fingerprint = fingerprint,
                        PollChoiceRecord = choice,
                        PollPartRecord = poll,
                        Username = user,
                        VoteDateUtc = DateTime.Now
                    };
                    _voteRepository.Create(vote);
                    var result = _resultRepository.Get(r => r.PollChoiceRecord.Id == choice.Id);
                    result.Count++;
                    _resultRepository.Update(result);
                }
                else
                {
                    // Poke a finger at the controller
                    // Services.Notifier.Information(T("How Da Der!"));
                }
            }

        }

        public void DeleteUserVote(PollChoiceRecord choice, string user)
        {
            var votes = _voteRepository.Fetch(v => v.PollChoiceRecord.Id == choice.Id && v.Username == user);
            foreach (var v in votes) // there can be only one..           
             _voteRepository.Delete(v);
        }
    }
}