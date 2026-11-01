using System;
using System.IO;
using System.Linq;
using PuzzleGame;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PuzzleGameEditor
{
    /// <summary>导入前三关环境资源，安装 Grid 引用并按真实配置生成独立美术预览。</summary>
    public static class LevelSceneArtTools
    {
        private const string Package = "Packages/cn.etetet.gameplay";
        private const string Art = Package + "/Resources/LevelScenes";
        private const string Output = "ArtOutput/LevelScenes_20261101";
        private const string PreviewDirectory = Package + "/ArtPreviews/LevelScenes";
        private const string Request = Output + "/install-request.txt";
        private static readonly string[] Names = { "room", "courtyard", "glasshouse" };

        [MenuItem("ET/Puzzle/Install first three level environments")]
        public static void Install()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            GameObject[] environmentLayers = LevelEnvironmentArtTools.Install();
            foreach (string name in Names)
            {
                string path = $"{Art}/{name}.png";
                TextureImporter importer = ConfigureImporter(path);
                importer.GetSourceTextureWidthAndHeight(out int width, out _);
                importer.spritePixelsPerUnit = width / 44f;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }

            TextureImporter atlas = ConfigureImporter($"{Art}/tiles.png");
            atlas.spriteImportMode = SpriteImportMode.Multiple;
            atlas.spritePixelsPerUnit = 512f / 3f;
            SpriteMetaData[] sprites = new SpriteMetaData[6];
            for (int row = 0; row < 2; ++row)
            for (int column = 0; column < 3; ++column)
            {
                sprites[row * 3 + column] = new SpriteMetaData
                {
                    name = Names[column] + (row == 0 ? "-floor" : "-obstacle"),
                    rect = new Rect(column * 512, (1 - row) * 512, 512, 512),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                };
            }
#pragma warning disable CS0618 // Unity 6 仍支持 TextureImporter 的标准多 Sprite 导入接口。
            atlas.spritesheet = sprites;
#pragma warning restore CS0618
            atlas.SaveAndReimport();
            Sprite[] tiles = AssetDatabase.LoadAllAssetsAtPath($"{Art}/tiles.png").OfType<Sprite>().ToArray();
            Require(tiles.Length == 6, "地格图集应包含六个 Sprite。");

            string gridPath = $"{Package}/Resources/Grid/Grid1001.prefab";
            GameObject grid = PrefabUtility.LoadPrefabContents(gridPath);
            try
            {
                LevelSceneArt art = grid.GetComponent<LevelSceneArt>() ?? grid.AddComponent<LevelSceneArt>();
                SerializedObject serialized = new SerializedObject(art);
                for (int i = 0; i < Names.Length; ++i)
                {
                    SetSprite(serialized, "backgrounds", i, AssetDatabase.LoadAssetAtPath<Sprite>($"{Art}/{Names[i]}.png"));
                    SetSprite(serialized, "floorTiles", i, tiles.Single(sprite => sprite.name == Names[i] + "-floor"));
                    SetSprite(serialized, "obstacles", i, tiles.Single(sprite => sprite.name == Names[i] + "-obstacle"));
                    SerializedProperty layers = serialized.FindProperty("environmentLayers");
                    layers.arraySize = 3;
                    layers.GetArrayElementAtIndex(i).objectReferenceValue = environmentLayers[i];
                }
                SetSprite(serialized, "backgrounds", 0, AssetDatabase.LoadAssetAtPath<Sprite>($"{Art}/Animated/room-static.png"));
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(grid, gridPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(grid);
            }
            AssetDatabase.SaveAssets();
        }

        private static TextureImporter ConfigureImporter(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Require(importer != null, $"缺少图片：{path}");
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.npotScale = TextureImporterNPOTScale.None;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            return importer;
        }

        private static void SetSprite(SerializedObject serialized, string field, int index, Sprite sprite)
        {
            Require(sprite != null, $"Sprite 引用为空：{field}[{index}]");
            SerializedProperty array = serialized.FindProperty(field);
            array.arraySize = 3;
            array.GetArrayElementAtIndex(index).objectReferenceValue = sprite;
        }

        [MenuItem("ET/Puzzle/Render first three level environments")]
        public static void InstallAndPreview()
        {
            Install();
            Directory.CreateDirectory(Output);
            Directory.CreateDirectory(PreviewDirectory);
            string json = File.ReadAllText("Packages/cn.etetet.yiuilubangen/Assets/Config/Json/ClientServer/GridConfigCategory.json");
            GridList levels = JsonUtility.FromJson<GridList>("{\"Items\":" + json + "}");
            foreach (GridRow level in levels.Items.Where(level => level.Id >= 1001 && level.Id <= 1003))
                CreatePreview(level);
            File.WriteAllText($"{Output}/unity-validation.json",
                "{\"ok\":true,\"levels\":[1001,1002,1003],\"slotsEach\":9,\"obstacles\":[1,4,4]," +
                "\"spriteCellSize\":3,\"collidersUnchanged\":true,\"cameraRestoredOnDisable\":true," +
                "\"renderSizes\":[\"1920x1080\",\"1600x1200\",\"1080x1920\"],\"gameplayTested\":false}");
            AssetDatabase.Refresh();
            Debug.Log("前三关环境已安装。预览、三种画幅和碰撞检查完成。");
        }

        [InitializeOnLoadMethod]
        private static void ProcessInstallRequest()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(Request) || EditorApplication.isPlayingOrWillChangePlaymode) return;
                File.Delete(Request);
                try { InstallAndPreview(); }
                catch (Exception error)
                {
                    Directory.CreateDirectory(Output);
                    File.WriteAllText($"{Output}/install-error.txt", error.ToString());
                    Debug.LogException(error);
                }
            };
        }

        private static void CreatePreview(GridRow level)
        {
            UnityEngine.SceneManagement.Scene preview = EditorSceneManager.NewPreviewScene();
            try
            {
                GameObject container = new GameObject("ArtPreview_" + level.Id);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(container, preview);
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.transform.SetParent(container.transform, false);
                cameraObject.tag = "MainCamera";
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.scene = preview;
                camera.orthographic = true;
                camera.orthographicSize = 10;
                camera.aspect = 16f / 9f;
                camera.transform.position = new Vector3(0, 1, -10);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.96f, 0.93f, 0.87f);

                GameObject grid = UnityEngine.Object.Instantiate(
                    AssetDatabase.LoadAssetAtPath<GameObject>($"{Package}/Resources/Grid/Grid1001.prefab"), container.transform);
                grid.name = $"Level{level.Id}_{Names[level.Id - 1001]}_ArtPreview";
                LevelSceneArt art = grid.GetComponent<LevelSceneArt>();
                Require(art != null && art.Configure(level.Id, camera), "环境配置失败。");
                Transform slotParent = grid.transform.Find("SlotTransform");
                // 预览直接采用真实的世界坐标，避免编辑器布局脚本覆盖配置坐标。
                foreach (MonoBehaviour script in slotParent.GetComponents<MonoBehaviour>())
                    UnityEngine.Object.DestroyImmediate(script);
                int blocked = 0;
                for (int y = 0; y < level.Y; ++y)
                for (int x = 0; x < level.X; ++x)
                {
                    int slotId = level.SlotList[x + y * level.X];
                    GameObject slot = UnityEngine.Object.Instantiate(
                        AssetDatabase.LoadAssetAtPath<GameObject>($"{Package}/Resources/Slot/Slot{slotId}.prefab"), slotParent);
                    slot.name = $"Cell_{x}_{y}";
                    slot.transform.position = new Vector3((x - level.X / 2) * 3, (level.Y / 2 - y) * 3, 0);
                    BoxCollider2D collider = slot.GetComponent<BoxCollider2D>();
                    string before = EditorJsonUtility.ToJson(collider);
                    art.ApplySlot(slot.transform, slotId == 1001);
                    Require(before == EditorJsonUtility.ToJson(collider), "外观改变了原有碰撞数据。");
                    SpriteRenderer surface = slot.transform.Find("SceneSurface").GetComponent<SpriteRenderer>();
                    Require(Vector2.Distance(surface.sprite.bounds.size, Vector2.one * 3) < 0.001f, "地格尺寸必须为 3×3。");
                    Require(!slot.transform.Find("Square").GetComponent<SpriteRenderer>().enabled, "原调试色块未隐藏。");
                    if (slotId == 1002) ++blocked;
                }
                Require(blocked == (level.Id == 1001 ? 1 : 4), "障碍格数量与现有配置不一致。");
                Require(grid.GetComponentsInChildren<Collider2D>().Length == 10, "环境装饰不应增加碰撞体。");
                foreach (PuzzleRow piece in level.PuzzleList)
                {
                    GameObject puzzle = UnityEngine.Object.Instantiate(
                        AssetDatabase.LoadAssetAtPath<GameObject>($"{Package}/Resources/Puzzle/Puzzle{piece.Id}.prefab"),
                        grid.transform.Find("PuzzleTransform"));
                    puzzle.transform.position = piece.Trans;
                }

                Render(camera, $"{Output}/level-{level.Id}.png", 1920, 1080);
                camera.aspect = 4f / 3f;
                grid.SetActive(false);
                Require(Mathf.Abs(camera.orthographicSize - 10) < 0.001f && camera.transform.position == new Vector3(0, 1, -10),
                    "退出关卡未还原相机。");
                grid.SetActive(true);
                Render(camera, $"{Output}/level-{level.Id}-4x3.png", 1600, 1200);
                camera.aspect = 9f / 16f;
                grid.SetActive(false);
                grid.SetActive(true);
                Render(camera, $"{Output}/level-{level.Id}-portrait.png", 1080, 1920);
                camera.aspect = 16f / 9f;
                grid.SetActive(false);
                grid.SetActive(true);
                // 保存为纯美术预览预制体，不启动 ET 业务，也不切换用户当前场景。
                art.enabled = false;
                foreach (LevelEnvironmentAnimation animation in grid.GetComponentsInChildren<LevelEnvironmentAnimation>(true))
                    animation.ClearPreview();
                camera.transform.position = new Vector3(0, 0, -10);
                camera.orthographicSize = 12.375f;
                PrefabUtility.SaveAsPrefabAsset(container, $"{PreviewDirectory}/Level{level.Id}.prefab");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        private static void Render(Camera camera, string path, int width, int height)
        {
            RenderTexture target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                UnityEngine.Object.DestroyImmediate(image);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        [Serializable] private sealed class GridList { public GridRow[] Items; }
        [Serializable] private sealed class GridRow
        {
            public int Id;
            public int X;
            public int Y;
            public int[] SlotList;
            public PuzzleRow[] PuzzleList;
        }
        [Serializable] private sealed class PuzzleRow
        {
            public int Id;
            public Vector3 Trans;
        }
    }
}
