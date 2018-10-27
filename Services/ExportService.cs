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

namespace Panmedia.SongVoting.Services
{
    public class ExportService : IExportService
    {

        private readonly IRepository<PollPartRecord> _pollRepository;
        private readonly IRepository<PollChoiceRecord> _choiceRepository;
        private readonly IRepository<PollVoteRecord> _voteRepository;
        private readonly IRepository<PollResultRecord> _resultRepository;

        public List<SinglePollEntry.Choice> Choices { get; set; }
        public ExportService(IRepository<PollPartRecord> pollRepository,
            IRepository<PollChoiceRecord> choiceRepository,
            IRepository<PollVoteRecord> voteRepository,
            IRepository<PollResultRecord> resultRepository)
           
        {
            _pollRepository = pollRepository;
            _choiceRepository = choiceRepository;
            _voteRepository = voteRepository;
            _resultRepository = resultRepository;            
        }


        public void ExportToXml(int poll_id)
        {

            var poll = _pollRepository.Get(poll_id);            
            var choices = _choiceRepository.Fetch(c => c.PollPartRecord.Id == poll_id).AsQueryable();
            var results = _resultRepository.Fetch(r => r.PollChoiceRecord.PollPartRecord.Id == poll_id);

            Choices = results.Select(r => new SinglePollEntry.Choice
            {
                Answer = r.PollChoiceRecord.Answer,
                Count = r.Count
            }).ToList();

            using (XmlWriter writer = XmlWriter.Create("Poll.xml"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Poll");

                writer.WriteElementString("ID", poll.Id.ToString());
                writer.WriteElementString("Question", poll.Question.ToString());
                writer.WriteElementString("OpenDateUtc", poll.OpenDateUtc.ToString());
                writer.WriteElementString("CloseDateUtc", poll.CloseDateUtc.ToString());
                writer.WriteElementString("MaxVotes", poll.MaxVotes.ToString());
                writer.WriteElementString("Shown", poll.Shown.ToString());
                writer.WriteElementString("MusicPoll", poll.MusicPoll.ToString());
                writer.WriteElementString("ShowVotingUI", poll.ShowVotingUI.ToString());
                writer.WriteElementString("ContentItemRecord", poll.ContentItemRecord.ToString());

                writer.WriteStartElement("Choices");
                foreach (var c in Choices)
                {
                    writer.WriteStartElement("Choice");
                    writer.WriteElementString("Choice", c.Answer);
                    writer.WriteElementString("Count", c.Count.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }



        }
    }

    [Serializable]
    public class SinglePollEntry
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
    
}
