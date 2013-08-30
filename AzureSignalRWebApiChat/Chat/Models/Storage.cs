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
		public static void RegisterChatEndPoint(string signalRConnectionId)
		{
			TableOperation retrieveOperation = TableOperation.Retrieve<ChatClientEntity>(signalRConnectionId, signalRConnectionId);
			TableResult retrievedResult = chatClientTable.Execute(retrieveOperation);
			ChatClientEntity updateEntity = (ChatClientEntity)retrievedResult.Result;
			if (updateEntity != null)
			{
				updateEntity.ChatClientId = "";
				updateEntity.WebRoleEndPoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WebApi"].IPEndpoint.ToString();
				updateEntity.SignalRConnectionId = signalRConnectionId;

				// Create the InsertOrReplace TableOperation
				TableOperation insertOrReplaceOperation = TableOperation.Replace(updateEntity);

				// Execute the operation.
				chatClientTable.Execute(insertOrReplaceOperation);
			}
			else
			{
				ChatClientEntity insertEntity = new ChatClientEntity(signalRConnectionId);
				insertEntity.ChatClientId = "";
				insertEntity.WebRoleEndPoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WebApi"].IPEndpoint.ToString();
				insertEntity.SignalRConnectionId = signalRConnectionId;

				// Create the InsertOrReplace TableOperation
				TableOperation insertOrReplaceOperation = TableOperation.Insert(insertEntity);

				// Execute the operation.
				chatClientTable.Execute(insertOrReplaceOperation);
			}

			Trace.WriteLine("***** RegisterChatEndPoint *********");
			Trace.WriteLine("SignalR Connection Id: " + signalRConnectionId);
			Trace.WriteLine("Web Api end point: " + RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WebApi"].IPEndpoint.ToString());
		}


		/// <summary>
		/// Registers the viewer.
		/// </summary>
		/// <param name="viewerId">The viewer id.</param>
		public static void RegisterChatClientId(string chatClientId, string signalRConnectionId)
		{
			// Delete any pre-existing entries for this chat client id
			TableQuery<ChatClientEntity> rangeQuery = new TableQuery<ChatClientEntity>().Where(TableQuery.CombineFilters(
					TableQuery.GenerateFilterCondition("ChatClientId", QueryComparisons.Equal, chatClientId), 
					TableOperators.And,
					TableQuery.GenerateFilterCondition("SignalRConnectionId", QueryComparisons.NotEqual, signalRConnectionId)));

			foreach (ChatClientEntity entity in chatClientTable.ExecuteQuery(rangeQuery))
			{
				TableOperation deleteOperation = TableOperation.Delete(entity);
				chatClientTable.Execute(deleteOperation);
			}			
			
			// Create a retrieve operation that takes a customer entity.
			TableOperation retrieveOperation = TableOperation.Retrieve<ChatClientEntity>(signalRConnectionId, signalRConnectionId);

			// Execute the retrieve operation.
			TableResult retrievedResult = chatClientTable.Execute(retrieveOperation);

			// Print the phone number of the result.
			if (retrievedResult.Result != null)
			{
				ChatClientEntity updateEntity = (ChatClientEntity)retrievedResult.Result;

				updateEntity.ChatClientId = chatClientId;

				// Create the InsertOrReplace TableOperation
				TableOperation insertOrReplaceOperation = TableOperation.Replace(updateEntity);

				// Execute the operation.
				chatClientTable.Execute(insertOrReplaceOperation);
			}

			
			Trace.WriteLine("***** RegisterChatClientId *********");
			Trace.WriteLine("Chat ClientId: " + chatClientId);
			Trace.WriteLine("SignalR Connection Id: " + signalRConnectionId);
			Trace.WriteLine("Web Api end point (ignore this though): " + RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["WebApi"].IPEndpoint.ToString());
		}

		/// <summary>
		/// Gets the end point for the chat client
		/// </summary>
		/// <param name="agentId">The chat client id.</param>
		/// <returns></returns>
		public static ChatClientEntity GetChatClientEndpoint(string chatClientId)
		{
			// Create the table query.
			TableQuery<ChatClientEntity> rangeQuery = new TableQuery<ChatClientEntity>().Where(
					TableQuery.GenerateFilterCondition("ChatClientId", QueryComparisons.Equal, chatClientId));

			// Loop through the results, displaying information about the entity.
			foreach (ChatClientEntity entity in chatClientTable.ExecuteQuery(rangeQuery))
			{
				Trace.WriteLine("Getting: " + entity.WebRoleEndPoint.ToString());
				return entity;
			}

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