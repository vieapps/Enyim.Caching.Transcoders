using System;
using System.Text.RegularExpressions;

namespace Enyim.Caching.Memcached
{
	internal static class Helper
	{
		static readonly Regex SubtractFullNameRegex = new Regex(@", Version=\d+.\d+.\d+.\d+, Culture=\w+, PublicKeyToken=\w+", RegexOptions.Compiled);

		internal static string BuildTypeName(Type type)
		{
			return SubtractFullNameRegex.Replace(type.AssemblyQualifiedName, "");
		}
	}
}