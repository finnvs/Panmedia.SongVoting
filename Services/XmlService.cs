using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Panmedia.SongVoting.Models;
using Orchard.Data;
using System.Web.Mvc;
using Orchard.ContentManagement;

namespace Panmedia.SongVoting.Services
{
    public class XmlService : IXmlService
    {

        private readonly IRepository<PollPartRecord> _pollRepository;
        private readonly IRepository<PollChoiceRecord> _choiceRepository;
        private readonly IRepository<PollVoteRecord> _voteRepository;
        private readonly IRepository<PollResultRecord> _resultRepository;
        private readonly IContentManager _contentManager;

        public XmlService(IRepository<PollPartRecord> pollRepository,
            IRepository<PollChoiceRecord> choiceRepository,
            IRepository<PollVoteRecord> voteRepository,
            IRepository<PollResultRecord> resultRepository,
            IContentManager contentManager)
        {
            _pollRepository = pollRepository;
            _choiceRepository = choiceRepository;
            _voteRepository = voteRepository;
            _resultRepository = resultRepository;
            _contentManager = contentManager;
        }

        public byte[] ExportToXml(int poll_id)
        {

            var poll = _pollRepository.Get(poll_id);
            var choices = _choiceRepository.Fetch(c => c.PollPartRecord.Id == poll_id).AsQueryable();
            var results = _resultRepository.Fetch(r => r.PollChoiceRecord.PollPartRecord.Id == poll_id);

            StringBuilder sb = new StringBuilder(); //TODO: Not used?
            var entry = new PollEntry
            {
                Question = poll.Question,
                OpenDateUtc = poll.OpenDateUtc.Value,
                CloseDateUtc = poll.CloseDateUtc.Value,
                MaxVotes = poll.MaxVotes,
                IsShown = poll.Shown,
                IsMusicPoll = poll.MusicPoll,
                ShowVotingUI = poll.ShowVotingUI,
                Choices = results.Select(r => new PollEntry.Choice
                {
                    Answer = r.PollChoiceRecord.Answer,
                    Count = r.Count
                }).ToList()
            };
            XmlSerializer serializer = new XmlSerializer(typeof(PollEntry));
            MemoryStream ms = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(ms, new XmlWriterSettings() { CheckCharacters = true });
            serializer.Serialize(writer, entry);
            return ms.ToArray();
        }

        [Serializable]
        public class PollEntry
        {
            public string Question { get; set; }
            public DateTime OpenDateUtc { get; set; }
            public DateTime CloseDateUtc { get; set; }
            public int MaxVotes { get; set; }
            public bool IsShown { get; set; }
            public bool IsMusicPoll { get; set; }
            public bool ShowVotingUI { get; set; }
            public List<Choice> Choices { get; set; }
            public class Choice
            {
                public string Answer { get; set; }
                public int Count { get; set; }
            }
        }


        public void Import(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PollEntry));
                var entry = serializer.Deserialize(reader) as PollEntry;

                if (entry != null)
                {
                    var p = _contentManager.Create<PollPart>("Poll", poll =>
                        {
                            poll.Record.Question = entry.Question;
                            poll.Record.OpenDateUtc = entry.OpenDateUtc;
                            poll.Record.CloseDateUtc = entry.CloseDateUtc;
                            poll.Record.MaxVotes = entry.MaxVotes;
                            poll.Record.Shown = entry.IsShown;
                            poll.Record.MusicPoll = entry.IsMusicPoll;
                            poll.Record.ShowVotingUI = entry.ShowVotingUI;
                        });
                    foreach (var choice in entry.Choices)
                    {
                        PollChoiceRecord c = new PollChoiceRecord
                        {
                            Answer = choice.Answer,
                            PollPartRecord = p.Record                            
                        };
                        _choiceRepository.Create(c);
                        var result = new PollResultRecord
                        {
                            Count = choice.Count,
                            PollChoiceRecord = c
                        };
                        _resultRepository.Create(result);
                    }
                }
            }
        }
    }
}
