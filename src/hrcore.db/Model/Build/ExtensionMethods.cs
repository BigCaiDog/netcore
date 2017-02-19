using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace hrcore.Model {
	public static partial class ExtensionMethods {
		public static string ToJson(this PersonInfo item) { return string.Concat(item); }
		public static string ToJson(this PersonInfo[] items) { return GetJson(items); }
		public static string ToJson(this IEnumerable<PersonInfo> items) { return GetJson(items); }
		public static IDictionary[] ToBson(this PersonInfo[] items, Func<PersonInfo, object> func = null) { return GetBson(items, func); }
		public static IDictionary[] ToBson(this IEnumerable<PersonInfo> items, Func<PersonInfo, object> func = null) { return GetBson(items, func); }

		public static string GetJson(IEnumerable items) {
			StringBuilder sb = new StringBuilder();
			sb.Append("[");
			IEnumerator ie = items.GetEnumerator();
			if (ie.MoveNext()) {
				while (true) {
					sb.Append(string.Concat(ie.Current));
					if (ie.MoveNext()) sb.Append(",");
					else break;
				}
			}
			sb.Append("]");
			return sb.ToString();
		}
		public static IDictionary[] GetBson(IEnumerable items, Delegate func = null) {
			List<IDictionary> ret = new List<IDictionary>();
			IEnumerator ie = items.GetEnumerator();
			while (ie.MoveNext()) {
				if (ie.Current == null) ret.Add(null);
				else if (func == null) ret.Add(ie.Current.GetType().GetMethod("ToBson").Invoke(ie.Current, new object[] { false }) as IDictionary);
				else {
					object obj = func.GetMethodInfo().Invoke(func.Target, new object[] { ie.Current });
					if (obj is IDictionary) ret.Add(obj as IDictionary);
					else {
						Hashtable ht = new Hashtable();
						PropertyInfo[] pis = obj.GetType().GetProperties();
						foreach (PropertyInfo pi in pis) ht[pi.Name] = pi.GetValue(obj);
						ret.Add(ht);
					}
				}
			}
			return ret.ToArray();
		}
	}
}