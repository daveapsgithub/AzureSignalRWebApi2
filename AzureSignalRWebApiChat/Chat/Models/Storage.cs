using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Diagnostics;

namespace Chat.Models
{
	public class Storage
	{
		static CloudStorageAccount storageAccount = null;

		static CloudTable chatClientTable = null;

		/// <summary>
		/// Starts this instance.
		/// </summary>
		public static void Start()
		{
			storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

			// Create the table client.
			CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

			// Create the table if it doesn't exist.
			chatClientTable = tableClient.GetTableReference("ChatClient");
			chatClientTable.CreateIfNotExists();
		}


		/// <summary>
		/// Registers the viewer.
		/// </summary>
		/// <param name="viewerId">The viewer id.</param>
		public static void RegisterChatEndPoint(string chatClientId, string signalRConnectionId)
		{
			TableOperation retrieveOperation = TableOperation.Retrieve<ChatClientEntity>(chatClientId, chatClientId);
			TableResult retrievedResult = chatClientTable.Execute(retrieveOperation);
			ChatClientEntity updateEntity = (ChatClientEntity)retrievedResult.Result;
			if (updateEntity != null)
			{
				updateEntity.ChatClientId = chatClientId;
				updateEntity.WebRoleEndPoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WebApi"].IPEndpoint.ToString();
				updateEntity.SignalRConnectionId = signalRConnectionId;

				// Create the InsertOrReplace TableOperation
				TableOperation insertOrReplaceOperation = TableOperation.Replace(updateEntity);

				// Execute the operation.
				chatClientTable.Execute(insertOrReplaceOperation);
			}
			else
			{
				ChatClientEntity insertEntity = new ChatClientEntity(chatClientId);
				insertEntity.ChatClientId = chatClientId;
				insertEntity.WebRoleEndPoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WebApi"].IPEndpoint.ToString();
				insertEntity.SignalRConnectionId = signalRConnectionId;

				// Create the InsertOrReplace TableOperation
				TableOperation insertOrReplaceOperation = TableOperation.Insert(insertEntity);

				// Execute the operation.
				chatClientTable.Execute(insertOrReplaceOperation);
			}

			Debug.WriteLine("Chat ClientId: " + chatClientId);
			Debug.WriteLine("Web Api end point: " + RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WebApi"].IPEndpoint.ToString());
			Debug.WriteLine("SignalR Connection Id: " + signalRConnectionId);
		}


		/// <summary>
		/// Gets the end point for the chat client
		/// </summary>
		/// <param name="agentId">The chat client id.</param>
		/// <returns></returns>
		public static ChatClientEntity GetChatClientEndpoint(string chatClientId)
		{
			// Create a retrieve operation that takes a customer entity.
			TableOperation retrieveOperation = TableOperation.Retrieve<ChatClientEntity>(chatClientId, chatClientId);

			// Execute the retrieve operation.
			TableResult retrievedResult = chatClientTable.Execute(retrieveOperation);

			// Print the phone number of the result.
			if (retrievedResult.Result != null)
				return (ChatClientEntity)retrievedResult.Result;
			else
				return null;
		}
	}

	/// <summary>
	/// Represents a chat client end point
	/// </summary>
	public class ChatClientEntity : TableEntity
	{
		public ChatClientEntity(string chatClientId)
		{
			this.PartitionKey = chatClientId;
			this.RowKey = chatClientId;
		}

		public ChatClientEntity()
		{
		}

		public string ChatClientId { get; set; }

		public string WebRoleEndPoint { get; set; }

		public string SignalRConnectionId { get; set; }
	}

}