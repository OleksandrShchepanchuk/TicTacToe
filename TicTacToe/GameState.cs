using System;
using System.Drawing;

namespace TicTacToe;

public class GameState
{
    public Player[,] GameGrid { get;  set; }
    public Player CurrentPlayer { get; private set; }
    public int TurnsPassed { get; private set; }
    public bool GameOver { get; private set; }
    public int ToWin { get; set; }

    
    public event Action<int, int> MoveMade;
    public event Action<GameResult> GameEnded;
    public event Action GameRestarted;

    public GameState()
    {
        GameGrid = new Player[3, 3];
        CurrentPlayer = Player.X;
        TurnsPassed = 0;
        GameOver = false;
    }
    public GameState(int size, int m)
    {
        GameGrid = new Player[size, size];
        CurrentPlayer = Player.X;
        TurnsPassed = 0;
        GameOver = false;
        ToWin = m;
    }

    private bool CanMakeMove(int r, int c)
    {
        return !GameOver && GameGrid[r, c] == Player.None;
    }

    private bool IsGridFull()
    {
        return TurnsPassed == GameGrid.GetLength(0)*GameGrid.GetLength(0);
    }

    private void SwitchPlayer()
    {
        CurrentPlayer = CurrentPlayer == Player.X ? Player.O : Player.X;
    }

    private bool AreSquaresMarked((int, int)[] squares, Player player)
    {
        foreach ((int r, int c) in squares)
        {
            if (GameGrid[r, c] != player)
            {
                return false;
            }
        }

        return true;
    }

    private bool DidMoveWin(int r, int c, out WinInfo winInfo)
    {

        var Size = GameGrid.GetLength(0);
        for (int i = 0; i < Size; i++)
            {
                var count = 1;
                for (int j = 0; j < Size-1; j++)
                {
                    if (GameGrid[i, j] == GameGrid[i, j + 1] && GameGrid[i,j] != Player.None)
                    {
                        count++;

                        if (count == ToWin)
                        {
                            winInfo = new WinInfo { point1 = (j-ToWin+2, i), point2 = (j+2, i), Type = WinType.Row };
                            return true;
                        }
                    }
                    else
                    {
                        count = 1;
                    }
                }
            }
            for (int j = 0; j < Size; j++)
            {
                var count = 1;
                for (int i = 0; i < Size-1; i++)
                {
                    if (GameGrid[i, j] == GameGrid[i+1, j] && GameGrid[i,j] != Player.None)
                    {
                        count++;
                        if (count == ToWin)
                        {
                            winInfo = new WinInfo() { point1 = (j,i -ToWin + 2), point2 = (j,i+2),Type = WinType.Column};
                            return true;
                        }
                    }
                    else
                    {
                        count = 1;
                    }
                }
            }

            var count1 = 1;
            for (int i = 0; i < Size-1; i++)
            {
                {
                    if (GameGrid[i, i] == GameGrid[i+1, i + 1] && GameGrid[i,i] != Player.None)
                    {
                        count1++;
                        if (count1 == ToWin)
                        {
                            winInfo = new WinInfo() { point1 = (i - ToWin + 2, i - ToWin + 2 ), point2 = (i + 2, i + 2),Type = WinType.MainDiagonal };
                            return true;
                        }
                    }
                    else
                    {
                        count1 = 1;
                    }
                }
            }
            // antidiagonal
            count1 = 1;
            for (int i = 0; i < Size-1; i++)
            {
                {
                    if (GameGrid[i, Size - i - 1] == GameGrid[i+1, Size - i - 1 - 1] && GameGrid[i, Size - i - 1] != Player.None)
                    { 
                        count1++;
                        
                        if (count1 == ToWin)
                        {
                            winInfo = new WinInfo() { point1 = (  Size - i+ToWin-2,i-ToWin+2), point2 = (  Size - i-2,i+2),Type = WinType.AntiDiagonal };
                            return true;
                        }
                    }
                    else
                    {
                        count1 = 1;
                    }
                }
            }
            //
            
            // check diagonals
            for (int i = 0; i <= Size - ToWin; i++) // iterate over rows that can form diagonal of length m
            {
                for (int j = 0; j <= Size - ToWin; j++) // iterate over columns that can form diagonal of length m
                {
                    count1 = 1;
                    for (int k = 1; k < ToWin; k++)
                    {
                        if (GameGrid[i, j] == GameGrid[i+k, j+k] && GameGrid[i,j] != Player.None)
                        {
                            count1++;
                            if (count1 == ToWin)
                            {
                                winInfo = new WinInfo() { point1 = (j-ToWin+1+k, i-ToWin+1+k), point2 = (j+k+1,i+k+1),Type = WinType.MainDiagonal};
                                return true;
                            }
                        }
                        else
                        {
                            count1 = 1;
                        }
                    }
                    count1 = 1;
                    for (int k = 1; k < ToWin; k++)
                    {
                        if (GameGrid[i, j+ToWin-1] == GameGrid[i+k, j+ToWin-1-k] && GameGrid[i,j+ToWin-1] != Player.None)
                        {
                            count1++;
                            if (count1 == ToWin)
                            {
                                winInfo = new WinInfo() { point1 = (j+ToWin, i), point2 = (j+ToWin-k-1,i+k+1),Type = WinType.AntiDiagonal };
                                // winInfo = new WinInfo() { point1 = (Size - i+ToWin-2,i-ToWin+2), point2 = (  Size - i-2,i+2),Type = WinType.AntiDiagonal };

                                return true;
                            }
                        }
                        else
                        {
                            count1 = 1;
                        }
                    }
                    
                }
            }

            winInfo = null;
            return false;
    }

    private bool DidMoveEndGame(int r, int c, out GameResult gameResult)
    {
        if (DidMoveWin(r, c, out WinInfo winInfo))
        {
            gameResult = new GameResult { Winner = CurrentPlayer, WinInfo = winInfo };
            return true;
            
        }

        if (IsGridFull())
        {
            gameResult = new GameResult { Winner = Player.None };
            return true;
        }

        gameResult = null;
        return false;
    }

    public void MakeMove(int r, int c)
    {
        if (!CanMakeMove(r, c))
        {
            return;
        }

        GameGrid[r, c] = CurrentPlayer;
        TurnsPassed++;
        if (DidMoveEndGame(r, c, out GameResult gameResult))
        {
            GameOver = true;
            MoveMade?.Invoke(r, c);
            GameEnded?.Invoke(gameResult);
        }
        else
        {
            SwitchPlayer();
            MoveMade?.Invoke(r,c);
        }

    }
    public void Reset()
        {
            GameGrid = new Player[GameGrid.GetLength(0), GameGrid.GetLength(0)];
            CurrentPlayer = Player.X;
            TurnsPassed = 0;
            GameOver = false;
            GameRestarted?.Invoke();
        }
}