using Panmedia.SongVoting.Models;
using Panmedia.SongVoting.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;

namespace Panmedia.SongVoting.Services
{
    [ComVisible(false)]
    public interface ISongService : IDependency
    {       
        void EditSong(SongPart part, SongViewModel model);
        void UpdateSong(SongPartRecord song);
        SongPartRecord GetSong(int id);        
        SongPartRecord GetSongByChoiceId(int choiceItemId);
        IContentQuery<SongPart, SongPartRecord> GetSongs();
        IContentQuery<SongPart, SongPartRecord> GetSongs(bool isShown);

        IList<SongPartRecord> GetSongsForPoll(int pollId);
        IContentQuery<PollPart, PollPartRecord> GetMusicPolls(bool isShown, bool isMusicPoll, bool showVotingUI);
        SelectList MusicPolls();

        IContentQuery<PollPart, PollPartRecord> GetAdminMusicPolls(bool isShown, bool isMusicPoll);
        SelectList AdminMusicPolls();
        void DeleteSong(int id);
    }
}