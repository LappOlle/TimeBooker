using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace TimeBookerApi.Hubs
{
    public class MyHub : Hub
    {
        private static IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<MyHub>();

        public void Update()
        {
            Clients.All.update();
        }

        public static void RequestUpdate()
        {
            hubContext.Clients.All.update();
        }
    }
}