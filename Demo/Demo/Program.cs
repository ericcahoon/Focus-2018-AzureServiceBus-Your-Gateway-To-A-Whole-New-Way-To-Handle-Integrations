using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Demo
{
	internal class Program
	{
		// ReSharper disable once ConvertToConstant.Local
		private static readonly string AzureConnectionString = "";

		private static void Main()
		{
			if (string.IsNullOrWhiteSpace(AzureConnectionString)) { throw new NullReferenceException("The Azure Connection String must be defined"); }

			var queueClient =
				QueueClientFactory(
					AzureConnectionString,
					"nonsessionbased");  //Modify the queue name to match what you created
			

			//Register a message handler for the NON session based queue
			queueClient?.OnMessageAsync(async message => await HandleMessage(message),
				new OnMessageOptions
				{
					AutoComplete = false
				});


			var queueSessionEnabledClient =
				QueueClientFactory(
					AzureConnectionString,
					"sessionbased");  //Modify the queue name to match what you created


			//Register a message handler for the Session based queue
			queueSessionEnabledClient.RegisterSessionHandlerAsync(typeof(MySessionHandler));


			Console.WriteLine("Waiting to receive messages.");
			Console.WriteLine("Press any key to exit...");
			Console.ReadLine();
		}


		/// <summary>
		/// Handles processing messages from a NON Session based queue
		/// </summary>
		/// <param name="message">The broker message from the Service Bus</param>
		private static async Task HandleMessage(BrokeredMessage message)
		{
			try
			{
				Utility.ParseExecutionContextFromBrokeredMessage(message);

				// This will remove the message from the topic
				await message.CompleteAsync();
			}
			catch (Exception ex)
			{				
				if (!(ex is MessageLockLostException))
				{
					//Let service bus know we failed to process the message
					await message.AbandonAsync();
					throw;
				}
			}
		}


		/// <summary>
		/// Handles creation of QueueClient objects, which receive messages from a queue
		/// </summary>
		/// <param name="connectionString">The Azure Service Bus Connection String</param>
		/// <param name="queueName">The name of the queue to connect to</param>
		/// <returns></returns>
		private static QueueClient QueueClientFactory(string connectionString, string queueName)
		{
			if (string.IsNullOrWhiteSpace(connectionString)) { throw new ArgumentNullException(nameof(connectionString)); }
			if (string.IsNullOrWhiteSpace(queueName)) { throw new ArgumentNullException(nameof(queueName)); }

			try
			{
				var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

				RegisterQueue(namespaceManager, queueName);

				var client = QueueClient.CreateFromConnectionString(connectionString, queueName);

				Console.WriteLine($"QueueClient created for Queue named '{queueName}'");

				return client;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error creating QueueClient for Queue named '{queueName}'");
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}

			return null;
		}


		/// <summary>
		/// Creates a queue on the service bus, if the queue doesn't already exists
		/// </summary>		
		/// <param name="namespaceManager">Service Bus namespace Manager that'll handle creation of the queue</param>
		/// /// <param name="queueName">The name of the queue to check for and create if not present</param>
		private static void RegisterQueue(NamespaceManager namespaceManager, string queueName)
		{
			if (namespaceManager == null) { throw new ArgumentNullException(nameof(namespaceManager)); }
			if (string.IsNullOrWhiteSpace(queueName)) { throw new ArgumentNullException(nameof(queueName)); }

			if (namespaceManager.QueueExists(queueName)) { return; }

			//Queue doesn't exist so create it
			var qd = new QueueDescription(queueName)
			{
				MaxSizeInMegabytes = 1024,
				DefaultMessageTimeToLive = TimeSpan.FromMinutes(5)
			};

		
			namespaceManager.CreateQueue(qd);
		}
	}
}
