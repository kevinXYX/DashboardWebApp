using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashboardWebApp.Data
{
	public static class CallContext
	{
		static readonly ConcurrentDictionary<string, AsyncLocal<object>> state = new();

		/// <summary>
		/// Stores a given object and associates it with the specified name.
		/// </summary>
		/// <param name="name">The name with which to associate the new item in the call context.</param>
		/// <param name="data">The object to store in the call context.</param>
		public static void SetData(string name, object data) =>
			state.GetOrAdd(name, _ => new AsyncLocal<object>()).Value = data;

		/// <summary>
		/// Retrieves an object with the specified name from the <see cref="CallContext"/>.
		/// </summary>
		/// <param name="name">The name of the item in the call context.</param>
		/// <returns>The object in the call context associated with the specified name, or <see langword="null"/> if not found.</returns>
		public static object GetData(string name) =>
			state.TryGetValue(name, out AsyncLocal<object> data) ? data.Value : null;
		public static void FreeNamedDataSlot(string name)
		{
			state.TryRemove(name, out var _);
		}
	}
}
