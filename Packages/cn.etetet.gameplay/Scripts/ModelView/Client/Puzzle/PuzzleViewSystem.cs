using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace ET
{
    [EntitySystemOf(typeof(PuzzleView))]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(Slot))]
    public static partial class PuzzleViewSystem
    {
        static PuzzleViewSystem()
        {
        }

        [EntitySystem]
        private static void Awake(this ET.PuzzleView self, Transform transform)
        {
            self.transform = transform;
            self.parentTransform = transform.parent;

            Puzzle puzzle = self.GetParent<Puzzle>();
            //拼图位置
            self.originPosition = self.transform.position;

            //随机颜色方便认
            Color color = new Color(Random.Range(0, 255) / 255f, Random.Range(0, 255) / 255f, Random.Range(0, 255) / 255f, 1);
            for (int i = 0; i < puzzle.slots.Count; i++)
            {
                self.transform.GetChild(i).Find("Square").GetComponent<SpriteRenderer>().color = color;
            }

            //拖拽相关组件
            self.AddComponent<DragComponent>();
            self.AddComponent<DraggableTag>();

            //碰撞相关组件
            var triggerColliderComponent = self.AddComponent<TriggerColliderComponent, Entity, GameObject>(self, transform.gameObject);
            triggerColliderComponent.SetTagList(new List<string>() { "Grid" });
        }

        public static void BackToOriginPosition(this ET.PuzzleView self)
        {
            //todo 使用dotween线性移动 先直接复原位置
            //转回需要重置旋转角度
            self.transform.position = self.originPosition;
            self.transform.rotation = Quaternion.Euler(Vector3.zero);

            Puzzle puzzle = self.GetParent<Puzzle>();
            puzzle.ResetBindSlots();
            puzzle.rotate = 0;
        }

        public static void ChangePuzzleLayOut(this ET.PuzzleView self, int lay)
        {
            self.transform.GetComponent<SpriteRenderer>().sortingOrder = lay;
        }

        /// <summary>
        /// 根据碰撞点返回对应的SlotView
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static SlotView GetSlotViewByClosePoint(this PuzzleView self, Vector3 closePoint)
        {
            Puzzle puzzle = self.GetParent<Puzzle>();
            SlotView index = null;
            float indexDistance = 999f;
            foreach (var puzzleAdsorptionSlot in puzzle.adsorptionSlots)
            {
                SlotView slotView = puzzleAdsorptionSlot.Entity.GetComponent<SlotView>();
                var distance = Vector2.Distance(closePoint, slotView.transform.position);
                index = distance < indexDistance ? slotView : index;
                indexDistance = Mathf.Min(distance, indexDistance);
            }

            return index;
        }

        /// <summary>
        /// 移动puzzle
        /// </summary>
        /// <param name="self"></param>
        /// <param name="targetPosition">目标坐标</param>
        /// <param name="isMust">是否强制 非强制不为空时直接退出 强制则重置tweener</param>
        public static void Move(this PuzzleView self, Vector3 targetPosition, float time = 0.1f, bool isMust = false)
        {
            if (self.tweener != null && !isMust)
            {
                return;
            }

            if (isMust)
            {
                self.tweener.Kill();
                self.tweener = null;
            }

            self.endPos = targetPosition;

            self.tweener = DOTween.To(() => self.transform.position,
                        pos => { self.transform.position = pos; },
                        self.endPos,
                        time)
                    .SetEase(Ease.Linear)
                    .OnUpdate(() =>
                    {
                        self.tweener.ChangeEndValue(self.endPos, time, true).Play();
                        if (Vector2.Distance(self.transform.position, self.endPos) <= 0.01f)
                        {
                            EventSystem.Instance.Publish(self.Scene(), new PuzzleMoveEndEvent() { puzzle = self.GetParent<Puzzle>() });
                        }
                    });
        }
    }
}