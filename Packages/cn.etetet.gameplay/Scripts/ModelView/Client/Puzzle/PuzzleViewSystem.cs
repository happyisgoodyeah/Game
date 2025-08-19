using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [EntitySystemOf(typeof(PuzzleView))]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(Slot))]
    public static partial class PuzzleViewSystem
    {
        [EntitySystem]
        private static void Awake(this ET.PuzzleView self, Transform transform)
        {
            self.transform = transform;
            self.parentTransform = transform.parent;

            var puzzle = self.GetParent<Puzzle>();
            //拼图位置
            self.originPosition = self.transform.position;

            //随机颜色方便认
            var color = new Color(Random.Range(0, 255) / 255f, Random.Range(0, 255) / 255f, Random.Range(0, 255) / 255f, 1);
            for (int i = 0; i < puzzle.slots.Count; i++)
            {
                self.transform.GetChild(i).Find("Square").GetComponent<SpriteRenderer>().color = color;
            }

            //拖拽相关组件
            self.AddComponent<DragComponent>();
            self.AddComponent<DraggableTag>();
            
            //碰撞相关组件
            var triggerColliderComponent = self.AddComponent<TriggerColliderComponent, Entity, GameObject>(self , transform.gameObject);
            triggerColliderComponent.SetTagList(new List<string>(){"Grid"});
        }

        public static void Rotate(this ET.PuzzleView self, float angle)
        {
            self.transform.Rotate(Vector3.up, angle);
        }

        public static void BackToOriginPosition(this ET.PuzzleView self)
        {
            //todo dotween
            //使用dotween线性移动 先直接复原位置
            //转回需要重置旋转角度
            self.transform.position = self.originPosition;
            self.transform.rotation = Quaternion.Euler(Vector3.zero);
            
            self.GetParent<Puzzle>().ResetBindSlots();
        }
        
        /// <summary>
        /// 根据碰撞点返回对应的SlotView
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static SlotView GetSlotViewByClosePoint(this PuzzleView self , Vector3 closePoint)
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
    }
}