using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using CacheUtils;

namespace Enyim.Caching.Memcached
{
	public class BsonTranscoder : DefaultTranscoder
	{
		static JsonSerializer Serializer { get; } = new JsonSerializer();

		protected override ArraySegment<byte> SerializeObject(object value)
		{
			using (var stream = Helper.CreateMemoryStream())
			using (var writer = new BsonDataWriter(stream))
			{
				Serializer.Serialize(writer, value);
				return new ArraySegment<byte>(stream.ToBytes());
			}
		}

		protected override object DeserializeObject(ArraySegment<byte> value)
		{
			using (var stream = Helper.CreateMemoryStream(value.Array, value.Offset, value.Count))
			using (var reader = new BsonDataReader(stream))
				return Serializer.Deserialize(reader);
		}
	}
}
