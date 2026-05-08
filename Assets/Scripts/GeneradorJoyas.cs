using UnityEngine;
using System.Collections.Generic;

public enum TipoJoya { Roja, Azul, Verde, Amarilla, Morada, Naranja }

public class GeneradorJoyas : MonoBehaviour
{
    public GameObject[] joyas;
    private TipoJoya tipo;

    public GameObject bomba;
    public GameObject superGema;
    public bool esBomba = false;
    public bool esSupergema = false;

    public int filas;
    public int columnas;
    public GameObject joyaActual;
    public Tablero tablero; // ← nuevo

    private void Start()
    {
        SpawnJoya();
    }

    public void SpawnJoya()
    {
        // Si no hay tablero aún, spawnear normal
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

        // Si todas forman match spawnear cualquiera
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