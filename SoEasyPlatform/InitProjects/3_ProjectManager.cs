﻿using Newtonsoft.Json.Linq;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace SoEasyPlatform
{
    /// <summary>
    /// 方案管理
    /// </summary>
    public partial class InitTable
    {
        private string projectNames;
        private List<int> AddProjects(string sln, string slnName)
        {
            List<int> result = new List<int>();
            var configUrl = FileSugar.MergeUrl(sln, "Config.json");
            CheckProjectConfig(configUrl);
            var json = FileSugar.FileToString(configUrl);
            var projects = JArray.Parse(json);
            foreach (var item in projects)
            {
                CheckConfigItemChilds(configUrl, item);
                var 模版 = item["模版"].ToString();
                var 文件夹 = item["文件夹"].ToString();
                var 子目录 = item["子目录"].ToString();
                var 文件后缀 = item["文件后缀"].ToString();
                var 描述 = item["描述"].ToString();
                var tempUrl = FileSugar.MergeUrl(sln, 模版);
                CheckProjectConfig(tempUrl);
                var tempId = AddTemplate(configUrl, 文件夹, 描述, tempUrl, slnName);
                var projectPath = FileSugar.MergeUrl(sln, 文件夹);
                var id = AddProject(tempId, slnName, 文件夹, sln, projectPath, 文件后缀, 子目录);
                result.Add(id);
            }
            return result;
        }

        private int AddProject(int tempId,string slnName, string apiName, string rootPath, string projectPath, string suff,string format)
        {
            var files = FileSugar.GetFileNames(projectPath,"*",true);
            Project project = new Project();
            List<int> fieldIds = new List<int>();
            JArray jArray = new JArray();
            foreach (var filePath in files)
            {

                var path = filePath.Replace(projectPath, "").TrimStart('\\').TrimStart('/');
                FileInfo file = new FileInfo()
                {
                    ChangeTime = DateTime.Now,
                    Directory=path.Replace(System.IO.Path.GetFileName(filePath),""),
                    Content = FileSugar.FileToString(filePath),
                    IsDeleted = false,
                    IsInit = true,
                    Name = System.IO.Path.GetFileNameWithoutExtension(filePath),
                    Json = "{\"name\":\"" + System.IO.Path.GetFileNameWithoutExtension(filePath) + "\"}",
                    Suffix =System.IO.Path.GetExtension(filePath),
                    SolutionId = groupId + "",
                    Sort = 999, 
                };
                if (file.Suffix == "dll") 
                {
                    continue;
                }
                jArray.Add(JObject.Parse( "{ \"name\":\""+file.Name+"\"}"));
                var fieldId = db.Insertable(file).ExecuteReturnIdentity();
                fieldIds.Add(fieldId);
            }
            project.ProjentName = slnName+"_"+apiName;
            projectNames += (apiName + ",");
            project.Path =FileSugar.MergeUrl("c:\\Projects\\",slnName,apiName);
            project.SolutionId = "0";
            project.TemplateId1 = tempId + "";
            project.FileInfo = String.Join(",", fieldIds);
            project.FileSuffix = suff;
            project.SolutionId = groupId + "";
            project.IsInit = true;
            project.FileModel = jArray.ToString();
            if (!string.IsNullOrEmpty(format))
            {
                project.NameFormat = format;
            }
            project.ModelId =tempTypeId;
            //project.ty = tempTypeId + "";
            var pid = db.Insertable(project).ExecuteReturnIdentity();
            return pid;
        }
    }
}
