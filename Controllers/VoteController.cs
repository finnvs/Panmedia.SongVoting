using Panmedia.SongVoting.Models;
using Panmedia.SongVoting.Services;
using Panmedia.SongVoting.ViewModels;
using Orchard.ContentManagement;
using Orchard;
using Orchard.UI.Notify;
using Orchard.Localization;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Mvc;
using System.Collections.Generic;
using Orchard.Themes;
using Orchard.DisplayManagement;
using Orchard.Mvc;
using Panmedia.Artist.Services;

namespace Panmedia.SongVoting.Controllers
{
    [ComVisible(false)]
    [Authorize]
    public class VoteController : Controller
    {
        private readonly IPollService _pollService;
        private readonly IOrchardServices _orchardServices;
        private readonly IVotingService _votingService;
        private readonly ISongService _songService;
        private readonly IOrchardServices Services;
        private readonly IArtistUserService _artistService;
        dynamic Shape { get; set; }

        public VoteController(
            IPollService pollService,
            IOrchardServices orchardServices,
            IVotingService votingService,
            ISongService songService,
            IOrchardServices services,
            IShapeFactory shapeFactory,
            IArtistUserService artistService)
        {
            _pollService = pollService;
            _orchardServices = orchardServices;
            _votingService = votingService;
            _songService = songService;
            Services = services;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            _artistService = artistService;
        }


        public Localizer T { get; set; }

        [Themed]
        public ActionResult Index()
        {
            // Get the currently active list of poll content items (ie. where IsShown == true and IsOpen == true)            
            // Return an IEnumerable of PollParts to view, and iterate over it there as itemlinks
            IContentQuery<PollPart, PollPartRecord> query = _pollService.GetPolls(true, true);
            IEnumerable<PollPart> activePolls = query.List<PollPart>();

            var pollSelectList = _songService.MusicPolls();

            var model = new PollIndexViewModel() 
            {
                Polls = activePolls,
                ShowEnterSongBtn = (pollSelectList.Count() >= 1)
            };
            
            return View(model);
        }

        
        [Themed]
        public ActionResult ClosedPollIndex()
        {
            // Get a list of closed poll content items (ie. where IsShown == true, but where Poll dates have passed)            
            // Return an IEnumerable of PollParts to view, and iterate over it there as itemlinks            
            IEnumerable<PollPart> closedPolls = _pollService.GetClosedPolls(true, true);   
            return View(closedPolls);
        }

        [Themed]
        public ActionResult GetPoll(int pollId)
        {
            var poll = _pollService.GetPoll(pollId);            
            if (poll == null)
                return Json(new { success = false, error = "Poll doesn't exist!" });

            // test for show voting UI flag (set in admin edit view)
            if (!poll.ShowVotingUI)
            {
                var pollChoices = _pollService.GetChoices(poll.Id).ToList();
                // var songs = _songService.GetSongsForPoll(poll.Id);                

                var castratedChoices = new List<ChoiceEntry>();
                foreach(PollChoiceRecord p in pollChoices)
                {
                    castratedChoices.Add(new ChoiceEntry {
                        Choice = p,
                        Action = "Alter",
                        Song = _songService.GetSongByChoiceId(p.Id)
                        });
                }


                var castratedPollViewModel = new object();
                castratedPollViewModel = Shape.Parts_CastratedPoll(
                    Id: poll.Id,
                    Question: poll.Question,
                    OpenDateUtc: poll.OpenDateUtc,
                    CloseDateUtc: poll.CloseDateUtc,
                    MaxVotes: poll.MaxVotes,
                    IsShown: poll.Shown,
                    IsOpen: !_pollService.IsPollClosed(poll.Id),
                    IsMusicPoll: _pollService.IsMusicPoll(poll.Id),
                    ShowVotingUI: _pollService.ShowVotingUI(poll.Id),
                    Choices: castratedChoices,
                    PollTitle: _pollService.GetPollTitle(poll.Id)              
                    );

                return new ShapeResult(this, castratedPollViewModel);
            }

            // query for any existing poll votes for current user
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            string user = currentUser == null ? "Anonymous" : currentUser.UserName;

            var userChoiceList = new List<PollChoiceRecord>();
            var existingVotes = _pollService.GetUserVotes(pollId, user);
            if (existingVotes.Count() >= 1)
            {
                // make um tmp list choices from list PollVoteRecords                
                foreach (PollVoteRecord record in existingVotes)
                {
                    userChoiceList.Add(record.PollChoiceRecord);
                }
            }

            // separate 'normal' choices from tmp list for display underneath
            var freePollChoices = _pollService.GetChoices(pollId).ToList();
            if (userChoiceList.Count() >= 1)
            {
                foreach (var c in freePollChoices.Reverse<PollChoiceRecord>())
                {
                    if (userChoiceList.Contains(c))
                    {
                        freePollChoices.Remove(c);
                    }
                }
            }

            var userChoices = userChoiceList;
            var freeChoices = freePollChoices;
            var model = new object();

            if (userChoices.Count() >= 1)
            {

                // user has already voted in poll
                model = Shape.Parts_UserPoll(
                    Id: poll.Id,
                    Question: poll.Question,
                    OpenDateUtc: poll.OpenDateUtc,
                    CloseDateUtc: poll.CloseDateUtc,
                    MaxVotes: poll.MaxVotes,
                    IsShown: poll.Shown,
                    IsOpen: !_pollService.IsPollClosed(poll.Id),
                    IsMusicPoll: _pollService.IsMusicPoll(poll.Id),
                    ShowVotingUI: _pollService.ShowVotingUI(poll.Id),
                    PollTitle: _pollService.GetPollTitle(poll.Id),
                    UserChoices: userChoices,
                    FreeChoices: freeChoices,
                    UserName: _artistService.GetFirstName(user)
                    );

            }
            else
            {
                // new poll, no votes yet by user
                model = Shape.Parts_NewPoll(
                    Id: poll.Id,
                    Question: poll.Question,
                    OpenDateUtc: poll.OpenDateUtc,
                    CloseDateUtc: poll.CloseDateUtc,
                    MaxVotes: poll.MaxVotes,
                    IsShown: poll.Shown,
                    IsOpen: !_pollService.IsPollClosed(poll.Id),
                    IsMusicPoll: _pollService.IsMusicPoll(poll.Id),
                    ShowVotingUI: _pollService.ShowVotingUI(poll.Id),
                    PollTitle: _pollService.GetPollTitle(poll.Id),
                    Choices: _pollService.GetChoices(poll.Id).ToList()
                    );
            }
            return new ShapeResult(this, model);
        }

        [Themed]
        public ActionResult GetClosedPoll(int pollId)
        {
            var poll = _pollService.GetPoll(pollId);
            if (poll == null)
                return Json(new { success = false, error = "Poll doesn't exist!" });
            var pollChoices = _pollService.GetChoices(poll.Id).ToList();                           

            var closedPollChoices = new List<ChoiceEntry>();
            foreach (PollChoiceRecord p in pollChoices)
            {
                closedPollChoices.Add(new ChoiceEntry
                {
                    Choice = p,
                    Action = "Alter",
                    Song = _songService.GetSongByChoiceId(p.Id)
                });
            }


            var closedPollViewModel = new object();
            closedPollViewModel = Shape.Parts_ClosedPoll(
                Id: poll.Id,
                Question: poll.Question,
                OpenDateUtc: poll.OpenDateUtc,
                CloseDateUtc: poll.CloseDateUtc,
                MaxVotes: poll.MaxVotes,
                IsShown: poll.Shown,
                IsOpen: !_pollService.IsPollClosed(poll.Id),
                IsMusicPoll: _pollService.IsMusicPoll(poll.Id),
                ShowVotingUI: _pollService.ShowVotingUI(poll.Id),
                Choices: closedPollChoices,
                PollTitle: _pollService.GetPollTitle(poll.Id)
                );

            return new ShapeResult(this, closedPollViewModel);
        }

        // delete vote will reorder page for each deleted vote choice, and display page again.
        public ActionResult DeleteUserVote(int choiceItemId, int pollId)
        {
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            string user = currentUser == null ? "Anonymous" : currentUser.UserName;
            var choice = _pollService.GetChoices(pollId).Where(c => c.Id == choiceItemId).FirstOrDefault();
            _votingService.DeleteUserVote(choice, user);

            return RedirectToAction("GetPoll", new { pollId = pollId });
        }

        // [AllowAnonymous]
        [HttpPost]
        public JsonResult Vote(int id, string fingerprint, Collection<string> votes)
        {
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            string user = currentUser == null ? "Anonymous" : currentUser.UserName;
            var poll = _pollService.GetPoll(id);
            if (poll == null)
                return Json(new { success = false, error = "Poll doesn't exist!" });
            if (votes == null || votes.Count == 0)
                return Json(new { success = false, error = "You have to choose at least 1 option!" });
            if (votes.Count > poll.MaxVotes)
                return Json(new { success = false, error = "You have chosen too many options!" });
            var voteIds = votes.Select(v => int.Parse(v));
            var list = _pollService.GetChoices(id).Where(c => voteIds.Contains(c.Id));
            if (_votingService.CanVote(id, user, fingerprint))
            {
                // Hvis vi har en MusicPoll, og en choice er uploadet af den samme bruger som stemmer, så hop over
                if (poll.MusicPoll)
                {
                    foreach (var choice in list)
                    {
                        var song = _songService.GetSongByChoiceId(choice.Id);
                        if (song != null)
                        {
                            if (song.SongEditorUserName == user)
                            {
                                // Services.Notifier.Information(@T("Du kan desværre ikke stemme på din egen sang " + song.SongTitle));
                                return Json(new { success = false, error = "Du kan desværre ikke stemme på din egen sang, " + song.SongTitle + "!" });
                            }
                        }

                    }
                }

                // Hvis bruger stemmer på den samme sang / choice to gange hurtigt efter hinanden, fx på mobilen, så spring anden gang over
                foreach (var choice in list)
                {
                    if(_pollService.IsVoteDuplicate(poll.Id, user, choice.Id))
                        return Json(new { success = false, error = "Du har allerede stemt på " + choice.Answer + "!" });
                }

                // TODO: Implementer tilbagemelding til bruger, hvis antallet af stemmer overskrider det tilladte
                // Som det er nu, stopper historien bare ved det sidst tilladte item.
                _votingService.TryVote(id, user, fingerprint, list);
                return Json(new { success = true });
            }
            else
            {
                if (_pollService.IsPollClosed(id))
                    return Json(new { success = false, error = "This poll is closed!" });
                return Json(new { success = false, error = "You have already voted!" });
            }
        }

        //[AllowAnonymous]
        [HttpGet]
        public JsonResult GetResult(int id)
        {
            var results = _pollService.GetPollWithResult(id);
            return Json(new
            {
                result = results.Choices.Select(r =>
                    new Object[] { r.Result.Count,
                        String.Format("<span original-title='{0}' class='tickSpan'>{1}{2}</span>",
                            r.Choice.Answer,
                            r.Choice.Answer.Substring(0, 10 > r.Choice.Answer.Length ? r.Choice.Answer.Length : 10),
                            10 > r.Choice.Answer.Length ? "" : "...") })
            }, JsonRequestBehavior.AllowGet);
        }

        //[AllowAnonymous]
        [HttpPost]
        public JsonResult CanVote(int id, string fingerprint)
        {
            if (_pollService.IsPollClosed(id))
                return Json(new { canVote = false });
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            string user = currentUser == null ? "Anonymous" : currentUser.UserName;
            return Json(new { canVote = _votingService.CanVote(id, user, fingerprint) });
        }
    }
}