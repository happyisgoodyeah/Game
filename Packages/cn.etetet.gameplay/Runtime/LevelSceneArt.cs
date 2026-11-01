using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// 为前三关提供环境和槽位外观。所有装饰随 Grid 销毁，不参与碰撞或占格计算。
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class LevelSceneArt : MonoBehaviour
    {
        [SerializeField] private Sprite[] backgrounds = new Sprite[3];
        [SerializeField] private Sprite[] floorTiles = new Sprite[3];
        [SerializeField] private Sprite[] obstacles = new Sprite[3];
        [SerializeField] private GameObject[] environmentLayers = new GameObject[3];

        private int theme = -1;
        private SpriteRenderer backdrop;
        private Camera sceneCamera;
        private Camera cameraOverride;
        private float previousSize;
        private Vector3 previousCameraPosition;

        public int Theme => theme;

        public bool Configure(int levelId, Camera previewCamera = null)
        {
            int index = levelId - 1001;
            if (index < 0 || index >= backgrounds.Length || index >= floorTiles.Length ||
                index >= obstacles.Length || backgrounds[index] == null || floorTiles[index] == null ||
                obstacles[index] == null)
            {
                return false;
            }

            theme = index;
            cameraOverride = previewCamera;
            Transform icon = transform.Find("Icon");
            if (icon != null) icon.gameObject.SetActive(false);
            backdrop = CreateRenderer("Environment", transform, backgrounds[index], "BackGround", 100);
            if (index < environmentLayers.Length && environmentLayers[index] != null)
            {
                GameObject layers = Instantiate(environmentLayers[index], backdrop.transform, false);
                layers.name = "EnvironmentLayers";
            }
            SpriteRenderer frame = CreateRenderer("BoardMat", transform, floorTiles[index], "Grid", -10);
            // 用同材质的大底垫连接九个地格；实际碰撞范围仍为原来的 9×9。
            frame.transform.localScale = Vector3.one * (11.3f / 3f);
            FitCamera();
            return true;
        }

        public void ApplySlot(Transform slot, bool allowPlace)
        {
            if (theme < 0) return;
            Transform square = slot.Find("Square");
            if (square == null) return;
            // 保留 Square 的原始 Sprite 和尺寸供 ET 坐标、吸附逻辑使用。
            square.GetComponent<SpriteRenderer>().enabled = false;
            Transform previous = slot.Find("SceneSurface");
            SpriteRenderer surface = previous != null ? previous.GetComponent<SpriteRenderer>() :
                CreateRenderer("SceneSurface", slot, floorTiles[theme], "Slot", 0);
            surface.sprite = allowPlace ? floorTiles[theme] : obstacles[theme];
        }

        private static SpriteRenderer CreateRenderer(string name, Transform parent, Sprite sprite, string layer, int order)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = layer;
            renderer.sortingOrder = order;
            return renderer;
        }

        private void OnEnable()
        {
            if (theme >= 0) FitCamera();
        }

        private void LateUpdate()
        {
            if (theme >= 0) FitCamera();
        }

        private void FitCamera()
        {
            if (sceneCamera == null)
            {
                Camera camera = cameraOverride != null ? cameraOverride : Camera.main;
                if (camera == null || !camera.orthographic) return;
                sceneCamera = camera;
                previousSize = camera.orthographicSize;
                previousCameraPosition = camera.transform.position;
            }

            // 两侧动物最远可达 ±19.5，留出操作边距；窄屏保持整个操作区可见。
            float aspect = Mathf.Max(0.1f, sceneCamera.aspect);
            sceneCamera.orthographicSize = Mathf.Max(12.375f, 22f / aspect);
            sceneCamera.transform.position = new Vector3(0, 0, previousCameraPosition.z);
            if (backdrop == null) return;
            Vector2 size = backdrop.sprite.bounds.size;
            float height = sceneCamera.orthographicSize * 2;
            float scale = Mathf.Max(height * aspect / size.x, height / size.y);
            backdrop.transform.localScale = Vector3.one * scale;
        }

        private void OnDisable()
        {
            if (sceneCamera == null) return;
            sceneCamera.orthographicSize = previousSize;
            sceneCamera.transform.position = previousCameraPosition;
            sceneCamera = null;
        }
    }
}
