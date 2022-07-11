namespace FinanceTracker.Core.Interfaces
{
	public struct DecodedTransactionRow
	{
		public DateTime Date;
		public double In;
		public double Out;
		public string Description;

		public DecodedTransactionRow(DateTime date, double inValue, double outValue, string description)
		{
			Date = date;
			In = inValue;
			Out = outValue;
			Description = description;
		}
	}

	public struct FormatProvider
	{
		public int DateIndex;
		public int InIndex;
		public int OutIndex;
		public int DescriptionIndex;

		public FormatProvider(int dateIndex, int inIndex, int outIndex, int descriptionIndex)
		{
			DateIndex = dateIndex;
			InIndex = inIndex;
			OutIndex = outIndex;
			DescriptionIndex = descriptionIndex;
		}
	}

	public interface ITransactionReaderService
	{
		event EventHandler<int> ReadingProgressUpdated;

		Task<List<DecodedTransactionRow>> Read(string path, FormatProvider formatProvider);
	}
}
