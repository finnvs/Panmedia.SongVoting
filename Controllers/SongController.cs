using Panmedia.SongVoting.Models;
using Panmedia.SongVoting.Services;
using Panmedia.SongVoting.ViewModels;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Settings;
using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Themes;
using Orchard;
using Orchard.Core.Title.Models;
using Orchard.UI.Notify;
using Orchard.UI.Navigation;
using System.Runtime.InteropServices;
using Orchard.UI.Admin;
using Orchard.Data;
using Orchard.Core.Contents.Controllers;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Orchard.Mvc;

namespace Panmedia.SongVoting.Controllers
{
    [Authorize]
    public class SongController : Controller
    {
        private readonly IOrchardServices Services;
        private readonly IContentManager _contentManager;
        private readonly ISongService _songService;
        private readonly IPollService _pollService;
        private readonly IRepository<PollChoiceRecord> _choiceRepository;
        private readonly ISiteService _siteService;

        public SongController(
            IOrchardServices services,
            IContentManager contentManager,
            ISongService songService,
            IPollService pollService,
            IRepository<PollChoiceRecord> choiceRepository,
            ISiteService siteService,
            IShapeFactory shapeFactory)
        {
            Services = services;
            _contentManager = contentManager;
            _pollService = pollService;
            _songService = songService;
            _choiceRepository = choiceRepository;
            _siteService = siteService;
            Shape = shapeFactory;

            T = NullLocalizer.Instance;
        }

        dynamic Shape { get; set; }

        public Localizer T { get; set; }


        // GET for create song
        [Themed]
        [Authorize]
        public ActionResult Create()
        {
            var model = new SongViewModel();
            model.MusicPolls = _songService.MusicPolls();
            // Hack: intercept af 'Tilmeld sang' knap i UI ved alle musikafstemninger sat på 'Vote'
            // Fordi knappen 'Tilmeld sang' kun findes på Vote controllerens Index method / side, og fordi Create metoden her
            // ellers kun kan kaldes som et 'Create' link fra backenden, kan vi give en specifik tilbagemelding og lave en redirect
            if (model.MusicPolls.Count() <= 0)
            {
                Services.Notifier.Information(T("Der er i øjeblikket ingen afstemninger, som er åbne for nye sange. Klik på et link i oversigten, hvis du ønsker at afgive din stemme."));
                return Redirect("~/Afstemninger/Liste");
            }

            return View("EditorTemplates/Parts/SongForm", model);
        }

        [Themed]
        [Authorize]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(SongViewModel song)
        {
            // Check if Music Poll is in test mode, allowing any number of song entries per user
            // If Poll is not in Test Mode (checkbox not checked in admin panel) allow only one song entry per user per poll
            var pollNo = song.SelectedPollId;            
            if (!_pollService.IsMusicPollInTestMode(pollNo))
            {
                var existingSongsInPoll = _songService.GetSongsForPoll(pollNo);
                if (existingSongsInPoll.Count() >= 1)
                {
                    foreach (var s in existingSongsInPoll)
                    {
                        if (s.SongEditorUserName == Services.WorkContext.CurrentUser.UserName)
                        {
                            Services.Notifier.Information(T("Du har allerede nomineret en sang til denne afstemning - slet den venligst igen, hvis du ønsker at nominere på ny."));
                            return Redirect("~/Endpoint");
                        }
                    }
                }
            }

            // Create song with link to Poll and Choice Id
            var newSong = Services.ContentManager.New("Song");
            var newSongPart = newSong.As<SongPart>();

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Information(T("Der opstod en fejl ved validering af sangens data - kontakt evt. webmaster@sang-skriver.dk"));
                return View("EditorTemplates/Parts/Song", song);
            }

            if (ModelState.IsValid)
            {
                newSongPart.SongTitle = song.SongTitle;
                newSongPart.SongAuthor = song.SongAuthor;

                // special handling of form input (embedded soundcloud html / long youtube url's etc.)
                // @ will escape html chars, ostensibly allowing for embedding soundcloud links
                //if (song.SongUrl.Contains("iframe"))
                //{
                //    newSongPart.SongUrl = HttpUtility.HtmlEncode(@song.SongUrl);
                //}

                if (song.SongUrl.Contains("youtube.com/watch?v="))
                {
                    newSongPart.SongUrl = song.SongUrl.Replace("watch?v=", "embed/");
                }
                else
                {
                    newSongPart.SongUrl = song.SongUrl;
                }

                newSongPart.SongThumbnailUrl = song.SongThumbnailUrl;
                newSongPart.SongDescription = song.SongDescription;

                newSongPart.SongEditorUserName = Services.WorkContext.CurrentUser.UserName;
                newSongPart.SongEditorUserItemId = Services.WorkContext.CurrentUser.Id;
                newSongPart.CreatedDateUtc = DateTime.UtcNow;
                newSongPart.SubmittedDateUtc = DateTime.UtcNow;

                newSongPart.Votes = 0;
                newSongPart.Shown = true;

                // Set title for item - important!
                newSongPart.As<TitlePart>().Title = song.SongTitle;

                // now, create a choice record to latch on to
                var pollId = Convert.ToInt32(song.SelectedPollId);

                var choice = new PollChoiceRecord
                {
                    Answer = song.SongTitle + "  (" + song.SongAuthor + ")",
                    PollPartRecord = _pollService.GetPoll(pollId)
                };

                _pollService.CreateChoice(choice);

                // set PollChoiceRecord_Id to the last choice item in poll, the newest addition. Best we can do..
                var choices = _pollService.GetChoices(pollId);
                if (choices.Count() >= 1)
                {
                    // re-order the random shuffle, so the last in sequence is indeed the last in sequence
                    choices = choices.OrderBy(c => c.Id).AsQueryable<PollChoiceRecord>();
                    newSongPart.PollChoiceRecord_Id = choices.Last().Id;
                }

                // Catch song category choice from chosen radio button vlaue
                switch (HttpContext.Request.Params.Get("SongCategory"))
                {
                    case ("Original"):
                        { newSongPart.SongCategory = SongCategory.Original; }
                        break;

                    case ("Cowrite"):
                        { newSongPart.SongCategory = SongCategory.Cowrite; }
                        break;

                    case ("IntOriginal"):
                        { newSongPart.SongCategory = SongCategory.IntOriginal; }
                        break;
                    case ("IntCowrite"):
                        { newSongPart.SongCategory = SongCategory.IntCowrite; }
                        break;

                    default:
                        newSongPart.SongCategory = SongCategory.Original;
                        break;
                }

            }

            // Persist new song content item (publish)
            Services.ContentManager.Create(newSong);             
            var pollTitle = _pollService.GetPollTitle(pollNo);
            Services.Notifier.Information(T("Din sang er nu tilmeldt " + pollTitle));

            var model = new SongViewModel(newSongPart);

            return View("Parts/Song", model);
        }


        // Modal Popup: Song
        public ActionResult SongModalPopUp(int choiceItemId) // id
        {
            var song = _songService.GetSongByChoiceId(choiceItemId); // id

            if (song != null)
            {
                var model = new SongViewModel
                {
                    SongTitle = song.SongTitle,
                    SongAuthor = song.SongAuthor,
                    SongUrl = song.SongUrl,
                    SongDescription = song.SongDescription,
                    SongCategory = song.SongCategory.ToString(),
                    SongThumbnailUrl = song.SongThumbnailUrl
                };
                return View("Song/SongModal", model);
            }
            return View("Song/SongModalNoSong");

        }

        private bool UserIsLegit(int Id)
        {
            var currentUser = Services.WorkContext.CurrentUser;
            string user = currentUser == null ? "Anonymous" : currentUser.UserName;
            var song = _songService.GetSong(Id);
            if (song.SongEditorUserName == user)
                return true;
            else
                return false;
        }

        public ActionResult Delete(int Id, string returnUrl)
        {
            if (UserIsLegit(Id))
            {
                var song = _songService.GetSong(Id);
                if (song == null)
                    return new HttpNotFoundResult();

                _songService.DeleteSong(Id);
                Services.Notifier.Information(T("Din sang er nu slettet"));
                return Redirect(returnUrl);
            }
            else
            {
                Services.Notifier.Information(T("Dit brugernavn matcher ikke det, der har uploadet sangen - handling afvist."));
                return Redirect(returnUrl);
            }
        }

        public ActionResult DeleteFromAdminPanel(int Id, string returnUrl)
        {
            if (Services.WorkContext.CurrentUser.UserName == "admin")
            {
                var song = _songService.GetSong(Id);
                if (song == null)
                    return new HttpNotFoundResult();

                _songService.DeleteSong(Id);
                Services.Notifier.Information(T("Din sang er nu slettet"));
                return Redirect(returnUrl);
            }
            else
            {
                Services.Notifier.Information(T("Please log in as Admin user in order to access this method."));
                return Redirect(returnUrl);
            }
        }

        // GET
        [Themed]
        public ActionResult EditFromAdminPanel(int Id, string returnUrl)
        {
            if (Services.WorkContext.CurrentUser.UserName == "admin")
            {
                var song = _songService.GetSong(Id);
                if (song == null)
                    return new HttpNotFoundResult();

                var model = new SongViewModel
                {
                    SongTitle = song.SongTitle == null ? "" : song.SongTitle,
                    SongAuthor = song.SongAuthor == null ? "" : song.SongAuthor,
                    SongUrl = song.SongUrl,
                    SongThumbnailUrl = song.SongThumbnailUrl,
                    SongDescription = song.SongDescription,
                    SongCategory = song.SongCategory.ToString(),
                    MusicPolls = _songService.AdminMusicPolls(),
                    PollChoiceRecord_Id = song.PollChoiceRecord_Id,
                    SongPartRecordId = Id,
                    Shown = true,
                    SongEditorUserName = song.SongEditorUserName,
                    SongEditorUserItemId = song.SongEditorUserItemId
                };
                return View("EditorTemplates/Parts/SongForm", model);
            }
            else
            {
                Services.Notifier.Information(T("Please log in as Admin user in order to access this method"));
                return Redirect(returnUrl);
            }
        }

        // POST fra edit screen i admin UI
        [Themed]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditFromAdminPanel(SongViewModel song)
        {
            var part = new SongPartRecord
            {
                Id = song.SongPartRecordId,
                SongTitle = song.SongTitle,
                SongAuthor = song.SongAuthor,
                SongUrl = song.SongUrl,
                SongDescription = song.SongDescription,
                SongThumbnailUrl = song.SongThumbnailUrl,
                PollChoiceRecord_Id = song.PollChoiceRecord_Id,
                Shown = true,
                SongEditorUserName = song.SongEditorUserName,
                SongEditorUserItemId = song.SongEditorUserItemId,
                SubmittedDateUtc = DateTime.UtcNow
            };

            // Catch song category choice from chosen radio button value
            switch (HttpContext.Request.Params.Get("SongCategory"))
            {
                case ("Original"):
                    { part.SongCategory = SongCategory.Original; }
                    break;

                case ("Cowrite"):  //  "2"
                    { part.SongCategory = SongCategory.Cowrite; }
                    break;

                case ("IntOriginal"): // "3"
                    { part.SongCategory = SongCategory.IntOriginal; }
                    break;
                case ("IntCowrite"): //  "4"
                    { part.SongCategory = SongCategory.IntCowrite; }
                    break;

                default:
                    part.SongCategory = SongCategory.Original;
                    break;
            }

            // slet den gamle Poll Choice Record
            var oldPollChoiceRecord = _choiceRepository.Fetch(c => c.Id == song.PollChoiceRecord_Id).FirstOrDefault();

            if (oldPollChoiceRecord != null)
            {
                _pollService.DeleteChoice(oldPollChoiceRecord);
            }

            // opret ny poll choice record, linked til den valgte poll id fra selectlisten
            var pollId = Convert.ToInt32(song.SelectedPollId);

            var choice = new PollChoiceRecord
            {
                Answer = song.SongTitle + "  (" + song.SongAuthor + ")",
                PollPartRecord = _pollService.GetPoll(pollId)
            };

            _pollService.CreateChoice(choice);

            // set PollChoiceRecord_Id to the last choice item in poll, the newest addition. Best we can do..
            var choices = _pollService.GetChoices(pollId);
            if (choices.Count() >= 1)
            {
                // re-order the random shuffle, so the last in sequence is indeed the last in sequence
                choices = choices.OrderBy(c => c.Id).AsQueryable<PollChoiceRecord>();
                part.PollChoiceRecord_Id = choices.Last().Id;
            }

            _songService.UpdateSong(part);           

            var songContentItem = _contentManager.Get(part.Id, VersionOptions.Latest);
            var songPart = songContentItem.As<SongPart>();
            var model = new SongViewModel(songPart);

            Services.Notifier.Information(T("Sangen er nu redigeret"));

            // return View("EditorTemplates/Parts/SongForm", model);
            // return View("Parts/Song", model);
            return RedirectToAction("SongIndex", "Admin");

            // var songViewModel = Shape.Parts_Song(part);
            // return new ShapeResult(this, songViewModel(typeof(SongViewModel)));

            //return Redirect("~/Afstemninger/Liste");
        }

        #region Edit_Song_Url_While_Voting_In_Progress

        // GET Edit Song Url from Admin Panel while voting is in progress
        [Themed]
        public ActionResult EditSongUrlFromAdminPanel(int Id, string returnUrl)
        {
            if (Services.WorkContext.CurrentUser.UserName == "admin")
            {
                var song = _songService.GetSong(Id);
                if (song == null)
                    return new HttpNotFoundResult();

                var model = new SongUrlViewModel
                {
                    SongTitle = song.SongTitle == null ? "" : song.SongTitle,
                    SongAuthor = song.SongAuthor == null ? "" : song.SongAuthor,
                    SongUrl = song.SongUrl,                    
                    SongDescription = song.SongDescription,                    
                    SongPartRecordId = Id                    
                };
                return View("EditorTemplates/Parts/SongUrlForm", model);
            }
            else
            {
                Services.Notifier.Information(T("Please log in as Admin user in order to access this method"));
                return Redirect(returnUrl);
            }
        }

        // POST fra edit song url screen i admin UI
        [Themed]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditSongUrlFromAdminPanel(SongUrlViewModel songUrlVM)
        {            
            var songPart = _songService.GetSong(songUrlVM.SongPartRecordId);

            var part = new SongPartRecord
            {
                // populate from posted form
                Id = songUrlVM.SongPartRecordId,
                SongTitle = songUrlVM.SongTitle,
                SongAuthor = songUrlVM.SongAuthor,
                SongUrl = songUrlVM.SongUrl,
                SongDescription = songUrlVM.SongDescription,
                // populate from part record fetch
                SongThumbnailUrl = songPart.SongThumbnailUrl,
                PollChoiceRecord_Id = songPart.PollChoiceRecord_Id,
                Shown = true,
                SongEditorUserName = songPart.SongEditorUserName,
                SongEditorUserItemId = songPart.SongEditorUserItemId,
                // hard code category to ''original' for now
                SongCategory = SongCategory.Original,
                // Set submitted datetime
                SubmittedDateUtc = DateTime.UtcNow
            };

            // Update Poll Choice Record, if Song Author or Song Title has changed
            var pollChoiceRecord = _choiceRepository.Fetch(c => c.Id == songPart.PollChoiceRecord_Id).FirstOrDefault();

            if (pollChoiceRecord != null)
            {
                pollChoiceRecord.Answer = part.SongTitle + "  (" + part.SongAuthor + ")";                
                _pollService.UpdateChoice(pollChoiceRecord);
            }

            _songService.UpdateSong(part);           
            Services.Notifier.Information(T("Sangen er nu redigeret"));    
            return RedirectToAction("SongIndex", "Admin");
        }

        #endregion

        #region End User Edit Scenario - fra knapper i afstemnings UI 

        // GET
        [Themed]
        public ActionResult EndUserEdit(int Id, string returnUrl)
        {
            if (UserIsLegit(Id))
            {
                var song = _songService.GetSong(Id);
                if (song == null)
                    return new HttpNotFoundResult();

                var model = new SongViewModel
                {
                    SongTitle = song.SongTitle == null ? "" : song.SongTitle,
                    SongAuthor = song.SongAuthor == null ? "" : song.SongAuthor,
                    SongUrl = song.SongUrl,
                    SongThumbnailUrl = song.SongThumbnailUrl,
                    SongDescription = song.SongDescription,
                    SongCategory = song.SongCategory.ToString(),
                    PollChoiceRecord_Id = song.PollChoiceRecord_Id,
                    SongPartRecordId = Id,
                    Shown = true,
                    SongEditorUserName = song.SongEditorUserName,
                    SongEditorUserItemId = song.SongEditorUserItemId
                };
                return View("EditorTemplates/Parts/EndUserSongForm", model);
            }
            else
            {
                Services.Notifier.Information(T("Dit brugernavn matcher ikke det, der har uploadet sangen - handling afvist."));
                return Redirect(returnUrl);
            }
        }


        [Themed]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EndUserEdit(SongViewModel song)
        {
            var part = new SongPartRecord
            {
                Id = song.SongPartRecordId,
                SongTitle = song.SongTitle,
                SongAuthor = song.SongAuthor,
                SongUrl = song.SongUrl,
                SongDescription = song.SongDescription,
                SongThumbnailUrl = song.SongThumbnailUrl,
                PollChoiceRecord_Id = song.PollChoiceRecord_Id,
                Shown = true,
                SongEditorUserName = song.SongEditorUserName,
                SongEditorUserItemId = song.SongEditorUserItemId,
                SubmittedDateUtc = DateTime.UtcNow
            };

            // Catch song category choice from chosen radio button value
            switch (HttpContext.Request.Params.Get("SongCategory"))
            {
                case ("Original"):
                    { part.SongCategory = SongCategory.Original; }
                    break;

                case ("Cowrite"):  //  "2"
                    { part.SongCategory = SongCategory.Cowrite; }
                    break;

                case ("IntOriginal"): // "3"
                    { part.SongCategory = SongCategory.IntOriginal; }
                    break;
                case ("IntCowrite"): //  "4"
                    { part.SongCategory = SongCategory.IntCowrite; }
                    break;

                default:
                    part.SongCategory = SongCategory.Original;
                    break;
            }           

            // Update Poll Choice Record, if Song Author or Song Title has changed
            var pollChoiceRecord = _choiceRepository.Fetch(c => c.Id == song.PollChoiceRecord_Id).FirstOrDefault();

            if (pollChoiceRecord != null)
            {
                pollChoiceRecord.Answer = part.SongTitle + "  (" + part.SongAuthor + ")";
                _pollService.UpdateChoice(pollChoiceRecord);
            }

            _songService.UpdateSong(part);

            Services.Notifier.Information(T("Din sang er nu redigeret"));

            return Redirect("~/Afstemninger/Liste");
        }
        #endregion

        #region Song admin methods - these are duplicated in Admin controller (for styling)

        [Themed]
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
        [Orchard.Mvc.FormValueRequired("submit.BulkEdit")]
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

