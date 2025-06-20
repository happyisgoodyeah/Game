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
            self.transform.Find("Square").GetComponent<SpriteRenderer>().color = new Color(Random.Range(0,255) / 255f , Random.Range(0,255) / 255f , Random.Range(0,255) / 255f, 1);
            self.AddComponent<SpriteRenderComponent , GameObject>(self.transform.Find("Square").gameObject);

            self.AddComponent<DragComponent>();
            self.AddComponent<DraggableTag>();
        }

        public static void BackToOriginPosition(this ET.PuzzleView self)
        {
            //todo dotween
            //使用dotween线性移动 先直接复原位置
            self.transform.position = self.originPosition;
            self.GetParent<Puzzle>().ResetSlots();
        }
    }
}