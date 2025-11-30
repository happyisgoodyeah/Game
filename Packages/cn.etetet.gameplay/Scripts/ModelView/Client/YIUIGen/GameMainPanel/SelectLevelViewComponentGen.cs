using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.View)]
    [ComponentOf(typeof(YIUIChild))]
    public partial class SelectLevelViewComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "GameMainPanel";
        public const string ResName = "SelectLevelView";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIViewComponent> u_UIView;
        public YIUIViewComponent UIView => u_UIView;
        public UnityEngine.RectTransform u_ComU_LevelContent;
        public UnityEngine.UI.Button u_ComU_LeftBtn;
        public UnityEngine.UI.Button u_ComU_RightBtn;
        public TMPro.TextMeshProUGUI u_ComU_TitleText;
        public UnityEngine.UI.Button u_ComU_BackMenuBtn;
        public UIEventP0 u_EventU_LeftBtnClick;
        public UIEventHandleP0 u_EventU_LeftBtnClickHandle;
        public const string OnEventU_LeftBtnClickInvoke = "SelectLevelViewComponent.OnEventU_LeftBtnClickInvoke";
        public UIEventP0 u_EventU_RightBtnClick;
        public UIEventHandleP0 u_EventU_RightBtnClickHandle;
        public const string OnEventU_RightBtnClickInvoke = "SelectLevelViewComponent.OnEventU_RightBtnClickInvoke";
        public UITaskEventP0 u_EventBackMenuBtnClick;
        public UITaskEventHandleP0 u_EventBackMenuBtnClickHandle;
        public const string OnEventBackMenuBtnClickInvoke = "SelectLevelViewComponent.OnEventBackMenuBtnClickInvoke";

    }
}