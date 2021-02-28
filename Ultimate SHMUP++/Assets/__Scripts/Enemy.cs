using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Enemy : MonoBehaviour
{

    [Header("Set in Inspector: Enemy")]
    public float speed = 10f;
    public float fireRate = .3f;
    public float health = 10;
    public int score = 100;
    public float showDamageDuration = 0.1f; //# seconds to show damage
    public float powerUpDropChance = 1f;  //Chance to drop a PowerUp

    public Weapon weapon; // reference to the equipped weapon
    public int cooldown = 5; // how long until enemy can shoot again
    public bool readyToFire = false; // check if ready to choot

    [Header("Set Dynamically: Enemy")]
    public Color[] originalColors;
    public Material[] materials; //All the Materials of this & its children
    public bool showingDamage = false;
    public float damageDoneTime; //Time to stop showing damage
    public bool notifiedOfDestruction = false; //Will be used later

    protected BoundsCheck bndCheck;

    //Declare a new delegate type WeaponFireDelegate
    public delegate void WeaponEnemyFireDelegate();
    //Create a WeaponFireDelegate field names fireDelegate
    public WeaponEnemyFireDelegate enemyFireDelegate;

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        //Get materials and colors for this Gameobject and its children
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];
        for(int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }
    }

    //This is a Property: A method that acts like a field
    public Vector3 pos
    {
        get
        {
            return (this.transform.position);
        }
        set
        {
            this.transform.position = value;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();

        if (showingDamage && Time.time > damageDoneTime)
        {
            UnShowDamage();
        }

        if (bndCheck != null && bndCheck.offDown)
        {
            //We're off the bottom, so destroy this GameObject
            Destroy(gameObject);
        }
    }

    // Fire after cooldown
    IEnumerator Cooldown()
    {        
        readyToFire = true; // prevent Update() from calling this function
        yield return new WaitForSeconds(cooldown);
        Fire();
    }

    // Basic movement
    public virtual void Move()
    {
        Vector3 tempPos = pos;
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
    }

    // Basic fire
    public virtual void Fire()
    {        
        enemyFireDelegate(); // use fire delegate
        readyToFire = false; // reset ability to fire
    }


    void OnCollisionEnter(Collision coll)
    {        
        GameObject otherGo = coll.gameObject;
        switch (otherGo.tag)
        {
            case "ProjectileHero": // hit Hero's projectile 

                AudioController.S.PlayEnemyDamage(); // play enemy damage sound
                Projectile p = otherGo.GetComponent<Projectile>();
                //If this Enemy is off screen, don't damage it
                if (!bndCheck.isOnScreen)
                {
                    Destroy(otherGo);
                    break;
                }
                //Hurt this enemy
                ShowDamage();
                //Get the damage amount from the Main WEAP_DICT
                health -= Main.GetWeaponDefinition(p.type).damageOnHit;
                if (health <= 0)
                {
                    //Tell the Main singleton that this ship was destroyed
                    if (!notifiedOfDestruction)
                    {
                        Main.S.ShipDestroyed(this);
                    }
                    notifiedOfDestruction = true;
                    //Destroy this Enemy
                    Destroy(this.gameObject);
                }
                Destroy(otherGo);
                break;

            default:
                print("Enemy hit by non-ProjectileHero: " + otherGo.name);
                break;
        }
    }

    // Show Damage
    public void ShowDamage()
    {
        foreach(Material m in materials)
        {
            m.color = Color.red;
        }
        showingDamage = true;
        damageDoneTime = Time.time + showDamageDuration;
    }

    // Hit By Laser
    public void HitByLaser()
    {
        if (bndCheck.isOnScreen) // Hurt if on screen
        {
            //Hurt this enemy
            ShowDamage();
            //Get the damage amount from the Main WEAP_DICT
            health -= Main.GetWeaponDefinition(WeaponType.laser).damageOnHit;
            if (health <= 0)
            {
                //Tell the Main singleton that this ship was destroyed
                if (!notifiedOfDestruction)
                {
                    Main.S.ShipDestroyed(this);
                }
                notifiedOfDestruction = true;
                //Destroy this Enemy
                Destroy(this.gameObject);
            }
        }        
    }

    void UnShowDamage()
    {
        for(int i = 0; i < materials.Length; i++)
        {
            materials[i].color = originalColors[i];
        }
        showingDamage = false;
    }

    private void OnDestroy()
    {
        Main.S.UpdateScore(score); // Update Score if destroyed
    }

}
