using System;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Enyim.Caching.Memcached
{
	public class BsonTranscoder : DefaultTranscoder
	{
		protected override ArraySegment<byte> SerializeObject(object value)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = new BsonDataWriter(stream))
				{
					(new JsonSerializer()).Serialize(writer, value);
					return new ArraySegment<byte>(stream.ToArray(), 0, (int)stream.Length);
				}
			}
		}

		protected override object DeserializeObject(ArraySegment<byte> value)
		{
			using (var stream = new MemoryStream(value.Array, value.Offset, value.Count))
			{
				using (var reader = new BsonDataReader(stream))
				{
					return (new JsonSerializer()).Deserialize(reader);
				}
			}
		}
	}
}
