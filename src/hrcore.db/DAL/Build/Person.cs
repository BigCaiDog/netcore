using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using hrcore.Model;

namespace hrcore.DAL {

	public partial class Person : IDAL {
		#region transact-sql define
		public string Table { get { return TSQL.Table; } }
		public string Field { get { return TSQL.Field; } }
		public string Sort { get { return TSQL.Sort; } }
		internal class TSQL {
			internal static readonly string Table = "`person`";
			internal static readonly string Field = "a.`id`, a.`name`";
			internal static readonly string Sort = "";
			public static readonly string Delete = "DELETE FROM `person` WHERE ";
			public static readonly string Insert = "INSERT INTO `person`(`id`, `name`) VALUES(?id, ?name)";
		}
		#endregion

		#region common call
		protected static MySqlParameter GetParameter(string name, MySqlDbType type, int size, object value) {
			MySqlParameter parm = new MySqlParameter(name, type, size);
			parm.Value = value;
			return parm;
		}
		protected static MySqlParameter[] GetParameters(PersonInfo item) {
			return new MySqlParameter[] {
				GetParameter("?id", MySqlDbType.Int32, 11, item.Id), 
				GetParameter("?name", MySqlDbType.VarChar, 50, item.Name)};
		}
		public PersonInfo GetItem(IDataReader dr) {
			int index = -1;
			return GetItem(dr, ref index) as PersonInfo;
		}
		public object GetItem(IDataReader dr, ref int index) {
			PersonInfo item = new PersonInfo();
				if (!dr.IsDBNull(++index)) item.Id = (int?)dr.GetInt32(index);
				if (!dr.IsDBNull(++index)) item.Name = dr.GetString(index);
			return item;
		}
		#endregion
	}
}