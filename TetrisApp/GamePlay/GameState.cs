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
            }
        }

        public Grid.GameGrid GameGrid { get; }

        public Blocks.BlockQueue BlockQueue { get; }

        public bool GameOver { get; private set; }

        public GameState()
        {
            GameGrid = new Grid.GameGrid(22, 10);
            BlockQueue = new Blocks.BlockQueue();
            CurrentBlock = BlockQueue.GetAndUpdate();
        }

        private bool BlockFits()
        {
            foreach (Position pos in CurrentBlock.TilePositions())
            {
                if (!GameGrid.IsEmpty(pos.Row, pos.Column)) return false;
            }
            return true;
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

        private bool IsGameOver()
        {
            //Possible mistake, if first two rows are hidden we should ask
            //about rows #2 and #3, and also why ask about two rows?
            //if we need to stop game when block gets second row above:
            //then-> return !GameGrid.IsEmpty(3);
            //or if we finish when up row is full -> !GameGrid.IsEmpty(0);
            return !GameGrid.IsRowEmpty(0);
        }

        private void PlaceBlock()
        {
            foreach (Position pos in CurrentBlock.TilePositions())
            {
                GameGrid[pos.Row, pos.Column] = CurrentBlock.Id;
            }

            GameGrid.ClearFullRows();

            if (IsGameOver()) GameOver = true;
            else CurrentBlock = BlockQueue.GetAndUpdate();
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
    }
}
