using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle_Collider : MonoBehaviour
{
    public ParticleSystem part;
    public ParticleCollisionEvent[] collisionEvents;

    void Start()
    {
        part = GetComponent<ParticleSystem>();
    }

    private float ColorToConcentration(Color color)
    {
        return color.a / 30.0f;
    }

    private void OnParticleCollision(GameObject other)
    {
        collisionEvents = new ParticleCollisionEvent[16];
        int CoLength = part.GetSafeCollisionEventSize();
        if (collisionEvents.Length < CoLength)
            collisionEvents = new ParticleCollisionEvent[CoLength];

        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        Debug.Log(numCollisionEvents.ToString() + " particles hit on " + other.name);
    }
}
