using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinnFragen.Web
{
	public class Notification
	{
		public string Address { get; set; }
		public string Name { get; set; }

		public Target MessageTarget { get; set; }

		public string MessageHTML { get; set; }
		public string MessageText { get; set; }


		public string Subject { get; set; }


		public enum Target
		{
			User,
			Admin
		}
	}
}
