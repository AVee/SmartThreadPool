
#if !(_WINDOWS_CE) && !(_SILVERLIGHT) && !(WINDOWS_PHONE)

using System;
using System.Diagnostics;
using System.Globalization;
using System.Security.Principal;
using System.Threading;
using System.Reflection;
using System.Web;
using System.Runtime.Remoting.Messaging;


namespace Amib.Threading.Internal
{
#region CallerThreadContext class

	/// <summary>
	/// This class stores the caller call context in order to restore
	/// it when the work item is executed in the thread pool environment. 
	/// </summary>
	internal class CallerThreadContext 
	{
#region Prepare reflection information

		private static string HttpContextSlotName = GetHttpContextSlotName();

		private static string GetHttpContextSlotName()
		{
			FieldInfo fi = typeof(HttpContext).GetField("CallContextSlotName", BindingFlags.Static | BindingFlags.NonPublic);

            if (fi != null)
            {
                return (string) fi.GetValue(null);
            }

		    return "HttpContext";
		}

        #endregion

#region Private fields

		private HttpContext _httpContext;
        private IPrincipal _principal;
        private CultureInfo _culture;
        private CultureInfo _uiCulture;

        #endregion

		/// <summary>
		/// Constructor
		/// </summary>
		private CallerThreadContext()
		{
		}

		public bool CapturedUserInfo
		{
			get
			{
                return (null != _culture);
			}
		}

		public bool CapturedHttpContext
		{
			get
			{
				return (null != _httpContext);
			}
		}

		/// <summary>
		/// Captures the current thread context
		/// </summary>
		/// <returns></returns>
		public static CallerThreadContext Capture(
			bool captureUserInfo, 
			bool captureHttpContext)
		{
			Debug.Assert(captureUserInfo || captureHttpContext);

			CallerThreadContext callerThreadContext = new CallerThreadContext();

            // Capture userinfo
            if (captureUserInfo)
			{
                callerThreadContext._principal = Thread.CurrentPrincipal;
                callerThreadContext._culture = Thread.CurrentThread.CurrentCulture;
                callerThreadContext._uiCulture = Thread.CurrentThread.CurrentUICulture;
			}

			// Capture httpContext
			if (captureHttpContext && (null != HttpContext.Current))
			{
				callerThreadContext._httpContext = HttpContext.Current;
			}

			return callerThreadContext;
		}

		/// <summary>
		/// Applies the thread context stored earlier
		/// </summary>
		/// <param name="callerThreadContext"></param>
        public static void Apply(CallerThreadContext callerThreadContext)
        {
            if (null == callerThreadContext)
            {
                throw new ArgumentNullException("callerThreadContext");
            }

            // Apply user information
            if (callerThreadContext.CapturedUserInfo)
            {
                Thread.CurrentThread.CurrentCulture = callerThreadContext._culture;
                Thread.CurrentThread.CurrentUICulture = callerThreadContext._uiCulture;
                Thread.CurrentPrincipal = callerThreadContext._principal;
            }

            // Restore HttpContext 
            if (callerThreadContext._httpContext != null)
            {
                HttpContext.Current = callerThreadContext._httpContext;
                //CallContext.SetData(HttpContextSlotName, callerThreadContext._httpContext);
            }
        }
	}

    #endregion
}
#endif
