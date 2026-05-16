using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TipoJoya { Roja, Azul, Verde, Amarilla, Morada, Naranja }

public class GeneradorJoyas : MonoBehaviour
{
    public GameObject[] joyas;
    public GameObject superGema;
    private bool esBomba = false;
    private bool esSupergema = false;

    private int filas;
    private int columnas;
    public GameObject joyaActual;
    private Tablero tablero;

    public int Filas { get => filas; set => filas = value; }
    public int Columnas { get => columnas; set => columnas = value; }
    public bool EsBomba { get => esBomba; set => esBomba = value; }
    public bool EsSupergema { get => esSupergema; set => esSupergema = value; }
    public Tablero Tablero { get => tablero; set => tablero = value; }

    private void Start() { }

    /// <summary>
    /// Instancia la gema desplazada 'offsetArriba' unidades sobre la posición actual del tile.
    /// Si offsetArriba es 0, nace exactamente donde está el tile (que ya fue puesto arriba por Tablero).
    /// </summary>
    public void SpawnArriba(float offsetArriba)
    {
        GameObject prefab = ElegirPrefab();
        if (prefab == null) return;

        Vector3 posicionSpawn = transform.position + Vector3.up * offsetArriba;
        joyaActual = Instantiate(prefab, posicionSpawn, Quaternion.identity);
        joyaActual.transform.parent = this.transform;
        joyaActual.name = this.gameObject.name;
    }

    private GameObject ElegirPrefab()
    {
        if (GestorContrarreloj.Instancia != null && GestorContrarreloj.Instancia.DebeSpawnearJoyaTiempo())
        {
            GameObject prefabTiempo = GestorContrarreloj.Instancia.ObtenerPrefabTiempoAleatorio();
            if (prefabTiempo != null) return prefabTiempo;
        }

        if (tablero == null)
            return joyas[Random.Range(0, joyas.Length)];

        List<GameObject> disponibles = new List<GameObject>(joyas);
        disponibles = Shuffle(disponibles);

        foreach (GameObject prefabJoya in disponibles)
        {
            TipoJoya tipoIntento = prefabJoya.GetComponent<Joya>().tipo;

            bool matchHorizontal = columnas >= 2 &&
                tablero.allTiles[columnas - 1, filas].GetTipo() == tipoIntento &&
                tablero.allTiles[columnas - 2, filas].GetTipo() == tipoIntento;

            bool matchVertical = filas >= 2 &&
                tablero.allTiles[columnas, filas - 1].GetTipo() == tipoIntento &&
                tablero.allTiles[columnas, filas - 2].GetTipo() == tipoIntento;

            if (!matchHorizontal && !matchVertical)
                return prefabJoya;
        }

        return joyas[0];
    }

    public void SpawnJoya()
    {
        GameObject prefab = ElegirPrefab();
        if (prefab != null) SpawnNormal(prefab);
    }

    private void SpawnNormal(GameObject prefab)
    {
        joyaActual = Instantiate(prefab, transform.position, Quaternion.identity);
        joyaActual.transform.parent = this.transform;
        joyaActual.name = this.gameObject.name;
    }

    // ─── ANIMACIÓN DE CAÍDA ───────────────────────────────────────────────────
    public IEnumerator AnimarCaida(float duracion, float delay = 0f)
    {
        if (joyaActual == null) yield break;

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        // La posición local destino es (0,0,0) porque la gema debe quedar
        // centrada en su tile padre
        Vector3 inicioLocal = joyaActual.transform.localPosition;
        Vector3 destinoLocal = Vector3.zero;

        // Si ya está en origen, no hay nada que animar
        if (inicioLocal == destinoLocal) yield break;

        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            joyaActual.transform.localPosition = Vector3.LerpUnclamped(inicioLocal, destinoLocal, EaseOutBounce(t));
            yield return null;
        }

        joyaActual.transform.localPosition = destinoLocal;
    }

    // Exactamente 2 rebotes: uno grande (~25% de altura) y uno pequeño (~6%)
    private float EaseOutBounce(float t)
    {
        if (t < 0.6364f)
        {
            return 6.0742f * t * t;                         // caída principal
        }
        else if (t < 0.8727f)
        {
            t -= 0.7545f;
            return 6.0742f * t * t + 0.75f;                // primer rebote
        }
        else
        {
            t -= 0.9636f;
            return 6.0742f * t * t + 0.9375f;              // segundo rebote pequeño
        }
    }

    public TipoJoya GetTipo()
    {
        if (joyaActual == null) return default;

        if (esBomba)
        {
            Bomba b = joyaActual.GetComponent<Bomba>();
            if (b != null) return b.tipo;
        }

        Joya j = joyaActual.GetComponent<Joya>();
        if (j != null) return j.tipo;

        return default;
    }

    public GameObject GetPrefabBomba()
    {
        if (joyaActual == null) return null;
        Joya j = joyaActual.GetComponent<Joya>();
        if (j != null) return j.prefabBomba;
        return null;
    }

    private List<GameObject> Shuffle(List<GameObject> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            GameObject temp = lista[i];
            lista[i] = lista[j];
            lista[j] = temp;
        }
        return lista;
    }
}