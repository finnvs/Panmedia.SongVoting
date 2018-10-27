using Orchard;
using Orchard.Localization;
using Orchard.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Panmedia.SongVoting
{
    public class AdminMenu : INavigationProvider
    {
        public string MenuName
        {
            get { return "admin"; }
        }

        public AdminMenu()
        {
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            builder.AddImageSet("Polls")
                .Add(T("Polls"), "4",
                    menu => menu.Add(T("Polls"), "2", 
                        item => item.Action("Index", "Admin", new { area = "Panmedia.SongVoting" })));
            builder.AddImageSet("Songs")
                .Add(T("Songs"), "4",
                    menu => menu.Add(T("Songs"), "2",
                           // Admin styling, men med edit kald direkte til Song Controller fra SongIndex view via actionlink
                           item => item.Action("SongIndex", "Admin", new { area = "Panmedia.SongVoting" }))); 
                           // item => item.Action("Index", "Song", new { area = "Panmedia.SongVoting" })));
        }
    }
}