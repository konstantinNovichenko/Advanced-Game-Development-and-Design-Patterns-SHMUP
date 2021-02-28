using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///Part is another serializable data storage class just like WeaponDefinition
///</summary>
[System.Serializable]
public class Part
{
    //These three fields need to be defined in the Inspector pane
    public string name; //the name of this part
    public float health; // the amount of health this part has
    public string[] protectedBy; //the other parts that protect this

    //These two fields are set automatically in Start
    //Caching like this makes it faster and easier to find these later
    [HideInInspector] //Makes field on the next line not appear in the Inspector
    public GameObject go;  //The GameObject for this part
    [HideInInspector]
    public Material mat;  //The Material to show damage
}


/// <summary>
/// Enemy_4 will start offscreen and then picks a random point on screen to
///  move to.  Once it has arrived, it will pick another random point and
///  continue until the player has shot it down.
/// </summary>

public class Enemy_4 : Enemy
{
    [Header("Set in Inspector: Enemy_4")]
    public Part[] parts; //The array of ship parts

    private Vector3 p0, p1; //The two points to interpolate
    private float timeStart; //birthTime for this Enemy_4
    private float duration = 4; //Duration of movement

    // Start is called before the first frame update
    void Start()
    {
        //There is already an initial position chosen by Main.SpawnEnemy()
        // so add it to points as the initial p0 and p1
        p0 = p1 = pos;
        InitMovement();

        //Cache GameObject & Material of each Part in parts
        Transform t;
        foreach(Part prt in parts)
        {
            t = transform.Find(prt.name);
            if (t != null)
            {
                prt.go = t.gameObject;
                prt.mat = prt.go.GetComponent<Renderer>().material;
            }
        }
    }

    void InitMovement()
    {
        p0 = p1;  //set p0 to the old p1
        //Assign a new on screen location to p1
        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        //Reset the time
        timeStart = Time.time;

    }

    // Do the default fire
    public override void Fire()
    {
        base.Fire();
    }

    public override void Move()
    {
        //This completely overrides Enemy.Move() with a linear interpolation
        float u = (Time.time - timeStart) / duration;

        if (u >= 1)
        {
            InitMovement();
            u = 0;
        }

        u = 1 - Mathf.Pow(1 - u, 2); //Apply Ease Out easing to u
        pos = (1 - u) * p0 + u * p1; //Simple linear interpolation
    }

    //These two functions find a part in parts based on name or GameObject
    Part FindPart(string n)
    {
        foreach(Part prt in parts)
        {
            if (prt.name == n)
            {
                return prt;
            }
        }
        return null;
    }

    Part FindPart(GameObject go)
    {
        foreach(Part prt in parts)
        {
            if (prt.go == go)
            {
                return prt;
            }
        }
        return null;
    }

    //These functions return true if the Part has been destroyed
    bool Destroyed(GameObject go)
    {
        return Destroyed(FindPart(go));
    }

    bool Destroyed(string n)
    {
        return Destroyed(FindPart(n));
    }
      

    bool Destroyed(Part prt)
    {
        if (prt == null) //If no real prt was passed in
        {
            return true;  //it was destroyed!
        }

        //Returns the result of the comparison: prt.health <= 0
        //If prt.health is 0 or less, returns true (yes, it was destroyed)
        return (prt.health <= 0);
    }

    //This changed the color of just one Part to red instead of the whole ship.
    void ShowLocalizedDamage(Material m)
    {
        m.color = Color.red;
        damageDoneTime = Time.time + showDamageDuration;
        showingDamage = true;
    }

    void OnCollisionEnter(Collision coll)
    {
        GameObject other = coll.gameObject;
        switch(other.tag)
        {
            case "ProjectileHero":
                AudioController.S.PlayEnemyDamage(); // Play enemy damage sound

                Projectile p = other.GetComponent<Projectile>();
                if (!bndCheck.isOnScreen)
                {
                    Destroy(other);
                    break;
                }

                GameObject goHit = coll.contacts[0].thisCollider.gameObject;
                Part prtHit = FindPart(goHit);
                if (prtHit == null)
                {
                    goHit = coll.contacts[0].otherCollider.gameObject;
                    prtHit = FindPart(goHit);
                }
                if (prtHit.protectedBy != null)
                {
                    foreach(string s in prtHit.protectedBy)
                    {
                        if (!(Destroyed(s)))
                        {
                            Destroy(other);
                            return;
                        }
                    }
                }

                //It's not protected
                prtHit.health -= Main.GetWeaponDefinition(p.type).damageOnHit;
                ShowLocalizedDamage(prtHit.mat);
                if (prtHit.health <= 0)
                {
                    prtHit.go.SetActive(false);
                }
                bool allDestroyed = true; 
                foreach(Part prt in parts)
                {
                    if (!Destroyed(prt))
                    {
                        allDestroyed = false;
                        break;
                    }
                }
                if (allDestroyed)
                {
                    Main.S.ShipDestroyed(this);
                    Destroy(this.gameObject);
                }
                Destroy(other);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();

        if (!readyToFire && enemyFireDelegate != null)
        {
            StartCoroutine("Cooldown"); // Wait for cooldown and shoot
        }
    }
}
