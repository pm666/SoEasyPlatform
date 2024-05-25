﻿using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoEasyPlatform 
{
    public class FileInfo
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Content { get; set; }
        public string Json { get; set; }
        public string Suffix { get; set; }
        public int Sort { get; set; }

        public DateTime ChangeTime { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsInit { get; set; }
        /// <summary>
        /// 项目ID
        /// </summary>
        public Guid PojectId { get; set; }
        /// <summary>
        /// 方案ID
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 50)]
        public string SolutionId { get; set; }

        [SugarColumn(IsNullable = true)]
        public string Directory { get; set; } 
    }
}
