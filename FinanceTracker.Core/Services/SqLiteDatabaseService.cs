using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;

namespace FinanceTracker.Core.Services
{
	public class SqLiteDatabaseService
	{
		private readonly string connectionString;

		private readonly SQLiteConnection readConnection;
		private readonly SQLiteConnection writeConnection;

		private readonly Dictionary<string, List<string>> currentTablesAndColumns = new();

		private readonly Dictionary<string, SQLiteCommand> commands = new();
		private readonly Dictionary<string, List<SQLiteParameter>> commandParameters = new();
		private readonly Dictionary<string, SQLiteParameter> commandCondtionalParameters = new();

		public SqLiteDatabaseService(string fullPath, bool newDatabase)
		{
			connectionString = $"Data Source={fullPath};Version=3;";

			if (newDatabase)
				SQLiteConnection.CreateFile(fullPath);

			readConnection = new SQLiteConnection(connectionString);
			writeConnection = new SQLiteConnection(connectionString);
			readConnection.Open();
			writeConnection.Open();
		}

		public void AddTableAndColumns(string tableName, KeyValuePair<string, string>[] columnNamesAndDataTypes,
			string[] columnsToIndex)
		{
			CreateTableIfNeeded(tableName);
			foreach (KeyValuePair<string, string> columnAndDataType in columnNamesAndDataTypes)
			{
				AddColumnToTableIfNeeded(tableName, columnAndDataType.Key, columnAndDataType.Value);
			}

			foreach (string columnToIndex in columnsToIndex)
			{
				IndexColumn(tableName, columnToIndex + "Index", columnToIndex);
			}
		}

		public void SetUpUpdateCommand(string tableName, string commandName, List<string> parametersToAdd, string conditionalMatchParameter)
		{
			SQLiteCommand command = new SQLiteCommand(writeConnection);
			string updateText = "";
			foreach (string name in parametersToAdd)
				updateText += $"{name} = ${name}" + (parametersToAdd.IndexOf(name) != parametersToAdd.Count - 1 ? ", " : "");
			string conditionalText = $"{conditionalMatchParameter} = ${conditionalMatchParameter}";
			string commandText = $"UPDATE {tableName} SET {updateText} WHERE {conditionalText};";
			command.CommandText = commandText;
			if (!commandParameters.ContainsKey(commandName))
				commandParameters[commandName] = new List<SQLiteParameter>();
			foreach (string name in parametersToAdd)
			{
				SQLiteParameter param = command.CreateParameter();
				param.ParameterName = name;
				command.Parameters.Add(param);
				commandParameters[commandName].Add(param);
			}

			SQLiteParameter conditionalParam = command.CreateParameter();
			conditionalParam.ParameterName = conditionalMatchParameter;
			command.Parameters.Add(conditionalParam);
			commandCondtionalParameters[commandName] = conditionalParam;

			commands[commandName] = command;
		}

		public void SetUpInsertCommand(string tableName, string commandName, List<string> parametersToAdd)
		{
			SQLiteCommand command = new SQLiteCommand(writeConnection);
			string columnNames = "";
			foreach (string name in parametersToAdd)
				columnNames += $"{name}" + (parametersToAdd.IndexOf(name) != parametersToAdd.Count - 1 ? ", " : "");
			string values = "";
			foreach (string name in parametersToAdd)
				values += $"${name}" + (parametersToAdd.IndexOf(name) != parametersToAdd.Count - 1 ? ", " : "");
			string commandText = $"INSERT INTO {tableName} ({columnNames}) VALUES ({values});";
			command.CommandText = commandText;
			if (!commandParameters.ContainsKey(commandName))
				commandParameters[commandName] = new List<SQLiteParameter>();
			foreach (string name in parametersToAdd)
			{
				SQLiteParameter param = command.CreateParameter();
				param.ParameterName = name;
				command.Parameters.Add(param);
				commandParameters[commandName].Add(param);
			}

			commands[commandName] = command;
		}

		public SQLiteTransaction GetAndOpenWriteTransaction()
		{
			return writeConnection.BeginTransaction();
		}

		public long RowCount(string tableName, string condition)
		{
			SQLiteCommand cmd = new SQLiteCommand(readConnection);

			cmd.CommandText = $"SELECT COUNT(*) FROM {tableName} {condition};";
			return Convert.ToInt32(cmd.ExecuteScalar());
		}

		public void ExecuteUpdateCommand(string commandName, List<KeyValuePair<string, string>> paramsToUpdate, KeyValuePair<string, string> conditionalParamToUpdate, SQLiteTransaction transaction = null)
		{
			foreach (KeyValuePair<string, string> param in paramsToUpdate)
				commandParameters[commandName].Find(x => x.ParameterName == param.Key)!.Value = param.Value;

			commandCondtionalParameters[commandName].Value = conditionalParamToUpdate.Value;

			SQLiteCommand command = commands[commandName];
			if (transaction != null && command.Transaction != transaction)
				command.Transaction = transaction;
			command.ExecuteNonQuery();
		}

		public void ExecuteInsertCommand(string commandName, List<KeyValuePair<string, string>> paramsToInsert, SQLiteTransaction transaction = null)
		{
			foreach (KeyValuePair<string, string> param in paramsToInsert)
				commandParameters[commandName].Find(x => x.ParameterName == param.Key)!.Value = param.Value;

			SQLiteCommand command = commands[commandName];
			if (transaction != null && command.Transaction != transaction)
				command.Transaction = transaction;
			command.ExecuteNonQuery();
		}

		public void Clear(string tableName)
		{
			SQLiteCommand cmd = new SQLiteCommand(writeConnection);

			cmd.CommandText = $"DELETE FROM {tableName};";
			cmd.ExecuteNonQuery();
		}

		public void AddRowToTable(string tableName, List<KeyValuePair<string, string>> namedValues, SQLiteTransaction transaction = null)
		{
			SQLiteCommand cmd = new SQLiteCommand(writeConnection);
			string columnNames = "";
			foreach (KeyValuePair<string, string> kvp in namedValues)
				columnNames += $"{kvp.Key}" + (namedValues.IndexOf(kvp) != namedValues.Count - 1 ? ", " : "");

			string values = "";
			foreach (KeyValuePair<string, string> kvp in namedValues)
				values += $"'{kvp.Value}'" + (namedValues.IndexOf(kvp) != namedValues.Count - 1 ? ", " : "");

			cmd.CommandText = $"INSERT INTO {tableName} ({columnNames}) VALUES ({values});";
			if (transaction != null)
				cmd.Transaction = transaction;

			cmd.ExecuteNonQuery();
		}

		public void CommitAndCloseTransaction(SQLiteTransaction transaction)
		{
			transaction.Commit();
			transaction.Dispose();
		}

		public IEnumerable GetReaderWithRowsBetweenIndices(string tableName, int startIndex, int endIndex, string condition, string ordering)
		{
			SQLiteCommand cmd = new SQLiteCommand(readConnection);
			cmd.CommandText = $"SELECT * FROM {tableName} {condition} {ordering} LIMIT {endIndex - startIndex} OFFSET {startIndex};";
			SQLiteDataReader reader = cmd.ExecuteReader();
			return reader;
		}

		public IEnumerable GetRows(string tableName, string rowCondition, string ordering)
		{
			SQLiteCommand cmd = new SQLiteCommand(readConnection);

			cmd.CommandText = $"SELECT * FROM {tableName} {rowCondition} {ordering};";
			SQLiteDataReader reader = cmd.ExecuteReader();

			return reader;
		}

		private void CreateTableIfNeeded(string tableName)
		{
			if (!currentTablesAndColumns.ContainsKey(tableName))
			{
				SQLiteCommand cmd = new SQLiteCommand(writeConnection);
				cmd.CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} (Id INTEGER);";
				cmd.ExecuteNonQuery();
				currentTablesAndColumns[tableName] = new List<string>();
			}
		}

		private void AddColumnToTableIfNeeded(string tableName, string columnName, string dataType)
		{
			if (!currentTablesAndColumns[tableName].Contains(columnName))
			{
				try
				{
					SQLiteCommand cmd = new SQLiteCommand(writeConnection);

					cmd.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {dataType};";
					cmd.ExecuteNonQuery();
				}
				catch (SQLiteException e)
				{
					// column already existed in table. Do nothing
				}
			}

			currentTablesAndColumns[tableName].Add(columnName);
		}

		public void IndexColumn(string tableName, string indexName, string columnName)
		{
			SQLiteCommand cmd2 = new SQLiteCommand(writeConnection);
			cmd2.CommandText = $"CREATE UNIQUE INDEX IF NOT EXISTS {indexName} ON {tableName} ({columnName});";
			cmd2.ExecuteNonQuery();
		}

		public void Disconnect()
		{
			SQLiteConnection.ClearAllPools();
			readConnection.Close();
			writeConnection.Close();
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}
