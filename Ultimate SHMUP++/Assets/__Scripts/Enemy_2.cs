﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_2 : Enemy
{

    [Header("Set in Inspector: Enemy_2")]
    //Determines how much the Sine wave will affect movement
    public float sinEccentricity = 0.6f;
    public float lifeTime = 10;

    [Header("Set Dynamically: Enemy_2")]
    //Enemy_2 uses a Sine wave to modify a 2-point linear interpolation
    public Vector3 p0;
    public Vector3 p1;
    public float birthTime;

    // Start is called before the first frame update
    void Start()
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
        weapon.SetType(WeaponType.enemyMissile); // Set missile as default weapon type
    }

    public override void Move()
    {
        //Bezier curves work based on a u value between 0 and 1
        float u = (Time.time - birthTime) / lifeTime;

        //If u > 1, then it has been longer than lifeTime since birthTime
        if (u > 1)
        {
            //This Enemy_2 has finished its life
            Destroy(this.gameObject);
            return;
        }

        //Adjust u by adding a U Curve based on a Since wave
        u += sinEccentricity * (Mathf.Sin(u * Mathf.PI * 2));

        //Interpolate the two linear interpolation points
        pos = (1 - u) * p0 + u * p1;
    }

    // Do the default fire
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
