using FinanceTracker.Core.Interfaces;

namespace FinanceTracker.Core.Services
{
	public class TransactionReaderService : ITransactionReaderService
	{
		public event EventHandler<int> ReadingProgressUpdated;

		public async Task<List<DecodedTransactionRow>> Read(string path, FormatProvider formatProvider)
		{
			var decodedRows = new List<DecodedTransactionRow>();

			await Task.Run(() => {

				var lines = File.ReadAllLines(path);
				var lineCount = lines.Length;
				var currentLine = 0;
				var currentProgressPercent = 0;

				while (currentLine < lineCount)
				{
					var line = lines[currentLine];
					currentLine++;
					if (string.IsNullOrWhiteSpace(line))
						continue;

					var lineArray = line.Split(',');
					var row = new DecodedTransactionRow(
						DateTime.Parse(lineArray[formatProvider.DateIndex]),
						double.Parse(lineArray[formatProvider.InIndex]),
						double.Parse(lineArray[formatProvider.OutIndex]),
						lineArray[formatProvider.DescriptionIndex]
					);

					decodedRows.Add(row);

					var newValue = (int)((double)currentLine / lineCount * 100);
					if (newValue != currentProgressPercent)
					{
						currentProgressPercent = newValue;
						ReadingProgressUpdated?.Invoke(this, currentProgressPercent);
					}
				}
			});

			return decodedRows;
		}
	}
}
