﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoEasyPlatform.Apis
{
    /// <summary>
    /// 数据库导入虚拟类相关逻辑
    /// </summary>
    public partial class CodeTableController : BaseController
    {
        private SortTypeInfo GetEntityType(List<CodeType> types, DbColumnInfo columnInfo, CodeTableController codeTableController,DbType dbtype)
        {
            var typeInfo = types.FirstOrDefault(y => y.DbType.Any(it => it.Name.Equals(columnInfo.DataType, StringComparison.OrdinalIgnoreCase)));
            if (typeInfo == null && (columnInfo.DataType + "").ToLower() == "double")
            {
                var type = types.First(it => it.Name == "decimal_18_4");
                return new SortTypeInfo() { CodeType = type, DbTypeInfo = type.DbType[0] };
            }
            else if (typeInfo == null && columnInfo.DataType == "int unsigned") 
            {
                var type = types.First(it => it.Name == "int");
                return new SortTypeInfo() { CodeType = type, DbTypeInfo = type.DbType[0] };
            }
            else if (typeInfo == null)
            {
                var type = types.First(it => it.Name == "string100");
                return new SortTypeInfo() { CodeType = type, DbTypeInfo = type.DbType[0] };
            }
            else if (columnInfo.DataType == "json")
            {
                var type = types.First(it => it.Name.ToLower() == "string2000");
                return new SortTypeInfo() { CodeType = type, DbTypeInfo = type.DbType[0] };
            }
            else if (columnInfo.DataType == "timestamp" && dbtype != DbType.SqlServer)
            {
                var type = types.First(it => it.Name.ToLower() == "datetime");
                return new SortTypeInfo() { CodeType = type, DbTypeInfo = type.DbType[0] };
            }
            else
            {
                List<SortTypeInfo> SortTypeInfoList = new List<SortTypeInfo>();
                foreach (var type in types)
                {
                    foreach (var cstype in type.DbType)
                    {
                        SortTypeInfo item = new SortTypeInfo();
                        item.DbTypeInfo = cstype;
                        item.CodeType = type;
                        item.Sort = GetSort(cstype, type, columnInfo, dbtype);
                        SortTypeInfoList.Add(item);
                    }
                }
                var result = SortTypeInfoList.Where(it => it.CodeType.Name != "json_default").OrderByDescending(it => it.Sort).FirstOrDefault();
                if (result == null)
                {
                    throw new Exception($"没有匹配到类型{columnInfo.DataType} 来自 {columnInfo.TableName} 表 {columnInfo.DbColumnName} ，请到类型管理添加");
                }
                if (result.CodeType.Name == "guid" && columnInfo.DataType == "char" && columnInfo.Length != 36)
                {
                    result = SortTypeInfoList.FirstOrDefault(it => it.CodeType.Name == "string100");
                }
                if (dbtype == DbType.PostgreSQL)
                {
                    if (columnInfo.DataType == "bit")
                    {
                        var type = types.First(it => it.Name.ToLower() == "bytearray");
                        return new SortTypeInfo() { CodeType = type, DbTypeInfo = type.DbType[0] };
                    }
                }
                return result;
            }
        }
        /// <summary>
        /// 匹配出最符合的类型
        /// </summary>
        /// <param name="cstype"></param>
        /// <param name="type"></param>
        /// <param name="columnInfo"></param>
        /// <param name="dbtype"></param>
        /// <returns></returns>
        private decimal GetSort(DbTypeInfo cstype, CodeType type, DbColumnInfo columnInfo, DbType dbtype)
        {
            decimal result = 0;
            if (columnInfo.DataType.Equals(cstype.Name,StringComparison.OrdinalIgnoreCase))
            {
                result = result + 10000;
            }
            else 
            {
                result = result - 30000;
            }
            if (columnInfo.Length ==Convert.ToInt32(cstype.Length) )
            {
                result = result + 5000;
            }
            else if (columnInfo.Length > Convert.ToInt32(cstype.Length))
            {
                result = result+(columnInfo.Length- Convert.ToInt32(cstype.Length))*-3;
            }
            else 
            {
                result = result - 500;
            }
            if (columnInfo.DecimalDigits == Convert.ToInt32(cstype.DecimalDigits))
            {
                result = result +5000;
            }
            else if (columnInfo.DecimalDigits > Convert.ToInt32(cstype.DecimalDigits))
            {
                result = result + (columnInfo.DecimalDigits - Convert.ToInt32(cstype.DecimalDigits)) * -3;
            }
            else
            {
                result = result - 500;
            }
            if (type.Name.Contains("nString") && columnInfo.DataType == "varchar")
            {
                result = result - 500;
            }
            return result;
        }
        /// <summary>
        /// 排序计算MODEL
        /// </summary>
        public class SortTypeInfo 
        {
            public DbTypeInfo DbTypeInfo { get; set; }
            public decimal Sort { get; set; }
            public CodeType CodeType { get; set; }
        }
    }
}
