using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  Behappy
    /// Date    2025.12.1
    /// Desc
    /// </summary>
    [FriendOf(typeof(SettingsViewComponent))]
    public static partial class SettingsViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this SettingsViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this SettingsViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this SettingsViewComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(SettingsViewComponent.OnEventBackBtnClickInvoke)]
        private static async ETTask OnEventBackBtnClickInvoke(this SettingsViewComponent self)
        {
            
            await ETTask.CompletedTask;
        }
        #endregion YIUIEvent结束
    }
}
