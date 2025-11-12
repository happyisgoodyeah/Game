namespace ET
{
    [EntitySystemOf(typeof(Slot))]
    [FriendOfAttribute(typeof(ET.Puzzle))]
    public static partial class SlotSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Slot self, int configId, IntVector2 position)
        {
            self.configId = configId;
            self.position = position;
            self.AddComponent<SlotStateComponent,bool>(self.Config().AllowPlace);
        }

        public static SlotConfig Config(this Slot self)
        {
            return SlotConfigCategory.Instance.Get(self.configId);
        }

        /// <summary>
        /// 获取偏移offset
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static (int x, int y) GetDirOffset(this ET.Slot self, SnapDirection direction)
        {
            switch (direction)
            {
                case SnapDirection.Up: return (0, 1);
                case SnapDirection.Down: return (0, -1);
                case SnapDirection.Left: return (-1, 0);
                case SnapDirection.Right: return (1, 0);
                case SnapDirection.UpRight: return (1, 1);
                case SnapDirection.UpLeft: return (-1, 1);
                case SnapDirection.DownRight: return (1, -1);
                case SnapDirection.DownLeft: return (-1, -1);
                default: return (0, 0);
            }
        }

        /// <summary>
        /// 设置当前Puzzle
        /// </summary>
        public static void SetPuzzle(this ET.Slot self, Puzzle puzzle)
        {
            puzzle.bindSlots.Add(self);
            self.puzzleRef = puzzle;
            EventSystem.Instance.Publish(self.Root(), new SlotSetPuzzle { slot = self });
        }
    }
}