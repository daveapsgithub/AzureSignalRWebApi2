using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Chat.Models;
using System.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Chat.WebApi;

namespace Chat.SignalRHub
{
	public class ChatHub : Hub
	{
		public void Register(string chatClientId)
		{
			Storage.RegisterChatEndPoint(chatClientId, this.Context.ConnectionId);
		}

		/// <summary>
		/// Receives the message and sends it to the SignalR client.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="connectionId">The connection id.</param>
		public static void SendMessageToClient(string message, string connectionId)
		{
			GlobalHost.ConnectionManager.GetHubContext<ChatHub>().Clients.Client(connectionId).SendMessageToClient(message);

			Debug.WriteLine("Sending a message to the client on SignalR connection id: " + connectionId);
			Debug.WriteLine("Via the Web Api end point: " + RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WebApi"].IPEndpoint.ToString());

		}

				
		/// <summary>
		/// Sends the message to other instance.
		/// </summary>
		/// <param name="chatClientId">The chat client id.</param>
		/// <param name="message">The message.</param>
		public void SendMessageToServer(string chatClientId, string message)
		{
			// Get the chatClientId of the destination.
			string otherChatClient = (chatClientId == "A" ? "B" : "A");

			// Find out this other chatClientId's end point
			ChatClientEntity chatClientEntity = Storage.GetChatClientEndpoint(otherChatClient);

			if (chatClientEntity != null)
				ChatWebApiController.SendMessage(chatClientEntity.WebRoleEndPoint, chatClientEntity.SignalRConnectionId, message);
		}
	}
}