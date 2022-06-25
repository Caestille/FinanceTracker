namespace LogExplorer.Core
{
	public static class EnumerableExtensions
	{
		public static IEnumerable<T> Clone<T>(this IEnumerable<T> toCopy)
		{
			return new List<T>(toCopy);
		}
	}
}