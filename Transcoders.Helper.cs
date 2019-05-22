using System;
using System.Text.RegularExpressions;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("VIEApps.Components.XUnitTests")]

namespace Enyim.Caching.Memcached
{
	internal static class TranscodersHelper
	{
		static readonly Regex SubtractFullNameRegex = new Regex(@", Version=\d+.\d+.\d+.\d+, Culture=\w+, PublicKeyToken=\w+", RegexOptions.Compiled);

		internal static string BuildTypeName(Type type)
			=> SubtractFullNameRegex.Replace(type.AssemblyQualifiedName, "");
	}
}
