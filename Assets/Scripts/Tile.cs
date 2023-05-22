using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameObject controller;
    public int numTile;
    public List<int> adjacency = new List<int>();

    public bool current = false;
    public bool selectable = false;
            
    //Variables para el BFS
    public bool visited = false;
    public Tile parent = null;//Casilla a través de la que llegamos
    public int distance = 0;//Distancia del original
        
    private void Awake()
    {     
        Vector3 pos = transform.position; 
        int fila = (int)(pos.z + 3.5);
        int columna = (int)(pos.x + 3.5);
        this.numTile = fila * 8 + columna;
    }

    //Coloreamos cada casilla
    private void Update()
    {
        if (current)
        {
            GetComponent<Renderer>().material.color = Color.magenta;
        }
        else if (selectable)
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }

    //Hacemos clic en casilla
    private void OnMouseDown()
    {
        controller.GetComponent<Controller>().ClickOnTile(numTile);
    }
        
    public void Reset()
    {
        current = false;
        selectable = false;
   
        visited = false;
        parent = null;
        distance = 0;
    }
 
}
