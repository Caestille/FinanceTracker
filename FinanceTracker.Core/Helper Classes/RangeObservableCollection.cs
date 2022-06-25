using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace LogExplorer.Core
{
	public class RangeObservableCollection<T> : ObservableCollection<T>
	{
		private bool suppressNotification;

		public RangeObservableCollection() { }

		public RangeObservableCollection(IEnumerable<T> list)
		{
			AddRange(list);
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (!suppressNotification)
				base.OnCollectionChanged(e);
		}

		public void AddRange(IEnumerable<T> list)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));

			suppressNotification = true;

			foreach (T item in list)
				Add(item);

			suppressNotification = false;
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public void RemoveRange(IEnumerable<T> list)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));

			suppressNotification = true;

			foreach (T item in list)
				Remove(item);

			suppressNotification = false;
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public void ForceRaiseCollectionChanged()
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
	}
}