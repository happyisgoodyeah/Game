using System;
using System.IO;
using System.Linq;
using PuzzleGame;
using UnityEditor;
using UnityEngine;

namespace PuzzleGameEditor
{
    /// <summary>安装统一云尘、首关小熊障碍格和独立篮筐资源，保留现有资源 GUID。</summary>
    public static class PlacementRevealArtTools
    {
        public const string Art = "Packages/cn.etetet.gameplay/Resources/LevelScenes/Interaction";

        public static void ImportSprites()
        {
            Configure("room-without-basket", 44);
            Configure("bear-obstacle", 3);
            Configure("reveal-basket", 5.3f);
            Configure("cloud-puff", 3);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath($"{Art}/cloud-puff.png");
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            if (width % 3 != 0 || height % 2 != 0 || width / 3 != height / 2)
                throw new InvalidOperationException("云尘图集必须为 3×2 的六个正方形帧。");
            int cell = width / 3;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = cell / 3f;
#pragma warning disable CS0618 // 与当前环境图集的多 Sprite 导入接口保持一致。
            importer.spritesheet = Enumerable.Range(0, 6).Select(i => new SpriteMetaData
            {
                name = $"cloud-{i:00}", rect = new Rect(i % 3 * cell, (1 - i / 3) * cell, cell, cell),
                alignment = (int)SpriteAlignment.Center, pivot = Vector2.one * 0.5f
            }).ToArray();
#pragma warning restore CS0618
            importer.SaveAndReimport();
        }

        private static void Configure(string name, float worldWidth)
        {
            string path = $"{Art}/{name}.png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new FileNotFoundException(path);
            importer.GetSourceTextureWidthAndHeight(out int width, out _);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = width / worldWidth;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        public static void CreateBasket(Transform parent)
        {
            GameObject basket = new GameObject("RevealBasket");
            basket.transform.SetParent(parent, false);
            basket.transform.localPosition = new Vector3(11.4f, 7.6f, 0);
            SpriteRenderer renderer = basket.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{Art}/reveal-basket.png");
            renderer.sortingLayerName = "BackGround";
            renderer.sortingOrder = 110;
        }

        public static void ConfigureGrid(GameObject grid)
        {
            LevelSceneArt art = grid.GetComponent<LevelSceneArt>();
            SerializedObject settings = new SerializedObject(art);
            settings.FindProperty("backgrounds").GetArrayElementAtIndex(0).objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>($"{Art}/room-without-basket.png");
            settings.FindProperty("roomBearObstacle").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>($"{Art}/bear-obstacle.png");
            settings.ApplyModifiedPropertiesWithoutUndo();
            PlacementFeedback feedback = grid.GetComponent<PlacementFeedback>() ?? grid.AddComponent<PlacementFeedback>();
            Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath($"{Art}/cloud-puff.png").OfType<Sprite>()
                .OrderBy(value => value.name).ToArray();
            if (frames.Length != 6) throw new InvalidOperationException("云尘必须包含六帧。");
            settings = new SerializedObject(feedback);
            SerializedProperty array = settings.FindProperty("cloudFrames");
            array.arraySize = frames.Length;
            for (int i = 0; i < frames.Length; ++i) array.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            settings.ApplyModifiedPropertiesWithoutUndo();
            if (grid.GetComponent<LevelOneBasketReveal>() == null) grid.AddComponent<LevelOneBasketReveal>();
        }
    }
}
