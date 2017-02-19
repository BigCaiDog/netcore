using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using hrcore.Model;

namespace hrcore.BLL {

	public partial class Person {

		protected static readonly hrcore.DAL.Person dal = new hrcore.DAL.Person();
		protected static readonly int itemCacheTimeout;

		static Person() {
			if (!int.TryParse(RedisHelper.Configuration["hrcore_BLL_ITEM_CACHE:Timeout_Person"], out itemCacheTimeout))
				int.TryParse(RedisHelper.Configuration["hrcore_BLL_ITEM_CACHE:Timeout"], out itemCacheTimeout);
		}
		public static List<PersonInfo> GetItems() {
			return Select.ToList();
		}
		public static PersonSelectBuild Select {
			get { return new PersonSelectBuild(dal); }
		}
	}
	public partial class PersonSelectBuild : SelectBuild<PersonInfo, PersonSelectBuild> {
		public PersonSelectBuild WhereId(params int?[] Id) {
			return this.Where1Or("a.`id` = {0}", Id);
		}
		public PersonSelectBuild WhereName(params string[] Name) {
			return this.Where1Or("a.`name` = {0}", Name);
		}
		public PersonSelectBuild WhereNameLike(params string[] Name) {
			if (Name == null || Name.Where(a => !string.IsNullOrEmpty(a)).Any() == false) return this;
			return this.Where1Or(@"a.`name` LIKE {0}", Name.Select(a => "%" + a + "%").ToArray());
		}
		protected new PersonSelectBuild Where1Or(string filterFormat, Array values) {
			return base.Where1Or(filterFormat, values) as PersonSelectBuild;
		}
		public PersonSelectBuild(IDAL dal) : base(dal, SqlHelper.Instance) { }
	}
}