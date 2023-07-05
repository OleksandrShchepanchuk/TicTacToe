using System.Drawing;

namespace TicTacToe;

public class WinInfo
{
    public WinType Type { get; set; }
    public (int, int) point1 { get; set; }
    public (int, int) point2 { get; set; }

    public int Number { get; set; }
}