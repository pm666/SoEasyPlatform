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
    /// 保存虚拟类到数据库相关逻辑
    /// </summary>
    public partial class CodeTableController : BaseController
    {
        private void SaveCodeTableToDb(CodeTableViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.TableName)) 
            {
                viewModel.TableName = viewModel.TableName.Trim();
            }
            if (!string.IsNullOrEmpty(viewModel.ClassName))
            {
                viewModel.ClassName = viewModel.ClassName.Trim();
            }

            base.Check(string.IsNullOrEmpty(viewModel.TableName) || string.IsNullOrEmpty(viewModel.ClassName), "表名或者实体类名必须填一个");
            viewModel.ColumnInfoList = viewModel.ColumnInfoList
                .Where(it => !string.IsNullOrEmpty(it.ClassProperName) || !string.IsNullOrEmpty(it.DbColumnName)).ToList();
            base.Check(viewModel.ColumnInfoList.Count == 0, "请配置实体属性");
            var dbTable = mapper.Map<CodeTable>(viewModel);
            AutoFillTable(dbTable);
            var dbColumns = mapper.Map<List<CodeColumns>>(viewModel.ColumnInfoList);
            AutoFillColumns(dbColumns);
            if (dbColumns.Where(it=>!string.IsNullOrEmpty(it.DbColumnName)).GroupBy(it => it.DbColumnName.ToLower()).Any(it => it.Count() > 1)) 
            {
                base.Check(true, "存在重复列不能保存");
            }
            if (viewModel.Id == null || viewModel.Id == 0)
            {
                CheckAddName(viewModel, CodeTableDb);
                var id = CodeTableDb.InsertReturnIdentity(dbTable);
                foreach (var item in dbColumns)
                {
                    item.CodeTableId = id;
                    item.DbColumnName = GetColumnName(item.DbColumnName);
                }
                CodeColumnsDb.InsertRange(dbColumns);
            }
            else
            {
                CheckUpdateName(viewModel, CodeTableDb);
                CodeTableDb.Update(dbTable);
                foreach (var item in dbColumns)
                {
                    item.CodeTableId = dbTable.Id;
                    item.DbColumnName = GetColumnName(item.DbColumnName);
                }

                var oldIds = CodeColumnsDb.GetList(it => it.CodeTableId == dbTable.Id).Select(it => it.Id).ToList();
                var delIds = oldIds.Where(it => !dbColumns.Select(y => y.Id).Contains(it)).ToList();
                CodeColumnsDb.DeleteByIds(delIds.Select(it => (object)it).ToArray());

                var updateColumns = dbColumns.Where(it => it.Id > 0).ToList();
                if (updateColumns.Count > 0)
                {
                    CodeColumnsDb.UpdateRange(updateColumns);
                }

                var insertColumns = dbColumns.Where(it => it.Id == 0).ToList();
                if (insertColumns.Count > 0)
                {
                    CodeColumnsDb.InsertRange(insertColumns);
                }

            }
        }

        private string GetColumnName(string dbColumnName)
        {
            if (dbColumnName == null)
                return null;
            if (Regex.IsMatch(dbColumnName, @"\[.+\]")) 
            {
                return Regex.Replace(dbColumnName, @"\[.+\]","");
            }
            return dbColumnName;
        }

        private void AutoFillTable(CodeTable dbTable)
        {
            if (string.IsNullOrEmpty(dbTable.TableName))
            {
                dbTable.TableName = dbTable.ClassName;
            }
            if (string.IsNullOrEmpty(dbTable.ClassName))
            {
                dbTable.ClassName = dbTable.TableName;
            }
        }
        private void AutoFillColumns(List<CodeColumns> dbColumns)
        {
            foreach (var item in dbColumns)
            {
                if (string.IsNullOrEmpty(item.ClassProperName))
                {
                    item.ClassProperName = item.DbColumnName;
                }
                if (string.IsNullOrEmpty(item.DbColumnName))
                {
                    item.DbColumnName = item.ClassProperName;
                }
                if (!string.IsNullOrEmpty(item.ClassProperName)) 
                {
                    item.ClassProperName = item.ClassProperName.Trim();
                }
                if (!string.IsNullOrEmpty(item.DbColumnName))
                {
                    item.DbColumnName = item.DbColumnName.Trim();
                }
            }
        }

        private void CheckAddName(CodeTableViewModel viewModel, Repository<CodeTable> codeTableDb)
        {
            CheckClassName(viewModel);
            var isAny = codeTableDb.IsAny(it => it.TableName == viewModel.TableName && it.IsDeleted == false&&(viewModel.DbId.Value==0||viewModel.DbId==it.DbId));
            if (isAny)
            {
                throw new Exception(viewModel.TableName + "表名已存在");
            }
        }
        private void CheckUpdateName(CodeTableViewModel viewModel, Repository<CodeTable> codeTableDb)
        {
            CheckClassName(viewModel);
            var isAny = codeTableDb.IsAny(it => it.TableName == viewModel.TableName && it.IsDeleted == false && it.Id != viewModel.Id&&(viewModel.DbId==it.DbId||viewModel.DbId.Value==0));
            if (isAny)
            {
                throw new Exception(viewModel.TableName + "表名已存在");
            }
        }
        private void CheckClassName(CodeTableViewModel viewModel)
        {
            var First = viewModel.ClassName.First().ToString();
            if (Regex.IsMatch(First, @"\d"))
            {
                new Exception(viewModel.ClassName + "不是有效类名");
            }
        }




      


    }
}
