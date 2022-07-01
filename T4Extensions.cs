#if NETSTANDARD || NET5_0_OR_GREATER || NETCOREAPP2_2_OR_GREATER

using System.Collections.Concurrent;
using System.Threading;

namespace System.CodeDom.Compiler
{
	public class CompilerErrorCollection : List<CompilerError>
	{
		public bool HasErrors => Count != 0;
	}

	public class CompilerError
	{
		private object p1;
		private int v1;
		private int v2;
		private object p2;
		private string message;

		public CompilerError()
		{
		}

		public CompilerError(object p1, int v1, int v2, object p2, string message)
		{
			this.p1 = p1;
			this.v1 = v1;
			this.v2 = v2;
			this.p2 = p2;
			this.message = message;
		}

		public string ErrorText { get; set; }

		public bool IsWarning { get; set; }
	}
}

namespace System.Runtime.Remoting.Messaging
{
	internal static class CallContext
	{
		static ConcurrentDictionary<string, AsyncLocal<object>> state = new ConcurrentDictionary<string, AsyncLocal<object>>();

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
		public static object LogicalGetData(string name) =>
			state.TryGetValue(name, out AsyncLocal<object> data) ? data.Value : null;
	}
}

#endif
