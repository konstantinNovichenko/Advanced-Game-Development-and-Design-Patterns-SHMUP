using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is an enum of the various possible wepaon types.
/// It also includes a "shield" type to allow a shiewld power up.
/// Items marked [NI] below are not implemented in the IGDPD book.
/// </summary>
public enum WeaponType
{
    none,       //The default / no weapon
    blaster,    //a simple blaster
    spread,     //Three shots simultaneously
    phaser,     //[NI] Shots that move in waves
    missile,    //[NI] Homing missiles
    laser,      //[NI] Damage over time
    shield,      //Raise shieldLevel
    enemyBlaster,
    enemyLaser,
    enemySpread,
    enemyMissile
}

/// <summary>
/// The WeaponDefinition class allows you to set the properties
///    of a specific weapon in the Inspector.  The Main class has
///    an array of WeaponDefintions that makes this possible.
/// </summary>
[System.Serializable]
public class WeaponDefinition
{
    public WeaponType type = WeaponType.none;
    public string letter;                       //letter to show on the power up
    public Color color = Color.white;           //Color of Collar & power-up
    public GameObject projectilePrefab;         //Prefab for projectiles
    public GameObject laserPrefab;
    public GameObject missilePrefab;
    public Color projectileColor = Color.white; 
    public float damageOnHit = 0;               //amount of damage caused
    public float continuousDamage = 0;          //damage per second (Laser)
    public float delayBetweenShots = 0;         
    public float velocity = 20;                 //Speed of projectiles
}

public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Set Dynamically")]
    [SerializeField]
    private WeaponType _type = WeaponType.none;
    public WeaponDefinition def;
    public GameObject collar;
    public float lastShotTime; //time last shot was fired
    private Renderer collarRend;
    public bool laserStarted;
    private GameObject laser;

    private void Start()
    {
        collar = transform.Find("Collar").gameObject;
        collarRend = collar.GetComponent<Renderer>();
        laserStarted = false;

        //Call SetType() for the default _type of WeaponType.none
        SetType(_type);

        //Dynamically create an anchor for all Projectiles
        if (PROJECTILE_ANCHOR == null) {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        //Find the fireDelegate of the root GameObject
        GameObject rootGO = transform.root.gameObject;
        if (rootGO.GetComponent<Hero>() != null)
        {
            rootGO.GetComponent<Hero>().fireDelegate += Fire;
        }
        else if (rootGO.GetComponent<Enemy>() != null)
        {
            rootGO.GetComponent<Enemy>().enemyFireDelegate += EnemyFire;           
        }
    }

    public WeaponType type
    {
        get { return _type; }
        set { SetType(value); }
    }

    public void SetType(WeaponType wt)
    {
        _type = wt;
        if (type == WeaponType.none)
        {            
            this.gameObject.SetActive(false);
            return;
        } else
        {
            if(wt != WeaponType.laser || wt != WeaponType.enemyLaser)
            {
                if (laser) // if laser exist and the current type is not the laser, destroy the laser
                {
                    laserStarted = false;
                    Destroy(laser.gameObject);
                }
            }
            this.gameObject.SetActive(true);
        }
        def = Main.GetWeaponDefinition(_type);
        collarRend.material.color = def.color;
        lastShotTime = 0; //You can fire immediately after _type is set
    }

    public void Fire()
    {
        //If this.gameObject is inactive, return
        if (!gameObject.activeInHierarchy) return;
        //If it hasn't been enough time between shots, return

        if (Time.time - lastShotTime < def.delayBetweenShots)
        {
            return;
        }
        Projectile p;
        Vector3 vel = Vector3.up * def.velocity;
        if (transform.up.y < 0)
        {
            vel.y = -vel.y;
        }
        switch(type)
        {
            case WeaponType.blaster:
                p = MakeProjectile();
                p.rigid.velocity = vel;
                AudioController.S.PlayBlasterShoot();
                break;

            case WeaponType.spread:
                p = MakeProjectile();  //Make middle Projectile
                p.rigid.velocity = vel;
                p = MakeProjectile(); //Make right Projectile
                p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                p.rigid.velocity = p.transform.rotation * vel;
                p = MakeProjectile(); //Make left Projectile
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                p.rigid.velocity = p.transform.rotation * vel;
                AudioController.S.PlayBlasterShoot();
                break;
            case WeaponType.laser:  // Shooting laser             
                if (laserStarted == false) // first time
                {
                    laser = Instantiate<GameObject>(def.laserPrefab); // Instantiate laser
                    laser.GetComponent<Laser>().weapon = this.gameObject; // set the weapon reference
                    laser.GetComponent<Laser>().isPlayer = true; // belongs to player
                    laserStarted = true; // start shooting
                }                
                break;
            case WeaponType.missile: // Shoot missile
                MakeMissile();
                break;
        }
    }

    // Enemy fire
    public void EnemyFire()
    {        
        if (Time.time - lastShotTime < def.delayBetweenShots) // Return if not ready to shoot
        {
            return;
        }
        Projectile p;
        Vector3 vel = Vector3.down * def.velocity;
        
        switch (type)
        {
            case WeaponType.enemyBlaster: // Shoot blaster
                p = MakeProjectile();
                p.rigid.velocity = vel;
                AudioController.S.PlayBlasterShoot(); // Play balster shoot sound
                break;

            case WeaponType.enemySpread: // Shoot spread blaster
                p = MakeProjectile();  //Make middle Projectile
                p.rigid.velocity = vel;
                p = MakeProjectile(); //Make right Projectile
                p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                p.rigid.velocity = p.transform.rotation * vel;
                p = MakeProjectile(); //Make left Projectile
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                p.rigid.velocity = p.transform.rotation * vel;
                AudioController.S.PlayBlasterShoot(); // Play balster shoot sound
                break;
            case WeaponType.enemyLaser: // Shoot laser
                
                if (laserStarted == false) // First time
                {
                    laser = Instantiate<GameObject>(def.laserPrefab); // Instantiate laser
                    laser.GetComponent<Laser>().SetEnemy(gameObject.transform.root.gameObject, false); // set enemy reference
                    laser.GetComponent<Laser>().weapon = this.gameObject; // set the weapon reference
                    laser.GetComponent<Laser>().isPlayer = false; // belongs to enemy
                    laserStarted = true; // start shooting
                }
                break;
            case WeaponType.enemyMissile:
                MakeMissile(); // shoot missile
                break;
        }
    }

    // Shoot missile
    public void MakeMissile()
    {
        GameObject missile = Instantiate<GameObject>(def.missilePrefab); // Instantiate a missile from the prefab

        if (transform.parent.gameObject.tag == "Hero") // belongs to hero
        {
            missile.tag = "ProjectileHero";
            missile.layer = LayerMask.NameToLayer("ProjectileHero");
        }
        else // Belongs to enemy
        {
            missile.tag = "ProjectileEnemy";
            missile.layer = LayerMask.NameToLayer("ProjectileEnemy");
        }
        missile.transform.position = collar.transform.position; // set postion to the collar
        lastShotTime = Time.time; // rese last shot time
    }

    public Projectile MakeProjectile()
    {
        GameObject go = Instantiate<GameObject>(def.projectilePrefab); // Instantiate a projectile from the prefab

        if (transform.parent.gameObject.tag == "Hero") // belongs to hero
        {
            go.tag = "ProjectileHero";
            go.layer = LayerMask.NameToLayer("ProjectileHero");
        }
        else // Belongs to enemy
        {
            go.tag = "ProjectileEnemy";
            go.layer = LayerMask.NameToLayer("ProjectileEnemy");
        }
        go.transform.position = collar.transform.position;
        go.transform.SetParent(PROJECTILE_ANCHOR, true);
        Projectile p = go.GetComponent<Projectile>();
        p.type = type;
        lastShotTime = Time.time;
        return p;
    }

    private void OnDestroy()
    {
        if(laser) // If laser exist, destroy it
        {
            Destroy(laser);
        }
    }
}
