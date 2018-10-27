using Panmedia.SongVoting.Models;
using Panmedia.SongVoting.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System.Runtime.InteropServices;

namespace Panmedia.SongVoting.Handlers
{
    [ComVisible(false)]
    public class SongPartHandler : ContentHandler
    {
        public SongPartHandler(IRepository<SongPartRecord> songRepository, ISongService songService)
        {
            Filters.Add(StorageFilter.For(songRepository));            
        }
    }
}