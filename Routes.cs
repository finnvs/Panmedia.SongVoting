using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Panmedia.SongVoting
{
    public class Routes : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                             new RouteDescriptor {   Priority = 6,
                                                     Route = new Route(
                                                         "Song/Create",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.SongVoting"},
                                                                                      {"controller", "Song"},
                                                                                      {"action", "Create"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.SongVoting"}
                                                                                  },
                                                         new MvcRouteHandler())

                             },

                             //// For displaying a song modal popup
                             //new RouteDescriptor {   Priority = 5,
                             //                        Route = new Route(
                             //                            "Song/Songmodal/{choiceItemId}",
                             //                            new RouteValueDictionary {
                             //                                                         {"area", "Panmedia.SongVoting"},
                             //                                                         {"controller", "Song"},
                             //                                                         {"action", "SongModalPopUp"},

                             //                            },
                             //                            new RouteValueDictionary (),
                             //                            new RouteValueDictionary {
                             //                                                         {"area", "Panmedia.SongVoting"}
                             //                                                     },
                             //                            new MvcRouteHandler())

                             //},                        
                             
                              // For editing a poll
                             new RouteDescriptor {   Priority = 4,
                                                     Route = new Route(
                                                         "Afstemning/Edit",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.SongVoting"},
                                                                                      {"controller", "Admin"},
                                                                                      {"action", "Edit"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.SongVoting"}
                                                                                  },
                                                         new MvcRouteHandler())

                             },
                             // For Vote-A-Poll test
                             new RouteDescriptor {   Priority = 3,
                                                     Route = new Route(
                                                         "Afstemninger/Liste",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.SongVoting"},
                                                                                      {"controller", "Vote"},
                                                                                      {"action", "Index"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.SongVoting"}
                                                                                  },
                                                         new MvcRouteHandler())

                             },
                             
                             // For getting a listing of closed polls
                             new RouteDescriptor {   Priority = 2,
                                                     Route = new Route(
                                                         "Afstemninger/Lukkede",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.SongVoting"},
                                                                                      {"controller", "Vote"},
                                                                                      {"action", "ClosedPollIndex"},

                                                         },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Panmedia.SongVoting"}
                                                                                  },
                                                         new MvcRouteHandler())

                             }

                         };
        }
    }
}