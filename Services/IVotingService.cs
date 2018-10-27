using Panmedia.SongVoting.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Panmedia.SongVoting.Services
{
    public interface IVotingService : IDependency
    {
        bool CanVote(int pollId, string user, string fingerprint);
        void TryVote(int pollId, string user, string fingerprint, IEnumerable<PollChoiceRecord> choices);
        void DeleteUserVote(PollChoiceRecord choice, string user);
    }
}
