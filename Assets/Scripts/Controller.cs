using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board; // Tablero del juego
    public GameObject[] cops = new GameObject[2]; // Array de policías
    public GameObject robber; // Ladrón
    public Text rounds; // Texto para mostrar las rondas
    public Text finalMessage; // Mensaje final (ganar o perder)
    public Button playAgainButton; // Botón para jugar de nuevo

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles]; // Array de casillas
    Dictionary<Tile, List<int>> robberAdjDistances = new Dictionary<Tile, List<int>>(); // Diccionario para guardar las distancias del ladrón a los policías
    private int roundCount = 0; // Contador de rondas
    private int state; // Estado del juego
    private int clickedTile = -1; // Casilla clicada
    private int clickedCop = 0; // Policía clicado

    public int mode = 0;

    // Método que se ejecuta al inicio del juego
    void Start()
    {
        InitTiles(); // Inicializar las casillas
        InitAdjacencyLists(); // Inicializar las listas de adyacencia
        state = Constants.Init; // Estado inicial
    }

    // Método para inicializar las casillas y posicionar las fichas
    void InitTiles()
    {
        // Recorremos todas las filas y columnas del tablero
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>(); // Asignamos la casilla correspondiente
            }
        }

        // Posicionamos las fichas en las casillas iniciales
        cops[0].GetComponent<CopMove>().currentTile = Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile = Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile = Constants.InitialRobber;
    }

    // Método para inicializar las listas de adyacencia
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

        // Rellenar la lista de adyacencia para cada celda con los índices de sus vecinos adyacentes
        for (int i = 0; i < Constants.NumTiles; ++i )       {
            for (int j = 0; j < Constants.NumTiles; ++j)
            {
                if (matrix[i, j] == 1)
                {
                    tiles[i].adjacency.Add(j);
                }
            }
        }
    }

    // Método para resetear cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    // Método que se ejecuta al hacer clic en un policía
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

    // Método que se ejecuta al hacer clic en una casilla
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

    // Método para finalizar el turno
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

    // Método para el turno del ladrón
    public void RobberTurn()
    {
        RobberMove robberMove = robber.GetComponent<RobberMove>();
        clickedTile = robberMove.currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        // Añadir las casillas seleccionables al diccionario
        robberAdjDistances.Clear();
        foreach (Tile t in tiles)
        {
            if (t.selectable)
            {
                robberAdjDistances.Add(t, new List<int>());
            }
        }

        // Calcular la distancia a cada policía
        for (int i = 0; i < cops.Count(); ++i)
        {
            clickedCop = i;
            clickedTile = cops[i].GetComponent<CopMove>().currentTile;
            tiles[clickedTile].current = true;

            // Actualizar después de cada policía
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

    // Método para terminar el juego
    public void EndGame(bool endCode)
    {
        finalMessage.text = endCode ? "¡Ganaste!" : "¡Perdiste!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    // Método para jugar de nuevo
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

    // Método para iniciar el juego
    public void InitGame() => state = Constants.Init;

    // Método para incrementar el contador de rondas
    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rondas: " + roundCount;
    }

    // Método para encontrar las casillas seleccionables
    public void FindSelectableTiles(bool isCop)
    {
        int currentTileIndex = GetCurrentTileIndex(isCop);

        // Las celdas actual y destino seleccionadas se pintan de rosa
        tiles[currentTileIndex].current = true;
        tiles[currentTileIndex].visited = true;

        // Casillas con otros policías en ellas
        List<int> copTileIndices = ObtainCopIndex();

        DisableAllTiles();

        if (mode== 0) {
            Debug.Log("BFS");
            BFS(currentTileIndex, copTileIndices);
        }
        else if(mode== 1){
            Debug.Log("DFS");
            DFS(currentTileIndex, copTileIndices);
        }
        else
        {
            Debug.Log("Error en la seleción del Algoritmo");
        }

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

    // Método para obtener el índice de la casilla actual
    private int GetCurrentTileIndex(bool isCop)
    {
        int index = isCop
                        ? cops[clickedCop].GetComponent<CopMove>().currentTile
                        : robber.GetComponent<RobberMove>().currentTile;

        return index;
    }

    // Método para obtener los índices de las casillas con policías
    private List<int> ObtainCopIndex()
    {
        List<int> indices = new List<int>();
        foreach (GameObject c in cops)
        {
            indices.Add(c.GetComponent<CopMove>().currentTile);
        }

        return indices;
    }

    // Método para desactivar todas las casillas
    private void DisableAllTiles()
    {
        foreach (Tile t in tiles)
        {
            t.selectable = false;
        };
    }

    // Método para realizar una búsqueda en anchura 
    private void BFS(int currIndex, List<int> copIndices)
    {
        
        Queue<Tile> nodes = new Queue<Tile>();

        // Añadimos a la cola todos los nodos adyacentes a la casilla actual
        foreach (int i in tiles[currIndex].adjacency)
        {
            tiles[i].parent = tiles[currIndex];  // Casilla raíz
            nodes.Enqueue(tiles[i]);
        }

        // Mientras haya nodos en la cola
        while (nodes.Count > 0)
        {
            Tile curr = nodes.Dequeue();
            if (!curr.visited)
            {
                if (copIndices.Contains(curr.numTile))
                {
                    // Si la casilla actual contiene un cop, incrementamos su distancia
                    curr.distance = curr.parent.distance + 1;
                    curr.visited = true;
                }

                else
                {
                    // Si la casilla actual no contiene un policía, añadimos a la cola todos sus nodos adyacentes
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
    private void DFS(int currIndex, List<int> copIndices)
    {
        
        Queue<Tile> nodes = new Queue<Tile>();

        // Añadimos a la cola todos los nodos adyacentes a la casilla actual
        foreach (int i in tiles[currIndex].adjacency)
        {
            tiles[i].parent = tiles[currIndex];  // Casilla raíz
            nodes.Enqueue(tiles[i]);
        }

        // Mientras haya nodos en la cola
        while (nodes.Count > 0)
        {
            Tile curr = nodes.Dequeue();
            if (!curr.visited)
            {
                if (copIndices.Contains(curr.numTile))
                {
                    // Si la casilla actual contiene un cop, incrementamos su distancia
                    curr.distance = curr.parent.distance + 1;
                    curr.visited = true;
                }

                else
                {
                    // Si la casilla actual no contiene un policía, añadimos a la cola todos sus nodos adyacentes
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
    public void selectBFS()
    {
        Debug.Log("BFS SELECTED");
        mode = 0;
    }
    public void selectDFS()
    {
        Debug.Log("DFS SELECTED");
        mode = 1;
    }


}
