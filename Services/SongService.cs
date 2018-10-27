using Panmedia.SongVoting.Models;
using Panmedia.SongVoting.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Core.Title.Models;

namespace Panmedia.SongVoting.Services
{
    public class SongService : ISongService
    {
        private readonly IRepository<SongPartRecord> _songRepository;
        private readonly IRepository<PollChoiceRecord> _choiceRepository;
        private readonly IPollService _pollService;
        private readonly IOrchardServices _orchardServices;


        public SongService(
            IRepository<SongPartRecord> songRepository,
            IRepository<PollChoiceRecord> choiceRepository,
            IPollService pollService,
            IOrchardServices orchardServices)
        {
            _songRepository = songRepository;
            _choiceRepository = choiceRepository;
            _pollService = pollService;
            _orchardServices = orchardServices;
        }

        public SongPartRecord GetSongByChoiceId(int choiceItemId)
        {
            var songItem = _orchardServices.ContentManager
                .Query<SongPart, SongPartRecord>()
                .Where(song => song.PollChoiceRecord_Id == choiceItemId)
                .Slice(1).FirstOrDefault();

            if (songItem != null)
            {
                var itemContentId = songItem.ContentItem.Id;
                return _songRepository.Get(itemContentId);
            }
            return null;
        }

        public SongPartRecord GetSong(int Id)
        {
            return _songRepository.Get(Id);
        }

        public IContentQuery<SongPart, SongPartRecord> GetSongs()
        {
            return _orchardServices.ContentManager
                .Query<SongPart, SongPartRecord>();
        }

        public IContentQuery<SongPart, SongPartRecord> GetSongs(bool isShown)
        {
            return _orchardServices.ContentManager
                .Query<SongPart, SongPartRecord>()
                .Where(song => song.Shown == isShown);
        }

        // Return all songs shown for a specific poll
        public IList<SongPartRecord> GetSongsForPoll(int pollId)
        {
            var songList = new List<SongPartRecord>();
            var choices = _pollService.GetChoices(pollId);
            if (choices.Count() >= 1)
            {
                foreach (var c in choices)
                {
                    if (GetSongByChoiceId(c.Id) != null)
                        songList.Add(GetSongByChoiceId(c.Id));
                }
            }
            return songList;
        }

        // Query to populate SelectList dropdown in song submit view, in order to let the user choose 
        // a currently active music poll which has showVotingUI set to false
        public IContentQuery<PollPart, PollPartRecord> GetMusicPolls(bool isShown, bool isMusicPoll, bool showVotingUI)
        {
            return _orchardServices.ContentManager
                .Query<PollPart, PollPartRecord>()
                .Where(poll => poll.Shown == isShown && poll.MusicPoll == isMusicPoll && poll.ShowVotingUI == showVotingUI);
        }

        public SelectList MusicPolls()
        {
            List<SelectListItem> MusicPollList = new List<SelectListItem>();
            var MPolls = GetMusicPolls(true, true, false).List();
            if (MPolls.Count() >= 1)
            {
                foreach (PollPart p in MPolls)
                {
                    if (DateTime.Now < p.Record.OpenDateUtc || DateTime.Now > p.Record.CloseDateUtc)
                    {
                        // skip Music Polls not within datetime range
                    }
                    else
                    {
                        var item = new SelectListItem { Text = p.ContentItem.As<TitlePart>().Title, Value = p.Id.ToString() };
                        MusicPollList.Add(item);
                    }
                };
                return new SelectList(MusicPollList, "Value", "Text");
            }
            return new SelectList(MusicPollList, "Value", "Text");
        }

        // Query to populate SelectList dropdown when editing a song from the admin UI - regardless of
        // whether the voting UI is shown or not, but including only currently shown music polls 
        public IContentQuery<PollPart, PollPartRecord> GetAdminMusicPolls(bool isShown, bool isMusicPoll)
        {
            return _orchardServices.ContentManager
                .Query<PollPart, PollPartRecord>()
                .Where(poll => poll.Shown == isShown && poll.MusicPoll == isMusicPoll);
        }

        public SelectList AdminMusicPolls()
        {
            List<SelectListItem> MusicPollList = new List<SelectListItem>();
            var MPolls = GetAdminMusicPolls(true, true).List();
            if (MPolls.Count() >= 1)
            {
                foreach (PollPart p in MPolls)
                {
                    if (DateTime.Now < p.Record.OpenDateUtc || DateTime.Now > p.Record.CloseDateUtc)
                    {
                        // skip Music Polls not within datetime range
                    }
                    else
                    {
                        var item = new SelectListItem { Text = p.ContentItem.As<TitlePart>().Title, Value = p.Id.ToString() };
                        MusicPollList.Add(item);
                    }
                };
                return new SelectList(MusicPollList, "Value", "Text");
            }
            return new SelectList(MusicPollList, "Value", "Text");
        }        

        public void UpdateSong(SongPartRecord song)
        {
            _songRepository.Update(song);
        }

        // Check for a Poll Choice connected to the song, and delete it too
        public void DeleteSong(int id)
        {
            var song = _songRepository.Get(id);
            var choice = _choiceRepository.Get(song.PollChoiceRecord_Id);

            _orchardServices.ContentManager.Remove(_orchardServices.ContentManager.Get(id));
            _songRepository.Delete(_songRepository.Get(id));

            if (choice != null)
                _choiceRepository.Delete(choice);
        }
        
        public void EditSong(SongPart part, SongViewModel model)
        {
            if (part == null)
                throw new ArgumentNullException("part");
            if (model == null)
                throw new ArgumentNullException("model");

            var song = new SongPartRecord
            {
                Id = part.Record.ContentItemRecord.Id,
                SongTitle = model.SongTitle,
                SongAuthor = model.SongAuthor,
                SongUrl = model.SongUrl,
                SongDescription = model.SongDescription,                
                ContentItemRecord = part.Record.ContentItemRecord
            };

            switch (model.SongCategory)
            {
                case ("Original"):  // "1"
                    { song.SongCategory = SongCategory.Original; }
                    break;
                case ("Cowrite"):  //  "2"
                    { song.SongCategory = SongCategory.Cowrite; }
                    break;
                case ("IntOriginal"): // "3"
                    { song.SongCategory = SongCategory.IntOriginal; }
                    break;
                case ("IntCowrite"): //  "4"
                    { song.SongCategory = SongCategory.IntCowrite; }
                    break;
                default:
                    song.SongCategory = SongCategory.Original;
                    break;
            }

            // slet den gamle Poll Choice Record
            var oldPollChoiceRecord = _choiceRepository.Fetch(r => r.Id == part.PollChoiceRecord_Id).FirstOrDefault();

            if (oldPollChoiceRecord != null)
            {               
                _pollService.DeleteChoice(oldPollChoiceRecord);
            }

            var pollId = Convert.ToInt32(model.SelectedPollId);

            var choice = new PollChoiceRecord
            {
                Answer = song.SongTitle,
                PollPartRecord = _pollService.GetPoll(pollId)
            };

            _pollService.CreateChoice(choice);

            // First, get all the poll choices in an ordered list.
            // then, set PollChoiceRecord_Id to the last choice item in poll, the newest addition. Best we can do..
            var choices = _pollService.GetOrderedChoices(pollId);
            if (choices.Count() >= 1)
            {
                song.PollChoiceRecord_Id = choices.Last().Id;
            }

            UpdateSong(song);
        }
    }
}