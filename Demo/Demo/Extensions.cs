using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Demo
{
	internal static class Extensions
	{
		/// <summary>
		/// Serializes the value to a JSON string.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value">The object to serialize to JSON</param>
		/// <returns>JSON representation of the object</returns>
		/// <exception cref="System.ArgumentNullException">text</exception>
		public static string SerializeToJson<T>(this T value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			using (var memoryStream = new MemoryStream())
			{
				var serializer = new DataContractJsonSerializer(typeof(T));
				serializer.WriteObject(memoryStream, value);
				return Encoding.Default.GetString(memoryStream.ToArray());
			}
		}
	}
}
