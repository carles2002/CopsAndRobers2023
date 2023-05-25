using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    Dictionary<Tile, List<int>> robberAdjDistances = new Dictionary<Tile, List<int>>();
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;

    void Start()
    {
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }

    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();
            }
        }

        cops[0].GetComponent<CopMove>().currentTile = Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile = Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile = Constants.InitialRobber;
    }

    public void InitAdjacencyLists()
    {
        // Matriz de adyacencia
        int[,] matrix = new int[Constants.NumTiles, Constants.NumTiles];

        
        // Establecer a 1 todas las celdas adyacentes para cada celda individual
       
        for (int i = 0; i < Constants.NumTiles; ++i)
        {
            // Arriba
            if (i > 7) { matrix[i, i - 8] = 1; }

            // Abajo
            if (i < 56) { matrix[i, i + 8] = 1; }

            // Derecha
            if (((i + 1) % 8) != 0) { matrix[i, i + 1] = 1; }

            // Izquierda
            if (i % 8 != 0) { matrix[i, i - 1] = 1; }
        }

        // Rellenar la lista de adyacencia 
        for (int i = 0; i < Constants.NumTiles; ++i)
        {
            for (int j = 0; j < Constants.NumTiles; ++j)
            {
                if (matrix[i, j] == 1)
                {
                    tiles[i].adjacency.Add(j);
                }
            }
        }
    }

    // Reseteamos cada casilla
    public void ResetTiles()
    {
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;
                break;
        }
    }

    public void ClickOnTile(int t)
    {
        clickedTile = t;

        switch (state)
        {
            case Constants.CopSelected:
                if (tiles[clickedTile].selectable)
                {
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile = tiles[clickedTile].numTile;

                    tiles[clickedTile].current = true;
                    state = Constants.TileSelected;
                }
                break;

            case Constants.TileSelected:
                state = Constants.Init;
                break;

            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {
            case Constants.TileSelected:
                ResetTiles();
                state = Constants.RobberTurn;
                RobberTurn();
                break;

            case Constants.RobberTurn:
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }
    }

    public void RobberTurn()
    {
        RobberMove robberMove = robber.GetComponent<RobberMove>();
        clickedTile = robberMove.currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        // Añadir las casillas seleccionables 
        robberAdjDistances.Clear();
        foreach (Tile t in tiles)
        {
            if (t.selectable)
            {
                robberAdjDistances.Add(t, new List<int>());
            }
        }

        // Calcular la distancia a cada cop
        for (int i = 0; i < cops.Count(); ++i)
        {
            clickedCop = i;
            clickedTile = cops[i].GetComponent<CopMove>().currentTile;
            tiles[clickedTile].current = true;

            // Actualizar después de cada cop 
            ResetTiles();
            FindSelectableTiles(true);
        }

        int maxDistance = 0;
        Tile destinyTile = new Tile();

        foreach (Tile t in robberAdjDistances.Keys)
        {
            // Elegir la que está más lejos de todas
            if (robberAdjDistances[t].Sum() > maxDistance)
            {
                destinyTile = t;
                maxDistance = robberAdjDistances[t].Sum();
            }

            // De lo contrario, elegir la que tiene los números de distancia más grandes
            else if (robberAdjDistances[t].Sum() == maxDistance)
            {
                bool isFurther = true;
                foreach (int d in robberAdjDistances[t])
                {
                    if (d < robberAdjDistances[destinyTile][0]
                    && d < robberAdjDistances[destinyTile][1])
                    {
                        isFurther = false;
                    }
                }

                if (isFurther) { destinyTile = t; }
            }
        }

        ResetTiles();
        robberMove.currentTile = destinyTile.numTile;
        robberMove.MoveToTile(destinyTile);
    }

    public void EndGame(bool endCode)
    {
        finalMessage.text = endCode ? "¡Ganaste!" : "¡Perdiste!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);

        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rondas: ";

        state = Constants.Restarting;
    }

    public void InitGame() => state = Constants.Init;

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rondas: " + roundCount;
    }

    public void FindSelectableTiles(bool isCop)
    {
        int currentTileIndex = GetCurrentTileIndex(isCop);

        // Las celdas actual y destino seleccionadas se pintan de rosa
        tiles[currentTileIndex].current = true;
        tiles[currentTileIndex].visited = true;

        // Casillas con otros policías en ellas
        List<int> copTileIndices = ObtainCopIndex();

        DisableAllTiles();

        BFS(currentTileIndex, copTileIndices);

        // Filtrar por casillas seleccionables (sin policía y alcanzables en 2 movimientos)
        foreach (Tile t in tiles)
        {
            if (!copTileIndices.Contains(t.numTile)
            && t.distance <= Constants.Distance && t.distance > 0)
            {
                t.selectable = true;
            }

            if (isCop && robberAdjDistances.ContainsKey(t) && robberAdjDistances.Count > 0)
            {
                robberAdjDistances[t].Add(t.distance);
            }
        }
    }

    private int GetCurrentTileIndex(bool isCop)
    {
        int index = isCop
                        ? cops[clickedCop].GetComponent<CopMove>().currentTile
                        : robber.GetComponent<RobberMove>().currentTile;

        return index;
    }

    private List<int> ObtainCopIndex()
    {
        List<int> indices = new List<int>();
        foreach (GameObject c in cops)
        {
            indices.Add(c.GetComponent<CopMove>().currentTile);
        }

        return indices;
    }

    private void DisableAllTiles()
    {
        foreach (Tile t in tiles)
        {
            t.selectable = false;
        };
    }

    private void BFS(int currIndex, List<int> copIndices)
    {
        Queue<Tile> nodes = new Queue<Tile>();

        foreach (int i in tiles[currIndex].adjacency)
        {
            tiles[i].parent = tiles[currIndex];  // Casilla raíz
            nodes.Enqueue(tiles[i]);
        }

        while (nodes.Count > 0)
        {
            Tile curr = nodes.Dequeue();
            if (!curr.visited)
            {
                if (copIndices.Contains(curr.numTile))
                {
                    //curr.distance = Constants.Distance + 1;
                    curr.distance = curr.parent.distance + 1;
                    curr.visited = true;
                }
                else
                {
                    foreach (int i in curr.adjacency)
                    {
                        if (!tiles[i].visited)
                        {
                            tiles[i].parent = curr;
                            nodes.Enqueue(tiles[i]);
                        }
                    }

                    curr.visited = true;
                    curr.distance = curr.parent.distance + 1;
                }

            }
        }
    }
}
