using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [FriendOf(typeof(YIUIChild))]
    [FriendOf(typeof(YIUIWindowComponent))]
    [FriendOf(typeof(YIUIViewComponent))]
    [EntitySystemOf(typeof(Main1ViewComponent))]
    public static partial class Main1ViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this Main1ViewComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this Main1ViewComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this Main1ViewComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIView = self.UIBase.GetComponent<YIUIViewComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIView.ViewWindowType = EViewWindowType.View;
            self.UIView.StackOption = EViewStackOption.Visible;

            self.u_ComArchiveEnterBtnButton = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComArchiveEnterBtnButton");
            self.u_ComExitBtnButton = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComExitBtnButton");
            self.u_ComU_SettingsBtn = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComU_SettingsBtn");
            self.u_ComU_PlayGameBtn = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComU_PlayGameBtn");
            self.u_EventArchiveEnter = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventArchiveEnter");
            self.u_EventArchiveEnterHandle = self.u_EventArchiveEnter.Add(self,Main1ViewComponent.OnEventArchiveEnterInvoke);
            self.u_EventExitGame = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventExitGame");
            self.u_EventExitGameHandle = self.u_EventExitGame.Add(self,Main1ViewComponent.OnEventExitGameInvoke);
            self.u_EventSettingsEnter = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventSettingsEnter");
            self.u_EventSettingsEnterHandle = self.u_EventSettingsEnter.Add(self,Main1ViewComponent.OnEventSettingsEnterInvoke);
            self.u_EventStartGame = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventStartGame");
            self.u_EventStartGameHandle = self.u_EventStartGame.Add(self,Main1ViewComponent.OnEventStartGameInvoke);

        }
    }
}
