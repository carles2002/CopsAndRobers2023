using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public int id;
    public int currentTile;

    protected Stack<Tile> path = new Stack<Tile>();
    
    //Variables para el movimiento
    protected bool moving = false;
    protected Vector3 heading = new Vector3();
    protected float moveSpeed = Constants.MoveSpeed;
    protected Vector3 velocity = new Vector3();    
            
    public void MoveToTile(Tile tile)
    {
        path.Clear();
        moving = true;

        Tile next = tile;
        while (next != null)
        {            
            path.Push(next);
            next = next.parent;
        }
    }

    public void DoMove()
    {
        Tile t = path.Peek();
        Vector3 target = t.transform.position;

        //Mantenemos la altura
        target.y = 0.3f;

        //Comprobamos si aún no hemos llegado al centro de la casilla
        if ((Mathf.Abs(transform.position.x - t.transform.position.x) >= 0.05f) || (Mathf.Abs(transform.position.z - t.transform.position.z) >= 0.05f))
        {
            CalculateHeading(target);
            SetVelocity();

            transform.forward = heading;
            transform.position += velocity * Time.deltaTime;
        }
        else
        {   //Hemos llegado al centro de la casilla         
            transform.position = target;            
            path.Pop();                    

        }
    }      

    protected void CalculateHeading(Vector3 target)
    {
        heading = target - transform.position;
        heading.Normalize();
    }

    protected void SetVelocity()
    {
        velocity = heading * moveSpeed;
    }


}
