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
    [EntitySystemOf(typeof(SettingsViewComponent))]
    public static partial class SettingsViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this SettingsViewComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this SettingsViewComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this SettingsViewComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIView = self.UIBase.GetComponent<YIUIViewComponent>();
            self.UIWindow.WindowOption = EWindowOption.BanOpenTween|EWindowOption.BanCloseTween;
            self.UIView.ViewWindowType = EViewWindowType.View;
            self.UIView.StackOption = EViewStackOption.VisibleTween;

            self.u_ComU_BackBtn = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComU_BackBtn");
            self.u_EventBackBtnClick = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventBackBtnClick");
            self.u_EventBackBtnClickHandle = self.u_EventBackBtnClick.Add(self,SettingsViewComponent.OnEventBackBtnClickInvoke);

        }
    }
}
