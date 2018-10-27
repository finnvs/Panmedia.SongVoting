using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard;
using System.IO;

namespace Panmedia.SongVoting.Services
{
    public interface IXmlService : IDependency
    {
        byte[] ExportToXml(int poll_Id);
        void Import(Stream stream);
    }
}
