using System;
using System.IO;
using Microsoft.ServiceBus.Messaging;

namespace Demo
{
	internal class MySessionHandler : IMessageSessionHandler
	{
		private int _count;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="session"></param>
		public void OnCloseSession(MessageSession session)
		{
			var result = GetState(session);
			result += _count;
			SetState(session, result);
			_count = 0;
			Console.WriteLine($"Persist State ({result}) and close session({session.SessionId}) processing.");
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="session"></param>
		/// <param name="message"></param>
		public void OnMessage(MessageSession session, BrokeredMessage message)
		{
			Console.WriteLine($"Process message({message.SequenceNumber}) from session:{message.SessionId}");

			Utility.ParseExecutionContextFromBrokeredMessage(message);

			_count++;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="exception"></param>
		public void OnSessionLost(Exception exception)
		{
			Console.WriteLine("Abandon recently processed state.");
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="session"></param>
		/// <returns></returns>
		private static int GetState(MessageSession session)
		{
			var state = 0;
			var stream = session.GetState();

			if (stream == null)
			{
				return state;
			}

			using (stream)
			{
				using (var reader = new StreamReader(stream))
				{
					state = int.Parse(reader.ReadToEnd());
				}
			}

			return state;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="session"></param>
		/// <param name="state"></param>
		private static void SetState(MessageSession session, int state)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					writer.Write(state);
					writer.Flush();

					stream.Position = 0;
					session.SetState(stream);
				}
			}
		}
	}
}
