using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

using ProtoBuf;

namespace Enyim.Caching.Memcached
{
	public class ProtocolBuffersTranscoder : DefaultTranscoder
	{
		static readonly ConcurrentDictionary<ArraySegment<byte>, Type> readCache = new ConcurrentDictionary<ArraySegment<byte>, Type>(new ByteSegmentComparer());
		static readonly ConcurrentDictionary<Type, byte[]> writeCache = new ConcurrentDictionary<Type, byte[]>();
		static readonly Encoding defaultEncoding = Encoding.UTF8;

		protected override ArraySegment<byte> SerializeObject(object value)
		{
			using (var stream = new MemoryStream())
			{
				WriteType(stream, value.GetType());
				Serializer.NonGeneric.Serialize(stream, value);
				return new ArraySegment<byte>(stream.ToArray(), 0, (int)stream.Length);
			}
		}

		protected override object DeserializeObject(ArraySegment<byte> value)
		{
			var raw = value.Array;
			var count = value.Count;
			var offset = value.Offset;
			var type = ReadType(raw, ref offset, ref count);
			using (var stream = new MemoryStream(raw, offset, count, writable: false))
			{
				return Serializer.NonGeneric.Deserialize(type, stream);
			}
		}

		static Type ReadType(byte[] buffer, ref int offset, ref int count)
		{
			if (count < 4) throw new EndOfStreamException();

			// len is size of header typeName(string)
			var len = (int)buffer[offset++]
				   | (buffer[offset++] << 8)
				   | (buffer[offset++] << 16)
				   | (buffer[offset++] << 24);
			count -= 4; // count is message total size, decr typeName length(int)
			if (count < len) throw new EndOfStreamException();
			var keyOffset = offset;
			offset += len; // skip typeName body size
			count -= len; // decr typeName body size

			// avoid encode string
			var key = new ArraySegment<byte>(buffer, keyOffset, len);
			Type type;
			if (!readCache.TryGetValue(key, out type))
			{
				var typeName = defaultEncoding.GetString(key.Array, key.Offset, key.Count);
				type = Type.GetType(typeName, throwOnError: true);

				// create ArraySegment has only typeName
				var cacheBuffer = new byte[key.Count];
				Buffer.BlockCopy(key.Array, key.Offset, cacheBuffer, 0, key.Count);
				key = new ArraySegment<byte>(cacheBuffer, 0, cacheBuffer.Length);
				readCache.TryAdd(key, type);
			}

			return type;
		}

		static void WriteType(MemoryStream stream, Type type)
		{
			var typeArray = writeCache.GetOrAdd(type, x =>
			{
				var typeName = Helper.BuildTypeName(x);
				var buffer = defaultEncoding.GetBytes(typeName);
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