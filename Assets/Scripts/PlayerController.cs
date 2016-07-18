﻿using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float Speed;

    private Rigidbody _rb;

	// Use this for initialization
	void Start ()
	{
	    _rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
	{
        //get movement input.
	    float hor = Input.GetAxis("Horizontal");
	    float vert = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(hor, 0.0f, vert);  
        
        _rb.AddForce(movement * Speed);
	}
}