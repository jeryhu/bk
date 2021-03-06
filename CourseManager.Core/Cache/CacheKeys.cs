﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseManager.Core.Cache
{
    public class CacheKeys
    {
        /// <summary>
        /// CourseManager站点的缓存数据统一放在这个节点下
        /// </summary>
        public const string CacheNodeName = "CourseManager";
        /// <summary>
        /// 获取所有Category表的数据的缓存key
        /// </summary>
        public const string GetAllCategory = "GetAllCategory";
    }
}
