using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReinicioModoContrareloj : MonoBehaviour
{
    public static ReinicioModoContrareloj Instancia;
    public Tablero tablero;
    private float tiempoLimite = 60f;
    private float tiempoRestante;
    private bool modoActivo = false;

    private float tiempoAcumulado = 0f;
    private bool esNivelCero = true;
    private float intervaloSpawnJoyaTiempo = 10f;

    public System.Action<float> OnTiempoActualizado;
    public System.Action OnTiempoAgotado;
    public System.Action OnGameOver;
    public System.Action<float> OnBonusTiempo;

    public float TiempoLimite { get => tiempoLimite; set => tiempoLimite = value; }
    public float TiempoRestante { get => tiempoRestante; set => tiempoRestante = value; }
    public float TiempoAcumulado { get => tiempoAcumulado; set => tiempoAcumulado = value; }

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
        IniciarContrarreloj();
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
                OnTiempoAgotado?.Invoke();
                StartCoroutine(ReiniciarConAnimacion());
            }
            else
            {
                OnGameOver?.Invoke();
                Debug.Log("GAME OVER - Sin tiempo acumulado");
            }
        }
    }

    public void IniciarContrarreloj()
    {
        if (esNivelCero)
        {
            tiempoRestante = tiempoLimite;
        }

        modoActivo = true;
        StartCoroutine(SpawnPeriodico());
    }

    public void PausarContrarreloj() => modoActivo = false;
    public void ReanudarContrarreloj() => modoActivo = true;

    IEnumerator SpawnPeriodico()
    {
        yield return new WaitForSeconds(intervaloSpawnJoyaTiempo);
        while (modoActivo)
        {
            yield return new WaitForSeconds(intervaloSpawnJoyaTiempo);
        }
    }

    public void AgregarTiempoSilencioso(float segundos)
    {
        tiempoAcumulado += segundos;
    }

    public void AgregarTiempo(float segundos)
    {
        tiempoAcumulado += segundos;
        OnBonusTiempo?.Invoke(segundos);
        Debug.Log($"Banco: +{segundos}s → Total acumulado: {tiempoAcumulado}s");
    }

    public float ObtenerYVaciarBanco()
    {
        float total = tiempoAcumulado;
        tiempoAcumulado = 0f;
        return total;
    }

    public void ReiniciarPartidaCompleta()
    {
        tiempoAcumulado = 0f;
        esNivelCero = true;
        modoActivo = false;
        StopAllCoroutines();
    }

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
                GestorPuntaje.Instancia.MostrarTextoFlotante((int)segundos, pos);
            }
        }
    }

    private IEnumerator ReiniciarConAnimacion()
    {
        Debug.Log("¡Tiempo agotado! Pasando al siguiente nivel...");
        modoActivo = false;
        yield return new WaitForSeconds(1.5f); //tiempo para que el jugador procese el cambio de nivel

        float tiempoGanado = ObtenerYVaciarBanco();

        if (tiempoGanado <= 0f)
        {
            OnGameOver?.Invoke();
            Debug.Log("GAME OVER - No acumulaste tiempo");
            yield break;
        }

        yield return StartCoroutine(ReiniciarTableroConAnimacion());

        tiempoRestante = tiempoGanado;
        modoActivo = true;
        StartCoroutine(SpawnPeriodico());

        // Reactivar joyas de tiempo con delay para el nuevo nivel
        if (GestorContrarreloj.Instancia != null)
            GestorContrarreloj.Instancia.StartCoroutine(
                GestorContrarreloj.Instancia.HabilitarSpawnConDelay());
    }

    private IEnumerator ReiniciarTableroConAnimacion()
    {
        // Evitar que spawneen joyas de tiempo durante el refresco
        if (GestorContrarreloj.Instancia != null)
            GestorContrarreloj.Instancia.DeshabilitarSpawnTemporalmente();

        // 1. Guardar potenciadores (sin joyas de tiempo)
        List<PotenciadorGuardadoContrareloj> potenciadores = new List<PotenciadorGuardadoContrareloj>();
        HashSet<(int, int)> celdasPotenciador = new HashSet<(int, int)>();

        for (int i = 0; i < tablero.Ancho; i++)
            for (int j = 0; j < tablero.Largo; j++)
            {
                GeneradorJoyas tile = tablero.allTiles[i, j];

                if (tile.joyaActual != null && tile.joyaActual.GetComponent<JoyaTiempo>() != null)
                    continue;

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
                    celdasPotenciador.Add((i, j));
                }
            }

        // 2. Refrescar — ahora sin riesgo de joyas de tiempo
        yield return StartCoroutine(tablero.RefrescarTablero(celdasPotenciador));

        // spawnHabilitado queda en false hasta que HabilitarSpawnConDelay lo reactive
        // en el siguiente IniciarNivel() que llama ReiniciarConAnimacion()
    }

    public bool EsNivelCero() => esNivelCero;
}

public class PotenciadorGuardadoContrareloj
{
    public int columna;
    public int fila;
    public bool esBomba;
    public bool esSupergema;
    public TipoJoya tipo;
}