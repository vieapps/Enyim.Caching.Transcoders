using System;
using System.Text.RegularExpressions;

#if !SIGN
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("VIEApps.Components.XUnitTests")]
#endif

namespace Enyim.Caching.Memcached
{
	internal static class TranscodersHelper
	{
		static readonly Regex SubtractFullNameRegex = new Regex(@", Version=\d+.\d+.\d+.\d+, Culture=\w+, PublicKeyToken=\w+", RegexOptions.Compiled);

		internal static string BuildTypeName(Type type)
			=> TranscodersHelper.SubtractFullNameRegex.Replace(type.AssemblyQualifiedName, "");
	}
}
