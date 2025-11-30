using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  Behappy
    /// Date    2025.11.26
    /// Desc
    /// </summary>
    public partial class SelectLevelViewComponent : Entity , IDynamicEvent<SelectLevelView_LevelSlotGoGrid>
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        public int nowPage;
        
        /// <summary>
        /// 当前页码
        /// </summary>
        public int maxPage;
        
        /// <summary>
        /// LevelSlotComponent List
        /// </summary>
        public List<EntityRef<LevelSlotComponent>> levelSlotComponents;
    }
}
