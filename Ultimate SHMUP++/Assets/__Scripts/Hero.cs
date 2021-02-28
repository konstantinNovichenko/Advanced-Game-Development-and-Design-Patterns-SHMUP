using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;

public class Hero : MonoBehaviour
{

    public static Hero S;

    [Header("Set in Inspector")]

    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;
    public float gameRestartDelay = 2f;
    public GameObject projectilePrefab;    
    public float projectileSpeed = 40;
    public Weapon[] weapons;

    [Header("Set Dynamically")]
    [SerializeField]
    private float _shieldLevel = 1;
    public float shieldLevelPercentage = 1.0f; // Used for keeping track of the damage dealt by laser

    //This variable holds a reference to the last triggering GameObject
    private GameObject lastTriggerGo = null;

    //Declare a new delegate type WeaponFireDelegate
    public delegate void WeaponFireDelegate();
    //Create a WeaponFireDelegate field names fireDelegate
    public WeaponFireDelegate fireDelegate;

    void Start()
    {
        if (S == null)
        {
            S = this;
        } else
        {
            Debug.Log("Hero.Awake() - attempted to assign second Hero.S!");
        }

        // Deactivate gameover screen
        Main.S.gameOverText.gameObject.SetActive(false);

        //Reset the weapons to start _Hero with 1 blaster        
        ClearAllWeapons();
        weapons[0].SetType(WeaponType.blaster);
    }

    // Update is called once per frame
    void Update()
    {
        //Pull in information from the Input class
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        //Change transform.position based on the axes
        Vector3 pos = transform.position;
        pos.x += xAxis * speed * Time.deltaTime;
        pos.y += yAxis * speed * Time.deltaTime;
        transform.position = pos;

        //Rotate the ship to make it feel more dynamic
        transform.rotation = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);
        
        //Use the fireDelegate to fire Weapons
        //First, make sure the button is pressed: Axis("Jump")
        //Then ensure that fireDelegate isn't null to avoid an error
        if (Input.GetAxis("Jump") == 1 && fireDelegate != null)
        {
            fireDelegate();
        }

        if(shieldLevelPercentage <= 0) // Decrease the shield level if laser damaged it to 0
        {
            shieldLevel--;
            Main.S.UpdateShieldLevel(shieldLevel);
            shieldLevelPercentage = 1.0f;
        }
    }

    

    void OnTriggerEnter(Collider other)
    {
        Transform rootT = other.gameObject.transform.root;
        GameObject go = rootT.gameObject;        

        //Make sure it's not the same triggering go as last time
        if (go == lastTriggerGo)
        {
            return;
        }
        lastTriggerGo = go;

        if (go.tag == "Enemy" || other.gameObject.CompareTag("ProjectileEnemy")) // Collision with Enemy or Enemy's projectile
        {
            AudioController.S.PlayHeroDamage();
            shieldLevel--;
            Main.S.UpdateShieldLevel(shieldLevel);            
            Destroy(go);
        }
        else if (go.tag == "PowerUp") // Got power up
        {
            //If the shield was triggered by a PowerUp
            AbsorbPowerUp(go);
        }
        else if (go.tag == "EnemyExplosionCollision") // Collided with the missile collision object
        {
            shieldLevel -= 2;
            Main.S.UpdateShieldLevel(shieldLevel);
            go.GetComponent<MissileExplosion>().Explode();
        }
        else
        {
            print("Triggered by non-enemy: " + go.name);
        }
    }

    public void AbsorbPowerUp(GameObject go)
    {
        PowerUp pu = go.GetComponent<PowerUp>();
        switch (pu.type)
        {
            case WeaponType.shield:
                shieldLevel++;
                Main.S.UpdateShieldLevel(shieldLevel);
                break;            
            default:
                if (pu.type == weapons[0].type && weapons[0].type != WeaponType.laser) //if it is the same type
                {
                    Weapon w = GetEmptyWeaponSlot();
                    if (w != null)
                    {
                        //Set it to pu.type
                        w.SetType(pu.type);
                    }
                }
                else //If this is a different weapon type
                {
                    ClearAllWeapons();
                    weapons[0].SetType(pu.type);
                }
                break;
        
        }
        pu.AbsorbedBy(this.gameObject);
        AudioController.S.PlayPowerUp(); // Play power up sound
    }

    public void HitByLaser() 
    {
        //Hurt the hero
        
        //Get the damage amount from the Main WEAP_DICT
        shieldLevelPercentage -= Main.GetWeaponDefinition(WeaponType.enemyLaser).damageOnHit;
        Main.S.UpdateShieldPercentage(shieldLevelPercentage);
    }


    public float shieldLevel
    {
        get {

            return _shieldLevel;
        }
        set
        {
            _shieldLevel = Mathf.Min(value, 4);
            //If the shield is going to be set to less than zero
            if (value < 0)
            {
                Main.S.gameOver = true;
                Destroy(this.gameObject);
                //Tell Main.S to restart the game after a delay
                Main.S.DelayedRestart(gameRestartDelay);
            }
        }
    }

    Weapon GetEmptyWeaponSlot()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].type == WeaponType.none)
            {
                return weapons[i];
            }
        }
        return null;
    }

    void ClearAllWeapons()
    {
        foreach(Weapon w in weapons)
        {
            w.SetType(WeaponType.none);
        }
    }

    private void OnDestroy()
    {
        if(Main.S.gameOverText)
        {
            Main.S.gameOverText.gameObject.SetActive(true); // Show game over text
        }

        // Set the shield info text to zeroes
        Main.S.UpdateShieldPercentage(0.0f); 
        Main.S.UpdateShieldLevel(0.0f);
    }

}
 