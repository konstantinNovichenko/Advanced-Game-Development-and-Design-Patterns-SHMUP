using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the rendering, movement, and target lock of the missile
/// </summary>
public class Missile : MonoBehaviour
{
    private Transform target;
    private Rigidbody2D rb;
    public GameObject explosionCollPrefab;
    public float speed = 20.0f; // movement speed
    public float rotationSpeed = 300f; // angular speed
    public bool isPlayer;
    private bool targetLocked = false;
    private GameObject explosionCollisionObject; // collision detection object
    // Start is called before the first frame update
    void Start()
    {
        // Instantiate a Collision detection object (Detects a collision of 3D colliders. Needed because missile object uses a 2D Rigidbody and can't track collisions with a 3D rigidbody)
        explosionCollisionObject = Instantiate<GameObject>(explosionCollPrefab);
        explosionCollisionObject.GetComponent<MissileExplosion>().SetExplosion(isPlayer);
        explosionCollisionObject.GetComponent<MissileExplosion>().parent = gameObject;

        rb = GetComponent<Rigidbody2D>();   // get rigidbody

        if(isPlayer && GameObject.FindGameObjectsWithTag("Enemy").Length != 0) // If belongs to player, choose the closest enemy as a target
        {
            GetClosestTarget();           
        }  
        else if(!isPlayer && GameObject.FindGameObjectsWithTag("Hero").Length != 0) // If belongs to enemy, choose hero as a target
        {
            TargetHero();
        }
    }

    // Fixed Update
    void FixedUpdate()
    {
        AudioController.S.PlayMissileShoot(); // Play missile movement sound

        if (!targetLocked) // If not target found, look for a target
        {
            if(isPlayer)
            {
                GetClosestTarget(); // get a target
                rb.velocity = transform.up * speed; // move up while looking for a target
            }
            else
            {
                TargetHero(); // target the Hero
                rb.velocity = -transform.up * speed; // move up while looking for a target
            }
            
        }        
        else // Target is locked
        {
            if(!target) // target was destroyed before missile got to it
            {
                if (isPlayer)
                {
                    GetClosestTarget(); // Look for a new target
                }
                else
                {
                    TargetHero(); // Look for a new target
                }

                if (!target) // Can't find a target anyway, thus has to be destoyed
                {
                    explosionCollisionObject.GetComponent<MissileExplosion>().Explode();
                }
            }

            if(target) // Target exists, follow it
            {
                Vector2 direction = (Vector2)target.position - rb.position;
                direction.Normalize();

                float rotateAmount = Vector3.Cross(direction, transform.up).z;

                rb.angularVelocity = rotateAmount * -rotationSpeed;

                rb.velocity = transform.up * speed;
            }           
          
        }

        explosionCollisionObject.transform.position = transform.position; // Update the Explosion Collision object position to a new missile position
    }

    // Target Hero or Hero's missiles based on the shortest distance
    void TargetHero()
    {
        GameObject hero = GameObject.FindGameObjectWithTag("Hero");
        GameObject[] allHeroMissiles = GameObject.FindGameObjectsWithTag("Missile");
        float shortestDistance = 0.0f; // shortedt distance

        for(int i = 0; i <= allHeroMissiles.Length; i++) // Iterate through the list of Hero missiles
        {
            if(i < allHeroMissiles.Length)
            {
                if (shortestDistance == 0.0f)
                {
                    shortestDistance = Vector3.Distance(transform.position, allHeroMissiles[i].transform.position);
                    target = allHeroMissiles[i].transform;
                }
                else
                {
                    if (Vector3.Distance(transform.position, allHeroMissiles[i].transform.position) < shortestDistance)
                    {
                        shortestDistance = Vector3.Distance(transform.position, allHeroMissiles[i].transform.position);
                        target = allHeroMissiles[i].transform;
                    }
                }
            }
            else // i is out of bound of array, thus checking Hero's position
            {
                if (hero)
                {
                    if (shortestDistance == 0.0f)
                    {
                        shortestDistance = Vector3.Distance(transform.position, hero.transform.position);
                        target = hero.transform;
                    }
                    else
                    {
                        if (Vector3.Distance(transform.position, hero.transform.position) < shortestDistance)
                        {
                            shortestDistance = Vector3.Distance(transform.position, hero.transform.position);
                            target = hero.transform;
                        }
                    }
                    
                }
            }
            targetLocked = true;
        }
        
    }

    // Target an Enemy or Enemy's missiles based on the shortest distance
    void GetClosestTarget()
    {
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] allEnemyMissiles = GameObject.FindGameObjectsWithTag("EnemyMissile");
        float shortestDistance = 0.0f;

        // Check both arrays - enemies and their missiles
        for(int i = 0; i < allEnemies.Length + allEnemyMissiles.Length; i++)
        {
            if(i < allEnemies.Length) // Check Enemies
            {
                if (shortestDistance == 0.0f)
                {
                    shortestDistance = Vector3.Distance(transform.position, allEnemies[i].transform.position);
                    target = allEnemies[i].transform;
                }
                else
                {
                    if (Vector3.Distance(transform.position, allEnemies[i].transform.position) < shortestDistance)
                    {
                        shortestDistance = Vector3.Distance(transform.position, allEnemies[i].transform.position);
                        target = allEnemies[i].transform;
                    }
                }
            }
            else // Check Enemy Missiles
            {
                if (shortestDistance == 0.0f)
                {
                    shortestDistance = Vector3.Distance(transform.position, allEnemyMissiles[i - allEnemies.Length].transform.position);
                    target = allEnemyMissiles[i - allEnemies.Length].transform;
                }
                else
                {
                    if (Vector3.Distance(transform.position, allEnemyMissiles[i - allEnemies.Length].transform.position) < shortestDistance)
                    {
                        shortestDistance = Vector3.Distance(transform.position, allEnemyMissiles[i - allEnemies.Length].transform.position);
                        target = allEnemyMissiles[i - allEnemies.Length].transform;
                    }
                }
            }
            

            targetLocked = true;
        }
    }

    
}
