using CoreUtilities.Services;
using FinanceTracker.Core.DataTypeObjects;
using FinanceTracker.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Core.Services
{
	public class TransactionRowDistributerService
	{
		private readonly SqLiteDatabaseService database;
		private string currentFilter;
		private List<string> topicsWithUpdatedSourceIds = new();

		private SQLiteDataReader lastRowReader;
		private SQLiteDataReader lastFilteredRowReader;
		private SQLiteDataReader lastToUpdateRowReader;

		private const string logRowTableName = "LogRows";

		private const string loadedFilesColumnName = "LoadedFilesColumn";
		private const string updateRowCommandName = "updateRow";
		private const string insertRowCommandName = "insertRow";
		private const string updateRowFilterStatusCommandName = "updateRowFilterStatus";

		private SQLiteTransaction writeTransaction;

		private bool breakOperation;

		private enum Ordering
		{
			Ascending,
			Descending
		}

		public TransactionRowDistributerService(string fullpath, bool newDatabase)
		{
			DatabaseName = fullpath.Split('\\')[fullpath.Split('\\').Length - 1].Replace(".sqlite", "");

			database = new SqLiteDatabaseService(fullpath, newDatabase);

			database.AddTableAndColumns(logRowTableName,
				new KeyValuePair<string, string>[] 
				{
					new KeyValuePair<string, string>(nameof(DatabaseTransactionStorageModel.TransactionDto), "TEXT"),
					new KeyValuePair<string, string>(nameof(DatabaseTransactionStorageModel.IsFilteredOut), "INTEGER"),
				},
				new string[] { nameof(TransactionDto.NormalisedProviderId) });

			database.SetUpUpdateCommand(logRowTableName, updateRowCommandName,
				new List<string>() 
				{
					"TransactionDtoJson",
					"IsFilteredOut"
				},
				nameof(TransactionDto.NormalisedProviderId));

			database.SetUpUpdateCommand(logRowTableName, updateRowFilterStatusCommandName,
				new List<string>() {
					"IsFilteredOut"
				},
				nameof(TransactionDto.NormalisedProviderId));

			database.SetUpInsertCommand(logRowTableName, insertRowCommandName,
				new List<string>() {
					nameof(LogRowModel.Id),
					nameof(LogRowModel.EntryType),
					nameof(LogRowModel.EntryTopic),
					nameof(LogRowModel.DateTimeValue),
					nameof(LogRowModel.EntryBody),
					nameof(LogRowModel.IsFilteredOut),
					nameof(LogRowModel.LastProfileProcessedWith),
					nameof(LogRowModel.ValuesDict)});
		}

		public string DatabaseName { get; private set; }

		public IEnumerable<object> LogRows()
		{
			SQLiteDataReader reader = (database.GetRows(logRowTableName, "", GenerateOrderingString(nameof(LogRowModel.DateTimeValue), Ordering.Ascending)) as SQLiteDataReader)!;
			lastRowReader = reader;
			return reader.Cast<object>();
		}

		public void CloseRowReader()
		{
			lastRowReader.Close();
			lastRowReader.Dispose();
			lastRowReader = null;
		}

		public IEnumerable<object> FilteredLogRows()
		{
			SQLiteDataReader reader = (database.GetRows(logRowTableName, $"WHERE NOT({nameof(LogRowModel.IsFilteredOut)})", GenerateOrderingString(nameof(LogRowModel.DateTimeValue), Ordering.Ascending)) as SQLiteDataReader)!;
			lastFilteredRowReader = reader;
			return reader.Cast<object>();
		}

		public void CloseFilteredRowReader()
		{
			lastFilteredRowReader.Close();
			lastFilteredRowReader.Dispose();
			lastFilteredRowReader = null;
		}

		public IEnumerable<object> LogRowsToUpdate()
		{
			string topicsCheckString = "";
			foreach (string topic in topicsWithUpdatedSourceIds)
				topicsCheckString += $" OR {nameof(LogRowModel.LastProfileProcessedWith)} = '{topic}'";
			string condition = $"WHERE {nameof(LogRowModel.LastProfileProcessedWith)} != '{currentFilter}' OR {nameof(LogRowModel.LastProfileProcessedWith)} = NULL{topicsCheckString}";
			SQLiteDataReader reader = database.GetRows(logRowTableName, condition, GenerateOrderingString(nameof(LogRowModel.DateTimeValue), Ordering.Ascending)) as SQLiteDataReader;
			lastToUpdateRowReader = reader;
			return reader.Cast<object>();
		}

		public void CloseUpdatedRowReader()
		{
			lastToUpdateRowReader.Close();
			lastToUpdateRowReader.Dispose();
			lastToUpdateRowReader = null;
		}

		public void Clear()
		{
			database.Clear(logRowTableName);
		}

		public int RowCount()
		{
			return (int)database.RowCount(logRowTableName, "");
		}

		public int FilteredRowCount()
		{
			return (int)database.RowCount(logRowTableName, $"WHERE NOT({nameof(LogRowModel.IsFilteredOut)})");
		}

		public int ToUpdateRowCount()
		{
			string topicsCheckString = "";
			foreach (string topic in topicsWithUpdatedSourceIds)
			{
				topicsCheckString += $" OR {nameof(LogRowModel.LastProfileProcessedWith)} = '{topic}'";
			}
			string condition = $"WHERE {nameof(LogRowModel.LastProfileProcessedWith)} != '{currentFilter}' OR {nameof(LogRowModel.LastProfileProcessedWith)} = NULL{topicsCheckString}";
			return (int)database.RowCount(logRowTableName, condition);
		}

		public void AddRange(IEnumerable<LogRowModel> list)
		{
			SQLiteTransaction transaction = database.GetAndOpenWriteTransaction();
			foreach (LogRowModel row in list)
			{
				if (breakOperation)
					break;

				database.ExecuteInsertCommand(insertRowCommandName, new List<KeyValuePair<string, string>>() {
						new KeyValuePair<string, string>(nameof(LogRowModel.Id), row.Id.ToString()),
						new KeyValuePair<string, string>(nameof(LogRowModel.EntryType), row.EntryType),
						new KeyValuePair<string, string>(nameof(LogRowModel.EntryTopic), row.EntryTopic),
						new KeyValuePair<string, string>(nameof(LogRowModel.DateTimeValue), row.DateTimeValue.ToString(CultureInfo.InvariantCulture)),
						new KeyValuePair<string, string>(nameof(LogRowModel.EntryBody), row.EntryBody),
						new KeyValuePair<string, string>(nameof(LogRowModel.IsFilteredOut), Convert.ToInt32(row.IsFilteredOut).ToString()),
						new KeyValuePair<string, string>(nameof(LogRowModel.LastProfileProcessedWith), row.LastProfileProcessedWith),
						new KeyValuePair<string, string>(nameof(LogRowModel.ValuesDict), JsonConvert.SerializeObject(row.ValuesDict))},
					transaction);
			}
			database.CommitAndCloseTransaction(transaction);
			transaction.Dispose();
		}

		public void Add(LogRowModel row)
		{
			database.ExecuteInsertCommand(insertRowCommandName, new List<KeyValuePair<string, string>>() {
					new KeyValuePair<string, string>(nameof(LogRowModel.Id), row.Id.ToString()),
					new KeyValuePair<string, string>(nameof(LogRowModel.EntryType), row.EntryType),
					new KeyValuePair<string, string>(nameof(LogRowModel.EntryTopic), row.EntryTopic),
					new KeyValuePair<string, string>(nameof(LogRowModel.DateTimeValue), row.DateTimeValue.ToString(CultureInfo.InvariantCulture)),
					new KeyValuePair<string, string>(nameof(LogRowModel.EntryBody), row.EntryBody),
					new KeyValuePair<string, string>(nameof(LogRowModel.IsFilteredOut), Convert.ToInt32(row.IsFilteredOut).ToString()),
					new KeyValuePair<string, string>(nameof(LogRowModel.LastProfileProcessedWith), row.LastProfileProcessedWith),
					new KeyValuePair<string, string>(nameof(LogRowModel.ValuesDict), JsonConvert.SerializeObject(row.ValuesDict))},
				writeTransaction);
		}

		public IEnumerable<LogRowModel> GetFilteredRowsBetweenIndices(int startIndex, int endIndex)
		{
			SQLiteDataReader reader = (database.GetReaderWithRowsBetweenIndices(logRowTableName, startIndex, endIndex, $"WHERE NOT({nameof(LogRowModel.IsFilteredOut)})", GenerateOrderingString(nameof(LogRowModel.DateTimeValue), Ordering.Ascending)) as SQLiteDataReader)!;

			List<LogRowModel> list = new List<LogRowModel>();

			while (reader.Read())
			{
				LogRowModel row = new LogRowModel()
				{
					Id = Convert.ToInt32(reader[nameof(LogRowModel.Id)]),
					EntryType = (string)reader[nameof(LogRowModel.EntryType)],
					EntryTopic = (string)reader[nameof(LogRowModel.EntryTopic)],
					DateTimeValue = DateTime.Parse((string)reader[nameof(LogRowModel.DateTimeValue)], CultureInfo.InvariantCulture),
					EntryBody = (string)reader[nameof(LogRowModel.EntryBody)],
					IsFilteredOut = Convert.ToBoolean(Convert.ToInt32(reader[nameof(LogRowModel.IsFilteredOut)])),
					LastProfileProcessedWith = (string)reader[nameof(LogRowModel.LastProfileProcessedWith)],
					ValuesDict = JsonConvert.DeserializeObject<Dictionary<string, double>>((string)reader[nameof(LogRowModel.ValuesDict)])
				};
				list.Add(row);
			}

			reader.Close();
			reader.Dispose();

			return list;
		}

		public void OpenWriteTransaction()
		{
			writeTransaction = database.GetAndOpenWriteTransaction();
		}

		public void UpdateRow(LogRowModel row)
		{
			if (breakOperation)
				return;

			database.ExecuteUpdateCommand(updateRowCommandName, new List<KeyValuePair<string, string>>() {
					new KeyValuePair<string, string>(nameof(LogRowModel.EntryType), row.EntryType),
					new KeyValuePair<string, string>(nameof(LogRowModel.EntryTopic), row.EntryTopic),
					new KeyValuePair<string, string>(nameof(LogRowModel.DateTimeValue), row.DateTimeValue.ToString(CultureInfo.InvariantCulture)),
					new KeyValuePair<string, string>(nameof(LogRowModel.EntryBody), row.EntryBody),
					new KeyValuePair<string, string>(nameof(LogRowModel.IsFilteredOut), Convert.ToInt32(row.IsFilteredOut).ToString()),
					new KeyValuePair<string, string>(nameof(LogRowModel.LastProfileProcessedWith), row.LastProfileProcessedWith),
					new KeyValuePair<string, string>(nameof(LogRowModel.ValuesDict), JsonConvert.SerializeObject(row.ValuesDict))},
				new KeyValuePair<string, string>(nameof(LogRowModel.Id), row.Id.ToString()),
				writeTransaction);
		}

		public void UpdateRowFilterStatus(LogRowModel row)
		{
			if (breakOperation)
				return;

			database.ExecuteUpdateCommand(updateRowFilterStatusCommandName, new List<KeyValuePair<string, string>>() {
					new KeyValuePair<string, string>(nameof(LogRowModel.IsFilteredOut), Convert.ToInt32(row.IsFilteredOut).ToString())},
				new KeyValuePair<string, string>(nameof(LogRowModel.Id), row.Id.ToString()),
				writeTransaction);
		}

		public void CloseWriteTransaction()
		{
			database.CommitAndCloseTransaction(writeTransaction);
			writeTransaction.Dispose();
			writeTransaction = null;
		}

		public void UpdateCurrentFilter(string filter)
		{
			currentFilter = filter;
		}

		public void UpdateTopicsWithUpdatedSourceIds(List<string> list)
		{
			topicsWithUpdatedSourceIds = list;
		}

		private string GenerateOrderingString(string columnName, Ordering order)
		{
			string orderingString = order == Ordering.Ascending ? "ASC" : "DESC";
			return $"ORDER BY {columnName} {orderingString}";
		}

		public void Disconnect()
		{
			breakOperation = true;
			if (lastRowReader != null) CloseRowReader();
			if (lastFilteredRowReader != null) CloseFilteredRowReader();
			if (lastToUpdateRowReader != null) CloseUpdatedRowReader();
			// Closing connections with transactions open should simply roll them back
			database.Disconnect();
		}
	}
}
}
