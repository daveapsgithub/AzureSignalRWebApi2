using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Chat.Models;
using System.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Chat.WebApi;
using System.Timers;

namespace Chat.SignalRHub
{
	public class ChatHub : Hub
	{
		static string staticEndPoint;
		static string staticConnectionId;
		
		public override System.Threading.Tasks.Task OnConnected()
		{
			Storage.RegisterChatEndPoint(this.Context.ConnectionId);

			staticEndPoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WebApi"].IPEndpoint.ToString();
			staticConnectionId = this.Context.ConnectionId;

			return base.OnConnected();
		}

		public void Register(string chatClientId)
		{
			Storage.RegisterChatClientId(chatClientId, this.Context.ConnectionId);
		}

		/// <summary>
		/// Receives the message and sends it to the SignalR client.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="connectionId">The connection id.</param>
		public static void SendMessageToClient(string message, string connectionId)
		{
			string endPoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WebApi"].IPEndpoint.ToString();

			bool same = ((endPoint == staticEndPoint) && (connectionId == staticConnectionId));
	
			GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients.Client(connectionId).SendMessageToClient(message);

			Trace.TraceInformation("Sending a message to the client on SignalR connection id: " + connectionId);
			Trace.TraceInformation("Via the Web Api end point: " + RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WebApi"].IPEndpoint.ToString());
		}

				
		/// <summary>
		/// Sends the message to other instance.
		/// </summary>
		/// <param name="chatClientId">The chat client id.</param>
		/// <param name="message">The message.</param>
		public void SendMessageToServer(string chatClientId, string message)
		{
			Trace.TraceInformation("Recevied a message from the client on SignalR connection id: " + this.Context.ConnectionId);
			Trace.TraceInformation("Via the Web Api end point: " + RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WebApi"].IPEndpoint.ToString());

			// Get the chatClientId of the destination.
			string otherChatClient = (chatClientId == "A" ? "B" : "A");

			// Find out this other chatClientId's end point
			ChatClientEntity chatClientEntity = Storage.GetChatClientEndpoint(otherChatClient);

			if (chatClientEntity != null)
				ChatWebApiController.SendMessage(chatClientEntity.WebRoleEndPoint, chatClientEntity.SignalRConnectionId, message);
		}


		/// <summary>
		/// Tests the call.
		/// </summary>
		public void TestCall()
		{
			Trace.WriteLine(this.Context.ConnectionId + "<== test message on SignalR connection id");
			Trace.WriteLine(RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WebApi"].IPEndpoint.ToString() + "<== Via the Web Api end point: ");
			Trace.WriteLine("");
		}
	}
}