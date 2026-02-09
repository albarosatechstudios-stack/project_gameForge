//using UnityEngine;

//public class SmokeFromBlow : MonoBehaviour
//{
//    [Header("References")]
//    public SoffioScript blow;
//    public ParticleSystem smoke;
//    public SphereCollider smokeCollider;

//    // Moduli del Particle System da controllare
//    private ParticleSystem.EmissionModule emissionModule;
//    private ParticleSystem.VelocityOverLifetimeModule velocityModule;
//    private ParticleSystem.NoiseModule noiseModule;
//    private ParticleSystem.MainModule mainModule;
//    private ParticleSystem.ShapeModule shapeModule;

//    [Header("Base Settings (A riposo)")]
//    public float baseEmission = 5f;
//    public float baseSpeed = 0.5f;
//    public float baseNoise = 0.2f;    // Poca turbolenza quando non soffi
//    public float baseSize = 1f;       // Grandezza normale
//    public float baseAngle = 10f;     // Cono stretto

//    [Header("Blow Multipliers (Reazione al soffio)")]
//    public float emissionFactor = 10f; // Aumenta drasticamente il numero di particelle
//    public float speedFactor = 2f;     // Spinta in avanti/alto
//    public float noiseFactor = 1.5f;   // Il fumo impazzisce e si sparge ai lati
//    public float sizeFactor = 0.5f;    // Diventa un po' pi� grosso
//    public float angleFactor = 20f;    // Il cono si apre (es. +20 gradi)

//    [Header("Room Fill Logic")]
//    public float fillRate = 5f;
//    public float decayRate = 2f;
//    public float maxFill = 100f;
//    public float activationThreshold = 30f;
//    public float maxRadius = 10f;      // Raggio massimo del collider

//    [Range(0, 100)]
//    public float currentSmokeLevel = 0f;

//    void Start()
//    {
//        // Otteniamo i riferimenti ai moduli del Particle System
//        emissionModule = smoke.emission;
//        velocityModule = smoke.velocityOverLifetime;
//        noiseModule = smoke.noise;
//        mainModule = smoke.main;
//        shapeModule = smoke.shape;

//        // Attiviamo il modulo Noise via codice per sicurezza
//        noiseModule.enabled = true;

//        if (smokeCollider)
//        {
//            smokeCollider.enabled = false;
//            smokeCollider.radius = 0f;
//            smokeCollider.isTrigger = true;
//        }
//    }

//    void Update()
//    {
//        float s = blow.GetSoundStrength(); // Valore soffio (es. 0 - 10)

//        // --- 1. CONTROLLO DINAMICO DELLE PARTICELLE ---

//        // Emissione: Pi� soffi, pi� fumo esce
//        emissionModule.rateOverTime = baseEmission + (s * emissionFactor);

//        // Velocit�: Pi� soffi, pi� va veloce (assicurati che Gravity sia negativo nell'editor!)
//        velocityModule.z = baseSpeed + (s * speedFactor); // Nota: se hai ruotato l'oggetto, potrebbe essere Y o Z

//        // Noise (Turbolenza): Questo � il segreto per riempire la stanza.
//        // Se soffi forte, il valore sale e il fumo si sparge ovunque.
//        noiseModule.strengthMultiplier = baseNoise + (s * noiseFactor);

//        // Grandezza: Particelle pi� grosse occupano pi� spazio visivo
//        mainModule.startSizeMultiplier = baseSize + (s * sizeFactor);

//        // Angolo: Apriamo il cono per spruzzare fumo in un'area pi� ampia
//        shapeModule.angle = baseAngle + (s * angleFactor);


//        // --- 2. LOGICA ACCUMULO (Uguale a prima) ---
//        if (s > 1f)
//        {
//            currentSmokeLevel += s * fillRate * Time.deltaTime;
//        }
//        else
//        {
//            currentSmokeLevel -= decayRate * Time.deltaTime;
//        }
//        currentSmokeLevel = Mathf.Clamp(currentSmokeLevel, 0f, maxFill);

//        // --- 3. GESTIONE COLLIDER ---
//        ManageCollider();
//    }

//    void ManageCollider()
//    {
//        if (currentSmokeLevel < activationThreshold)
//        {
//            smokeCollider.radius = 0f;
//            smokeCollider.enabled = false;
//        }
//        else
//        {
//            smokeCollider.enabled = true;
//            float range = maxFill - activationThreshold;
//            if (range <= 0.001f) range = 1f;

//            float progress = (currentSmokeLevel - activationThreshold) / range;
//            smokeCollider.radius = Mathf.Lerp(0f, maxRadius, progress);
//        }
//    }


//}
using UnityEngine;

public class SmokeFromBlow : MonoBehaviour
{
    [Header("References")]
    public SoffioScript blow;
    public ParticleSystem smoke;
    public SphereCollider smokeCollider;

    // Moduli del Particle System
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;
    private ParticleSystem.NoiseModule noiseModule;
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.ShapeModule shapeModule;

    [Header("Base Settings (A riposo)")]
    public float baseEmission = 5f;
    public float baseSpeed = 0.5f;
    public float baseNoise = 0.2f;
    public float baseSize = 1f;
    public float baseAngle = 10f;

    [Header("Blow Multipliers (Reazione al soffio)")]
    public float emissionFactor = 10f;
    public float speedFactor = 2f;
    public float noiseFactor = 1.5f;
    public float sizeFactor = 0.5f;
    public float angleFactor = 20f;

    [Header("Room Fill Logic")]
    public float fillRate = 5f;
    public float decayRate = 2f;
    public float maxFill = 100f;
    public float activationThreshold = 30f;
    public float maxRadius = 10f;

    [Range(0, 100)]
    public float currentSmokeLevel = 0f;

    void Start()
    {
        currentSmokeLevel = 3f;
        emissionModule = smoke.emission;
        velocityModule = smoke.velocityOverLifetime;
        noiseModule = smoke.noise;
        mainModule = smoke.main;
        shapeModule = smoke.shape;

        noiseModule.enabled = true;

        if (smokeCollider)
        {
            smokeCollider.enabled = false;
            smokeCollider.radius = 0f;
            smokeCollider.isTrigger = true; // Fondamentale per far funzionare OnTrigger
        }
    }

    void Update()
    {
        float s = blow.GetSoundStrength();

        // --- 1. CONTROLLO DINAMICO DELLE PARTICELLE ---
        emissionModule.rateOverTime = baseEmission + (s * emissionFactor);
        velocityModule.z = baseSpeed + (s * speedFactor);
        noiseModule.strengthMultiplier = baseNoise + (s * noiseFactor);
        mainModule.startSizeMultiplier = baseSize + (s * sizeFactor);
        shapeModule.angle = baseAngle + (s * angleFactor);

        // --- 2. LOGICA ACCUMULO ---
        if (s > 1f)
        {
            currentSmokeLevel += s * fillRate * Time.deltaTime;
        }
        else
        {
            currentSmokeLevel -= decayRate * Time.deltaTime;
        }
        currentSmokeLevel = Mathf.Clamp(currentSmokeLevel, 0f, maxFill);

        // --- 3. GESTIONE COLLIDER ---
        ManageCollider();
    }

    void ManageCollider()
    {
        if (currentSmokeLevel < activationThreshold)
        {
            smokeCollider.radius = 0f;
            smokeCollider.enabled = false;
        }
        else
        {
            smokeCollider.enabled = true;
            float range = maxFill - activationThreshold;
            if (range <= 0.001f) range = 1f;

            float progress = (currentSmokeLevel - activationThreshold) / range;
            smokeCollider.radius = Mathf.Lerp(0f, maxRadius, progress);
        }
    }

    // --- NUOVA PARTE: Interazione con il Nemico ---

    // OnTriggerStay viene chiamato ogni frame finch� un altro collider � dentro questo trigger
    private void OnTriggerStay(Collider other)
    {
        // Controlliamo se l'oggetto toccato ha il tag "Nemico"
        if (other.CompareTag("nemico"))
        {
            // Cerchiamo lo script NemicoScript sull'oggetto colpito
            NemicoScript nemico = other.GetComponent<NemicoScript>();

            // Se lo script esiste, chiamiamo il metodo pubblico per addormentarlo
            if (nemico != null)
            {
                // Passiamo 'this.gameObject' cos� il nemico sa chi lo sta tenendo addormentato
                nemico.ForzaSonno(this.gameObject);
            }
        }
    }
}
