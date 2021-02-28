using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Enemy_5 will start offscreen and then picks randomly a movement strategy
/// from 3 available stategies - MovementS like Enemy_1, Enemy_2, and Enemy_4.
/// It also picks a random weapong from all the weapons available - Blaster, Spread Blaster, Laser, or Missile.
/// Once the movement is finished it randomly picks another movement strategy and a weapon.
/// </summary>

public class Enemy_5 : Enemy
{
    [Header("Set in Inspector: Enemy_5")]
    public Part[] parts; //The array of ship parts


    private float birthTime;
    private bool finishedMove = true;
    public int currentMove = 1;

    private Vector3 p0, p1; //The two points to interpolate    
    private float duration = 4; //Duration of movement

    //Move 1
    //# seconds for a full sine wave
    public float waveFrequency = 2;
    private float x0; //the initial x vlaue of pos


    //Move 2
    public float sinEccentricity = 0.6f;

    //sine wave width in meters
    public float waveWidth = 4;
    public float waveRotY = 45;   


    public GameObject projectilePrefab;
    public float projectileSpeed = 40;    

    // Initial Movement for Move3
    void InitMovement()
    {
        p0 = p1;  //set p0 to the old p1
        //Assign a new on screen location to p1
        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        //Reset the time
        birthTime = Time.time;

    }

    // Do default fire
    public override void Fire()
    {
        base.Fire();
    }


    public override void Move()
    {
        // Move based on the current Movement strategy
        switch (currentMove)
        {
            case 1: // Move 1
                {
                    if (finishedMove) // first time
                    {
                        x0 = pos.x;
                        birthTime = Time.time;
                    }
                    MoveOne();
                    break;
                }
            case 2: // Move 2
                {
                    if(finishedMove) // first time
                    {                        
                        //Pick any point on the left side of the screen
                        p0 = Vector3.zero;
                        p0.x = -bndCheck.camWidth - bndCheck.radius;
                        p0.y = Random.Range(-bndCheck.camHeight, bndCheck.camHeight);

                        //Pick any point on the right side of the screen
                        p1 = Vector3.zero;
                        p1.x = bndCheck.camWidth + bndCheck.radius;
                        p1.y = Random.Range(-bndCheck.camHeight, bndCheck.camHeight);

                        //Possibly swap sides
                        if (Random.value > 0.5f)
                        {
                            //Setting the .x of each point the its negative will move it to
                            //the other side of the screen
                            p0.x *= -1;
                            p1.x *= -1;
                        }

                        //Set the birthTime to the current time
                        birthTime = Time.time;                        
                    }
                    MoveTwo();
                    break;
                }
            case 3: // Move 3
                {
                    if(finishedMove) // first time
                    {
                        birthTime = Time.time;
                        p0 = p1 = pos;
                        InitMovement();
                    }
                    MoveThree();
                    break;
                }
        }
    }

    // Randomly choose movement strategy
    public void ChooseMove()
    {
        currentMove = Random.Range(1, 4);
        Move();
    }

    // Randomly Choose current weapon
    public void ChooseWeapon()
    {
        int newWeapon = Random.Range(1, 5);
        switch (newWeapon)
        {
            case 1:
                {
                    weapon.SetType(WeaponType.enemyBlaster);
                    break;
                }
            case 2:
                {
                    weapon.SetType(WeaponType.enemySpread);
                    break;
                }
            case 3:
                {
                    if(currentMove != 2) // Laser doesn't work well with Move 2
                    {
                        weapon.SetType(WeaponType.enemyLaser);
                    }
                    else
                    {
                        ChooseWeapon();
                    }
                    
                    break;
                }
            case 4:
                {
                    weapon.SetType(WeaponType.enemyMissile);
                    break;
                }

        }
    }

    // Move like Enemy_1
    public void MoveOne()
    {
        finishedMove = false;
        //Because pos is a property, you can't directly set pos.x
        // so get the pos as an editable Vector3
        Vector3 tempPos = pos;
        //theta adjusts based on time
        float age = Time.time - birthTime;
        float theta = Mathf.PI * 2 * age / waveFrequency;
        float sin = Mathf.Sin(theta);
        tempPos.x = x0 + waveWidth * sin;
        pos = tempPos;

        //rotate a bit about y
        Vector3 rot = new Vector3(0, sin * waveRotY, 0);
        this.transform.rotation = Quaternion.Euler(rot);

        //base.Move() still handles the movement down in y
        base.Move();

        //print(bndCheck.isOnScreen);
    }

    // Move like Enemy_2
    public void MoveTwo()
    {
        finishedMove = false;
        //Bezier curves work based on a u value between 0 and 1
        float u = (Time.time - birthTime) / duration;

        //If u > 1, then it has been longer than lifeTime since birthTime
        if (u > 1)
        {
            finishedMove = true;
            return;
        }

        //Adjust u by adding a U Curve based on a Since wave
        u += sinEccentricity * (Mathf.Sin(u * Mathf.PI * 2));

        //Interpolate the two linear interpolation points
        pos = (1 - u) * p0 + u * p1;
    }

    // Move like Enemy_4
    public void MoveThree()
    {
        finishedMove = false;
        

        //This completely overrides Enemy.Move() with a linear interpolation
        float u = (Time.time - birthTime) / duration;

        if (u >= 1)
        {
            //InitMovement();
            //u = 0;
            finishedMove = true;
            return;
        }

        u = 1 - Mathf.Pow(1 - u, 2); //Apply Ease Out easing to u
        pos = (1 - u) * p0 + u * p1; //Simple linear interpolation
    }
     
        

    void OnCollisionEnter(Collision coll)
    {
        GameObject other = coll.gameObject;
        switch (other.tag)
        {
            case "ProjectileHero": // Hit by Hero's projectile
                AudioController.S.PlayEnemyDamage(); // play enemy damage sound

                Projectile p = other.GetComponent<Projectile>();
                if (!bndCheck.isOnScreen)
                {
                    Destroy(other);
                    break;
                }                

                //It's not protected
                health -= Main.GetWeaponDefinition(p.type).damageOnHit;
                ShowDamage();

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

                Destroy(other);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(finishedMove) // If finished the previous move, randomly pick a move and a weapon
        {
            ChooseMove();
            ChooseWeapon();
        }
        else
        {
            Move();
        }

        if (bndCheck != null && bndCheck.offDown) // Restriction for Move1
        {
            finishedMove = true;
        }

        if(bndCheck.offLeft || bndCheck.offRight) // Restriction for Move2
        {
            Destroy(this.gameObject);
        }

        if (!readyToFire && enemyFireDelegate != null)
        {
            StartCoroutine("Cooldown"); // Wait for cooldown and shoot
        }
    }
}
