using System;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LevelSessionComponent))]
    [FriendOf(typeof(LevelSessionComponent))]
    [FriendOf(typeof(Grid))]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(GridView))]
    public static partial class LevelSessionComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LevelSessionComponent self)
        {
            self.CurrentGrid = default;
            self.Version = 0;
            self.IsLoading = false;
            self.IsReturning = false;
            self.IsCompleting = false;
        }

        public static LevelSessionComponent GetOrCreate(Scene scene)
        {
            Scene root = scene.Root();
            return root.GetComponent<LevelSessionComponent>() ?? root.AddComponent<LevelSessionComponent>();
        }

        /// <summary>
        /// 使未完成的加载失效，并释放本局的实体和游戏对象。
        /// </summary>
        public static void ClearLevel(this LevelSessionComponent self)
        {
            ++self.Version;
            Grid grid = self.CurrentGrid;
            self.CurrentGrid = default;
            self.IsLoading = false;
            self.IsCompleting = false;
            if (grid == null)
            {
                return;
            }

            // Destroy 在帧末执行，先隐藏根对象，立即停止碰撞和输入。
            GameObject gameObject = grid.GetComponent<GameObjectComponent>()?.GameObject;
            if (gameObject != null)
            {
                gameObject.SetActive(false);
            }
            grid.Dispose();
        }

        public static async ETTask<bool> EnterLevelAsync(this LevelSessionComponent self, int configId)
        {
            if (self.IsLoading || self.IsReturning || self.IsCompleting)
            {
                return false;
            }

            GridConfig config = GridConfigCategory.Instance.GetOrDefault(configId);
            if (config == null || config.X <= 0 || config.Y <= 0 ||
                config.SlotList.Count != config.X * config.Y || config.PuzzleCount != config.PuzzleList.Count)
            {
                Log.Error($"关卡配置无效: {configId}");
                return false;
            }

            self.ClearLevel();
            self.IsLoading = true;
            long version = self.Version;
            EntityRef<LevelSessionComponent> sessionRef = self;
            Grid grid;
            try
            {
                grid = self.Root().AddChild<Grid, int>(configId);
                EntityRef<Grid> gridRef = grid;
                self.CurrentGrid = grid;
                await EventSystem.Instance.PublishAsync(self.Root(), new AfterCreateGrid { grid = grid });
                self = sessionRef;
                grid = gridRef;
                if (self == null || grid == null || self.Version != version)
                {
                    return false;
                }
                GameObject gridObject = grid.GetComponent<GameObjectComponent>()?.GameObject;
                if (gridObject == null)
                {
                    throw new InvalidOperationException($"关卡预制体加载失败: {configId}");
                }
                gridObject.SetActive(false);

                await grid.SpawnSlot();
                self = sessionRef;
                grid = gridRef;
                if (self == null || grid == null || self.Version != version)
                {
                    return false;
                }

                grid.SpawnPuzzle();
                float deadline = Time.realtimeSinceStartup + 30f;
                while (!ViewsReady(grid))
                {
                    if (Time.realtimeSinceStartup >= deadline)
                    {
                        throw new TimeoutException($"关卡资源加载超时: {configId}");
                    }
                    await self.Root().GetComponent<TimerComponent>().WaitFrameAsync();
                    self = sessionRef;
                    grid = gridRef;
                    if (self == null || grid == null || self.Version != version)
                    {
                        return false;
                    }
                }
                // 资源可能以不同顺序完成加载，最终按配置顺序排列所有槽位。
                GridView gridView = grid.GetComponent<GridView>();
                for (int y = 0; y < grid.gridSize.Y; ++y)
                {
                    for (int x = 0; x < grid.gridSize.X; ++x)
                    {
                        grid.GetSlot(new IntVector2(x, y)).GetComponent<SlotView>().transform
                            .SetSiblingIndex(x + y * grid.gridSize.X);
                    }
                }
                CenteredGridLayout layout = gridView.slotTransform.GetComponent<CenteredGridLayout>();
                layout.constraint = CenteredGridLayout.Constraint.FixedColumnCount;
                layout.constraintCount = grid.gridSize.X;
                layout.UpdateLayout();
                grid.GetComponent<GameObjectComponent>().GameObject.SetActive(true);
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                self = sessionRef;
                if (self != null && self.Version == version)
                {
                    self.ClearLevel();
                }
                return false;
            }
            finally
            {
                self = sessionRef;
                if (self != null && self.Version == version)
                {
                    self.IsLoading = false;
                }
            }
        }

        private static bool ViewsReady(Grid grid)
        {
            if (grid.GetComponent<GridView>() == null)
            {
                return false;
            }
            foreach (EntityRef<Slot> slotRef in grid.slotDic.Values)
            {
                if (slotRef.Entity?.GetComponent<SlotView>() == null)
                {
                    return false;
                }
            }
            foreach (EntityRef<Puzzle> puzzleRef in grid.PuzzleDic.Values)
            {
                Puzzle puzzle = puzzleRef;
                if (puzzle?.GetComponent<PuzzleView>() == null)
                {
                    return false;
                }
                foreach (EntityRef<Slot> slotRef in puzzle.slots)
                {
                    if (slotRef.Entity?.GetComponent<SlotView>() == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static async ETTask ExitLevelAsync(this LevelSessionComponent self)
        {
            if (self.IsReturning)
            {
                return;
            }
            self.IsReturning = true;
            EntityRef<LevelSessionComponent> sessionRef = self;
            self.ClearLevel();
            try
            {
                await YIUIMgrComponent.Inst.Root.OpenPanelAsync<GameMainPanelPanelComponent, EGameMainPanelPanelViewEnum>(
                    EGameMainPanelPanelViewEnum.SelectLevelView);
            }
            finally
            {
                self = sessionRef;
                if (self != null)
                {
                    self.IsReturning = false;
                }
            }
        }

        public static async ETTask<bool> RestartLevelAsync(this LevelSessionComponent self)
        {
            Grid grid = self.CurrentGrid;
            if (grid == null)
            {
                return false;
            }
            return await self.EnterLevelAsync(grid.Config().Id);
        }
    }
}
