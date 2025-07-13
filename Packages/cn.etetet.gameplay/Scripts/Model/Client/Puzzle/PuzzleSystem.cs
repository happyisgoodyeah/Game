using System;
using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(Puzzle))]
    [FriendOf(typeof(Puzzle))]
    [FriendOf((typeof(Slot)))]
    public static partial class PuzzleSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Puzzle self, int configId, int positionId)
        {
            self.configId = configId;
            self.positionIndex = positionId;
            self.Init();
        }

        public static PuzzleConfig Config(this Puzzle self)
        {
            return PuzzleConfigCategory.Instance.Get(self.configId);
        }

        /// <summary>
        /// 获得slot的偏移量数组
        /// </summary>
        /// <returns></returns>
        public static List<IntVector2> GetCoveredPositions(this Puzzle self, IntVector2 originPosition)
        {
            var positions = new List<IntVector2>();
            for (int i = 0; i < self.slots.Count; i++)
            {
                var index = self.GetOffsetByRotate(self.slotOffset[i]);
                positions.Add(new IntVector2(originPosition.X + index.X, originPosition.Y + index.Y));
            }

            return positions;
        }

        /// <summary>
        /// 获得坐标旋转后的偏移量
        /// </summary>
        /// <param name="self"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IntVector2 GetOffsetByRotate(this Puzzle self , IntVector2 position)
        {
            if (Math.Abs(self.rotate % 90) > 1e-5)
            {
                throw new ArgumentException("角度不为90°倍数");
            }
            
            // 将角度转换为等效的[0, 360)范围内的正角度
            double normalizedAngle = self.rotate % 360;
            if (normalizedAngle < 0) normalizedAngle += 360;
        
            // 计算旋转次数（90°倍数）
            int rotations = (int)(normalizedAngle / 90) % 4;
        
            // 应用旋转公式
            return rotations switch
            {
                0 => new IntVector2( position.X,  position.Y), // 0°
                1 => new IntVector2(-position.Y,  position.X), // 90°逆时针
                2 => new IntVector2(-position.X, -position.Y), // 180°
                3 => new IntVector2( position.Y, -position.X), // 270°逆时针
                _ => position
            };
        }

        /// <summary>
        /// 初始化 根据配置同步信息 生成slot数据层
        /// </summary>
        /// <param name="self"></param>
        public static void Init(this Puzzle self)
        {
            var spawn = self.AddComponent<SlotSpawnComponent>();
            var list = self.Config().SlotOffset;
            for (int i = 0; i < list.Count; i++)
            {
                //拼图用slot ConfigID为1000 偏移量为二维数组坐标
                var slot = spawn.PuzzleSpawnSlot(1000, new IntVector2(list[i][0], list[i][1]));
                self.slots.Add(slot);
            }
        }

        /// <summary>
        /// 重置拼图绑定的所有格子
        /// </summary>
        /// <param name="self"></param>
        public static void ResetBindSlots(this ET.Puzzle self)
        {
            foreach (var slotRef in self.bindSlots)
            {
                var slot = slotRef.Entity;
                slot.puzzleRef = default;
            }

            self.bindSlots.Clear();
        }

        /// <summary>
        /// 旋转拼图 数据层
        /// </summary>
        /// <param name="self"></param>
        public static void RotatePuzzle(this Puzzle self)
        {
            self.rotate = (self.rotate + 90) % 360;
            EventSystem.Instance.Publish(self.Scene(), new PuzzleRotate { puzzle = self });
        }
    }
}