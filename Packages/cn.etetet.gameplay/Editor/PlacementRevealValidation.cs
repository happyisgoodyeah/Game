using System;
using System.IO;
using System.Linq;
using PuzzleGame;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PuzzleGameEditor
{
    /// <summary>在独立预览场景检查放置反馈和篮筐揭晓，不读写玩家存档。</summary>
    public static class PlacementRevealValidation
    {
        private const string Output = "ArtOutput/PlacementReveal_20261102";
        private const string Package = "Packages/cn.etetet.gameplay";

        [MenuItem("ET/Puzzle/Validate placement and basket reveal")]
        public static void Run()
        {
            Directory.CreateDirectory(Output);
            LevelSceneArtTools.InstallAndPreview();
            for (int level = 1001; level <= 1003; ++level) Validate(level);
            File.WriteAllText($"{Output}/validation.json", "{\"ok\":true,\"levels\":[1001,1002,1003]," +
                "\"sixFrameLanding\":true,\"pillowRestored\":true,\"collidersAndSlotTransformsUnchanged\":true," +
                "\"cloudPoolLimit\":8,\"hiddenChick\":true,\"threeClickReveal\":true," +
                "\"repeatClicksRejected\":true,\"chickCollidersRestored\":true,\"reentryReset\":true," +
                "\"validationMode\":\"Unity preview scene deterministic sampling\",\"etGameplayTested\":false}");
            Debug.Log("放置特效、首关篮筐解谜和三关外观检查通过。");
        }

        private static void Validate(int level)
        {
            Scene scene = EditorSceneManager.NewPreviewScene();
            try
            {
                GameObject container = new GameObject("PlacementRevealPreview");
                SceneManager.MoveGameObjectToScene(container, scene);
                Camera camera = new GameObject("Camera").AddComponent<Camera>();
                camera.transform.SetParent(container.transform);
                camera.transform.position = new Vector3(0, 0, -10);
                camera.orthographic = true;
                camera.aspect = 16f / 9f;
                camera.scene = scene;
                GameObject grid = UnityEngine.Object.Instantiate(
                    AssetDatabase.LoadAssetAtPath<GameObject>($"{Package}/Resources/Grid/Grid1001.prefab"), container.transform);
                LevelSceneArt art = grid.GetComponent<LevelSceneArt>();
                Require(art.Configure(level, camera), "无法配置关卡。");
                Transform slotParent = grid.transform.Find("SlotTransform");
                foreach (MonoBehaviour script in slotParent.GetComponents<MonoBehaviour>()) UnityEngine.Object.DestroyImmediate(script);
                for (int i = 0; i < 9; ++i)
                {
                    bool allowed = level == 1001 ? i != 8 : i < 5;
                    GameObject slot = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                        $"{Package}/Resources/Slot/Slot{(allowed ? 1001 : 1002)}.prefab"), slotParent);
                    slot.transform.position = new Vector3((i % 3 - 1) * 3, (1 - i / 3) * 3, 0);
                    art.ApplySlot(slot.transform, allowed);
                }
                PlacementFeedback feedback = grid.GetComponent<PlacementFeedback>();
                LevelOneBasketReveal reveal = grid.GetComponent<LevelOneBasketReveal>();
                GameObject chick = null;
                Transform landingAnimal = null;
                if (level == 1001)
                {
                    int[] ids = { 1003, 1004, 1004, 1001 };
                    Vector3[] positions = { new Vector3(-15, -5), new Vector3(-15, 5), new Vector3(15, 5), new Vector3(15, -5) };
                    for (int i = 0; i < ids.Length; ++i)
                    {
                        GameObject piece = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                            $"{Package}/Resources/Puzzle/Puzzle{ids[i]}.prefab"), grid.transform.Find("PuzzleTransform/Puzzle"));
                        piece.transform.position = positions[i];
                        if (i == 1) landingAnimal = piece.transform;
                        if (ids[i] == 1001) chick = piece;
                    }
                    reveal.RegisterChick(chick);
                    // 模拟 ET 在 PuzzleView 之后异步创建槽位碰撞体。
                    GameObject lateSlot = new GameObject("LateLoadedSlotCollider");
                    lateSlot.transform.SetParent(chick.transform, false);
                    lateSlot.AddComponent<BoxCollider2D>();
                    Require(!chick.activeSelf, "Chick 初始必须隐藏。");
                    SpriteRenderer blocked = slotParent.GetChild(8).Find("SceneSurface").GetComponent<SpriteRenderer>();
                    Require(blocked.sprite.name == "bear-obstacle", "首关障碍格不是小熊。");
                }
                Transform[] slots = slotParent.GetComponentsInChildren<Transform>();
                Transform[] stable = slots.Where(value => value.name != "SceneSurface").ToArray();
                Matrix4x4[] matrices = stable.Select(value => value.localToWorldMatrix).ToArray();
                Collider2D[] colliders = slotParent.GetComponentsInChildren<Collider2D>();
                string[] collision = colliders.Select(EditorJsonUtility.ToJson).ToArray();
                Render(camera, $"{Output}/level-{level}-initial.png");
                Transform board = grid.transform.Find("BoardMat");
                Vector3 rest = board.localScale;
                Vector3 impact = Vector3.zero;
                if (landingAnimal != null)
                {
                    landingAnimal.position = new Vector3(-3, 3, 0);
                    impact = landingAnimal.GetComponent<SpriteRenderer>().bounds.center;
                }
                feedback.PlayLanding(impact);
                feedback.Advance(0.08f);
                Require(level != 1001 || board.localScale.y < rest.y, "枕头没有塌陷。");
                feedback.Advance(1);
                feedback.PlayLanding(impact);
                for (int frame = 0; frame < 6; ++frame)
                {
                    Render(camera, $"{Output}/landing-{level}-{frame:00}.png");
                    feedback.Advance(0.07f);
                }
                feedback.Advance(1);
                if (landingAnimal != null) landingAnimal.position = new Vector3(-15, 5, 0);
                Require(board.localScale == rest && !feedback.IsPlaying, "放置特效未复原。");
                Require(stable.Select(value => value.localToWorldMatrix).SequenceEqual(matrices), "放置动画移动了槽位变换。");
                Require(colliders.Select(EditorJsonUtility.ToJson).SequenceEqual(collision), "放置动画改变了碰撞体。");
                for (int i = 0; i < 120; ++i) feedback.PlayLanding(Vector3.zero);
                Require(grid.GetComponentsInChildren<SpriteRenderer>().Count(value => value.name == "CartoonCloud") == 8,
                    "快速放置导致云尘对象超过上限。");
                feedback.Advance(1);
                if (level != 1001)
                {
                    GameObject otherChick = new GameObject("OtherLevelChick");
                    otherChick.transform.SetParent(grid.transform);
                    reveal.RegisterChick(otherChick);
                    Require(otherChick.activeSelf && !reveal.TryClick(), "其他关卡被首关解谜影响。");
                    return;
                }
                Vector3 chickRest = chick.transform.position;
                bool[] chickCollision = chick.GetComponentsInChildren<Collider2D>().Select(value => value.enabled).ToArray();
                Require(reveal.TryClick() && reveal.TryClick(), "前两次点击未计数。");
                reveal.Advance(0.3f);
                Require(!chick.activeSelf && reveal.ClickCount == 2, "第三次点击之前 Chick 提前出现。");
                Transform basket = grid.transform.Find("Environment/EnvironmentLayers/PuzzleProps/RevealBasket");
                Vector3 basketRest = basket.position;
                Require(reveal.TryClick() && !reveal.TryClick(), "第三次点击未触发或重复点击未被拦截。");
                Directory.CreateDirectory($"{Output}/reveal-frames");
                for (int frame = 0; frame < 32; ++frame)
                {
                    Render(camera, $"{Output}/reveal-frames/{frame:00}.png");
                    reveal.Advance(0.05f);
                    feedback.Advance(0.05f);
                    if (frame == 13)
                        Require(chick.activeSelf && chick.GetComponentsInChildren<Collider2D>().All(value => !value.enabled),
                            "飞出中的 Chick 不应可拖拽。");
                }
                Require(reveal.IsRevealed && chick.activeSelf && chick.transform.position == chickRest, "Chick 未返回待放区。");
                Require(basket.position.y < basketRest.y, "篮筐未拉出。");
                Require(chick.GetComponentsInChildren<Collider2D>().Select(value => value.enabled).SequenceEqual(chickCollision),
                    "Chick 碰撞状态未恢复。");
                Require(!reveal.TryClick() && reveal.ClickCount == 3, "揭晓后仍可重复触发。");
                Render(camera, $"{Output}/level-1001-revealed.png");
                // 重新实例化 Grid 使用全新计数，旧关卡销毁不会留下全局解谜状态。
                GameObject next = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                    $"{Package}/Resources/Grid/Grid1001.prefab"), container.transform);
                next.GetComponent<LevelSceneArt>().Configure(1001, camera);
                Require(next.GetComponent<LevelOneBasketReveal>().ClickCount == 0, "重进关卡未重置点击次数。");
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
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
