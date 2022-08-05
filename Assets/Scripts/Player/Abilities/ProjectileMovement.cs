using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    private Rigidbody projectileRb;
    private bool shot = false;
    private bool alreadyShot = false;

    public ParticleSystem fireballParticle;

    // Start is called before the first frame update
    void Start()
    {
        projectileRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!alreadyShot)
        {
            shot = true;
        }
        if (shot)
        {
            projectileRb.velocity = ((transform.forward).normalized * speed);
            shot = false;
            alreadyShot = true;
        }

    }
}
