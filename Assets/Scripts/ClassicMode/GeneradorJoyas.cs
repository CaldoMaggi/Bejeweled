using UnityEngine;
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
    public GameObject joyaActual; //Referencia a la joya que se ha generado en esta casilla
    private Tablero tablero;

    public int Filas { get => filas; set => filas = value; }
    public int Columnas { get => columnas; set => columnas = value; }
    public bool EsBomba { get => esBomba; set => esBomba = value; }
    public bool EsSupergema { get => esSupergema; set => esSupergema = value; }
    public Tablero Tablero { get => tablero; set => tablero = value; }

    private void Start()
    {
        SpawnJoya();
    }

    public void SpawnJoya()
    {
        // ─── Joya de tiempo (solo en modo contrarreloj) ───────────────
        // Solo spawnea joya de tiempo si GestorContrarreloj está activo en la escena contrareloj
        if (GestorContrarreloj.Instancia != null && GestorContrarreloj.Instancia.DebeSpawnearJoyaTiempo())
        {
            GameObject prefabTiempo = GestorContrarreloj.Instancia.ObtenerPrefabTiempoAleatorio();
            if (prefabTiempo != null)
            {
                SpawnNormal(prefabTiempo);
                return;
            }
        }

        // ─── Lógica normal (clasico y contrarreloj sin bonus) ─────────
        if (tablero == null)
        {
            SpawnNormal(joyas[Random.Range(0, joyas.Length)]);
            return;
        }

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
            {
                SpawnNormal(prefabJoya);
                return;
            }
        }

        SpawnNormal(joyas[0]);
    }

    private void SpawnNormal(GameObject prefab)
    {
        joyaActual = Instantiate(prefab, transform.position, Quaternion.identity);
        joyaActual.transform.parent = this.transform;
        joyaActual.name = this.gameObject.name;
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