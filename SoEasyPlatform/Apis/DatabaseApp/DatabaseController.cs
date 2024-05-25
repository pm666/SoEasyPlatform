﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace SoEasyPlatform.Apis
{
    /// <summary>
    /// 数据库管理
    /// </summary>
    public class DatabaseController : BaseController
    {
        public DatabaseController(IMapper mapper) : base(mapper)
        {

        }


        /// <summary>
        /// 获取数据库列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getdatabase")]
        public ActionResult<ApiResult<TableModel<DatabaseGridViewModel>>> GetDatabase([FromForm] DatabaseViewModel model)
        {
            var result = new ApiResult<TableModel<DatabaseGridViewModel>>();
            result.Data = new TableModel<DatabaseGridViewModel>();
            int count = 0;
            var list = databaseDb.AsQueryable()
                .Where(it => it.IsDeleted == false)
                .WhereIF(!string.IsNullOrEmpty(model.Desc), it => it.Desc.Contains(model.Desc))
                .ToPageList(model.PageIndex, model.PageSize, ref count);
            result.Data.Rows = base.mapper.Map<List<DatabaseGridViewModel>>(list);
            foreach (var item in result.Data.Rows)
            {

                if (base.IsConnectionDb(mapper.Map<Database>(item)))
                {

                    try
                    {
                        item.IsExist = true;
                        item.IsConnection = true;
                    }
                    catch  
                    {
                        item.IsExist = false;
                        item.IsConnection = false;
                    }
                }

            }
            result.Data.Total = count;
            result.Data.PageSize = model.PageSize;
            result.Data.PageNumber = model.PageIndex;
            result.IsSuccess = true;
            return result;
        }

        /// <summary>
        /// 保存数据库
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [FormValidateFilter]
        [Route("savedatabase")]
        public ActionResult<ApiResult<string>> SaveDatabase([FromForm] DatabaseViewModel model)
        {
            JsonResult errorResult = base.ValidateModel(model.Id);
            if (errorResult != null) return errorResult;
            var saveObject = base.mapper.Map<Database>(model);
            var result = new ApiResult<string>();
            int dbid = 0;
            if (saveObject.Id == 0)
            {
                saveObject.ChangeTime = DateTime.Now;
                saveObject.IsDeleted = false;
                dbid= databaseDb.InsertReturnIdentity(saveObject);
                result.IsSuccess = true;
                result.Data = Pubconst.MESSAGEADDSUCCESS;
            }
            else
            {
                saveObject.ChangeTime = DateTime.Now;
                saveObject.IsDeleted = false;
                databaseDb.Update(saveObject);
                dbid = saveObject.Id;
                result.IsSuccess = true;
                result.Data = Pubconst.MESSAGEADDSUCCESS;
            }
            base.CreateDatebase(dbid);
            return result;
        }

        /// <summary>
        /// 删除数据库
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("deletedatabase")]
        public ActionResult<ApiResult<bool>> DeleteDatabase([FromForm] string model)
        {
            var result = new ApiResult<bool>();
            if (!string.IsNullOrEmpty(model))
            {
                var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DatabaseViewModel>>(model);
                var exp = Expressionable.Create<Database>();
                foreach (var item in list)
                {
                    exp.Or(it => it.Id == item.Id);
                }
                databaseDb.Update(it => new Database() { IsDeleted = true }, exp.ToExpression());
            }
            result.IsSuccess = true;
            return result;
        }
    }
}
