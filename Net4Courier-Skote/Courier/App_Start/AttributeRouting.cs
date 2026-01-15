using System.Web.Routing;
using AttributeRouting.Web.Mvc;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Net4Courier.AttributeRouting), "Start")]

namespace Net4Courier
{
    public static class AttributeRouting 
	{
		public static void RegisterRoutes(RouteCollection routes) 
		{
			// See http://github.com/mccalltd/AttributeRouting/wiki for more options.
			// To debug routes locally using the built in ASP.NET development server, go to /routes.axd
			if (routes.Count > 0)
			{
				routes.MapAttributeRoutes();
			}
		}

        public static void Start() 
		{
            RegisterRoutes(RouteTable.Routes);
        }
    }
}
