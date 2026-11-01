using UnityEngine;

namespace PuzzleGame
{
    /// <summary>对独立装饰网格施加轻微风摆，保持挂点或花盆区域固定。</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class EnvironmentSwayLayer : MonoBehaviour
    {
        public enum AnchorMode { Top, Bottom, Pot }

        [SerializeField, Tooltip("固定窗帘上缘、花盆底部，或悬垂植物的花盆区域。")] private AnchorMode anchor;
        [SerializeField, Range(0, 0.1f), Tooltip("摆动幅度占该图层宽度的比例。")] private float amplitude = 0.025f;
        [SerializeField, Min(0.01f), Tooltip("每秒摆动次数。")] private float cyclesPerSecond = 0.18f;
        [SerializeField] private float phase;

        private Mesh source;
        private Mesh animated;
        private Vector3[] restVertices;
        private Vector3[] vertices;

        public void Sample(double seconds)
        {
            if (animated == null)
            {
                MeshFilter filter = GetComponent<MeshFilter>();
                source = filter.sharedMesh;
                if (source == null) return;
                restVertices = source.vertices;
                vertices = new Vector3[restVertices.Length];
                animated = Instantiate(source);
                animated.name = source.name + " (wind instance)";
                animated.hideFlags = HideFlags.DontSave;
                animated.MarkDynamic();
                filter.sharedMesh = animated;
            }

            float time = (float)(seconds % 3600) * cyclesPerSecond * Mathf.PI * 2 + phase;
            for (int i = 0; i < vertices.Length; ++i)
            {
                Vector3 vertex = restVertices[i];
                float y = vertex.y + 0.5f;
                float weight = anchor == AnchorMode.Top ? Mathf.Clamp01((0.88f - y) / 0.88f) :
                    anchor == AnchorMode.Bottom ? Mathf.Clamp01((y - 0.35f) / 0.65f) :
                    Mathf.Max(Mathf.Clamp01((y - 0.5f) / 0.5f), Mathf.Clamp01((0.25f - y) / 0.25f));
                weight *= weight;
                vertex.x += Mathf.Sin(time + y * 0.7f) * amplitude * weight;
                vertex.y += Mathf.Sin(time * 0.83f) * amplitude * weight * 0.08f;
                vertices[i] = vertex;
            }
            animated.vertices = vertices;
            animated.RecalculateBounds();
        }

        /// <summary>保存预制体或释放关卡时恢复资产网格，不保存瞬时变形副本。</summary>
        public void ReleaseMesh()
        {
            if (animated == null) return;
            GetComponent<MeshFilter>().sharedMesh = source;
            if (Application.isPlaying) Destroy(animated);
            else DestroyImmediate(animated);
            animated = null;
            restVertices = null;
            vertices = null;
        }

        private void OnDisable() => ReleaseMesh();
        private void OnDestroy() => ReleaseMesh();
    }
}
