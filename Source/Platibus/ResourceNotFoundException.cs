using System;
using System.Runtime.Serialization;

namespace Platibus
{
	/// <summary>
	/// Thrown to indicate that the requested resource could not be found
	/// </summary>
	[Serializable]
	public class ResourceNotFoundException : InvalidRequestException
	{
		/// <summary>
		/// Initializes a new <see cref="InvalidRequestException"/>
		/// </summary>
		public ResourceNotFoundException()
		{
		}

		/// <summary>
		/// Initializes a new <see cref="InvalidRequestException"/> with the specified
		/// detail <paramref name="message"/>
		/// </summary>
		/// <param name="message">The detail message</param>
		public ResourceNotFoundException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="InvalidRequestException"/> with the specified
		/// detail <paramref name="message"/> and nested <paramref name="innerException"/>
		/// </summary>
		/// <param name="message">The detail message</param>
		/// <param name="innerException">The nested exception</param>
		public ResourceNotFoundException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a serialized <see cref="InvalidRequestException"/> from a streaming context
		/// </summary>
		/// <param name="info">The serialization info</param>
		/// <param name="context">The streaming context</param>
		public ResourceNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}