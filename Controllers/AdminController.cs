using Panmedia.SongVoting.Models;
using Panmedia.SongVoting.Services;
using Panmedia.SongVoting.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Contents.Controllers;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using Orchard.Themes;
using Orchard;
using Orchard.Core.Title.Models;
using Panmedia.Artist.Services;

namespace Panmedia.SongVoting.Controllers
{
    [ComVisible(false)]
    public class AdminController : Controller
    {
        private readonly IPollService _pollService;
        private readonly ISongService _songService;
        private readonly ISiteService _siteService;
        private readonly IXmlService _xmlService;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices Services;
        private readonly IArtistUserService _artistUserService;

        public AdminController(
            IPollService pollService,
            ISongService songService,
            ISiteService siteService,
            IXmlService xmlService,
            IShapeFactory shapeFactory,
            IContentManager contentManager,
            IOrchardServices services,
            IArtistUserService artitsUserService)
        {
            _pollService = pollService;
            _songService = songService;
            _siteService = siteService;
            Shape = shapeFactory;
            _xmlService = xmlService;
            _contentManager = contentManager;
            Services = services;
            _artistUserService = artitsUserService;

            T = NullLocalizer.Instance;
        }

        dynamic Shape { get; set; }

        public Localizer T { get; set; }

        public ActionResult Index(PollIndexOptions options, PagerParameters pagerParameters)
        {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // Default options
            if (options == null)
                options = new PollIndexOptions();

            // Filtering
            IContentQuery<PollPart, PollPartRecord> pollsQuery;
            switch (options.Filter)
            {
                case PollIndexFilter.All:
                    pollsQuery = _pollService.GetPolls();
                    break;
                case PollIndexFilter.Open:
                    pollsQuery = _pollService.GetPolls(true);
                    break;
                case PollIndexFilter.Closed:
                    pollsQuery = _pollService.GetPolls(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("options");
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(pollsQuery.Count());
            var entries = pollsQuery
                .OrderByDescending<PollPartRecord>(poll => poll.Id)
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList()
                .Select(poll => CreatePollEntry(poll.Record));

            var model = new AdminIndexViewModel
            {
                Polls = entries.ToList(),
                Options = options,
                Pager = pagerShape
            };
            return View((object)model);
        }

        public ActionResult Edit(int id)
        {
            var poll = _pollService.GetPoll(id);
            var model = new PollViewModel
            {
                Question = poll.Question,
                Open = poll.OpenDateUtc.Value,
                Close = poll.CloseDateUtc.Value,
                MaxVotes = poll.MaxVotes,
                Shown = poll.Shown,
                ShowVotingUI = poll.ShowVotingUI,
                MusicPoll = poll.MusicPoll,
                MusicPollInTestMode = poll.MusicPollInTestMode,
                Choices = _pollService.GetChoices(id)
                                .Select(c => new ChoiceEntry { Choice = c, Action = "Alter" })
                                .ToList()
            };
            // return View((object)model); // vil returnere Edit view under Views/Admin
            return View("EditorTemplates/Parts/Poll", model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection form)
        {
            var viewModel = new AdminIndexViewModel { Polls = new List<PollEntry>(), Options = new PollIndexOptions() };
            UpdateModel(viewModel);

            IEnumerable<PollEntry> checkedEntries = viewModel.Polls.Where(p => p.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PollIndexBulkAction.None:
                    break;
                case PollIndexBulkAction.Open:
                    foreach (PollEntry entry in checkedEntries)
                    {
                        _pollService.OpenPoll(entry.Poll.Id);
                    }
                    break;
                case PollIndexBulkAction.Close:
                    foreach (PollEntry entry in checkedEntries)
                    {
                        _pollService.ClosePoll(entry.Poll.Id);
                    }
                    break;
                case PollIndexBulkAction.Delete:
                    foreach (PollEntry entry in checkedEntries)
                    {
                        _pollService.DeletePoll(entry.Poll.Id);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("form");
            }
            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            if (Services.WorkContext.CurrentUser.UserName == "admin")
            {
                var poll = _pollService.GetPoll(id);
                var choices = _pollService.GetChoices(id);
                var results = _pollService.GetPollWithResult(id);
                var votes = _pollService.GetVotes(id);
                var minDate = votes.Count() == 0 ? DateTime.Now : votes.Min(v => v.VoteDateUtc);
                var maxDate = votes.Count() == 0 ? DateTime.Now : votes.Max(v => v.VoteDateUtc);
                var result1 = new List<VoteEntry>();
                var result2 = new List<VoteEntry>();
                var result3 = new List<VoteEntry>();

                foreach (var choice in choices)
                {
                    var entry1 = new VoteEntry
                    {
                        Choice = choice,
                        Results = new Dictionary<DateTime, int>()
                    };
                    var entry2 = new VoteEntry
                    {
                        Choice = choice,
                        IsAnonymous = true,
                        Results = new Dictionary<DateTime, int>()
                    };
                    var entry3 = new VoteEntry
                    {
                        Choice = choice,
                        IsAnonymous = false,
                        Results = new Dictionary<DateTime, int>()
                    };
                    for (var date = minDate.Date; date <= maxDate.Date; date = date.AddDays(1))
                    {
                        entry1.Results.Add(new DateTime(date.Ticks), votes.Count(v => v.PollChoiceRecord.Id == choice.Id && v.VoteDateUtc.Date == date));
                        entry2.Results.Add(new DateTime(date.Ticks), votes.Count(v => v.PollChoiceRecord.Id == choice.Id && v.VoteDateUtc.Date == date && v.Username == "Anonymous"));
                        entry3.Results.Add(new DateTime(date.Ticks), votes.Count(v => v.PollChoiceRecord.Id == choice.Id && v.VoteDateUtc.Date == date && v.Username != "Anonymous"));
                    }
                    result1.Add(entry1);
                    result2.Add(entry2);
                    result3.Add(entry3);
                }

                var model = new AdminDetailsViewModel
                {
                    Question = poll.Question,
                    Open = poll.OpenDateUtc.Value,
                    Close = poll.CloseDateUtc.Value,
                    MaxVotes = poll.MaxVotes,
                    ResultByChoices = results.Choices,
                    VotesByChoices = result1,
                    AnonymVotesByChoices = result2,
                    UserVotesByChoices = result3
                };
                return View(model);

            }
            else
            {
                Services.Notifier.Information(T("Please log in as Admin user in order to access this method."));
                return Redirect("~/Endpoint");
            }
        }

        public ActionResult SongDetails(int songId)
        {
            if (Services.WorkContext.CurrentUser.UserName == "admin")
            {
                var song = _songService.GetSong(songId);
                var pollChoiceRecordId = song.PollChoiceRecord_Id;
                var poll = _pollService.GetPollFromChoiceId(pollChoiceRecordId);
                var songVotes = _pollService.GetSongVotes(poll.Id, pollChoiceRecordId);
                var songResult = _pollService.GetSongResult(pollChoiceRecordId);                
                var userVotes = new List<SongVoteEntry>();                
                var songTitleAndAuthor = song.SongTitle + " (" + song.SongAuthor + ")";

                foreach (var vote in songVotes)
                {
                    // var results = new Dictionary<DateTime, int>();
                    var songVotesEnteredByUser = _pollService.GetUserVotesForSong(poll.Id, vote.Username, pollChoiceRecordId);
                    var userFullName = "x";
                    if (vote.Username == "Anonymous")
                    {
                        userFullName = "Anonymous";
                    }
                    else { 
                        // get user fullname from vote record via artist service
                        userFullName = _artistUserService.GetFullName(vote.Username);
                    }
                    var entry = new SongVoteEntry
                    {
                        UserFullName = userFullName,
                        NumberOfVotesByUser = songVotesEnteredByUser.ToList()
                    };
                    userVotes.Add(entry);
                    
                }

                var model = new SongDetailsViewModel
                {
                    VotesByUser = userVotes,
                    SongTitleAndAuthor = songTitleAndAuthor                  
                };
                return View(model);
            }
            else
            {
                Services.Notifier.Information(T("Please log in as Admin user in order to access this method."));
                return Redirect("~/Endpoint");
            }
        }

        [HttpPost]
        public ActionResult Delete(int id, string returnUrl)
        {
            if (Services.WorkContext.CurrentUser.UserName == "admin")
            {
                var poll = _pollService.GetPoll(id);
                if (poll == null)
                    return new HttpNotFoundResult();

                _pollService.DeletePoll(id);
                return Redirect(returnUrl);
            }
            else
            {
                Services.Notifier.Information(T("Please log in as Admin user in order to access this method."));
                return Redirect(returnUrl);
            }
        }

        [HttpPost]
        public ActionResult Open(int id, string returnUrl)
        {
            var poll = _pollService.GetPoll(id);
            if (poll == null)
                return new HttpNotFoundResult();

            _pollService.OpenPoll(id);
            return Redirect(returnUrl);
        }

        [HttpPost]
        public ActionResult Close(int id, string returnUrl)
        {
            var poll = _pollService.GetPoll(id);
            if (poll == null)
                return new HttpNotFoundResult();

            _pollService.ClosePoll(id);
            return Redirect(returnUrl);
        }

        // GET
        public ActionResult CreatePoll()
        {            
            var model = new PollViewModel();            
            return View("EditorTemplates/Parts/PollForm", model);
        }

        [Themed]
        [Authorize]
        [HttpPost]
        public ActionResult CreatePoll(PollViewModel poll)
        {
            // Create a new Poll and Choices with values posted
            var newPoll = Services.ContentManager.New("Poll");
            var newPollPart = newPoll.As<PollPart>();

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Information(T("Der opstod en fejl ved validering af data - kontakt evt. webmaster@sang-skriver.dk"));
                return View("EditorTemplates/Parts/PollForm", poll);
            }

            if (ModelState.IsValid)
            {
                // Set title for item - important!
                newPollPart.As<TitlePart>().Title = poll.Question;
                                               
                newPollPart.Question = poll.Question;
                newPollPart.OpenDateUtc = poll.Open;
                newPollPart.CloseDateUtc = poll.Close;
                newPollPart.MaxVotes = poll.MaxVotes;
                newPollPart.IsShown = poll.Shown;
                newPollPart.ShowVotingUI = poll.ShowVotingUI;
                newPollPart.IsMusicPoll = poll.MusicPoll;                
                newPollPart.IsMusicPollInTestMode = poll.MusicPollInTestMode;

                foreach (ChoiceEntry c in poll.Choices)
                {
                    if (c.Choice.Answer != null && c.Choice.Answer.Length > 0)
                    {
                        var choice = new PollChoiceRecord
                        {
                            Answer = c.Choice.Answer,
                            PollPartRecord = newPollPart.Record
                        };
                        _pollService.CreateChoice(choice);
                    }
                    
                }

            }

            // Persist new poll content item (publish)
            Services.ContentManager.Create(newPoll); 
            Services.Notifier.Information(T("Din afstemning er nu oprettet"));

            // var model = new PollViewModel(newPollPart, poll.Choices);
            // return View("Parts/Poll", model);
            return RedirectToAction("Index");
        }

        public PollEntry CreatePollEntry(PollPartRecord poll)
        {
            if (poll == null)
                throw new ArgumentNullException("poll");
            return new PollEntry
            {
                Poll = poll,
                NumberOfChoices = _pollService.GetChoices(poll.Id).Count(),
                IsChecked = false
            };
        }


        [AllowAnonymous]
        [HttpPost]
        public ActionResult Vote(int id, string[] votes)
        {
            return this.Json(new { id = id, votes = votes });
        }

        [HttpGet]
        public FileResult ExportToXml(int id)
        {
            return File(_xmlService.ExportToXml(id), "text/xml", String.Format("Poll_{0}.xml", id));
        }

        [HttpPost]
        public ActionResult ImportFromXml()
        {
            for (int i = 0; i < Request.Files.Count; i++)
            {
                if (Request.Files[i].InputStream != null)
                    _xmlService.Import(Request.Files[i].InputStream);
            }
            return RedirectToAction("Index");
        }

        [Themed]
        // public ActionResult ShowPoll(PollPart poll, int itemid)
       public ActionResult ShowPoll(int itemid)
        {
            var poll = _pollService.GetPoll(itemid);

            var model = new PollViewModel
            {
                Question = poll.Question,
                Open = poll.OpenDateUtc.Value,
                Close = poll.CloseDateUtc.Value,
                MaxVotes = poll.MaxVotes,
                Shown = poll.Shown,
                ShowVotingUI = poll.ShowVotingUI,
                MusicPoll = poll.MusicPoll,                
                MusicPollInTestMode = poll.MusicPollInTestMode,
                Choices = _pollService.GetChoices(itemid)
                                .Select(c => new ChoiceEntry { Choice = c, Action = "Alter" })
                                .ToList()
            };
            return View("Parts/Poll", model);

            // dette sker med itemlinket (ref B Le Roy) så den løsning kan bruges indtil videre
            // alternative: return the expected contentshape to view 'parts/poll': need ref to shapehelper
            //return Combined(
            //    ContentShape("Parts_Poll",
            //        () => shapeHelper.Parts_Poll(
            //        Id: part.Id,
            //        Question: part.Question,
            //        OpenDateUtc: part.OpenDateUtc,
            //        CloseDateUtc: part.CloseDateUtc,
            //        MaxVotes: part.MaxVotes,
            //        IsShown: part.IsShown,
            //        IsOpen: !_pollService.IsPollClosed(part.Id),
            //        Choices: _pollService.GetChoices(part.Id).ToList()))
            //     );


        }

        #region SongAdmin Methods - duplicate here from SongController for Admin UI reasons

        // GET - Admin UI for songs
        public ActionResult SongIndex(SongIndexOptions options, PagerParameters pagerParameters)
        {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // Default options
            if (options == null)
                options = new SongIndexOptions();

            // Filtering
            IContentQuery<SongPart, SongPartRecord> songsQuery;
            switch (options.Filter)
            {
                case SongIndexFilter.All:
                    songsQuery = _songService.GetSongs();
                    break;
                case SongIndexFilter.Open:
                    songsQuery = _songService.GetSongs(true);
                    break;
                case SongIndexFilter.Closed:
                    songsQuery = _songService.GetSongs(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("options");
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(songsQuery.Count());
            var entries = songsQuery
                .OrderByDescending<SongPartRecord>(song => song.Id)
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList()
                .Select(song => CreateSongEntry(song.Record));

            var model = new SongIndexViewModel
            {
                Songs = entries.ToList(),
                Options = options,
                Pager = pagerShape
            };
            return View((object)model);
        }

        public SongEntry CreateSongEntry(SongPartRecord song)
        {
            if (song == null)
                throw new ArgumentNullException("song");
            return new SongEntry
            {
                Song = song,
                IsChecked = false
            };
        }

        // POST - bulk delete action for songs
        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult SongIndex(FormCollection form)
        {
            var viewModel = new SongIndexViewModel { Songs = new List<SongEntry>(), Options = new SongIndexOptions() };
            UpdateModel(viewModel);

            IEnumerable<SongEntry> checkedEntries = viewModel.Songs.Where(s => s.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case SongIndexBulkAction.None:
                    break;
                case SongIndexBulkAction.Open:
                    foreach (SongEntry entry in checkedEntries)
                    {
                        throw new NotImplementedException();
                        // _songService.OpenSong(entry.Song.Id);
                    }
                    break;
                case SongIndexBulkAction.Close:
                    foreach (SongEntry entry in checkedEntries)
                    {
                        throw new NotImplementedException();
                        // _songService.CloseSong(entry.Song.Id);
                    }
                    break;
                case SongIndexBulkAction.Delete:
                    foreach (SongEntry entry in checkedEntries)
                    {
                        _songService.DeleteSong(entry.Song.Id);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("form");
            }
            return RedirectToAction("SongIndex");
        }

        #endregion
    }
}