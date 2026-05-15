using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GestorContrarreloj : MonoBehaviour
{
    public static GestorContrarreloj Instancia;

    public Tablero tablero;
    public float tiempoNivelCero = 60f;
    public float intervaloSpawnJoyaTiempo = 8f;
    public GameObject[] prefabsJoyaTiempo;

    public System.Action<float> OnTiempoActualizado;
    public System.Action OnGameOver;
    public System.Action<float> OnBonusTiempo;

    private float tiempoRestante;
    private bool activo = false;
    private bool spawnHabilitado = false;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }
    void OnDestroy()
    {
        Instancia = null;
    }

    void Start()
    {
        IniciarNivel();
    }

    [Header("Probabilidad de joya de tiempo (0 a 1)")]
    [Range(0f, 1f)]
    public float probabilidadJoyaTiempo = 0.08f; // 8% de chance por spawn

    public bool DebeSpawnearJoyaTiempo()
    {
        if (!spawnHabilitado) return false; // ← no spawna hasta que pasen los 15s
        if (prefabsJoyaTiempo == null || prefabsJoyaTiempo.Length == 0) return false;
        return Random.value < probabilidadJoyaTiempo;
    }

    public GameObject ObtenerPrefabTiempoAleatorio()
    {
        if (prefabsJoyaTiempo == null || prefabsJoyaTiempo.Length == 0) return null;
        return prefabsJoyaTiempo[Random.Range(0, prefabsJoyaTiempo.Length)];
    }
    void Update()
    {
        if (!activo) return;

        tiempoRestante -= Time.deltaTime;
        OnTiempoActualizado?.Invoke(tiempoRestante);

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            activo = false;
            TriggerGameOver();
        }
    }

    void IniciarNivel()
    {
        if (ReinicioModoContrareloj.Instancia == null)
        {
            Debug.LogError("No hay instancia de ReinicioModoContrareloj en la escena.");
            return;
        }

        bool esNivelCero = ReinicioModoContrareloj.Instancia.EsNivelCero();

        if (esNivelCero)
        {
            tiempoRestante = tiempoNivelCero;
        }
        else
        {
            tiempoRestante = ReinicioModoContrareloj.Instancia.ObtenerYVaciarBanco();
            if (tiempoRestante <= 0f)
            {
                TriggerGameOver();
                return;
            }
        }

        activo = true;
        StartCoroutine(HabilitarSpawnConDelay());
    }

    IEnumerator HabilitarSpawnConDelay()
    {
        yield return new WaitForSeconds(15f); // 15 segundos antes de que aparezca la primera
        spawnHabilitado = true;
    }

    public void RegistrarMatchConBonus(List<GameObject> joyasDelMatch)
    {
        foreach (GameObject joya in joyasDelMatch)
        {
            if (joya == null) continue;
            JoyaTiempo bonus = joya.GetComponent<JoyaTiempo>();
            if (bonus != null)
            {
                Vector3 pos = joya.transform.position; // posición real de la joya
                float segundos = bonus.ObtenerBonus();
                ReinicioModoContrareloj.Instancia.TiempoAcumulado += segundos;
                OnBonusTiempo?.Invoke(segundos);
                GestorPuntaje.Instancia.MostrarTextoFlotante((int)segundos, pos); // ← posición correcta
                Debug.Log($"+{segundos}s en posición {pos}");
            }
        }
    }

    // ─── Game Over ────────────────────────────────────────────────

    void TriggerGameOver()
    {
        Debug.Log("GAME OVER - Sin tiempo");
        OnGameOver?.Invoke();
    }

    public void Pausar() => activo = false;
    public void Reanudar() => activo = true;
    public float TiempoRestante() => tiempoRestante;
}