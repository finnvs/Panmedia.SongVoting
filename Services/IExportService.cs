using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard;

namespace Panmedia.SongVoting.Services
{
    public interface IExportService : IDependency
    {
        void ExportToXml(int poll_id);
    }
}
