using Panmedia.SongVoting.Models;
using Panmedia.SongVoting.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace Panmedia.SongVoting.Handlers
{
    [ComVisible(false)]
    public class PollPartHandler : ContentHandler
    {
        public PollPartHandler(IRepository<PollPartRecord> pollRepository, IPollService pollService)
        {
            Filters.Add(StorageFilter.For(pollRepository));

            // Cascade delete any choices / answers stored for this poll
            OnRemoving<PollPart>((context, polls) =>
                {
                    var choices = pollService.GetChoices(polls.Id).ToList();
                    foreach (var c in choices)
                    {
                        pollService.DeleteChoice(c);
                    }
                });
        }
    }
}