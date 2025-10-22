using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Author  YIUI
    /// Date    2025.10.17
    /// Desc
    /// </summary>
    [FriendOf(typeof(ArchivePanelComponent))]
    public static partial class ArchivePanelComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this ArchivePanelComponent self)
        {
             InitShow(self);
        }

        [EntitySystem]
        private static void Destroy(this ArchivePanelComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this ArchivePanelComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region selfFunc

        private static async ETTask<bool> InitShow(this ArchivePanelComponent self)
        {
            //await YIUIMgrComponent.Inst.Root.OpenPanelAsync<MainArchiveViewComponent>();
            await self.UIPanel.OpenViewAsync<MainArchiveViewComponent>();
            return true;
        }

        #endregion

        #region YIUIEvent开始

        #endregion YIUIEvent结束
    }
}