using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instancia { get; private set; }
    //sonidos
    public AudioClip sonidoCaida;
    public AudioClip sonidoBomba;
    public AudioClip sonidoSupergema;
    public AudioClip sonidoSpawnBomba;
    public AudioClip sonidoMatch3;
    public AudioClip sonidoMatch4;

    private float pitchBase = 1f;
    private float pitchIncremento = 0.1f;
    private float pitchMaximo = 2f;
    private float tiempoResetPitch = 1.5f;
    private float pitchActual;
    private Coroutine corrutineReset;

    public float volumen = 1f;
    private AudioSource audioSource;
    private AudioSource audioSourceMatch3; // ← nuevo

    private void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        audioSourceMatch3 = gameObject.AddComponent<AudioSource>(); // ← nuevo
        audioSourceMatch3.playOnAwake = false;
    }

    private void Start()
    {
        pitchActual = pitchBase - pitchIncremento;
    }

    public void ReproducirCaida()
    {
        if (sonidoCaida == null) { Debug.LogWarning("sonidoCaida es null"); return; }
        if (!audioSource.isActiveAndEnabled) { Debug.LogWarning("AudioSource inactivo"); return; }
        audioSource.PlayOneShot(sonidoCaida, volumen);
    }

    public void ReproducirMatch3()
    {
        if (sonidoMatch3 == null) { Debug.LogWarning("sonidoMatch3 es null"); return; }

        pitchActual = Mathf.Min(pitchActual + pitchIncremento, pitchMaximo);
        audioSourceMatch3.pitch = pitchActual;
        audioSourceMatch3.PlayOneShot(sonidoMatch3, volumen);

        if (corrutineReset != null) StopCoroutine(corrutineReset);
        corrutineReset = StartCoroutine(ResetearPitch());
    }

    private IEnumerator ResetearPitch()
    {
        yield return new WaitForSeconds(tiempoResetPitch);
        pitchActual = pitchBase - pitchIncremento;
        audioSourceMatch3.pitch = pitchBase;
    }

    public void ReproducirMatch4()
    {
        if (sonidoMatch4 == null) { Debug.LogWarning("sonidoMatch4 es null"); return; }
        audioSource.PlayOneShot(sonidoMatch4, volumen);
    }

    public void ReproducirSpawnBomba()
    {
        if (sonidoSpawnBomba == null) { Debug.LogWarning("sonidoSpawnBomba es null"); return; }
        audioSource.PlayOneShot(sonidoSpawnBomba, volumen);
    }

    public void ReproducirBomba()
    {
        if (sonidoBomba == null) { Debug.LogWarning("sonidoBomba es null"); return; }
        audioSource.PlayOneShot(sonidoBomba, volumen);
    }

    public void ReproducirSupergema()
    {
        if (sonidoSupergema == null) { Debug.LogWarning("sonidoSupergema es null"); return; }
        audioSource.PlayOneShot(sonidoSupergema, volumen);
    }
}
