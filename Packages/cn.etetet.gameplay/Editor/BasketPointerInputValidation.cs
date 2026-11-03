using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PuzzleGame;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PuzzleGameEditor
{
    /// <summary>复现全屏 UIBlock 拦截，并验证实际指针路径、按钮遮挡和三次点击揭晓。</summary>
    public static class BasketPointerInputValidation
    {
        private const string Package = "Packages/cn.etetet.gameplay";
        private const string Output = "ArtOutput/PlacementReveal_20261102/basket-input-validation.json";

        [MenuItem("ET/Puzzle/Validate basket pointer input")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("请在非 Play Mode 下运行独立输入验证。");
            Scene preview = EditorSceneManager.NewPreviewScene();
            EventSystem previousEventSystem = EventSystem.current;
            float previousTimeScale = Time.timeScale;
            EventSystem events = null;
            GraphicRaycaster uiRaycaster = null;
            try
            {
                Time.timeScale = 1;
                GameObject container = new GameObject("BasketInputValidation");
                SceneManager.MoveGameObjectToScene(container, preview);
                Camera camera = new GameObject("Camera").AddComponent<Camera>();
                camera.transform.SetParent(container.transform, false);
                camera.transform.position = new Vector3(0, 0, -10);
                camera.orthographic = true;
                camera.scene = preview;
                GameObject grid = UnityEngine.Object.Instantiate(
                    AssetDatabase.LoadAssetAtPath<GameObject>($"{Package}/Resources/Grid/Grid1001.prefab"), container.transform);
                Require(grid.GetComponent<LevelSceneArt>().Configure(1001, camera), "首关外观配置失败。");
                LevelOneBasketReveal reveal = grid.GetComponent<LevelOneBasketReveal>();
                GameObject chick = UnityEngine.Object.Instantiate(
                    AssetDatabase.LoadAssetAtPath<GameObject>($"{Package}/Resources/Puzzle/Puzzle1001.prefab"), grid.transform);
                chick.transform.position = new Vector3(15, -5);
                reveal.RegisterChick(chick);
                SpriteRenderer basket = grid.transform.Find("Environment/EnvironmentLayers/PuzzleProps/RevealBasket")
                    .GetComponent<SpriteRenderer>();
                Vector2 point = camera.WorldToScreenPoint(basket.bounds.center);
                Require(point.x > 0 && point.x < Screen.width && point.y > 0 && point.y < Screen.height,
                    "篮筐测试点不在相机屏幕内。");

                events = new GameObject("EventSystem").AddComponent<EventSystem>();
                events.transform.SetParent(container.transform, false);
                // 预览场景不运行普通 MonoBehaviour 生命周期，显式注册真实的事件系统和射线器。
                if (EventSystem.current != events) events.SendMessage("OnEnable", SendMessageOptions.RequireReceiver);
                EventSystem.current = events;
                GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
                canvasObject.transform.SetParent(container.transform, false);
                uiRaycaster = canvasObject.GetComponent<GraphicRaycaster>();
                if (!RaycasterManager.GetRaycasters().Contains(uiRaycaster))
                    uiRaycaster.SendMessage("OnEnable", SendMessageOptions.RequireReceiver);
                Canvas canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1;
                Type blockType = AppDomain.CurrentDomain.GetAssemblies().Select(value => value.GetType("YIUIFramework.UIBlock"))
                    .FirstOrDefault(value => value != null);
                Require(blockType != null, "缺少真实 YIUI UIBlock 类型，不能验证拦截问题。");
                GameObject block = new GameObject("UIBlockBG", typeof(RectTransform), typeof(CanvasRenderer), blockType,
                    typeof(PuzzleInputPassthrough));
                Stretch(block, canvasObject.transform);
                PuzzleInputPassthrough passthrough = block.GetComponent<PuzzleInputPassthrough>();
                Canvas.ForceUpdateCanvases();
                camera.Render();
                Canvas.ForceUpdateCanvases();
                List<RaycastResult> hits = new List<RaycastResult>();
                events.RaycastAll(new PointerEventData(events) { position = point }, hits);
                Require(hits.Any(value => value.gameObject == block),
                    $"没有真实命中全屏 UIBlock，测试环境无效。depth={block.GetComponent<Graphic>().depth}, " +
                    $"cull={block.GetComponent<CanvasRenderer>().cull}, rect={block.GetComponent<RectTransform>().rect}, point={point}");

                void Click(Vector2 position, int pointer = -1)
                {
                    reveal.PointerDown(position, pointer);
                    reveal.PointerUp(position, pointer);
                }

                // 先复现旧问题：没有透传标记时，完整点击仍被全屏 UIBlock 拦截。
                passthrough.enabled = false;
                Click(point);
                Require(reveal.ClickCount == 0, "没有复现基础 UIBlock 对篮筐点击的拦截。");
                passthrough.enabled = true;
                Click(new Vector2(-100, -100));
                reveal.PointerDown(point, -1);
                reveal.PointerUp(point + Vector2.right * 40, -1);
                Require(reveal.ClickCount == 0, "点空白处或拖动仍被计为篮筐点击。");

                GameObject button = new GameObject("OverlayButton", typeof(RectTransform), typeof(Image), typeof(Button));
                Stretch(button, canvasObject.transform);
                Canvas.ForceUpdateCanvases();
                camera.Render();
                Click(point);
                Require(reveal.ClickCount == 0, "按钮没有阻挡篮筐点击。");
                UnityEngine.Object.DestroyImmediate(button);
                GameObject modal = new GameObject("ModalBlock", typeof(RectTransform), typeof(CanvasRenderer), blockType);
                Stretch(modal, canvasObject.transform);
                Canvas.ForceUpdateCanvases();
                camera.Render();
                Click(point);
                Require(reveal.ClickCount == 0, "未标记的弹窗背景被错误透传。");
                UnityEngine.Object.DestroyImmediate(modal);
                Canvas.ForceUpdateCanvases();
                Time.timeScale = 0;
                Click(point);
                Require(reveal.ClickCount == 0, "暂停时仍接受篮筐点击。");
                Time.timeScale = 1;
                Click(point);
                Click(point);
                Require(reveal.ClickCount == 2 && !chick.activeSelf, "鼠标前两次点击未正确计数或提前显示 Chick。");
                // 第三次使用触摸指针 ID，走相同的屏幕命中与 UI 遮挡路径。
                Click(point, 0);
                Require(reveal.ClickCount == 3 && reveal.IsRevealing, "第三次指针点击未触发揭晓。");
                reveal.Advance(1.3f);
                Require(reveal.IsRevealed && chick.activeSelf, "指针点击后的揭晓动画未完成。");
                Click(point);
                Require(reveal.ClickCount == 3, "揭晓后仍重复计数。");

                GameObject panel = AssetDatabase.LoadAssetAtPath<GameObject>(
                    $"{Package}/Assets/GameRes/YIUI/GameMainPanel/Prefabs/GameMainPanelPanel.prefab");
                PuzzleInputPassthrough[] markers = panel.GetComponentsInChildren<PuzzleInputPassthrough>(true);
                Require(markers.Length == 1 && markers[0].name == "UIBlockBG", "正式主面板未正确配置透传标记。");
                Directory.CreateDirectory(Path.GetDirectoryName(Output));
                File.WriteAllText(Output, "{\"ok\":true,\"oldBlockerReproduced\":true,\"realYIUIBlockRaycast\":true," +
                    "\"screenPointerPath\":true,\"mouseAndTouchPointerIds\":true,\"buttonAndModalStillBlock\":true," +
                    "\"outsideAndDragRejected\":true,\"pausedInputRejected\":true,\"thirdClickRevealsChick\":true," +
                    "\"productionPanelConfigured\":true,\"hardwareMouseInjected\":false}");
                Debug.Log("篮筐完整指针输入验证通过：已复现旧拦截，并验证按钮和弹窗仍阻挡。");
            }
            finally
            {
                if (uiRaycaster != null) uiRaycaster.SendMessage("OnDisable", SendMessageOptions.RequireReceiver);
                if (events != null) events.SendMessage("OnDisable", SendMessageOptions.RequireReceiver);
                if (previousEventSystem != null) EventSystem.current = previousEventSystem;
                Time.timeScale = previousTimeScale;
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        private static void Stretch(GameObject gameObject, Transform parent)
        {
            RectTransform rect = (RectTransform)gameObject.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
