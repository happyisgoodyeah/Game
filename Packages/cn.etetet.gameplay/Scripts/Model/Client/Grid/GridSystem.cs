namespace ET
{
    [EntitySystemOf(typeof(Grid))]
    public static partial class GridSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Grid self)
        {
            self.InitComponent();
        }

        public static void InitComponent(this Grid self)
        {
            self.AddComponent<GridDataComponent,int,int,int>(2,2,1);
            self.AddComponent<PuzzleSpawnComponent>();
        }
    }
}
