using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace hrcore.BLL {

	public partial class RedisHelper : CSRedis.QuickHelperBase {
		internal static IConfigurationRoot Configuration;
		public static void InitializeConfiguration(IConfigurationRoot cfg) {
			Configuration = cfg;
			int port, poolsize, database;
			string ip, pass;
			if (!int.TryParse(cfg["ConnectionStrings:redis:port"], out port)) port = 6379;
			if (!int.TryParse(cfg["ConnectionStrings:redis:poolsize"], out poolsize)) poolsize = 50;
			if (!int.TryParse(cfg["ConnectionStrings:redis:database"], out database)) database = 0;
			ip = cfg["ConnectionStrings:redis:ip"];
			pass = cfg["ConnectionStrings:redis:pass"];
			Name = cfg["ConnectionStrings:redis:name"];
			Instance = new CSRedis.ConnectionPool(ip, port, poolsize);
			Instance.Connected += (s, o) => {
				CSRedis.RedisClient rc = s as CSRedis.RedisClient;
				if (!string.IsNullOrEmpty(pass)) rc.Auth(pass);
				if (database > 0) rc.Select(database);
			};
		}
	}

	public static partial class BLLExtensionMethods {
		public static List<TReturnInfo> ToList<TReturnInfo>(this SelectBuild<TReturnInfo> select, int expireSeconds, string cacheKey = null) { return select.ToList(RedisHelper.Get, RedisHelper.Set, TimeSpan.FromSeconds(expireSeconds), cacheKey); }
	}
}