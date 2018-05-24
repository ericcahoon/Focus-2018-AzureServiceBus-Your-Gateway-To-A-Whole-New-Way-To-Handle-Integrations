using System;
using System.Runtime.Serialization.Json;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Xrm.Sdk;

namespace Demo
{
	internal class Utility
	{
		/// <summary>
		/// Extracts the CRM ExecutionContext from the provided BrokeredMessage
		/// </summary>
		/// <param name="message"></param>
		public static void ParseExecutionContextFromBrokeredMessage(BrokeredMessage message)
		{
			if (message == null) { throw new ArgumentNullException(nameof(message)); }

			var context = message.GetBody<RemoteExecutionContext>(
				new DataContractJsonSerializer(typeof(RemoteExecutionContext)));

			Console.WriteLine($@"

*** New Message Received @ {DateTime.Now} ***

Id: {message.MessageId}

Context:  {context.SerializeToJson()}

*** END OF NEW MESSAGE ***

");
		}
	}
}
