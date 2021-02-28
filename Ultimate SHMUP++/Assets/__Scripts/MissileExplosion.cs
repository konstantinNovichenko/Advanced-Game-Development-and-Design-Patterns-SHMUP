using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Track Collisions for missiles, plyas explosion animation, and destroys both missile and itself
/// </summary>
public class MissileExplosion : MonoBehaviour
{
    public GameObject parent; // missile
    public GameObject explosionPrefab;
    private BoundsCheck bndCheck;
    private bool explosionSet = false;
    private bool isPlayer = true;
    // Start is called before the first frame update
    void Start()
    {
        bndCheck = GetComponent<BoundsCheck>(); // get bound check
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!bndCheck.isOnScreen) // if off the screen - explode
        {
            Explode();
        }
    }

    // Set is belongs to the player (fixes Null reference bug)
    public void SetExplosion(bool isP)
    {
        isPlayer = isP;
        explosionSet = true;
    }

    private void OnTriggerEnter(Collider other)
    {
       if(explosionSet) // fixes Null reference bug
        {
            if(isPlayer) // if shoot by player (enemy missile collision is detected in Hero script)
            {
                switch (other.gameObject.tag)
                {
                    case "EnemyPart": // Enemy collision
                        {                             
                            Enemy e = other.gameObject.transform.parent.gameObject.GetComponent<Enemy>();
                            //If this Enemu is off screen, don't damage it
                            if (!bndCheck.isOnScreen)
                            {
                                Explode();
                                break;
                            }
                            //Hurt this enemy
                            e.ShowDamage();
                            //Get the damage amount from the Main WEAP_DICT
                            e.health -= Main.GetWeaponDefinition(WeaponType.missile).damageOnHit;
                            if (e.health <= 0)
                            {
                                //Tell the Main singleton that this ship was destroyed
                                if (!e.notifiedOfDestruction)
                                {
                                    Main.S.ShipDestroyed(e);
                                }
                                e.notifiedOfDestruction = true;
                                //Destroy this Enemy
                                Destroy(e.gameObject);
                            }
                            Explode();
                            break;
                        }
                    case "EnemyExplosionCollision": // Enemy missile collision
                        {
                            other.GetComponent<MissileExplosion>().Explode();                            
                            Explode();
                            break;
                        }
                }
            }            
            
        }        
        
    }

    // Explode the missile
    public void Explode()
    {
        AudioController.S.PlayExplosion(); // Play explosion sound
        GameObject explosion = Instantiate<GameObject>(explosionPrefab, transform.position, transform.rotation); // Instantiate explosion particle animation
        Destroy(explosion, 1.0f); // Destroy explosion animation after 1 second
        Destroy(parent); // Destroy missile
        Destroy(gameObject); // Destroy this object
    }    
}
