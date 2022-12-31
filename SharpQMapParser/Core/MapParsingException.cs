using System;

namespace SharpQMapParser.Core
{

	[Serializable]
	public class MapParsingException : Exception
	{
		public MapParsingException() { }
		public MapParsingException(string message) : base(message) { }
		public MapParsingException(string message, Exception inner) : base(message, inner) { }
		protected MapParsingException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
