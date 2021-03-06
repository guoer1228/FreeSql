﻿using FreeSql.Internal;
using FreeSql.Internal.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace FreeSql.Sqlite {

	class SqliteUtils : CommonUtils {
		public SqliteUtils(IFreeSql orm) : base(orm) {
		}

		internal override DbParameter AppendParamter(List<DbParameter> _params, string parameterName, Type type, object value) {
			if (string.IsNullOrEmpty(parameterName)) parameterName = $"p_{_params?.Count}";
			else if (_orm.CodeFirst.IsSyncStructureToLower) parameterName = parameterName.ToLower();
			var dbtype = (DbType)_orm.CodeFirst.GetDbInfo(type)?.type;
			switch (dbtype) {
				case DbType.Guid:
					if (value == null) value = null;
					else value = ((Guid)value).ToString();
					dbtype = DbType.String;
					break;
				case DbType.Time:
					if (value == null) value = null;
					else value = ((TimeSpan)value).Ticks / 10000;
					dbtype = DbType.Int64;
					break;
			}
			var ret = new SQLiteParameter { ParameterName = $"@{parameterName}", DbType = dbtype, Value = value };
			_params?.Add(ret);
			return ret;
		}

		internal override DbParameter[] GetDbParamtersByObject(string sql, object obj) =>
			Utils.GetDbParamtersByObject<SQLiteParameter>(sql, obj, "@", (name, type, value) => {
				var dbtype = (DbType)_orm.CodeFirst.GetDbInfo(type)?.type;
				switch (dbtype) {
					case DbType.Guid:
						if (value == null) value = null;
						else value = ((Guid)value).ToString();
						dbtype = DbType.String;
						break;
					case DbType.Time:
						if (value == null) value = null;
						else value = ((TimeSpan)value).Ticks / 10000;
						dbtype = DbType.Int64;
						break;
				}
				var ret = new SQLiteParameter { ParameterName = $"@{name}", DbType = dbtype, Value = value };
				return ret;
			});

		internal override string FormatSql(string sql, params object[] args) => sql?.FormatSqlite(args);
		internal override string QuoteSqlName(string name) => _orm.CodeFirst.IsQuoteSqlName ? $"\"{name.Trim('"').Replace(".", "\".\"")}\"" : name;
		internal override string QuoteParamterName(string name) => $"@{(_orm.CodeFirst.IsSyncStructureToLower ? name.ToLower() : name)}";
		internal override string IsNull(string sql, object value) => $"ifnull({sql}, {value})";
		internal override string StringConcat(string left, string right, Type leftType, Type rightType) => $"{left} || {right}";
		internal override string Mod(string left, string right, Type leftType, Type rightType) => $"{left} % {right}";

		internal override string QuoteWriteParamter(Type type, string paramterName) => paramterName;
		internal override string QuoteReadColumn(Type type, string columnName) => columnName;
		internal override string DbName => "Sqlite";
	}
}
