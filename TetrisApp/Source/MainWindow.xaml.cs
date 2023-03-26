using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using TetrisApp.Controls;
using TetrisApp.Blocks;
using TetrisApp.GamePlay;
using TetrisApp.Grid;

namespace TetrisApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ImageSource[] tileImages = new ImageSource[]
        {
            new BitmapImage(new Uri("../Assets/TileEmpty.png", UriKind.Relative)),
            new BitmapImage(new Uri("../Assets/TileCyan.png", UriKind.Relative)),
            new BitmapImage(new Uri("../Assets/TileBlue.png", UriKind.Relative)),
            new BitmapImage(new Uri("../Assets/TileOrange.png", UriKind.Relative)),
            new BitmapImage(new Uri("../Assets/TileYellow.png", UriKind.Relative)),
            new BitmapImage(new Uri("../Assets/TileGreen.png", UriKind.Relative)),
            new BitmapImage(new Uri("../Assets/TilePurple.png", UriKind.Relative)),
            new BitmapImage(new Uri("../Assets/TileRed.png", UriKind.Relative))
        };

        private readonly ImageSource[] blockImages = new ImageSource[]
        {
            new BitmapImage(new Uri("../Assets/Block-Empty.png", UriKind.Relative)),
            new BitmapImage(new Uri("../Assets/Block-I.png", UriKind.Relative)),
            new BitmapImage(new Uri("../Assets/Block-J.png", UriKind.Relative)),
            new BitmapImage(new Uri("../Assets/Block-L.png", UriKind.Relative)),
            new BitmapImage(new Uri("../Assets/Block-O.png", UriKind.Relative)),
            new BitmapImage(new Uri("../Assets/Block-S.png", UriKind.Relative)),
            new BitmapImage(new Uri("../Assets/Block-T.png", UriKind.Relative)),
            new BitmapImage(new Uri("../Assets/Block-Z.png", UriKind.Relative))
        };

        private readonly Image[,] imageControls;

        private GameState gameState = new GameState();

        //public ICommand PlayAgainCommand { get; }

        private readonly int delayMax = 900;

        private readonly int delayMin = 50;

        private readonly int delayStep = 12;

        public int DelayCurrent { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            imageControls = SetupGameCanvas(gameState.GameGrid);
            //
            //PlayAgainCommand = new PlayAgainCommand();
            //DataContext = this;
            //Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private Image[,] SetupGameCanvas(GameGrid grid)
        {
            Image[,] imageControls = new Image[grid.Rows, grid.Columns];
            int cellSize = 25;
            for (int row = 0; row < grid.Rows; row++)
            {
                for (int col = 0; col < grid.Columns; col++)
                {
                    Image imageControl = new Image { Width = cellSize, Height = cellSize };
                    Canvas.SetTop(imageControl, (row - 2) * cellSize);
                    Canvas.SetLeft(imageControl, col * cellSize); // why not SetRight???
                    GameCanvas.Children.Add(imageControl);
                    imageControls[row, col] = imageControl;
                }
            }

            return imageControls;
        }

        private void DrawGrid(GameGrid grid)
        {
            for (int row = 0; row < grid.Rows; row++)
            {
                for (int col = 0; col < grid.Columns; col++)
                {
                    int id = grid[row, col];
                    imageControls[row, col].Opacity = 1;
                    imageControls[row, col].Source = tileImages[id];
                }
            }
        }

        private void DrawBlock(Blocks.Block block)
        {
            foreach (Position pos in block.TilePositions())
            {
                imageControls[pos.Row, pos.Column].Opacity = 1;
                imageControls[pos.Row, pos.Column].Source = tileImages[block.Id];
            }
        }

        private void DrawNextBlock(BlockQueue blockQueue)
        {
            Blocks.Block next = blockQueue.NextBlock;
            NextImage.Source = blockImages[next.Id];
        }

        private void DrawHeldBlock(Blocks.Block heldBlock)
        {
            if (heldBlock == null)
                HoldImage.Source = blockImages[0];
            else
                HoldImage.Source = blockImages[heldBlock.Id];
        }

        private void DrawGhostBlock(Blocks.Block block)
        {
            int dropDistance = gameState.BlockDropDistance();

            foreach (Position pos in block.TilePositions())
            {
                imageControls[pos.Row + dropDistance, pos.Column].Opacity = 0.25;
                imageControls[pos.Row + dropDistance, pos.Column].Source = tileImages[block.Id];
            }
        }

        private void Draw(GameState gamestate)
        {
            DrawGrid(gamestate.GameGrid);
            DrawGhostBlock(gameState.CurrentBlock); // Draw ghost before current block, or ghost will appear above current block
            DrawBlock(gamestate.CurrentBlock);
            DrawNextBlock(gamestate.BlockQueue);
            DrawHeldBlock(gameState.HeldBlock);
            ScoreText.Text = $"Score: {gameState.Score}";
        }

        private async Task GameLoop()
        {
            Draw(gameState);

            while (!gameState.GameOver && !gameState.GameRestarted)
            {
                DelayCurrent = Math.Max(delayMin, delayMax - (gameState.Score * delayStep));
                await Task.Delay(DelayCurrent);

                if (gameState.GamePause)
                {
                    await Task.Delay(1); // sleep for 1 millisecond
                }
                else
                {
                    gameState.MoveBlockDown();
                    if (gameState.RowCleared)
                    {
                        PlaySound("D:\\CSHARP\\Projects\\WPF_Projects\\Tetris\\TetrisApp\\Assets\\Sounds\\success.wav");
                        await Task.Delay(1000); // sleep for 1 second
                        gameState.RowCleared = false;
                    }
                    Draw(gameState);
                }
            }
            if (gameState.GameOver)
            {
                PlaySound("D:\\CSHARP\\Projects\\WPF_Projects\\Tetris\\TetrisApp\\Assets\\Sounds\\loose.wav");
                GameOverMenu.Visibility = Visibility.Visible;
                FinalScoreText.Text = $"Your final score is: {gameState.Score}";
            }
            else
            {
                gameState.GameRestarted = false;
            }
        }

        private async void Window_KeyDown(object sender, KeyEventArgs events)
        {
            //if (gameState.GameOver) return;
            switch (events.Key)
            {
                case Key.Left:
                    gameState.MoveBlockLeft();
                    break;
                case Key.Right:
                    gameState.MoveBlockRight();
                    break;
                case Key.Down:
                    gameState.MoveBlockDown();
                    break;
                case Key.D:
                    gameState.RotateBlockCW();
                    break;
                case Key.A:
                    gameState.RotateBlockCCW();
                    break;
                case Key.S:
                    gameState.HoldBlock();
                    break;
                case Key.Space:
                    gameState.DropBlock();
                    PlaySound("D:\\CSHARP\\Projects\\WPF_Projects\\Tetris\\TetrisApp\\Assets\\Sounds\\throw.wav");
                    break;
                case Key.Escape:
                    if (gameState.GameStarted)
                    {
                        GamePauseMenu.Visibility = Visibility.Visible;
                        gameState.GamePause = true;
                    }
                    else Application.Current.Shutdown();
                    break;
                case Key.Return:
                    if (gameState.GameOver)
                    {
                        gameState = new GameState();
                        GameOverMenu.Visibility = Visibility.Hidden;
                        await GameLoop();
                    }
                    else if (!gameState.GameStarted)
                    {
                        gameState = new GameState
                        {
                            GameStarted = true
                        };
                        GameStartMenu.Visibility = Visibility.Hidden;
                        await GameLoop();
                    }
                    break;
                default:
                    return;
            }

            Draw(gameState);
        }

        //private async void GameCanvas_Loaded(object sender, RoutedEventArgs events)
        //{
        //await GameLoop();
        //Draw(gameState);
        //}

        private async void StartGame_Click(object sender, RoutedEventArgs events)
        {
            gameState = new GameState();
            gameState.GameStarted = true;
            GameStartMenu.Visibility = Visibility.Hidden;
            await GameLoop();
        }

        private async void Restart_Click(object sender, RoutedEventArgs events)
        {
            gameState = new GameState();
            gameState.GameStarted = true;
            gameState.GameRestarted = true; //*
            GamePauseMenu.Visibility = Visibility.Hidden;
            GameOverMenu.Visibility = Visibility.Hidden;
            DelayCurrent = 0;
            await GameLoop();
        }

        private async void PlayAgain_Click(object sender, RoutedEventArgs events)
        {
            gameState = new GameState();
            gameState.GameStarted = true;
            GamePauseMenu.Visibility = Visibility.Hidden;
            GameOverMenu.Visibility = Visibility.Hidden;
            DelayCurrent = 0;
            await GameLoop();
        }

        private void UnPauseGame_Click(object sender, RoutedEventArgs events)
        {
            gameState.GamePause = false;
            GamePauseMenu.Visibility = Visibility.Hidden;
            //await GameLoop();
        }

        private void QuitGame_Click(object sender, RoutedEventArgs events)
        {
            Application.Current.Shutdown();
        }

        public static void PlaySound(System.String path)
        {
            //var uri = new Uri(path, UriKind.Absolute);
            var player = new MediaPlayer();
            player.Open(new Uri(path, UriKind.Absolute));
            player.Play();
        }
    }
}
