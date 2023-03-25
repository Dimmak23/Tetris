using TetrisApp.Grid;

namespace TetrisApp.GamePlay
{
    public class GameState
    {
        private Blocks.Block currentBlock;

        public Blocks.Block CurrentBlock
        {
            get => currentBlock;
            private set
            {
                currentBlock = value;
                currentBlock.Reset();

                // Block are created two rows above the main grid, this loop will bring them to the main grid
                for (int i = 0; i < 2; i++)
                {
                    currentBlock.Move(1, 0);
                    if (!BlockFits()) currentBlock.Move(-1, 0);
                }
            }
        }

        public Grid.GameGrid GameGrid { get; }

        public Blocks.BlockQueue BlockQueue { get; }

        public bool GamePause { get; set; }

        public bool GameOver { get; set; }

        public bool GameStarted { get; set; }

        public bool GameRestarted { get; set; }

        public int Score { get; private set; }

        public Blocks.Block HeldBlock { get; private set; }

        public bool CanHold { get; private set; }

        public GameState()
        {
            GameGrid = new Grid.GameGrid(22, 10);
            BlockQueue = new Blocks.BlockQueue();
            CurrentBlock = BlockQueue.GetAndUpdate();
            CanHold = true;
            GameStarted = false;
            GamePause = false;
            GameRestarted = false;
        }

        private bool BlockFits()
        {
            foreach (Position pos in CurrentBlock.TilePositions())
            {
                if (!GameGrid.IsEmpty(pos.Row, pos.Column)) return false;
            }
            return true;
        }

        public void HoldBlock()
        {
            if (!CanHold) return;

            if (HeldBlock == null)
            {
                HeldBlock = CurrentBlock;
                CurrentBlock = BlockQueue.GetAndUpdate();
            }
            else
            {
                Blocks.Block tmp = CurrentBlock;
                CurrentBlock = HeldBlock;
                HeldBlock = tmp;
            }

            CanHold = false;
        }

        public void RotateBlockCW()
        {
            CurrentBlock.RotateCW();
            if (!BlockFits()) CurrentBlock.RotateCCW();
        }

        public void RotateBlockCCW()
        {
            CurrentBlock.RotateCCW();
            if (!BlockFits()) CurrentBlock.RotateCW();
        }

        public void MoveBlockLeft()
        {
            CurrentBlock.Move(0, -1);
            if (!BlockFits()) CurrentBlock.Move(0, 1);
        }

        public void MoveBlockRight()
        {
            CurrentBlock.Move(0, 1);
            if (!BlockFits()) CurrentBlock.Move(0, -1);
        }

        //private bool IsGameOver()
        //{
        //    //Possible mistake, if first two rows are hidden we should ask
        //    //about rows #2 and #3, and also why ask about two rows?
        //    //if we need to stop game when block gets second row above:
        //    //then-> return !GameGrid.IsEmpty(3);
        //    //or if we finish when up row is full -> !GameGrid.IsEmpty(0);
        //    return !GameGrid.IsRowEmpty(0);
        //}

        private void PlaceBlock()
        {
            foreach (Position pos in CurrentBlock.TilePositions())
            {
                GameGrid[pos.Row, pos.Column] = CurrentBlock.Id;
            }

            Score += GameGrid.ClearFullRows();

            if (!GameGrid.IsRowEmpty(1)) GameOver = true;
            else
            {
                CurrentBlock = BlockQueue.GetAndUpdate();
                CanHold = true;
            }
        }

        public void MoveBlockDown()
        {
            CurrentBlock.Move(1, 0);
            if (!BlockFits())
            {
                CurrentBlock.Move(-1, 0);
                PlaceBlock();
            }
        }

        private int TileDropDistance(Position pos)
        {
            int drop = 0;
            while (GameGrid.IsEmpty(pos.Row + drop + 1, pos.Column))
            {
                drop++;
            }
            return drop;
        }

        public int BlockDropDistance()
        {
            int drop = GameGrid.Rows;
            foreach (Position pos in CurrentBlock.TilePositions())
            {
                drop = System.Math.Min(drop, TileDropDistance(pos));
            }
            return drop;
        }

        public void DropBlock()
        {
            CurrentBlock.Move(BlockDropDistance(), 0);
            PlaceBlock();
        }
    }
}
