using System;
using System.IO;
using System.Collections.Concurrent;
using MsgPack.Serialization;
using CacheUtils;

namespace Enyim.Caching.Memcached
{
	public class MessagePackMapTranscoder : DefaultTranscoder
	{
		static readonly ConcurrentDictionary<string, Type> readCache = new ConcurrentDictionary<string, Type>();
		static readonly ConcurrentDictionary<Type, string> writeCache = new ConcurrentDictionary<Type, string>();
		static readonly SerializationContext defaultContext = new SerializationContext
		{
			SerializationMethod = SerializationMethod.Map
		};

		protected override ArraySegment<byte> SerializeObject(object value)
		{
			var type = value.GetType();
			var typeName = writeCache.GetOrAdd(type, TranscodersHelper.BuildTypeName);
			using (var stream = Helper.CreateMemoryStream())
			{
				var packer = MsgPack.Packer.Create(stream);
				packer.PackArrayHeader(2);
				packer.PackString(typeName);
				MessagePackSerializer.Get(type, defaultContext).PackTo(packer, value);
				return stream.ToArraySegment();
			}
		}

		protected override object DeserializeObject(ArraySegment<byte> value)
		{
			using (var stream = Helper.CreateMemoryStream(value.Array, value.Offset, value.Count))
			{
				var unpacker = MsgPack.Unpacker.Create(stream);
				unpacker.Read();
				if (unpacker.IsArrayHeader)
				{
					unpacker.Read();
					var typeName = (string)unpacker.LastReadData;
					var type = readCache.GetOrAdd(typeName, x => Type.GetType(x, throwOnError: true));
					unpacker.Read();
					return MessagePackSerializer.Get(type, defaultContext).UnpackFrom(unpacker);
				}
				else
				{
					throw new InvalidDataException("MessagePackMapTranscoder only supports [\"TypeName\", object]");
				}
			}
		}
	}
}
