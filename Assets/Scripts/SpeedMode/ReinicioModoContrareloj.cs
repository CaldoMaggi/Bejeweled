using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReinicioModoContrareloj : MonoBehaviour
{
    public static ReinicioModoContrareloj Instancia;// singleton
    public Tablero tablero;
    private float tiempoLimite = 60f;
    private float tiempoRestante;
    private bool modoActivo = false;

    // ─── Banco de tiempo (acumulado entre niveles) ────────────────
    private float tiempoAcumulado = 0f;
    private bool esNivelCero = true;

    public GameObject[] prefabsJoyaTiempo; // las 6 joyas con JoyaTiempo.cs
    public float intervaloSpawnJoyaTiempo = 8f;

    // Eventos para la UI
    public System.Action<float> OnTiempoActualizado;
    public System.Action OnTiempoAgotado;
    public System.Action OnGameOver;
    public System.Action<float> OnBonusTiempo; // para popup "+5s"

    public float TiempoLimite { get => tiempoLimite; set => tiempoLimite = value; }
    public float TiempoRestante { get => tiempoRestante; set => tiempoRestante = value; }
    public float TiempoAcumulado { get => tiempoAcumulado; set => tiempoAcumulado = value; }

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
            IniciarContrarreloj(); // ← mover aquí, antes que cualquier Start()
        }
        else Destroy(gameObject);
    }



    IEnumerator SpawnPeriodico()
    {
        yield return new WaitForSeconds(intervaloSpawnJoyaTiempo);
        while (modoActivo)
        {
            SpawnJoyaTiempoAleatoria();
            yield return new WaitForSeconds(intervaloSpawnJoyaTiempo);
        }
    }

    void SpawnJoyaTiempoAleatoria()
    {
        if (prefabsJoyaTiempo == null || prefabsJoyaTiempo.Length == 0) return;

        List<GeneradorJoyas> celdas = new List<GeneradorJoyas>();
        for (int i = 0; i < tablero.Ancho; i++)
            for (int j = 0; j < tablero.Largo; j++)
            {
                GeneradorJoyas tile = tablero.allTiles[i, j];
                if (!tile.EsBomba && !tile.EsSupergema && tile.joyaActual != null
                    && tile.joyaActual.GetComponent<JoyaTiempo>() == null)
                    celdas.Add(tile);
            }

        if (celdas.Count == 0) return;

        GeneradorJoyas elegida = celdas[Random.Range(0, celdas.Count)];
        GameObject prefab = prefabsJoyaTiempo[Random.Range(0, prefabsJoyaTiempo.Length)];

        Object.Destroy(elegida.joyaActual);
        elegida.joyaActual = Instantiate(prefab, elegida.transform.position, Quaternion.identity, elegida.transform);
    }

    void Update()
    {
        if (!modoActivo) return;

        tiempoRestante -= Time.deltaTime;
        OnTiempoActualizado?.Invoke(tiempoRestante);

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            modoActivo = false;

            if (esNivelCero)
            {
                // Nivel 0 terminó → pasa al siguiente con el banco
                OnTiempoAgotado?.Invoke();
                StartCoroutine(ReiniciarConAnimacion());
            }
            else
            {
                // Nivel 1+ sin tiempo → Game Over
                OnGameOver?.Invoke();
                Debug.Log("GAME OVER - Sin tiempo acumulado");
            }
        }
    }

    // ─── Inicialización ───────────────────────────────────────────

    public void IniciarContrarreloj()
    {
        if (esNivelCero)
        {
            tiempoRestante = tiempoLimite; // 60s fijos en nivel 0
        }
        else
        {
            tiempoRestante = ObtenerYVaciarBanco();
            if (tiempoRestante <= 0f)
            {
                OnGameOver?.Invoke();
                Debug.Log("GAME OVER - No acumulaste tiempo suficiente");
                return;
            }
        }

        modoActivo = true;
        StartCoroutine(SpawnPeriodico());
    }

    public void PausarContrarreloj() => modoActivo = false;
    public void ReanudarContrarreloj() => modoActivo = true;

    // ─── Banco de tiempo ─────────────────────────────────────────

    public void AgregarTiempo(float segundos)
    {
        tiempoAcumulado += segundos;
        OnBonusTiempo?.Invoke(segundos);
        Debug.Log($"Banco: +{segundos}s → Total acumulado: {tiempoAcumulado}s");
        GestorPuntaje.Instancia.MostrarTextoFlotante((int)segundos, Vector3.zero);
    }

    public float ObtenerYVaciarBanco()
    {
        float total = tiempoAcumulado;
        tiempoAcumulado = 0f;
        return total;
    }

    // Llama esto si el jugador reinicia la partida desde cero
    public void ReiniciarPartidaCompleta()
    {
        tiempoAcumulado = 0f;
        esNivelCero = true;
    }

    // ─── Llamado desde el sistema de match ───────────────────────

    public void RegistrarMatchConBonus(List<GameObject> joyasDelMatch)
    {
        foreach (GameObject joya in joyasDelMatch)
        {
            if (joya == null) continue;
            JoyaTiempo bonus = joya.GetComponent<JoyaTiempo>();
            if (bonus != null)
            {
                Vector3 pos = joya.transform.position;
                float segundos = bonus.ObtenerBonus();
                tiempoAcumulado += segundos;
                OnBonusTiempo?.Invoke(segundos);
                // Texto flotante en la posición de la joya — el cast a int es obligatorio
                GestorPuntaje.Instancia.MostrarTextoFlotante((int)segundos, pos);
            }
        }
    }

    // ─── Reinicio de tablero ──────────────────────────────────────

    private IEnumerator ReiniciarConAnimacion()
    {
        Debug.Log("¡Tiempo agotado! Pasando al siguiente nivel...");
        yield return new WaitForSeconds(1.5f);

        esNivelCero = false; // a partir de aquí usa el banco
        ReiniciarTablero();
        IniciarContrarreloj();
    }
    public void ReiniciarTablero()
    {
        List<PotenciadorGuardadoContrareloj> potenciadores = new List<PotenciadorGuardadoContrareloj>();

        for (int i = 0; i < tablero.Ancho; i++)
            for (int j = 0; j < tablero.Largo; j++)
            {
                GeneradorJoyas tile = tablero.allTiles[i, j];
                if (tile.EsBomba || tile.EsSupergema)
                {
                    potenciadores.Add(new PotenciadorGuardadoContrareloj
                    {
                        columna = i,
                        fila = j,
                        esBomba = tile.EsBomba,
                        esSupergema = tile.EsSupergema,
                        tipo = tile.GetTipo()
                    });
                }
            }

        for (int i = 0; i < tablero.Ancho; i++)
            for (int j = 0; j < tablero.Largo; j++)
            {
                GeneradorJoyas tile = tablero.allTiles[i, j];
                if (!tile.EsBomba && !tile.EsSupergema && tile.joyaActual != null)
                {
                    Object.Destroy(tile.joyaActual);
                    tile.joyaActual = null;
                }
            }

        for (int i = 0; i < tablero.Ancho; i++)
            for (int j = 0; j < tablero.Largo; j++)
                if (tablero.allTiles[i, j].joyaActual == null)
                    tablero.allTiles[i, j].SpawnJoya();
    }

    public bool EsNivelCero() => esNivelCero;
    public void MarcarNivelSiguiente() => esNivelCero = false;
}

public class PotenciadorGuardadoContrareloj
{
    public int columna;
    public int fila;
    public bool esBomba;
    public bool esSupergema;
    public TipoJoya tipo;
}