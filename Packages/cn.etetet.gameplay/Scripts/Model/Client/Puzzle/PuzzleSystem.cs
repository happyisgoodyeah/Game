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
                var x = list[i][0];
                var y = list[i][1];

                var slot = spawn.PuzzleSpawnSlot(1000, new IntVector2(x, y));
                self.slots.Add(slot);

                //若处于最边缘一圈 加入吸附slots中
                if (x == 0 || y == 0)
                {
                    self.adsorptionSlots.Add(slot);
                }
            }
        }

        /// <summary>
        /// 获得slot的偏移量数组 若originPosition为（0，0）则返回puzzle对应的slot偏移量数组 originPosition为偏移值
        /// </summary>
        /// <returns></returns>
        public static List<IntVector2> GetCoveredPositions(this Puzzle self, IntVector2 originPosition)
        {
            var positions = new List<IntVector2>();
            for (int i = 0; i < self.slots.Count; i++)
            {
                IntVector2 index = self.GetOffsetByRotate(self.slotOffset[i]);
                /*IntVector2 index = self.slotOffset[i];*/
                positions.Add(new IntVector2(originPosition.X + index.Y, originPosition.Y + index.X));
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
        public static IntVector2 GetOffsetByRotate(this Puzzle self, IntVector2 position)
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
                0 => new IntVector2(position.X, position.Y), // 0°
                1 => new IntVector2(-position.Y, position.X), // 90°逆时针
                2 => new IntVector2(-position.X, -position.Y), // 180°
                3 => new IntVector2(position.Y, -position.X), // 270°逆时针
                _ => position
            };
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
        public static void RotatePuzzle(this Puzzle self, int rotAngle)
        {
            //rotate view & rotate offsetVector
            self.rotate = (self.rotate + rotAngle) % 360;

            //顺时针旋转90度
            /*for (int i = 0; i < self.slotOffset.Count; i++)
            {
                self.slotOffset[i] = new IntVector2(self.slotOffset[i].Y, -self.slotOffset[i].X);
            }*/

            EventSystem.Instance.Publish(self.Scene(), new PuzzleRotate { puzzle = self });
        }

        /// <summary>
        /// 数据层旋转，不执行view层旋转，用于判断旋转之后的Puzzle放置是否合法
        /// </summary>
        /// <param name="self"></param>
        /// <param name="rotAngle"></param>
        public static void RotatePuzzleData(this Puzzle self, int rotAngle)
        {
            self.rotate = (self.rotate + rotAngle) % 360;
        }

        /// <summary>
        /// View层旋转，请确认旋转后合法之后再执行此逻辑
        /// </summary>
        /// <param name="self"></param>
        /// <param name="rotAngle"></param>
        public static void RotatePuzzleView(this Puzzle self)
        {
            EventSystem.Instance.Publish(self.Scene(), new PuzzleRotate { puzzle = self });
        }

        /// <summary>
        /// 更改移动模式
        /// </summary>
        /// <param name="self"></param>
        /// <param name="moveModeType"></param>
        public static void ChangeMoveMode(this Puzzle self, PuzzleMoveModeType moveModeType)
        {
            self.moveMode = moveModeType;
        }

        /// <summary>
        /// 获得当前拼图的x范围 y范围
        /// </summary>
        /// <param name="self"></param>
        public static (IntVector2 xRange, IntVector2 yRange) GetPuzzleXYRange(this Puzzle self)
        {
            var positions = self.GetCoveredPositions(new IntVector2(0, 0));
            var minX = 100;
            var maxX = -100;
            var minY = 100;
            var maxY = -100;
            foreach (var pos in positions)
            {
                minX = Math.Min(minX, pos.X);
                maxX = Math.Max(maxX, pos.X);
                minY = Math.Min(minY, pos.Y);
                maxY = Math.Max(maxY, pos.Y);
            }

            return (new IntVector2(minX, maxX), new IntVector2(minY, maxY));
        }
    }

    /// <summary>
    /// 吸附方向枚举
    /// </summary>
    public enum SnapDirection
    {
        None,
        Up,
        Down,
        Left,
        Right,
        UpRight,
        UpLeft,
        DownRight,
        DownLeft,
    }
}