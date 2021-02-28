using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controls rendering of the laser and detecting raycast collisions
/// </summary>

public class BeamController : MonoBehaviour
{
    private LineRenderer lineRend;
    private GameObject origin;

    public bool isPlayer;
    // Start is called before the first frame update

    

    void Start()
    {
        lineRend = this.gameObject.GetComponent<LineRenderer>(); // get line renderer
        
        
        if(origin) // allow render line if the origin exist
        {
            //Change transform.position based on the axes
            Vector3 pos = transform.parent.gameObject.transform.position;
            pos.x = origin.transform.position.x;

            // Set positiong to the origin + y offset

            if (isPlayer) // Player laser
            {
                pos.y = origin.transform.position.y + 3.0f;
            }
            else // enemy laser
            {
                pos.y = origin.transform.position.y - 3.0f;
            }

            transform.parent.gameObject.transform.position = pos;
        }
        else // disable line renderer
        {
            lineRend.enabled = false;
        }
        

    }

    // Set the origin for the beam
    public void SetOrigin(GameObject o, bool isP)
    {
        origin = o; // origin
        isPlayer = isP; // player or enemy

        // Set position to the origin + y offset
        Vector3 pos = transform.parent.gameObject.transform.position;
        pos.x = origin.transform.position.x;

        if (isPlayer)
        {
            pos.y = origin.transform.position.y + 3.0f;
        }
        else
        {
            pos.y = origin.transform.position.y - 3.0f;
        }

        transform.parent.gameObject.transform.position = pos;

        // Get line renderer and enable it
        lineRend = this.gameObject.GetComponent<LineRenderer>();
        lineRend.enabled = true;
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        if(origin) // if origin exist
        {
            // original beam (fixes bug with incorrect beam rendering)
            lineRend.SetPosition(0, transform.position);
            lineRend.SetPosition(1, transform.position);

            // Check raycast hit and render the beam to a collision. Otherwise render beam to 5000 forward (or down)
            RaycastHit hit;
            AudioController.S.PlayLaserShoot(); // play laser sound

            if (isPlayer)
            {
                if (Physics.Raycast(transform.position, transform.forward, out hit))
                {
                    if (hit.collider.tag == "EnemyPart")
                    {
                        lineRend.SetPosition(1, hit.point);
                        hit.collider.gameObject.transform.parent.gameObject.GetComponent<Enemy>().HitByLaser(); // let enemy know that it's been hit by laser
                    }
                    else if (hit.collider.tag == "ProjectileEnemy")
                    {
                        Destroy(hit.collider.gameObject);
                    }
                    else if(hit.collider.tag == "EnemyExplosionCollision")
                    {
                        hit.collider.GetComponent<MissileExplosion>().Explode();                        
                    }
                    else
                    {
                        lineRend.SetPosition(1, transform.forward * 5000);
                    }
                }
                else lineRend.SetPosition(1, transform.forward * 5000);
            }
            else if (transform.parent.GetComponent<Laser>().IsEnemy())
            {
                if (Physics.Raycast(transform.position, -transform.forward, out hit))
                {
                    if (hit.collider.tag == "EnemyPart")
                    {
                        lineRend.SetPosition(1, hit.point);
                    }
                    else if (hit.collider.tag == "HeroPart" || hit.collider.tag == "Hero")
                    {
                        lineRend.SetPosition(1, hit.point);
                        hit.collider.gameObject.transform.parent.gameObject.GetComponent<Hero>().HitByLaser(); // let hero know that it's been hit by laser
                    }
                    else if(hit.collider.tag == "ProjectileHero")
                    {
                        Destroy(hit.collider.gameObject);
                    }
                    else if (hit.collider.tag == "ExplosionCollision")
                    {
                        hit.collider.GetComponent<MissileExplosion>().Explode();
                    }
                    else
                    {
                        lineRend.SetPosition(1, -transform.forward * 5000);
                    }
                }
                else lineRend.SetPosition(1, -transform.forward * 5000);
            }
        }               
        
    }
}
