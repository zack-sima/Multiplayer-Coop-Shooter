using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Video;

public class Lobbing : Bullet {

    private Vector3 downwardVelocity = Vector3.zero;
    private float colliderTimer = .5f;
    private bool isCollidable = false;

    public void Awake() {
        gameObject.GetComponent<Collider>().enabled = false;
    }
    void Start() {
        GetComponent<Rigidbody>().velocity = speed * transform.forward;
    }

    //first half needs to not be able to collide

    protected override void Update() {
        if (colliderTimer < 0) {
            gameObject.GetComponent<Collider>().enabled = true;

        } else colliderTimer -= Time.deltaTime;
	}
}
