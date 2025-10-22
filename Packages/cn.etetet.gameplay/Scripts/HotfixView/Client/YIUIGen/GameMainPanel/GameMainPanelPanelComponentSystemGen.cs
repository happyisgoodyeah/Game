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
    [FriendOf(typeof(YIUIPanelComponent))]
    [EntitySystemOf(typeof(GameMainPanelPanelComponent))]
    public static partial class GameMainPanelPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this GameMainPanelPanelComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this GameMainPanelPanelComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this GameMainPanelPanelComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<YIUIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIPanel.Layer = EPanelLayer.Panel;
            self.UIPanel.PanelOption = EPanelOption.TimeCache;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;
            self.UIPanel.CachePanelTime = 10;

            self.u_ComSettingBtnButton = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComSettingBtnButton");
            self.u_EventOpenSetting = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventOpenSetting");
            self.u_EventOpenSettingHandle = self.u_EventOpenSetting.Add(self,GameMainPanelPanelComponent.OnEventOpenSettingInvoke);

        }
    }
}
