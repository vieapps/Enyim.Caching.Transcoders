using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using ProtoBuf;
using CacheUtils;

namespace Enyim.Caching.Memcached
{
	public class ProtocolBuffersTranscoder : DefaultTranscoder
	{
		static ConcurrentDictionary<ArraySegment<byte>, Type> ReadCache = new ConcurrentDictionary<ArraySegment<byte>, Type>(new ByteSegmentComparer());
		static ConcurrentDictionary<Type, byte[]> WriteCache = new ConcurrentDictionary<Type, byte[]>();
		static Encoding DefaultEncoding = Encoding.UTF8;

		protected override ArraySegment<byte> SerializeObject(object value)
		{
			using (var stream = Helper.CreateMemoryStream())
			{
				ProtocolBuffersTranscoder.WriteType(stream, value.GetType());
				Serializer.NonGeneric.Serialize(stream, value);
				return stream.ToArraySegment();
			}
		}

		protected override object DeserializeObject(ArraySegment<byte> value)
		{
			var raw = value.Array;
			var count = value.Count;
			var offset = value.Offset;
			var type = ProtocolBuffersTranscoder.ReadType(raw, ref offset, ref count);
			using (var stream = Helper.CreateMemoryStream(raw, offset, count))
			{
				return Serializer.NonGeneric.Deserialize(type, stream);
			}
		}

		static Type ReadType(byte[] buffer, ref int offset, ref int count)
		{
			if (count < 4)
				throw new EndOfStreamException();

			// len is size of header typeName(string)
			var length = (int)buffer[offset++] | (buffer[offset++] << 8) | (buffer[offset++] << 16) | (buffer[offset++] << 24);
			count -= 4; // count is message total size, decr typeName length(int)
			if (count < length)
				throw new EndOfStreamException();
			var keyOffset = offset;
			offset += length; // skip typeName body size
			count -= length; // decr typeName body size

			// avoid encode string
			var key = new ArraySegment<byte>(buffer, keyOffset, length);
			if (!ProtocolBuffersTranscoder.ReadCache.TryGetValue(key, out Type type))
			{
				var typeName = ProtocolBuffersTranscoder.DefaultEncoding.GetString(key.Array, key.Offset, key.Count);
				type = Type.GetType(typeName, throwOnError: true);

				// create ArraySegment has only typeName
				var cacheBuffer = new byte[key.Count];
				Buffer.BlockCopy(key.Array, key.Offset, cacheBuffer, 0, key.Count);
				key = new ArraySegment<byte>(cacheBuffer, 0, cacheBuffer.Length);
				ProtocolBuffersTranscoder.ReadCache.TryAdd(key, type);
			}
			return type;
		}

		static void WriteType(MemoryStream stream, Type type)
		{
			var typeArray = ProtocolBuffersTranscoder.WriteCache.GetOrAdd(type, x =>
			{
				var typeName = TranscodersHelper.BuildTypeName(x);
				var buffer = ProtocolBuffersTranscoder.DefaultEncoding.GetBytes(typeName);
				return buffer;
			});

			var len = typeArray.Length;
			// BinaryWrite Int32
			stream.WriteByte((byte)len);
			stream.WriteByte((byte)(len >> 8));
			stream.WriteByte((byte)(len >> 16));
			stream.WriteByte((byte)(len >> 24));
			// BinaryWrite String
			stream.Write(typeArray, 0, len);
		}
	}

	internal sealed class ByteSegmentComparer : IEqualityComparer<ArraySegment<byte>>
	{
		bool IEqualityComparer<ArraySegment<byte>>.Equals(ArraySegment<byte> x, ArraySegment<byte> y)
		{
			if (x.Count != y.Count)
				return false;
			byte[] xBuf = x.Array, yBuf = y.Array;
			int xOffset = x.Offset, yOffset = y.Offset, xMax = xOffset + x.Count;
			while (xOffset < xMax)
			{
				if (xBuf[xOffset++] != yBuf[yOffset++])
					return false;
			}
			return true;
		}

		int IEqualityComparer<ArraySegment<byte>>.GetHashCode(ArraySegment<byte> segment)
		{
			byte[] buffer = segment.Array;
			int result = -1623343517;
			int offset = segment.Offset, max = offset + segment.Count;
			while (offset < max)
			{
				result = (-1521134295 * result) + (int)buffer[offset++];
			}
			return result;
		}
	}
}
