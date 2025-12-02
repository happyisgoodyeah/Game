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
    [FriendOf(typeof(GameViewComponent))]
    public static partial class GameViewComponentSystem
    {
        [EntitySystem]
        private static void YIUIInitialize(this GameViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this GameViewComponent self)
        {
        }

        [EntitySystem]
        private static async ETTask<bool> YIUIOpen(this GameViewComponent self)
        {
            await ETTask.CompletedTask;
            return true;
        }

        #region YIUIEvent开始
        [YIUIInvoke(GameViewComponent.OnEventBackBtnClickInvoke)]
        private static async ETTask OnEventBackBtnClickInvoke(this GameViewComponent self)
        {
            self.UIView.Close();
            await YIUIMgrComponent.Inst.Root.OpenPanelAsync<GameMainPanelPanelComponent , EGameMainPanelPanelViewEnum>(EGameMainPanelPanelViewEnum.SelectLevelView);
        }
        #endregion YIUIEvent结束
    }
}
