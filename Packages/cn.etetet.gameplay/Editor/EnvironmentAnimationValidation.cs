using System;
using System.IO;
using System.Linq;
using PuzzleGame;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PuzzleGameEditor
{
    /// <summary>验证环境动画的固定点、对象复用和阵风间隔，并渲染可重复的动画帧。</summary>
    public static class EnvironmentAnimationValidation
    {
        private const string Output = "ArtOutput/LevelScenes_Animated_20261101";
        private const string Request = Output + "/validate-request.txt";

        [InitializeOnLoadMethod]
        private static void ObservePlayModeWhenRequested()
        {
            string request = Output + "/playcheck-request.txt";
            if (!File.Exists(request)) return;
            bool[] verified = new bool[3];
            LevelEnvironmentAnimation current = null;
            double started = 0;
            Vector3[] initial = null;
            Matrix4x4[] props = null;
            int currentTheme = -1;
            void Check()
            {
                if (!EditorApplication.isPlaying) return;
                LevelSceneArt art = UnityEngine.Object.FindObjectsByType<LevelSceneArt>(FindObjectsSortMode.None)
                    .FirstOrDefault(value => value.isActiveAndEnabled && value.Theme >= 0);
                if (art == null || verified[art.Theme]) return;
                LevelEnvironmentAnimation animation = art.GetComponentInChildren<LevelEnvironmentAnimation>();
                if (animation == null) return;
                if (current != animation)
                {
                    current = animation;
                    currentTheme = art.Theme;
                    started = EditorApplication.timeSinceStartup;
                    initial = currentTheme == 0 ? animation.GetComponentInChildren<MeshFilter>().sharedMesh.vertices :
                        animation.transform.Find("Weather").GetComponentsInChildren<SpriteRenderer>(true)
                            .Select(value => value.transform.localPosition).ToArray();
                    props = animation.transform.Find("PuzzleProps").GetComponentsInChildren<Transform>()
                        .Select(value => value.localToWorldMatrix).ToArray();
                    return;
                }
                if (EditorApplication.timeSinceStartup - started < 4) return;
                Vector3[] sampled = currentTheme == 0 ? animation.GetComponentInChildren<MeshFilter>().sharedMesh.vertices :
                    animation.transform.Find("Weather").GetComponentsInChildren<SpriteRenderer>(true)
                        .Select(value => value.transform.localPosition).ToArray();
                try
                {
                    Require(initial.Length == sampled.Length && initial.Where((value, i) => value != sampled[i]).Any(),
                        "Play Mode 中环境没有自动播放。");
                    Require(animation.transform.Find("PuzzleProps").GetComponentsInChildren<Transform>()
                        .Select(value => value.localToWorldMatrix).SequenceEqual(props), "Play Mode 中解谜挂点被动画移动。");
                    verified[currentTheme] = true;
                    File.WriteAllText($"{Output}/play-validation.json", "{\"automaticPlayback\":[" +
                        string.Join(",", verified.Select(value => value ? "true" : "false")) + "],\"puzzlePropsStable\":true}");
                    if (!verified.All(value => value)) return;
                    File.Delete(request);
                    EditorApplication.update -= Check;
                    Debug.Log("三关 Play Mode 环境动画自动播放检查通过。");
                }
                catch (Exception error)
                {
                    File.WriteAllText($"{Output}/play-error.txt", error.ToString());
                    EditorApplication.update -= Check;
                    Debug.LogException(error);
                }
            }
            EditorApplication.update += Check;
        }

        [InitializeOnLoadMethod]
        private static void ProcessRequest()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode) return;
                File.Delete(Request);
                try { InstallValidateAndRender(); }
                catch (Exception error)
                {
                    Directory.CreateDirectory(Output);
                    File.WriteAllText($"{Output}/error.txt", error.ToString());
                    Debug.LogException(error);
                }
            };
        }

        [MenuItem("ET/Puzzle/Validate and render environment animations")]
        public static void InstallValidateAndRender()
        {
            LevelSceneArtTools.InstallAndPreview();
            Directory.CreateDirectory(Output);
            for (int id = 1001; id <= 1003; ++id) ValidateAndRender(id);
            File.WriteAllText($"{Output}/validation.json",
                "{\"ok\":true,\"levels\":[1001,1002,1003],\"swayLayers\":4," +
                "\"pinnedRegionsStable\":true,\"puzzlePropsStable\":true,\"collidersUnchanged\":true," +
                "\"particlePools\":[24,8],\"gustIdleAndActiveVerified\":true,\"disableAndReuseVerified\":true}");
            Debug.Log("前三关分层动画验证及逐帧渲染完成。");
        }

        private static void ValidateAndRender(int id)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewPreviewScene();
            try
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    $"Packages/cn.etetet.gameplay/ArtPreviews/LevelScenes/Level{id}.prefab");
                GameObject instance = UnityEngine.Object.Instantiate(prefab);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(instance, scene);
                Camera camera = instance.GetComponentInChildren<Camera>();
                camera.scene = scene;
                camera.aspect = 16f / 9f;
                LevelEnvironmentAnimation animation = instance.GetComponentInChildren<LevelEnvironmentAnimation>();
                Require(animation != null, "预览缺少环境动画组件。");
                Transform props = animation.transform.Find("PuzzleProps");
                Matrix4x4[] anchors = props.GetComponentsInChildren<Transform>().Select(value => value.localToWorldMatrix).ToArray();
                int colliders = instance.GetComponentsInChildren<Collider2D>(true).Length;
                EnvironmentSwayLayer[] layers = animation.GetComponentsInChildren<EnvironmentSwayLayer>();
                Vector3[][] rest = layers.Select(layer => layer.GetComponent<MeshFilter>().sharedMesh.vertices).ToArray();
                animation.Sample(2);
                if (id == 1001)
                {
                    Require(layers.Length == 4, "首关必须有两个窗帘和两个盆栽的独立风摆层。");
                    for (int i = 0; i < layers.Length; ++i)
                    {
                        Vector3[] sampled = layers[i].GetComponent<MeshFilter>().sharedMesh.vertices;
                        Require(sampled.Where((vertex, j) => Vector3.Distance(vertex, rest[i][j]) > 0.00001f).Any(),
                            "风摆没有改变装饰网格。");
                        for (int j = 0; j < sampled.Length; ++j)
                        {
                            float y = rest[i][j].y + 0.5f;
                            bool pinned = i < 2 ? y >= 0.88f : i == 2 ? y <= 0.35f : y >= 0.25f && y <= 0.5f;
                            if (pinned) Require(sampled[j] == rest[i][j], "窗帘挂点或花盆固定区域发生移动。");
                        }
                    }
                }
                else if (id == 1002)
                    Require(animation.ActiveParticleCount == 24, "樱花粒子数量错误。");
                else
                {
                    animation.Sample(0);
                    Require(animation.ActiveParticleCount == 0, "阵风之前应保持安静。");
                    animation.Sample(4);
                    Require(animation.ActiveParticleCount > 0, "阵风期间没有落叶。");
                    animation.Sample(14);
                    Require(animation.ActiveParticleCount == 0, "阵风结束后应有静止间隔。");
                }

                Transform weather = animation.transform.Find("Weather");
                int poolSize = weather.childCount;
                for (int i = 0; i < 200; ++i) animation.Sample(i * 0.23);
                Require(weather.childCount == poolSize, "动画持续创建新的粒子对象。");
                Require(props.GetComponentsInChildren<Transform>().Select(value => value.localToWorldMatrix).SequenceEqual(anchors),
                    "环境动画移动了解谜物件挂点。");
                Require(instance.GetComponentsInChildren<Collider2D>(true).Length == colliders &&
                    weather.GetComponentsInChildren<Collider2D>(true).Length == 0, "动画增加了碰撞体。");
                instance.SetActive(false);
                Require(weather.GetComponentsInChildren<SpriteRenderer>(true).All(value => !value.enabled || !value.gameObject.activeInHierarchy),
                    "关卡隐藏后粒子仍在显示。");
                instance.SetActive(true);
                Require(weather.childCount == poolSize, "重新启用关卡时重复创建粒子池。");

                string frames = $"{Output}/level-{id}-frames";
                Directory.CreateDirectory(frames);
                int frameCount = id == 1003 ? 96 : 48;
                for (int frame = 0; frame < frameCount; ++frame)
                {
                    animation.Sample(frame / 6.0);
                    Render(camera, $"{frames}/{frame:000}.png");
                }
                animation.Sample(id == 1003 ? 4 : 2);
                Render(camera, $"{Output}/level-{id}.png");
                animation.ClearPreview();
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        private static void Render(Camera camera, string path)
        {
            const int width = 960;
            const int height = 540;
            RenderTexture target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
