using System.Collections;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    private static ParticleManager _instance;

    [System.Obsolete]
    public static ParticleManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ParticleManager>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("ParticleManager");
                    _instance = go.AddComponent<ParticleManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Spawna una particella in una posizione specifica (per particelle generiche)
    /// </summary>
    /// <param name="particlePrefab">Il prefab della particella da spawnare</param>
    /// <param name="position">La posizione dove spawnare la particella</param>
    /// <param name="rotation">La rotazione della particella (opzionale)</param>
    /// <param name="destroyTime">Tempo in secondi dopo il quale distruggere la particella (default: 5). Usa -1 per non distruggere automaticamente</param>
    /// <returns>Il GameObject della particella creata</returns>
    public GameObject SpawnParticle(GameObject particlePrefab, Vector3 position, Quaternion rotation = default, float destroyTime = 5f)
    {
        if (particlePrefab == null)
        {
            Debug.LogWarning("ParticleManager: Prefab particella è null!");
            return null;
        }

        // Se rotation è default, usa Quaternion.identity
        if (rotation == default)
            rotation = Quaternion.identity;

        // Spawna la particella
        GameObject particle = Instantiate(particlePrefab, position, rotation);

        // Controlla se ha un ParticleSystem - se sì, usa la gestione automatica
        ParticleSystem ps = particle.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // È un ParticleSystem, distruggilo quando finisce
            StartCoroutine(DestroyWhenParticleSystemFinished(particle, ps));
        }
        else if (destroyTime > 0)
        {
            // Non è un ParticleSystem, usa il tempo specificato
            StartCoroutine(DestroyParticleAfterTime(particle, destroyTime));
        }

        return particle;
    }

    /// <summary>
    /// Spawna una particella con solo la posizione (rotazione identity, tempo default)
    /// </summary>
    public GameObject SpawnParticle(GameObject particlePrefab, Vector3 position, float destroyTime = 5f)
    {
        return SpawnParticle(particlePrefab, position, Quaternion.identity, destroyTime);
    }

    /// <summary>
    /// Spawna una particella con sistema ParticleSystem e la distrugge automaticamente quando finisce
    /// </summary>
    /// <param name="particleSystemPrefab">Prefab con ParticleSystem</param>
    /// <param name="position">Posizione di spawn</param>
    /// <param name="rotation">Rotazione</param>
    /// <param name="autoDestroy">Se true, distrugge automaticamente quando il ParticleSystem finisce</param>
    /// <returns>Il GameObject della particella</returns>
    public GameObject SpawnParticleSystem(GameObject particleSystemPrefab, Vector3 position, Quaternion rotation = default, bool autoDestroy = true)
    {
        if (particleSystemPrefab == null)
        {
            Debug.LogWarning("ParticleManager: Prefab ParticleSystem è null!");
            return null;
        }

        if (rotation == default)
            rotation = Quaternion.identity;

        GameObject particle = Instantiate(particleSystemPrefab, position, rotation);

        if (autoDestroy)
        {
            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Avvia la coroutine che controlla quando il ParticleSystem ha finito
                StartCoroutine(DestroyWhenParticleSystemFinished(particle, ps));
            }
            else
            {
                Debug.LogWarning("ParticleManager: Il prefab non ha un componente ParticleSystem!");
                // Distruggi dopo 5 secondi come fallback
                StartCoroutine(DestroyParticleAfterTime(particle, 5f));
            }
        }

        return particle;
    }

    /// <summary>
    /// Distrugge manualmente una particella
    /// </summary>
    public void DestroyParticle(GameObject particle)
    {
        if (particle != null)
        {
            Destroy(particle);
        }
    }

    /// <summary>
    /// Coroutine che distrugge la particella dopo un tempo specificato
    /// </summary>
    private IEnumerator DestroyParticleAfterTime(GameObject particle, float time)
    {
        yield return new WaitForSeconds(time);

        if (particle != null)
        {
            Destroy(particle);
        }
    }

    /// <summary>
    /// Coroutine che aspetta che il ParticleSystem finisca completamente prima di distruggere
    /// </summary>
    private IEnumerator DestroyWhenParticleSystemFinished(GameObject particle, ParticleSystem ps)
    {
        // Aspetta che il ParticleSystem smetta di emettere
        while (ps != null && ps.isEmitting)
        {
            yield return null;
        }

        // Aspetta che tutte le particelle esistenti siano finite
        while (ps != null && ps.particleCount > 0)
        {
            yield return null;
        }

        // Ora distruggi il GameObject
        if (particle != null)
        {
            Destroy(particle);
        }
    }

    /// <summary>
    /// Distrugge tutte le particelle attualmente attive (figli di questo oggetto)
    /// </summary>
    public void DestroyAllParticles()
    {
        // Trova tutti i GameObjects che potrebbero essere particelle
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}