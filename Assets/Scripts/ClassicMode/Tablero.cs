using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tablero : MonoBehaviour
{
    private int ancho = 8;
    private int largo = 8;
    public GameObject PrefabTile;
    public GeneradorJoyas[,] allTiles;

    [Header("Animación de caída")]
    public float alturaInicial = 15f;
    public float duracionCaida = 1.3f;      // Sube esto para bounce más lento
    public float delayPorColumna = 0.08f;

    public int Ancho { get => ancho; set => ancho = value; }
    public int Largo { get => largo; set => largo = value; }

    void Start()
    {
        allTiles = new GeneradorJoyas[ancho, largo];
        SetUp();
    }

    private void SetUp()
    {
        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < largo; j++)
            {
                Vector2 posicionFinal = new Vector2(i, j);

                // Instanciar el tile YA arriba, no en su posición final
                Vector2 posicionArriba = posicionFinal + Vector2.up * alturaInicial;
                GameObject backgroundTile = Instantiate(PrefabTile, posicionArriba, Quaternion.identity);
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "(" + i + "," + j + ")";

                GeneradorJoyas tile = backgroundTile.GetComponent<GeneradorJoyas>();
                tile.Columnas = i;
                tile.Filas = j;
                tile.Tablero = this;
                allTiles[i, j] = tile;

                tile.SpawnArriba(0f); // 0f porque el tile ya está arriba, la gema nace en la misma posición que el tile
            }
        }

        StartCoroutine(AnimarCaidaTablero());
    }

    private IEnumerator AnimarCaidaTablero()
    {
        yield return null;
        yield return null;

        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < largo; j++)
            {
                GeneradorJoyas tile = allTiles[i, j];
                float delay = i * delayPorColumna;

                // Animar el tile (fondo de casilla)
                Vector3 destino = new Vector3(i, j, tile.transform.position.z);
                StartCoroutine(AnimarObjeto(tile.gameObject, destino, duracionCaida, delay));

                // Animar la gema encima
                if (tile.joyaActual != null)
                    StartCoroutine(tile.AnimarCaida(duracionCaida, delay));
            }
        }
    }

    // Corrutina genérica que baja cualquier GameObject a su destino con EaseOutBounce
    private IEnumerator AnimarObjeto(GameObject obj, Vector3 destino, float duracion, float delay)
    {
        if (obj == null) yield break;

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        Vector3 inicio = obj.transform.position;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            obj.transform.position = Vector3.LerpUnclamped(inicio, destino, EaseOutBounce(t));
            yield return null;
        }

        obj.transform.position = destino;
    }

    private float EaseOutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;
        if (t < 1f / d1)
        {
            return n1 * t * t;
        }
        else if (t < 2f / d1)
        {
            t -= 1.5f / d1;
            return n1 * t * t + 0.75f;
        }
        else if (t < 2.5f / d1)
        { 
            t -= 2.25f / d1;
            return n1 * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }
    }

    public IEnumerator RefrescarTablero(HashSet<(int, int)> celdasExcluidas = null)
    {
        // 1. Destruir gemas, saltando potenciadores
        for (int i = 0; i < ancho; i++)
            for (int j = 0; j < largo; j++)
            {
                if (celdasExcluidas != null && celdasExcluidas.Contains((i, j)))
                    continue;

                GeneradorJoyas tile = allTiles[i, j];
                if (tile.joyaActual != null)
                {
                    Destroy(tile.joyaActual);
                    tile.joyaActual = null;
                }
            }

        yield return null;

        // 2. Subir tiles y spawnear solo celdas vacías
        for (int i = 0; i < ancho; i++)
            for (int j = 0; j < largo; j++)
            {
                GeneradorJoyas tile = allTiles[i, j];
                Vector3 posicionArriba = new Vector3(i, j + alturaInicial, tile.transform.position.z);
                tile.transform.position = posicionArriba;

                if (tile.joyaActual == null)
                    tile.SpawnArriba(0f);
            }

        yield return null;

        // 3. Animar caída
        yield return StartCoroutine(AnimarCaidaTablero());
    }
}