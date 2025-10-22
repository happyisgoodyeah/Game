using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2025.10.21
    /// Desc
    /// </summary>
    [FriendOf(typeof(MainArchiveViewComponent))]
    public static partial class MainArchiveViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this MainArchiveViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this MainArchiveViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this MainArchiveViewComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        
        [YIUIInvoke(MainArchiveViewComponent.OnEventBackInvoke)]
        private static void OnEventBackInvoke(this MainArchiveViewComponent self)
        {
            ChangePanel();
            //self.UIView.Close();
        }
        #endregion YIUIEvent结束

        #region MyRegion

        private static async ETTask<bool> ChangePanel()
        {
            await YIUIMgrComponent.Inst.Root.OpenPanelAsync<GameMainPanelPanelComponent>();
            return true;
        }

        #endregion
    }
}
