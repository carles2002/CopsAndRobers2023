using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
    /* Constantes generales */
    public const int NumTiles = 64;
    public const int TilesPerRow = 8;
    public const int InitialCop0 = 7;
    public const int InitialCop1 = 0;
    public const int InitialRobber = 60;
    public const int Distance = 2;
    public const int MaxRounds = 10;
    public const int MoveSpeed = 2;
    public const int EndSpeed = 4;

    /* Diagrama de estados */
    public const int Init = 0;
    public const int CopSelected = 1;
    public const int TileSelected = 2;
    public const int RobberTurn = 3;
    public const int End = 4;
    public const int Restarting = 5;


}
