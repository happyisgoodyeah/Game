using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGame
{
    /// <summary>首关篮筐三次点击解谜。仅管理视图显隐和动画，Chick 数据始终由 ET 管理。</summary>
    [DisallowMultipleComponent]
    public sealed class LevelOneBasketReveal : MonoBehaviour
    {
        private SpriteRenderer basket;
        private Camera inputCamera;
        private GameObject chick;
        private Collider2D[] chickColliders;
        private bool[] colliderStates;
        private Vector3 basketRest;
        private Vector3 basketScale;
        private Vector3 chickRest;
        private Vector3 chickScale;
        private Vector3 emergence;
        private float elapsed;
        private float shakeTime = 1;
        private bool smokePlayed;
        private bool chickShown;
        private Vector2? pressPosition;
        private readonly List<RaycastResult> uiHits = new List<RaycastResult>();

        public int ClickCount { get; private set; }
        public bool IsRevealed { get; private set; }
        public bool IsRevealing => ClickCount == 3 && !IsRevealed;

        public void Configure(SpriteRenderer basketRenderer, Camera cameraOverride = null)
        {
            basket = basketRenderer;
            inputCamera = cameraOverride;
            if (basket == null) return;
            basketRest = basket.transform.localPosition;
            basketScale = basket.transform.localScale;
        }

        public void RegisterChick(GameObject puzzle)
        {
            if (basket == null || chick != null) return;
            chick = puzzle;
            chickRest = puzzle.transform.position;
            chickScale = puzzle.transform.localScale;
            chick.SetActive(false);
        }

        /// <summary>与真实输入共用的点击入口，第三次开始后拒绝重复触发。</summary>
        public bool TryClick()
        {
            if (!isActiveAndEnabled || basket == null || chick == null || ClickCount >= 3) return false;
            ++ClickCount;
            shakeTime = 0;
            if (ClickCount == 3) elapsed = 0;
            return true;
        }

        private void Update()
        {
            Advance(Time.deltaTime);
            if (basket == null || Time.timeScale == 0 || ClickCount >= 3) return;
            // 必须在篮筐内按下并松开；拖着动物经过篮筐不会记为点击。
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) PointerDown(touch.position, touch.fingerId);
                if (touch.phase == TouchPhase.Ended) PointerUp(touch.position, touch.fingerId);
                if (touch.phase == TouchPhase.Canceled) pressPosition = null;
            }
            else
            {
                if (Input.GetMouseButtonDown(0)) PointerDown(Input.mousePosition, -1);
                if (Input.GetMouseButtonUp(0)) PointerUp(Input.mousePosition, -1);
            }
        }

        private bool IsOverBlockingUI(Vector2 position, int pointer)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null) return false;
            // 显式按当前坐标射线检测，不依赖输入模块上一帧缓存的悬停对象。
            PointerEventData data = new PointerEventData(eventSystem) { position = position, pointerId = pointer };
            uiHits.Clear();
            eventSystem.RaycastAll(data, uiHits);
            foreach (RaycastResult hit in uiHits)
            {
                if (!(hit.module is GraphicRaycaster) || hit.gameObject == null) continue;
                PuzzleInputPassthrough passthrough = hit.gameObject.GetComponent<PuzzleInputPassthrough>();
                if (passthrough != null && passthrough.isActiveAndEnabled) continue;
                return true;
            }
            return false;
        }

        /// <summary>鼠标和触屏共用的按下入口，验证时也经过实际 UI 射线和篮筐命中判定。</summary>
        public void PointerDown(Vector2 position, int pointer)
        {
            pressPosition = isActiveAndEnabled && Time.timeScale > 0 && !IsOverBlockingUI(position, pointer) &&
                HitBasket(position) ? position : (Vector2?)null;
        }

        /// <summary>只有完整的篮筐点击才计数，拖出篮筐、UI 遮挡和暂停期间均取消。</summary>
        public void PointerUp(Vector2 position, int pointer)
        {
            bool click = isActiveAndEnabled && Time.timeScale > 0 && pressPosition.HasValue &&
                Vector2.Distance(pressPosition.Value, position) <= 24 && !IsOverBlockingUI(position, pointer) && HitBasket(position);
            pressPosition = null;
            if (click) TryClick();
        }

        private bool HitBasket(Vector2 screenPosition)
        {
            Camera camera = inputCamera != null ? inputCamera : Camera.main;
            if (camera == null || basket == null || !basket.enabled) return false;
            Ray ray = camera.ScreenPointToRay(screenPosition);
            Plane plane = new Plane(Vector3.forward, basket.transform.position);
            if (!plane.Raycast(ray, out float distance)) return false;
            Vector3 point = ray.GetPoint(distance);
            Bounds bounds = basket.bounds;
            return point.x >= bounds.min.x && point.x <= bounds.max.x &&
                point.y >= bounds.min.y && point.y <= bounds.max.y;
        }

        /// <summary>第三次点击：0.45 秒拉出，烟雾遮挡，Chick 从篮筐飞到待放区后恢复碰撞。</summary>
        public void Advance(float seconds)
        {
            if (basket == null) return;
            seconds = Mathf.Max(0, seconds);
            shakeTime += seconds;
            if (ClickCount < 3)
            {
                float shake = shakeTime < 0.28f ? Mathf.Sin(shakeTime * 65) * (1 - shakeTime / 0.28f) : 0;
                basket.transform.localRotation = Quaternion.Euler(0, 0, shake * 4);
                return;
            }
            if (IsRevealed) return;
            elapsed += seconds;
            float pull = Mathf.SmoothStep(0, 1, elapsed / 0.45f);
            basket.transform.localPosition = basketRest + new Vector3(-0.25f, -1.8f, 0) * pull;
            basket.transform.localScale = basketScale * Mathf.Lerp(1, 1.08f, pull);
            basket.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(pull * Mathf.PI) * -5);
            if (elapsed >= 0.45f && !smokePlayed)
            {
                smokePlayed = true;
                emergence = basket.bounds.center + Vector3.up * 0.6f;
                GetComponent<PlacementFeedback>()?.PlayReveal(emergence);
            }
            if (elapsed < 0.65f || chick == null) return;
            if (!chickShown)
            {
                chickShown = true;
                // Puzzle 的槽位视图异步生成，揭晓时再收集，包含后加载的子碰撞体。
                chickColliders = chick.GetComponentsInChildren<Collider2D>(true);
                colliderStates = new bool[chickColliders.Length];
                for (int i = 0; i < chickColliders.Length; ++i) colliderStates[i] = chickColliders[i].enabled;
                for (int i = 0; i < chickColliders.Length; ++i) chickColliders[i].enabled = false;
                chick.SetActive(true);
            }
            float travel = Mathf.SmoothStep(0, 1, (elapsed - 0.65f) / 0.55f);
            chick.transform.position = Vector3.Lerp(emergence, chickRest, travel) + Vector3.up * Mathf.Sin(travel * Mathf.PI) * 1.5f;
            chick.transform.localScale = chickScale * Mathf.Lerp(0.3f, 1, Mathf.Clamp01(travel * 2));
            if (elapsed < 1.2f) return;
            chick.transform.position = chickRest;
            chick.transform.localScale = chickScale;
            for (int i = 0; i < chickColliders.Length; ++i) chickColliders[i].enabled = colliderStates[i];
            IsRevealed = true;
        }

        private void OnDisable() => pressPosition = null;
    }
}
