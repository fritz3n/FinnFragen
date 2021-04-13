using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FinnFragen.Web.Services
{
	public class NotifyQueue
	{
		private ConcurrentQueue<Notification> queue = new();
		private AsyncAutoResetEvent resetEvent = new AsyncAutoResetEvent();


		public async Task<Notification> WaitForNotification(CancellationToken token = default)
		{
			Notification notification;
			do
			{
				token.ThrowIfCancellationRequested();
				await resetEvent.WaitAsync(token);
			} while (!queue.TryDequeue(out notification));

			return notification;
		}

		public void EnqueueNotification(Notification notification)
		{
			queue.Enqueue(notification);
			resetEvent.Set();
		}
	}
}
