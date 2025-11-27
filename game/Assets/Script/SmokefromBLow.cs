using UnityEngine;



public class SmokeFromBlow : MonoBehaviour
{
    public SoffioScript blow;            // riferimento allo script che misura il soffio
    public ParticleSystem smoke;         // il Particle System del fumo

    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.VelocityOverLifetimeModule velocity;

    public float emissionBase = 10f;     // emissione quando non parli/soffi
    public float emissionFactor = 2f;    // quanto aumenta per unità di soffio

    public float speedBase = 0.2f;       // velocità base del fumo
    public float speedFactor = 0.03f;    // quanto aumenta con il soffio

    void Start()
    {
        emission = smoke.emission;
        velocity = smoke.velocityOverLifetime;
    }

    void Update()
    {
        float s = blow.GetSoundStrength();   // valore del soffio (es: 0–20)

        // Aumenta l’emissione del fumo
        emission.rateOverTime = emissionBase + s * emissionFactor;

        // Spinge il fumo in avanti (asse Z)
        velocity.z = speedBase + s * speedFactor;
    }
}

