using Panmedia.SongVoting.Models;
using Panmedia.SongVoting.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace Panmedia.SongVoting.Services
{
    [ComVisible(false)]
    public interface IPollService : IDependency
    {
        void CreatePoll(PollPart poll);
        void EditPoll(PollPart part, PollViewModel model);
        void UpdatePoll(PollPartRecord poll);
        PollPartRecord GetPoll(int id);
        PollPartRecord GetPollFromChoiceId(int id);
        IContentQuery<PollPart, PollPartRecord> GetPolls();
        IContentQuery<PollPart, PollPartRecord> GetPolls(bool isShown);
        IContentQuery<PollPart, PollPartRecord> GetPolls(bool isShown, bool isOpen);
        IEnumerable<PollPart> GetClosedPolls(bool isShown, bool isPollClosed);
        void OpenPoll(int id);
        void ClosePoll(int id);
        void DeletePoll(int id);
        void CreateChoice(PollChoiceRecord choice);
        void UpdateChoice(PollChoiceRecord choice);
        void DeleteChoice(PollChoiceRecord choice);
        IQueryable<PollChoiceRecord> GetChoices(int pollId);
        IQueryable<PollChoiceRecord> GetOrderedChoices(int pollId);
        PollResultViewModel GetPollWithResult(int id);
        PollResultRecord GetSongResult(int pollChoiceRecordId);
        IQueryable<PollVoteRecord> GetVotes(int pollId);
        IQueryable<PollVoteRecord> GetSongVotes(int pollId, int pollChoiceRecordId);
        IQueryable<PollVoteRecord> GetUserVotes(int pollId, string userName);
        IQueryable<PollVoteRecord> GetUserVotesForSong(int pollId, string userName, int pollChoiceRecordId);
        bool IsPollClosed(int pollId);
        bool IsMusicPoll(int pollId);
        bool ShowVotingUI(int pollId);
        bool IsMusicPollInTestMode(int pollId);
        bool IsVoteDuplicate(int pollId, string userName, int pollVoteRecordId);
        string GetPollTitle(int pollId);
    }
}