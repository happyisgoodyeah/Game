using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PuzzleGame;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PuzzleGameEditor
{
    /// <summary>
    /// 将已导出的动物待机和拎起帧接入拼图，并验证资源、占格和循环播放。
    /// </summary>
    public static class PuzzleAnimalArtTools
    {
        private const string Package = "Packages/cn.etetet.gameplay";
        private const string ReportDirectory = "ArtOutput/PuzzleSketch_20261106/qa";
        private const string PlayCheckKey = "PuzzleGame.AnimalAnimationPlayCheck";
        private const string InstallRequest = "ArtOutput/PuzzleC_Loop8_20261031/install-request.txt";

        private static readonly int[] Ids = { 1001, 1002, 1003, 1004 };
        private static readonly string[] Animals = { "chick", "dog-long", "dog-l", "cat" };
        private static readonly Vector2[] Pivots =
        {
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.25f, 0.25f), new Vector2(0.25f, 0.5f)
        };

        [MenuItem("ET/Puzzle/Install C animal animations")]
        public static void Install()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            for (int animal = 0; animal < Ids.Length; ++animal)
            {
                Sprite[] frames = new Sprite[8];
                Sprite[] pickedUpFrames = new Sprite[8];
                for (int index = 0; index < frames.Length; ++index)
                {
                    string path = $"{Package}/Resources/Sprite/PuzzleAnimals/{Animals[animal]}/{index:00}.png";
                    ConfigureSprite(path, Pivots[animal]);
                    frames[index] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    Require(frames[index] != null, $"缺少动画帧：{path}");
                    string pickedUpPath = $"{Package}/Resources/Sprite/PuzzleAnimals/{Animals[animal]}/PickedUp/{index:00}.png";
                    ConfigureSprite(pickedUpPath, Pivots[animal]);
                    pickedUpFrames[index] = AssetDatabase.LoadAssetAtPath<Sprite>(pickedUpPath);
                    Require(pickedUpFrames[index] != null, $"缺少拎起动画帧：{pickedUpPath}");
                }

                if (animal < 2)
                {
                    ConfigureSprite($"{Package}/Resources/Sprite/puzzle{animal + 1}.png", Pivots[animal]);
                }

                string destination = $"{Package}/Resources/Puzzle/Puzzle{Ids[animal]}.prefab";
                GameObject root = PrefabUtility.LoadPrefabContents(destination);
                try
                {
                    root.name = $"Puzzle{Ids[animal]}";
                    root.transform.localPosition = Vector3.zero;
                    SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
                    renderer.drawMode = SpriteDrawMode.Simple;
                    renderer.sprite = frames[0];
                    renderer.color = Color.white;

                    PuzzleIdleAnimation animation = root.GetComponent<PuzzleIdleAnimation>();
                    if (animation == null) animation = root.AddComponent<PuzzleIdleAnimation>();
                    SerializedObject serialized = new SerializedObject(animation);
                    SerializedProperty property = serialized.FindProperty("frames");
                    property.arraySize = frames.Length;
                    for (int i = 0; i < frames.Length; ++i)
                        property.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
                    serialized.FindProperty("framesPerSecond").floatValue = 5f;
                    SerializedProperty pickedUp = serialized.FindProperty("pickedUpFrames");
                    pickedUp.arraySize = pickedUpFrames.Length;
                    for (int i = 0; i < pickedUpFrames.Length; ++i)
                        pickedUp.GetArrayElementAtIndex(i).objectReferenceValue = pickedUpFrames[i];
                    serialized.FindProperty("pickedUpFramesPerSecond").floatValue = 8f;
                    serialized.FindProperty("heldLoopStart").intValue = 2;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    animation.enabled = true;

                    // 美术更新保留现有固定碰撞区域和子槽位，不重建玩法层级。
                    PrefabUtility.SaveAsPrefabAsset(root, destination);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            AssetDatabase.SaveAssets();
            ValidateAssets();
            Debug.Log("四种拼图的待机和拎起动画已接入。");
        }

        private static void ConfigureSprite(string path, Vector2 pivot)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Require(importer != null, $"无法导入图片：{path}");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            importer.spritePixelsPerUnit = 256f / 3f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        private static void ConfigureCollider(PolygonCollider2D collider, int animal)
        {
            Require(collider != null, "拼图缺少 PolygonCollider2D。");
            collider.autoTiling = false;
            collider.isTrigger = true;
            collider.pathCount = 1;
            Vector2[] points;
            if (animal == 2)
            {
                points = new[] { new Vector2(-1.5f, -1.5f), new Vector2(4.5f, -1.5f),
                    new Vector2(4.5f, 1.5f), new Vector2(1.5f, 1.5f),
                    new Vector2(1.5f, 4.5f), new Vector2(-1.5f, 4.5f) };
            }
            else
            {
                float left = animal == 1 ? -4.5f : -1.5f;
                float right = animal == 0 ? 1.5f : 4.5f;
                points = new[] { new Vector2(left, -1.5f), new Vector2(right, -1.5f),
                    new Vector2(right, 1.5f), new Vector2(left, 1.5f) };
            }
            collider.SetPath(0, points);
        }

        private static void ConfigureSlots(GameObject root, int animal)
        {
            while (root.transform.childCount > 0)
                UnityEngine.Object.DestroyImmediate(root.transform.GetChild(0).gameObject);
            GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>($"{Package}/Resources/Slot/Slot1000.prefab");
            Vector3[] offsets = animal switch
            {
                0 => new[] { Vector3.zero },
                1 => new[] { Vector3.zero, Vector3.right * 3, Vector3.left * 3 },
                2 => new[] { Vector3.zero, Vector3.up * 3, Vector3.right * 3 },
                _ => new[] { Vector3.zero, Vector3.right * 3 }
            };
            for (int i = 0; i < offsets.Length; ++i)
            {
                GameObject slot = (GameObject)PrefabUtility.InstantiatePrefab(template, root.transform);
                slot.transform.localPosition = offsets[i];
                slot.name = $"Slot1000_{i}";
                slot.transform.Find("Square").GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        [MenuItem("ET/Puzzle/Validate C animal animations")]
        public static void ValidateAssets()
        {
            foreach (int id in Ids)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Package}/Resources/Puzzle/Puzzle{id}.prefab");
                Require(prefab != null, $"缺少 Puzzle{id}。");
                PuzzleIdleAnimation animation = prefab.GetComponent<PuzzleIdleAnimation>();
                Require(animation != null && animation.enabled && animation.FrameCount == 8, $"Puzzle{id} 动画配置错误。");
                Require(animation.FramesPerSecond == 5f, $"Puzzle{id} 帧率错误。");
                Require(animation.PickedUpFrameCount == 8, $"Puzzle{id} 缺少八帧拎起动画。");
                PolygonCollider2D collider = prefab.GetComponent<PolygonCollider2D>();
                Require(collider != null && !collider.autoTiling && collider.isTrigger, $"Puzzle{id} 碰撞配置错误。");
                SerializedProperty frames = new SerializedObject(animation).FindProperty("frames");
                Sprite first = frames.GetArrayElementAtIndex(0).objectReferenceValue as Sprite;
                for (int i = 0; i < frames.arraySize; ++i)
                {
                    Sprite sprite = frames.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
                    Require(sprite != null && sprite.rect.size == first.rect.size && sprite.pivot == first.pivot,
                        $"Puzzle{id} 第 {i} 帧尺寸或枢轴不一致。");
                }
                SerializedProperty pickedUp = new SerializedObject(animation).FindProperty("pickedUpFrames");
                for (int i = 0; i < pickedUp.arraySize; ++i)
                {
                    Sprite sprite = pickedUp.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
                    Require(sprite != null && sprite.rect.size == first.rect.size && sprite.pivot == first.pivot &&
                        Mathf.Approximately(sprite.pixelsPerUnit, first.pixelsPerUnit),
                        $"Puzzle{id} 第 {i} 帧拎起动画尺寸、枢轴或 PPU 不一致。");
                }
            }
            Directory.CreateDirectory(ReportDirectory);
            File.WriteAllText($"{ReportDirectory}/unity-assets-validation.json", "{\"ok\":true,\"prefabs\":4,\"idleFramesPerPrefab\":8,\"pickedUpFramesPerPrefab\":8}");
        }

        /// <summary>批处理入口：安装后进入隔离场景，检查默认播放及暂停时的循环。</summary>
        public static void InstallAndPlayCheck()
        {
            Install();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(PlayCheckKey, true);
            EditorApplication.isPlaying = true;
        }

        [InitializeOnLoadMethod]
        private static void ResumePlayCheck()
        {
            EditorApplication.delayCall += () =>
            {
                if (File.Exists(InstallRequest) && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    try
                    {
                        Install();
                        File.Delete(InstallRequest);
                    }
                    catch (Exception error)
                    {
                        Debug.LogException(error);
                    }
                }
                if (!SessionState.GetBool(PlayCheckKey, false) || !EditorApplication.isPlaying) return;
                RunPlayCheck();
            };
        }

        private static void RunPlayCheck()
        {
            List<GameObject> instances = new List<GameObject>();
            List<HashSet<Sprite>> seen = new List<HashSet<Sprite>>();
            List<Vector2[]> paths = new List<Vector2[]>();
            foreach (int id in Ids)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Package}/Resources/Puzzle/Puzzle{id}.prefab");
                GameObject instance = UnityEngine.Object.Instantiate(prefab);
                // 动画不能覆盖用户操作产生的旋转和缩放。
                instance.transform.rotation = Quaternion.Euler(0, 0, 90);
                instance.transform.localScale = Vector3.one * 1.25f;
                instances.Add(instance);
                seen.Add(new HashSet<Sprite>());
                paths.Add(instance.GetComponent<PolygonCollider2D>().GetPath(0));
            }
            double started = EditorApplication.timeSinceStartup;
            Time.timeScale = 0;
            void Check()
            {
                try
                {
                    for (int i = 0; i < instances.Count; ++i)
                    {
                        GameObject instance = instances[i];
                        seen[i].Add(instance.GetComponent<SpriteRenderer>().sprite);
                        Require(Mathf.Abs(Mathf.DeltaAngle(instance.transform.eulerAngles.z, 90)) < 0.01f,
                            "动画改变了拼图旋转。");
                        Require(instance.transform.localScale == Vector3.one * 1.25f, "动画改变了拼图缩放。");
                        Require(paths[i].SequenceEqual(instance.GetComponent<PolygonCollider2D>().GetPath(0)),
                            "动画改变了碰撞区域。");
                    }
                    if (EditorApplication.timeSinceStartup - started < 4) return;
                    Require(seen.All(sprites => sprites.Count == 8), "默认播放未覆盖全部八帧。");
                    File.WriteAllText($"{ReportDirectory}/unity-play-validation.json",
                        "{\"ok\":true,\"prefabs\":4,\"observedFramesEach\":8,\"timeScale\":0,\"transformAndColliderStable\":true}");
                    Finish(0);
                }
                catch (Exception error)
                {
                    Debug.LogException(error);
                    Finish(1);
                }
            }
            void Finish(int code)
            {
                EditorApplication.update -= Check;
                SessionState.SetBool(PlayCheckKey, false);
                Time.timeScale = 1;
                if (Application.isBatchMode) EditorApplication.Exit(code);
                else EditorApplication.isPlaying = false;
            }
            EditorApplication.update += Check;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
