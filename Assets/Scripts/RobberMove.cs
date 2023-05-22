using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobberMove : Movement
{

    public GameObject controller;

    public bool finishing = false;
    public bool restarting = false;
         
    private float x_finish = 0;
    private float z_finish = 0;
              
    public void Restart(Tile t)
    {
        currentTile = t.numTile;
        MoveToTile(t);
        finishing = false;
        moveSpeed = Constants.MoveSpeed;
        restarting = true;        
    }
                
    void Update()
    {
        if(moving)
        {            
            Move();  
        }
        if (finishing)
        {
            Finish();
        }
    }            

    public void Move()
    {
        if(path.Count > 0)
        {
            DoMove();
        }
        else//Ya hemos llegado a la casilla destino
        {
            moving = false;            
            controller.GetComponent<Controller>().FinishTurn();
           
            //Si estamos reseteando, el caco es posible que haya quedado de lado con el choque. Lo ponemos recto
            if (restarting)
            {                
                transform.rotation = Quaternion.identity;
                restarting = false;
                controller.GetComponent<Controller>().InitGame();
            }

        }
    }

    //Movimiento especial hacia una posición fuera del tablero
    public void Finish()
    {               
        Vector3 target = new Vector3(x_finish, 2.5f, z_finish);

        if ((Mathf.Abs(transform.position.x - target.x) >= 0.05f) || (Mathf.Abs(transform.position.z - target.z) >= 0.05f))
            {
                CalculateHeading(target);
                SetVelocity();

                transform.forward = heading;
                transform.position += velocity * Time.deltaTime*2;
            }
            else
            {                
                transform.position = target;
                Physics.gravity = new Vector3(0, -1f, 0);
                finishing = false;
                moving = false;            
            }                
    }

    //Recibimos colisión. Nos han capturado
    private void OnCollisionEnter(Collision collision)
    { 
        if (collision.gameObject.tag == "Cop")
        {
            finishing = true;
            moveSpeed = Constants.EndSpeed;
                       
            float vertical = collision.gameObject.GetComponent<CopMove>().transform.position.x - transform.position.x;
            float horizontal = collision.gameObject.GetComponent<CopMove>().transform.position.z - transform.position.z;

            //Poli y caco están en la misma vertical. Le pega por arriba o por abajo
            if(Mathf.Abs(vertical)< Mathf.Abs(horizontal))
            {
                if (horizontal > 0)
                {                   
                    x_finish = transform.position.x;
                    z_finish = transform.position.z - 15f;
                }
                else
                {                    
                    x_finish = transform.position.x;
                    z_finish = transform.position.z + 15f;
                }
            }
            else//Poli y caco están en la misma horizontal. Le pega por la izquierda o por la derecha
            {
                if (vertical > 0)
                {                    
                    x_finish = transform.position.x - 15f;
                    z_finish = transform.position.z;
                }
                else
                {                    
                    x_finish = transform.position.x + 15f;
                    z_finish = transform.position.z;
                }
            }
            
            controller.GetComponent<Controller>().EndGame(true);                       
        }
    }

   
}
