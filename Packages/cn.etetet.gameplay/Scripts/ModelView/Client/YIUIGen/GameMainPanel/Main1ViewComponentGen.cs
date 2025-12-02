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
    public partial class Main1ViewComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "GameMainPanel";
        public const string ResName = "Main1View";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIViewComponent> u_UIView;
        public YIUIViewComponent UIView => u_UIView;
        public UnityEngine.UI.Button u_ComArchiveEnterBtnButton;
        public UnityEngine.UI.Button u_ComExitBtnButton;
        public UnityEngine.UI.Button u_ComU_SettingsBtn;
        public UnityEngine.UI.Button u_ComU_PlayGameBtn;
        public UIEventP0 u_EventArchiveEnter;
        public UIEventHandleP0 u_EventArchiveEnterHandle;
        public const string OnEventArchiveEnterInvoke = "Main1ViewComponent.OnEventArchiveEnterInvoke";
        public UIEventP0 u_EventExitGame;
        public UIEventHandleP0 u_EventExitGameHandle;
        public const string OnEventExitGameInvoke = "Main1ViewComponent.OnEventExitGameInvoke";
        public UIEventP0 u_EventSettingsEnter;
        public UIEventHandleP0 u_EventSettingsEnterHandle;
        public const string OnEventSettingsEnterInvoke = "Main1ViewComponent.OnEventSettingsEnterInvoke";
        public UIEventP0 u_EventStartGame;
        public UIEventHandleP0 u_EventStartGameHandle;
        public const string OnEventStartGameInvoke = "Main1ViewComponent.OnEventStartGameInvoke";

    }
}