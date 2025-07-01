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
        private static void Awake(this ET.PuzzleView self , Transform transform)
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
            
            self.AddComponent<DragComponent>();
            self.AddComponent<DraggableTag>();
        }

        public static void BackToOriginPosition(this ET.PuzzleView self)
        {
            //todo dotween
            //使用dotween线性移动 先直接复原位置
            self.transform.position = self.originPosition;
            self.GetParent<Puzzle>().ResetBindSlots();
        }
    }
}