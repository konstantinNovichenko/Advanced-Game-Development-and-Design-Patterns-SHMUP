using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_1 : Enemy
{

    [Header("Set in Inspector: Enemy 1")]
    //# seconds for a full sine wave
    public float waveFrequency = 2;
    //sine wave width in meters
    public float waveWidth = 4;
    public float waveRotY = 45;

    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    

    private float x0; //the initial x vlaue of pos
    private float birthTime;    

    // Start works well because it's not used by the Enemy superclass
    void Start()
    {
        //Set x0 is the initial x position of Enemy_1
        x0 = pos.x;

        birthTime = Time.time;
        weapon.SetType(WeaponType.enemyBlaster); // Set default weapon
    }

    //Override the Move function on Enemy
    public override void Move()
    {
       
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

    // Do the base fire pattern
    public override void Fire()
    {
        base.Fire();
    }

    // Update is called once per frame
    void Update()
    {
        Move();

        if (!readyToFire && enemyFireDelegate != null)
        {
            StartCoroutine("Cooldown"); // Wait for cooldown and shoot
        }

        if (bndCheck != null && bndCheck.offDown)
        {
            //We're off the bottom, so destroy this GameObject
            Destroy(gameObject);
        }
    }
}
