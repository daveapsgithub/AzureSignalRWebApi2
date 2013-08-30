using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using System.Web;
using Chat.SignalRHub;
using System.Diagnostics;

namespace Chat.WebApi
{
	public class ChatWebApiController : ApiController
	{
		[HttpGet]
		public void SendMessage(string message, string connectionId)
		{
			Trace.TraceInformation("Received a WebApi request on: " + this.Request.RequestUri.ToString());

			//return message;
			ChatHub.SendMessageToClient(message, connectionId);
		}

		/// <summary>
		/// This calls the method above but on a different instance via Web API
		/// </summary>
		/// <param name="endPoint">The end point.</param>
		/// <param name="connectionId">The connection id.</param>
		/// <param name="message">The message.</param>
		public static void SendMessage(string endPoint, string connectionId, string message)
		{
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri("http://" + endPoint);

			// Add an Accept header for JSON format.
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			string url = "http://" + endPoint + "/api/ChatWebApi/SendMessage/?Message=" + HttpUtility.UrlEncode(message) + "&ConnectionId=" + connectionId;

			Trace.WriteLine("Sending a WebApi request to: " + url);

			client.GetAsync(url);
		}

	}
}
