using System;
using System.IO;
using System.Linq;
using PuzzleGame;
using UnityEditor;
using UnityEngine;

namespace PuzzleGameEditor
{
    /// <summary>建立静态底图、可独立放置的风摆层、天气效果和静止的解谜物件挂点。</summary>
    public static class LevelEnvironmentArtTools
    {
        private const string DirectoryPath = "Packages/cn.etetet.gameplay/Resources/LevelScenes/Animated";
        private static readonly string[] SpriteNames =
            { "curtain-left", "curtain-right", "window-plant", "shelf-plant", "sakura-petal", "wind-leaf" };

        public static GameObject[] Install()
        {
            ConfigureTexture($"{DirectoryPath}/room-static.png", false);
            ConfigureTexture($"{DirectoryPath}/ambience-atlas.png", true);
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath($"{DirectoryPath}/ambience-atlas.png").OfType<Sprite>().ToArray();
            if (sprites.Length != 6) throw new InvalidOperationException("环境图集应包含六个 Sprite。");
            Material material = AssetDatabase.LoadAssetAtPath<Material>($"{DirectoryPath}/sway.mat");
            if (material == null)
            {
                material = new Material(Shader.Find("Sprites/Default"));
                AssetDatabase.CreateAsset(material, $"{DirectoryPath}/sway.mat");
            }
            material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{DirectoryPath}/ambience-atlas.png");
            EditorUtility.SetDirty(material);

            GameObject[] prefabs = new GameObject[3];
            for (int index = 0; index < prefabs.Length; ++index)
            {
                GameObject root = new GameObject($"Environment{1001 + index}");
                try
                {
                    Transform decor = Child(root.transform, "AnimatedDecor");
                    Transform props = Child(root.transform, "PuzzleProps");
                    Child(root.transform, "Weather");
                    if (index == 0)
                    {
                        AddSway(decor, sprites, material, "curtain-left", new Vector2(-20.8f, 9.25f),
                            new Vector2(5.8f, 7.1f), 0, 0.03f, 0.16f, 0);
                        AddSway(decor, sprites, material, "curtain-right", new Vector2(-2.8f, 9.25f),
                            new Vector2(7.7f, 7.1f), 0, 0.028f, 0.19f, 1.1f);
                        AddSway(decor, sprites, material, "window-plant", new Vector2(-15.4f, 10.65f),
                            new Vector2(3.3f, 3.6f), 1, 0.04f, 0.23f, 0.7f);
                        AddSway(decor, sprites, material, "shelf-plant", new Vector2(7.7f, 9.35f),
                            new Vector2(4.1f, 5.9f), 2, 0.04f, 0.2f, 2.1f);
                        Child(props, "WindowSill").localPosition = new Vector3(-15.4f, 8.9f, 0);
                        Child(props, "Shelf").localPosition = new Vector3(7.7f, 8.9f, 0);
                    }
                    else if (index == 1)
                        Child(props, "CherryTree").localPosition = new Vector3(18, 9, 0);
                    else
                        Child(props, "PottingBench").localPosition = new Vector3(-18, -8, 0);

                    LevelEnvironmentAnimation animation = root.AddComponent<LevelEnvironmentAnimation>();
                    SerializedObject serialized = new SerializedObject(animation);
                    serialized.FindProperty("effect").enumValueIndex = index;
                    serialized.FindProperty("particleCount").intValue = index == 1 ? 24 : 8;
                    serialized.FindProperty("particleSprite").objectReferenceValue = index == 0 ? null :
                        sprites.Single(sprite => sprite.name == (index == 1 ? "sakura-petal" : "wind-leaf"));
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    animation.ClearPreview();
                    prefabs[index] = PrefabUtility.SaveAsPrefabAsset(root, $"{DirectoryPath}/Environment{1001 + index}.prefab");
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            AssetDatabase.SaveAssets();
            return prefabs;
        }

        private static void ConfigureTexture(string path, bool multiple)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new FileNotFoundException(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = multiple ? SpriteImportMode.Multiple : SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.npotScale = TextureImporterNPOTScale.None;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            if (multiple)
            {
                importer.spritePixelsPerUnit = 128;
                SpriteMetaData[] metadata = new SpriteMetaData[6];
                for (int i = 0; i < metadata.Length; ++i)
                    metadata[i] = new SpriteMetaData
                    {
                        name = SpriteNames[i], rect = new Rect(i % 3 * 512, (1 - i / 3) * 512, 512, 512),
                        alignment = (int)SpriteAlignment.Center, pivot = Vector2.one * 0.5f
                    };
#pragma warning disable CS0618 // 与当前项目的 TextureImporter 多 Sprite 工作流一致。
                importer.spritesheet = metadata;
#pragma warning restore CS0618
            }
            else
            {
                importer.GetSourceTextureWidthAndHeight(out int width, out _);
                importer.spritePixelsPerUnit = width / 44f;
            }
            importer.SaveAndReimport();
        }

        private static Transform Child(Transform parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static void AddSway(Transform parent, Sprite[] sprites, Material material, string name,
            Vector2 position, Vector2 size, int anchor, float amplitude, float frequency, float phase)
        {
            Sprite sprite = sprites.Single(value => value.name == name);
            string meshPath = $"{DirectoryPath}/{name}-mesh.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh == null)
            {
                mesh = CreateMesh(sprite);
                mesh.name = name + "-mesh";
                AssetDatabase.CreateAsset(mesh, meshPath);
            }
            Transform layer = Child(parent, name);
            layer.localPosition = position;
            layer.localScale = new Vector3(size.x, size.y, 1);
            layer.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = layer.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingLayerName = "BackGround";
            renderer.sortingOrder = 105;
            EnvironmentSwayLayer sway = layer.gameObject.AddComponent<EnvironmentSwayLayer>();
            SerializedObject serialized = new SerializedObject(sway);
            serialized.FindProperty("anchor").enumValueIndex = anchor;
            serialized.FindProperty("amplitude").floatValue = amplitude;
            serialized.FindProperty("cyclesPerSecond").floatValue = frequency;
            serialized.FindProperty("phase").floatValue = phase;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Mesh CreateMesh(Sprite sprite)
        {
            const int columns = 12;
            const int rows = 16;
            Vector3[] vertices = new Vector3[(columns + 1) * (rows + 1)];
            Vector2[] uv = new Vector2[vertices.Length];
            Color[] colors = new Color[vertices.Length];
            int[] triangles = new int[columns * rows * 6];
            Rect rect = sprite.rect;
            for (int y = 0; y <= rows; ++y)
            for (int x = 0; x <= columns; ++x)
            {
                int i = x + y * (columns + 1);
                float u = x / (float)columns;
                float v = y / (float)rows;
                vertices[i] = new Vector3(u - 0.5f, v - 0.5f, 0);
                uv[i] = new Vector2((rect.x + u * rect.width) / sprite.texture.width,
                    (rect.y + v * rect.height) / sprite.texture.height);
                colors[i] = Color.white;
                if (x == columns || y == rows) continue;
                int triangle = (x + y * columns) * 6;
                triangles[triangle] = i;
                triangles[triangle + 1] = i + columns + 1;
                triangles[triangle + 2] = i + 1;
                triangles[triangle + 3] = i + 1;
                triangles[triangle + 4] = i + columns + 1;
                triangles[triangle + 5] = i + columns + 2;
            }
            Mesh mesh = new Mesh { vertices = vertices, uv = uv, colors = colors, triangles = triangles };
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
