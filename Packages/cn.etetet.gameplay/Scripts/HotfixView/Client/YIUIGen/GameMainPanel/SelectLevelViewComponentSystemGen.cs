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
    [EntitySystemOf(typeof(SelectLevelViewComponent))]
    public static partial class SelectLevelViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this SelectLevelViewComponent self)
        {
        }

        [EntitySystem]
        private static void YIUIBind(this SelectLevelViewComponent self)
        {
            self.UIBind();
        }

        private static void UIBind(this SelectLevelViewComponent self)
        {
            self.u_UIBase = self.GetParent<YIUIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<YIUIWindowComponent>();
            self.u_UIView = self.UIBase.GetComponent<YIUIViewComponent>();
            self.UIWindow.WindowOption = EWindowOption.BanOpenTween|EWindowOption.BanCloseTween;
            self.UIView.ViewWindowType = EViewWindowType.View;
            self.UIView.StackOption = EViewStackOption.VisibleTween;

            self.u_ComU_LevelContent = self.UIBase.ComponentTable.FindComponent<UnityEngine.RectTransform>("u_ComU_LevelContent");
            self.u_ComU_LeftBtn = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComU_LeftBtn");
            self.u_ComU_RightBtn = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComU_RightBtn");
            self.u_ComU_TitleText = self.UIBase.ComponentTable.FindComponent<TMPro.TextMeshProUGUI>("u_ComU_TitleText");
            self.u_ComU_BackMenuBtn = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.Button>("u_ComU_BackMenuBtn");
            self.u_EventU_LeftBtnClick = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventU_LeftBtnClick");
            self.u_EventU_LeftBtnClickHandle = self.u_EventU_LeftBtnClick.Add(self,SelectLevelViewComponent.OnEventU_LeftBtnClickInvoke);
            self.u_EventU_RightBtnClick = self.UIBase.EventTable.FindEvent<UIEventP0>("u_EventU_RightBtnClick");
            self.u_EventU_RightBtnClickHandle = self.u_EventU_RightBtnClick.Add(self,SelectLevelViewComponent.OnEventU_RightBtnClickInvoke);
            self.u_EventBackMenuBtnClick = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventBackMenuBtnClick");
            self.u_EventBackMenuBtnClickHandle = self.u_EventBackMenuBtnClick.Add(self,SelectLevelViewComponent.OnEventBackMenuBtnClickInvoke);

        }
    }
}
