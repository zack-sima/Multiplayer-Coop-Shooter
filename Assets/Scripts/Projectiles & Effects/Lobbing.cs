using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobbing : Bullet {

    private const float GRAVITY = .2f;
    private Vector3 downwardVelocity = Vector3.zero;
    private float colliderTimer = .5f;
    private bool isCollidable = false;

    public void Awake() {
        gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    //first half needs to not be able to collide

    public void Update() {
        if (colliderTimer < 0) {
            gameObject.GetComponent<BoxCollider>().enabled = true;

        } else colliderTimer -= Time.deltaTime;
            
        downwardVelocity -= Vector3.up * GRAVITY * Time.deltaTime;
		transform.Translate(speed * Time.deltaTime * Vector3.forward + transform.InverseTransformDirection(downwardVelocity));

        
	}
}
