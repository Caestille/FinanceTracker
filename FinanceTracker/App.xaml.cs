using CoreUtilities.Interfaces;
using CoreUtilities.Services;
using FinanceTracker.Core.ViewModels;
using FinanceTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace FinanceTracker
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		public static string[]? StartingArgs { get; set; }
		public static string CrashReportsDirectory => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\FinanceTracker\CrashReports";

		public App()
		{
			AppDomain.CurrentDomain.UnhandledException += ExceptionHandler;
		}

		private static void ExceptionHandler(object sender, UnhandledExceptionEventArgs args)
		{
#if RELEASE
			Exception e = (Exception)args.ExceptionObject;
			if (!Directory.Exists(CrashReportsDirectory))
				Directory.CreateDirectory(CrashReportsDirectory);
			string path = CrashReportsDirectory;

			string exceptionText = "FinanceTracker Crash Report\n";

			exceptionText += $"Date/Time: {DateTime.UtcNow} UTC\n";
			exceptionText += $"Version: {Assembly.GetExecutingAssembly().GetName().Version}\n";
			exceptionText += $"Source: {e.Source}\n";
			exceptionText += $"Message: {e.Message}\n";
			exceptionText += $"InnerException:\n{e.InnerException}";
			exceptionText += $"Stack trace:\n{e.StackTrace}\n";

			File.WriteAllText(string.Format(path + "\\FinanceTrackerCrashReport{0}.txt", DateTime.UtcNow.ToString("ddMMyyyy-HHmmss")), exceptionText);
			MessageBox.Show(string.Format("FinancetrackerCrashReport{0}.txt written to \n" + path, DateTime.UtcNow.ToString("ddMMyyyy-HHmmss")));
			Process.Start(path);
#endif
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			if (e.Args.Length > 0)
			{
				StartingArgs = e.Args;
			}

			var registryService = new RegistryService(@"SOFTWARE\FinanceTracker");

			var viewModels = new List<ViewModelBase>()
			{
				new DashboardViewModel(),
				new BanksViewModel(),
				new AnalysisViewModel(),
				new BudgetingViewModel(),
			};

			var mainViewModel = new MainViewModel(viewModels, registryService);
			viewModels[0].SelectCommand.Execute(null);

			var mainView = new MainWindow()
			{
				DataContext = mainViewModel
			};

			mainView.Show();
		}
	}
}
