using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PuzzleGame;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PuzzleGameEditor
{
    /// <summary>在隔离预览场景验证两种动画的切换、循环、复位和玩法变换不变性。</summary>
    public static class PuzzleAnimationValidation
    {
        private const string Output = "ArtOutput/PuzzleSketch_20261106/qa";

        [MenuItem("ET/Puzzle/Validate idle and picked up animations")]
        public static void Run()
        {
            Directory.CreateDirectory(Output);
            PuzzleAnimalArtTools.ValidateAssets();
            Scene scene = EditorSceneManager.NewPreviewScene();
            float timeScale = Time.timeScale;
            try
            {
                Time.timeScale = 0;
                foreach (int id in new[] { 1001, 1002, 1003, 1004 }) Validate(scene, id);
                RenderRoom(scene);
                File.WriteAllText(Output + "/unity-animation-validation.json",
                    "{\"ok\":true,\"prefabs\":4,\"idleFrames\":8,\"pickedUpFrames\":8," +
                    "\"introOnceThenSixFrameHoldLoop\":true,\"repeatSelectionDoesNotRestart\":true," +
                    "\"releaseAndDisableReset\":true,\"fourRotations\":true,\"timeScaleZero\":true," +
                    "\"transformAndColliderStable\":true,\"etGameplayTested\":false}");
            }
            finally
            {
                Time.timeScale = timeScale;
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static void Validate(Scene scene, int id)
        {
            GameObject instance = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                $"Packages/cn.etetet.gameplay/Resources/Puzzle/Puzzle{id}.prefab"));
            SceneManager.MoveGameObjectToScene(instance, scene);
            PuzzleIdleAnimation animation = instance.GetComponent<PuzzleIdleAnimation>();
            // 预览场景中的非 ExecuteAlways 组件需要显式初始化。
            InvokeLifecycle(animation, "OnEnable");
            SpriteRenderer renderer = instance.GetComponent<SpriteRenderer>();
            SerializedObject data = new SerializedObject(animation);
            Sprite[] idle = ReadFrames(data.FindProperty("frames"));
            Sprite[] held = ReadFrames(data.FindProperty("pickedUpFrames"));
            Collider2D[] colliders = instance.GetComponentsInChildren<Collider2D>();
            string[] collision = colliders.Select(EditorJsonUtility.ToJson).ToArray();
            Transform[] transforms = instance.GetComponentsInChildren<Transform>();
            instance.transform.localScale = Vector3.one * 1.25f;
            instance.transform.position = new Vector3(2, -1, 0);
            for (int turn = 0; turn <= 4; ++turn)
            {
                instance.transform.rotation = Quaternion.Euler(0, 0, turn * 90);
                Matrix4x4[] matrices = transforms.Select(t => t.localToWorldMatrix).ToArray();
                animation.SetPickedUp(false);
                HashSet<Sprite> seen = new HashSet<Sprite>();
                for (int i = 0; i < 80; ++i)
                {
                    seen.Add(renderer.sprite);
                    animation.Advance(0.05f);
                }
                Require(seen.SetEquals(idle), "待机八帧循环不完整。");
                animation.SetPickedUp(true);
                Require(renderer.sprite == held[0], "拎起未从起手帧开始。");
                animation.Advance(0.28f);
                Sprite current = renderer.sprite;
                animation.SetPickedUp(true);
                Require(renderer.sprite == current, "重复选中重播了拎起起手。");
                animation.SetPickedUp(false);
                animation.SetPickedUp(true);
                seen.Clear();
                for (int i = 0; i < 20; ++i)
                {
                    seen.Add(renderer.sprite);
                    animation.Advance(0.05f);
                }
                Require(seen.SetEquals(held), "拎起八帧播放不完整。");
                for (int i = 0; i < 100; ++i)
                {
                    animation.Advance(0.05f);
                    Require(renderer.sprite != held[0] && renderer.sprite != held[1], "悬停循环错误地重播起手帧。");
                }
                animation.SetPickedUp(false);
                Require(renderer.sprite == idle[0] && !animation.IsPickedUp, "松开未恢复待机。");
                animation.SetPickedUp(true);
                InvokeLifecycle(animation, "OnDisable");
                Require(renderer.sprite == idle[0] && !animation.IsPickedUp, "禁用未复位状态。");
                Require(transforms.Select(t => t.localToWorldMatrix).SequenceEqual(matrices), "动画修改了玩法变换。");
                Require(colliders.Select(EditorJsonUtility.ToJson).SequenceEqual(collision), "动画修改了碰撞区域。");
            }
            UnityEngine.Object.DestroyImmediate(instance);
        }

        private static Sprite[] ReadFrames(SerializedProperty property)
        {
            Sprite[] values = new Sprite[property.arraySize];
            for (int i = 0; i < values.Length; ++i) values[i] = property.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
            return values;
        }

        private static void InvokeLifecycle(PuzzleIdleAnimation animation, string method)
        {
            // 预览模式没有正常的 MonoBehaviour 调度；直接调用生命周期方法进行确定性验证。
            typeof(PuzzleIdleAnimation).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(animation, null);
        }

        private static void RenderRoom(Scene scene)
        {
            const string package = "Packages/cn.etetet.gameplay/Resources";
            GameObject container = new GameObject("AnimationRoomPreview");
            SceneManager.MoveGameObjectToScene(container, scene);
            Camera camera = new GameObject("Camera").AddComponent<Camera>();
            camera.transform.SetParent(container.transform, false);
            camera.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true;
            camera.aspect = 16f / 9f;
            camera.scene = scene;
            GameObject grid = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                package + "/Grid/Grid1001.prefab"), container.transform);
            LevelSceneArt art = grid.GetComponent<LevelSceneArt>();
            Require(art.Configure(1001, camera), "无法创建首关外观预览。");
            for (int i = 0; i < 9; ++i)
            {
                bool allowed = i != 8;
                GameObject slot = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                    $"{package}/Slot/Slot{(allowed ? 1001 : 1002)}.prefab"), grid.transform.Find("SlotTransform"));
                slot.transform.position = new Vector3((i % 3 - 1) * 3, (1 - i / 3) * 3, 0);
                art.ApplySlot(slot.transform, allowed);
            }
            int[] ids = { 1003, 1004, 1002, 1001 };
            Vector3[] positions = { new Vector3(-15, -5), new Vector3(-15, 5), new Vector3(13, 5), new Vector3(15, -5) };
            List<PuzzleIdleAnimation> animations = new List<PuzzleIdleAnimation>();
            for (int i = 0; i < ids.Length; ++i)
            {
                GameObject piece = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                    $"{package}/Puzzle/Puzzle{ids[i]}.prefab"), container.transform);
                piece.transform.position = positions[i];
                PuzzleIdleAnimation animation = piece.GetComponent<PuzzleIdleAnimation>();
                InvokeLifecycle(animation, "OnEnable");
                animations.Add(animation);
            }
            Render(camera, Output + "/room-idle.png");
            foreach (PuzzleIdleAnimation animation in animations) animation.SetPickedUp(true);
            for (int frame = 0; frame < 16; ++frame)
            {
                foreach (PuzzleIdleAnimation animation in animations) animation.Advance(0.125f);
                Render(camera, $"{Output}/room-picked-up-{frame:00}.png");
            }
            UnityEngine.Object.DestroyImmediate(container);
        }

        private static void Render(Camera camera, string path)
        {
            RenderTexture target = RenderTexture.GetTemporary(1280, 720, 24);
            RenderTexture previous = RenderTexture.active;
            Texture2D image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
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
    }
}
