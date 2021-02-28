using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the instantiation of the laser beam and passing the origin to the beam object
/// </summary>
public class Laser : MonoBehaviour
{   
    private GameObject hero;
    private GameObject enemy;

    [Header("Set Dynamically")]
    public GameObject weapon;
    public bool isPlayer;
    // Start is called before the first frame update

   

    void Start()
    {       
        if(isPlayer)
        {
            hero = GameObject.FindGameObjectWithTag("Hero");
            transform.Find("Beam").GetComponent<BeamController>().SetOrigin(hero, isPlayer);
        }        
       
    }

    // Set the enemy reference and pass it to the beam
    public void SetEnemy(GameObject e, bool isP)
    {
        enemy = e;
        transform.Find("Beam").GetComponent<BeamController>().SetOrigin(enemy, isP);
        StartCoroutine(DelayedDestroy()); // Destroy after a few seconds

    }

    // Check if it belongs to an enemy
    public bool IsEnemy()
    {
        if(enemy)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    IEnumerator DelayedDestroy()
    {        
        // If Enemy_3 then shot laser for the lifetime - 2 seconds
        if(enemy.transform.GetComponent<Enemy_3>())
        {
            yield return new WaitForSeconds((float)(enemy.transform.GetComponent<Enemy_3>().lifeTime - 2));
        }
        else if(enemy.transform.GetComponent<Enemy_5>()) // If Enemy_5 then shoot for 2 seconds.
        {
            yield return new WaitForSeconds(2);
        }
        
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (Main.S.gameOver)
        {
            Destroy(gameObject); // Destroy if the game is over (fixes the bug when hero died by laser was stil shooting)
        }
        else
        {
            if(isPlayer) // if player
            {
                // Set shooting offset position
                Vector3 pos = transform.position;
                pos.x = hero.transform.position.x;
                pos.y = hero.transform.position.y + 3.0f;
                transform.position = pos;

                if (Input.GetAxis("Jump") != 1) // Shoot while holding 'space' key
                {
                    weapon.GetComponent<Weapon>().laserStarted = false;
                    Destroy(gameObject);
                }
            }
            else // if enemy
            {               
                if(enemy) // if exist, set shooting offset position
                {                           
                    Vector3 pos = transform.position;
                    pos.x = enemy.transform.position.x;
                    pos.y = enemy.transform.position.y - 3.0f;                   
                    transform.position = pos;                   
                }                  

            }
            
        }
        
    }
}
