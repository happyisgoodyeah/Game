namespace ET
{
    [EntitySystemOf(typeof(Slot))]
    public static partial class SlotSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Slot self, int configId, ET.IntVector2 position)
        {
            self.configId = configId;
            self.position = position;
        }

        public static SlotConfig Config(this Slot self)
        {
            return SlotConfigCategory.Instance.Get(self.configId);
        }

        /// <summary>
        /// 复位偏移
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static (int x, int y) GetResetOffset(this ET.Slot self , SnapDirection direction)
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
    }
}