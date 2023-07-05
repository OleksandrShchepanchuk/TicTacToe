using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TicTacToe;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{


    private readonly Dictionary<Player, ObjectAnimationUsingKeyFrames> animations = new()
    {
        { Player.X, new ObjectAnimationUsingKeyFrames() },
        { Player.O, new ObjectAnimationUsingKeyFrames() },
    };

    private readonly Dictionary<Player, ImageSource> _imageSources = new()
    {
        { Player.X, new BitmapImage((new Uri("pack://application:,,,/assets/X15.png"))) },
        { Player.O, new BitmapImage((new Uri("pack://application:,,,/assets/O15.png"))) }
    };

    private readonly DoubleAnimation fadeOutAnimation = new DoubleAnimation()
    {
        Duration = TimeSpan.FromSeconds(.5),
        From = 1,
        To = 0,
    };

    private readonly DoubleAnimation fadeInAnimation = new DoubleAnimation()
    {
        Duration = TimeSpan.FromSeconds(.5),
        From = 0,
        To = 1,
    };

    private  GameState gameState {get; set; }
    private  Image[,] imageControls  {get; set; }



    public MainWindow()
    {
        InitializeComponent();
        gameState = new GameState(5,3);
        imageControls = new Image[5, 5];
        gameState.MoveMade += OnMoveMade;
        gameState.GameEnded += OnGameEnded;
        gameState.GameRestarted += OnGameRestarted;
        StartGame();

    }


    
    private void SetupAnimations()
    {
        animations[Player.X].Duration = TimeSpan.FromSeconds(.25);
        animations[Player.O].Duration = TimeSpan.FromSeconds(.25);
        for (int i = 0; i < 16; i++)
        {
            Uri xUri = new Uri($"pack://application:,,,/assets/X{i}.png");
            BitmapImage xImg = new BitmapImage(xUri);
            DiscreteObjectKeyFrame xKeyFrame = new DiscreteObjectKeyFrame(xImg);
            animations[Player.X].KeyFrames.Add(xKeyFrame);

            Uri oUri = new Uri($"pack://application:,,,/assets/O{i}.png");
            BitmapImage oImg = new BitmapImage(oUri);
            DiscreteObjectKeyFrame oKeyFrame = new DiscreteObjectKeyFrame(oImg);
            animations[Player.O].KeyFrames.Add(oKeyFrame);

        }
    }

    private async Task FadeOut(UIElement uiElement)
    {
        uiElement.BeginAnimation(OpacityProperty, fadeOutAnimation);
        await Task.Delay(fadeOutAnimation.Duration.TimeSpan);
        uiElement.Visibility = Visibility.Hidden;
    }

    private async Task FadeIn(UIElement uiElement)
    {
        uiElement.Visibility = Visibility.Visible;
        uiElement.BeginAnimation(OpacityProperty, fadeInAnimation);
        await Task.Delay(fadeOutAnimation.Duration.TimeSpan);
    }

    private async Task TransitionToEndScreen(string text, ImageSource winnerImage)
    {
        await Task.WhenAll(FadeOut(TurnPannel), FadeOut(GameCanvas));
        TurnPannel.Visibility = Visibility.Hidden;
        GameCanvas.Visibility = Visibility.Hidden;
        ResultText.Text = text;
        WinnerImage.Source = winnerImage;
        await FadeIn(EndScreen);
    }

    private async Task TransitionToGameScreen()
    {
        await (FadeOut(EndScreen));
        await (FadeOut(SettingsScreen));
        Line.Visibility = Visibility.Hidden;
        await Task.WhenAll(FadeIn(TurnPannel), FadeIn(GameCanvas));
    }

    private async Task TransitionToSettingsScreen()
    {
        await (FadeOut(EndScreen));
        Line.Visibility = Visibility.Hidden;
        await Task.WhenAll(FadeIn(SettingsScreen));
    }

    private (Point, Point) FindLinePoints(WinInfo winInfo)
    {
        // if (winInfo.Type == WinType.Row)
        // {
        //     double y = winInfo.Number * squareSize + margin;
        //     return (new Point(0, y), new Point(GameGrid.Width, y));
        // }
        //
        // if (winInfo.Type == WinType.Column)
        // {
        //     double x = winInfo.Number * squareSize + margin;
        //     return (new Point(x, 0), new Point(x, GameGrid.Width));
        // }
        //
        // if (winInfo.Type == WinType.MainDiagonal)
        // {
        //     return (new Point(0, 0), new Point(GameGrid.Width, GameGrid.Width));
        // }
        //
        // return (new Point(GameGrid.Width, 0), new Point(0, GameGrid.Width));
        double squareSize = GameGrid.Width / (double)gameState.GameGrid.GetLength(0);
        double margin = squareSize / 2;
        double xStart = squareSize*winInfo.point1.Item1;
        double xEnd = squareSize*winInfo.point2.Item1;
        double yStart = squareSize* winInfo.point1.Item2;
        double yEnd = squareSize*winInfo.point2.Item2;
        if (winInfo.Type == WinType.Column)
        {
            xStart += margin;
            xEnd += margin;

        }
        if (winInfo.Type == WinType.Row)
        {
            yStart += margin;
            yEnd += margin;
            // yEnd += margin;
        }

        
        return (new Point(xStart,(yStart)), new Point(
            (xEnd), (yEnd)));
    }

    private async Task ShowLine(WinInfo winInfo)
    {
        (Point start, Point end) = FindLinePoints(winInfo);
        Line.X1 = start.X;
        Line.Y1 = start.Y;
        DoubleAnimation x2Animation = new DoubleAnimation()
        {
            Duration = TimeSpan.FromSeconds(.25),
            From = start.X,
            To = end.X,
        };
        DoubleAnimation y2Animation = new DoubleAnimation()
        {
            Duration = TimeSpan.FromSeconds(.25),
            From = start.Y,
            To = end.Y,
        };
        Line.Visibility = Visibility.Visible;
        Line.BeginAnimation(Line.X2Property, x2Animation);
        Line.BeginAnimation(Line.Y2Property, y2Animation);
        await Task.Delay(x2Animation.Duration.TimeSpan);
    }

    private void OnMoveMade(int r, int c)
    {
        Player player = gameState.GameGrid[r, c];
        imageControls[r, c].BeginAnimation(Image.SourceProperty, animations[player]);
        PlayerImage.Source = _imageSources[gameState.CurrentPlayer];
    }

    private async void OnGameEnded(GameResult gameResult)
    {
        await Task.Delay(1000);
        if (gameResult.Winner == Player.None)
        {
            await TransitionToEndScreen("it's a draw!", null);
        }
        else
        {
            await ShowLine(gameResult.WinInfo);
            await Task.Delay(1000);
            await TransitionToEndScreen("Winner: ", _imageSources[gameResult.Winner]);
        }
    }

    private async void OnGameRestarted()
    {
        for (int r = 0; r < gameState.GameGrid.GetLength(0); r++)
        {
            for (int c = 0; c < gameState.GameGrid.GetLength(0); c++)
            {
                imageControls[r, c].BeginAnimation(Image.SourceProperty, null);
                imageControls[r, c].Source = null;
            }
        }

        PlayerImage.Source = _imageSources[gameState.CurrentPlayer];
        await TransitionToSettingsScreen();
    }

    private void SetupGameGrid()
    {
        int size;
        int toWin;
        if (int.TryParse(SizeText.Text, out size)) 
        {
            gameState.GameGrid = new Player[size,size];
        }
        else
        {
            size = 3;
            gameState.GameGrid = new Player[size,size];
        }

        if (int.TryParse(ToWinText.Text, out toWin))
        {
                gameState.ToWin = (toWin);
        }
        else
        {
            toWin = size;
            gameState.ToWin = (toWin);
        }
        GameGrid.Children.Clear();
        imageControls =  new Image[size, size];
        GameGrid.Rows = gameState.GameGrid.GetLength(0);
        GameGrid.Columns = gameState.GameGrid.GetLength(0);
        for (int r = 0; r < gameState.GameGrid.GetLength(0); r++)
        {
            for (int c = 0; c < gameState.GameGrid.GetLength(0); c++)
            {
                Image imageControl = new Image();
                GameGrid.Children.Add((imageControl));
                imageControls[r, c] = imageControl;
            }

        }

        DrawGridLines();
    }
    
    private  void PlayAgain_OnClick(object sender, RoutedEventArgs e)
    {
        if (gameState.GameOver)
        {
            gameState.Reset();
        }


    }

    private void GameGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        double squareSize = GameGrid.Width / gameState.GameGrid.GetLength(0);
        Point clickPosition = e.GetPosition(GameGrid);
        int row = (int)(clickPosition.Y / squareSize);
        int col = (int)(clickPosition.X / squareSize);
        gameState.MakeMove(row, col);
    }

    private async void OnPlayClick(object sender, RoutedEventArgs e)
    {
        SetupGameGrid();
        await TransitionToGameScreen();
    }

    private void StartGame()
    {
        SetupGameGrid();
        SetupAnimations();
    }

    private void DrawGridLines()
    {
        var size = gameState.GameGrid.GetLength(0);

//
// Create a VisualBrush and set it as the background of the UniformGrid
        VisualBrush visualBrush = new VisualBrush();
        visualBrush.TileMode = TileMode.Tile;
        visualBrush.Stretch = Stretch.Uniform;
        visualBrush.Viewport = new Rect(0, 0, 300, 300); // Adjust the size as needed
        visualBrush.ViewportUnits = BrushMappingMode.Absolute;

// Create a DrawingVisual and draw rectangles inside it
        DrawingVisual drawingVisual = new DrawingVisual();
        using (DrawingContext drawingContext = drawingVisual.RenderOpen())
        {
            // for (int row = 0; row < size; row++)
            // {
            //     for (int col = 0; col < size; col++)
            //     {
            //         double x = col * 10; // Adjust the size and positioning as needed
            //         double y = row * 10;
            //         
            //         drawingContext.DrawRectangle(null, new Pen(Brushes.Black, 0.1), new Rect(x, y, 10, 10)); // Adjust the size as needed
            //         
            //
            //     }
            // }
            // for (int row = 0; row < size; row++)
            // {

                for (int col = 1; col < size; col++)
                {
                    double y = (col - 1) * GameGrid.Height/(double)gameState.GameGrid.GetLength(0);
                    double startY = y;
                    double x = col * GameGrid.Height/(double)gameState.GameGrid.GetLength(0);
                    double startX = x;

                    drawingContext.DrawLine(new Pen(Brushes.Black, 1), new Point(startX, 0), new Point(startX, GameGrid.Height));
                    drawingContext.DrawLine(new Pen(Brushes.Black, 1), new Point(0, startX), new Point(GameGrid.Height, startX));
                }
            // }
        }

        visualBrush.Visual = drawingVisual;

// Set the VisualBrush as the background of the UniformGrid
        GameGrid.Background = visualBrush;
    }
}


//gameState.GameGrid.GetLength(0)