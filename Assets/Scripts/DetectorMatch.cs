using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputManagerEntry;

public class DetectorMatch : MonoBehaviour
{
    public Tablero tablero;
    public void ChequearDespuesDeSwap(GeneradorJoyas a, GeneradorJoyas b, System.Action revertir)
    {
        StartCoroutine(ChequearYResolver(a, b, revertir));
    }

    private IEnumerator ChequearYResolver(GeneradorJoyas a, GeneradorJoyas b, System.Action revertir)
    {
        yield return new WaitForSeconds(0.2f);

        List<GeneradorJoyas> matchA = GetMatch(a);
        List<GeneradorJoyas> matchB = GetMatch(b);

        // Sin match — revertir el swap
        if (matchA.Count == 0 && matchB.Count == 0)
        {
            revertir();
            yield break;
        }

        HashSet<GeneradorJoyas> todos = new HashSet<GeneradorJoyas>(matchA);
        todos.UnionWith(matchB);

        foreach (var tile in todos)
        {
            if (tile.joyaActual != null)
            {
                Destroy(tile.joyaActual);
                tile.joyaActual = null;
            }
        }

        yield return new WaitForSeconds(0.3f);
        RellenarTablero();
        yield return new WaitForSeconds(0.4f);
        yield return StartCoroutine(ChequearCascada());
    }

    private IEnumerator ChequearCascada()
    {
        bool huboCambios = true;

        while (huboCambios)
        {
            huboCambios = false;
            HashSet<GeneradorJoyas> todosLosMatches = new HashSet<GeneradorJoyas>();

            // Buscar matches en todo el tablero
            for (int i = 0; i < tablero.ancho; i++)
            {
                for (int j = 0; j < tablero.largo; j++)
                {
                    GeneradorJoyas tile = tablero.allTiles[i, j];
                    if (tile.joyaActual == null) continue;

                    List<GeneradorJoyas> match = GetMatch(tile);
                    if (match.Count >= 3)
                    {
                        todosLosMatches.UnionWith(match);
                        huboCambios = true;
                    }
                }
            }

            if (!huboCambios) break;

            // Destruir todos los matches encontrados
            foreach (var tile in todosLosMatches)
            {
                if (tile.joyaActual != null)
                {
                    Destroy(tile.joyaActual);
                    tile.joyaActual = null;
                }
            }

            yield return new WaitForSeconds(0.3f);

            RellenarTablero();

            yield return new WaitForSeconds(0.4f);
        }
    }

    // Busca si hay 3+ joyas del mismo tipo en fila o columna
    private List<GeneradorJoyas> GetMatch(GeneradorJoyas tile)
    {
        if (tile.joyaActual == null) return new List<GeneradorJoyas>();

        TipoJoya tipo = tile.GetTipo();
        List<GeneradorJoyas> horizontal = new List<GeneradorJoyas> { tile };
        List<GeneradorJoyas> vertical = new List<GeneradorJoyas> { tile };

        horizontal.AddRange(Buscar(tile, 1, 0, tipo));
        horizontal.AddRange(Buscar(tile, -1, 0, tipo));
        vertical.AddRange(Buscar(tile, 0, 1, tipo));
        vertical.AddRange(Buscar(tile, 0, -1, tipo));

        List<GeneradorJoyas> resultado = new List<GeneradorJoyas>();
        if (horizontal.Count >= 3) resultado.AddRange(horizontal);
        if (vertical.Count >= 3) resultado.AddRange(vertical);

        return resultado;
    }

    private List<GeneradorJoyas> Buscar(GeneradorJoyas origen, int dc, int df, TipoJoya tipo)
    {
        List<GeneradorJoyas> lista = new List<GeneradorJoyas>();
        int c = origen.columnas + dc;
        int f = origen.filas + df;

        while (true)
        {
            if (c < 0 || c >= tablero.ancho || f < 0 || f >= tablero.largo) break;

            GeneradorJoyas vecino = tablero.allTiles[c, f];
            if (vecino.joyaActual == null) break;
            if (vecino.GetTipo() != tipo) break;

            lista.Add(vecino);
            c += dc;
            f += df;
        }
        return lista;
    }

    private void RellenarTablero()
    {
        // Bajar joyas existentes para llenar huecos
        for (int i = 0; i < tablero.ancho; i++)
        {
            for (int j = 0; j < tablero.largo; j++)
            {
                if (tablero.allTiles[i, j].joyaActual == null)
                {
                    for (int k = j + 1; k < tablero.largo; k++)
                    {
                        GeneradorJoyas superior = tablero.allTiles[i, k];
                        if (superior.joyaActual != null)
                        {
                            GeneradorJoyas hueco = tablero.allTiles[i, j];
                            superior.joyaActual.transform.SetParent(hueco.transform);
                            superior.joyaActual.transform.position = hueco.transform.position;
                            hueco.joyaActual = superior.joyaActual;
                            superior.joyaActual = null;
                            break;
                        }
                    }
                }
            }
        }

        // Spawnear joyas nuevas donde sigan habiendo huecos
        for (int i = 0; i < tablero.ancho; i++)
            for (int j = 0; j < tablero.largo; j++)
                if (tablero.allTiles[i, j].joyaActual == null)
                    tablero.allTiles[i, j].SpawnJoya();
    }
}