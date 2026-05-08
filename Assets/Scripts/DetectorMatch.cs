using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

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

        // --- Caso 1: Supergema + cualquier gema → explotar todo ese color ---
        if (a.esSupergema || b.esSupergema)
        {
            GeneradorJoyas supergema = a.esSupergema ? a : b;
            GeneradorJoyas otra = a.esSupergema ? b : a;

            // La otra puede ser Joya normal o Bomba, usamos helper
            TipoJoya tipoObjetivo = ObtenerTipo(otra);
            ExplotarColor(tipoObjetivo);
            DestruirJoya(supergema);

            yield return StartCoroutine(ProcesoPostDestruccion());
            yield break;
        }

        // --- Caso 2: Match normal (bombas incluidas como gemas con tipo) ---
        List<GeneradorJoyas> matchA = GetMatch(a);
        List<GeneradorJoyas> matchB = GetMatch(b);

        if (matchA.Count == 0 && matchB.Count == 0)
        {
            revertir();
            yield break;
        }

        // Unión de todos los tiles que hacen match
        HashSet<GeneradorJoyas> todos = new HashSet<GeneradorJoyas>(matchA);
        todos.UnionWith(matchB);

        // Centro = tile del match más largo (el que "ganó" el swap)
        List<GeneradorJoyas> matchPrincipal = matchA.Count >= matchB.Count ? matchA : matchB;
        GeneradorJoyas centro = matchA.Count >= matchB.Count ? a : b;
        int largo = matchPrincipal.Count;

        if (largo >= 5) // se crea la superjoya
        {
            foreach (var tile in todos)
                if (tile != centro) DestruirJoya(tile);
            ConvertirEnSupergema(centro);
        }
        else if (largo == 4) //se crea la bomba
        {
            foreach (var tile in todos)
                if (tile != centro) DestruirJoya(tile);
            ConvertirEnBomba(centro);
        }
        else // match de 3
        {
            foreach (var tile in todos) DestruirJoya(tile);
        }

        yield return StartCoroutine(ProcesoPostDestruccion());
    }

    // ─── Post-destrucción común ───────────────────────────────────────────────

    private IEnumerator ProcesoPostDestruccion()
    {
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

            for (int i = 0; i < tablero.ancho; i++)
                for (int j = 0; j < tablero.largo; j++)
                {
                    GeneradorJoyas tile = tablero.allTiles[i, j];
                    if (tile.joyaActual == null || tile.esSupergema) continue;

                    List<GeneradorJoyas> match = GetMatch(tile);
                    if (match.Count >= 3)
                    {
                        todosLosMatches.UnionWith(match);
                        huboCambios = true;
                    }
                }

            if (!huboCambios) break;

            foreach (var tile in todosLosMatches) DestruirJoya(tile);

            yield return new WaitForSeconds(0.3f);
            RellenarTablero();
            yield return new WaitForSeconds(0.4f);
        }
    }

    // ─── Destrucción ─────────────────────────────────────────────────────────

    public void DestruirJoya(GeneradorJoyas tile)
    {
        if (tile == null || tile.joyaActual == null) return;

        bool eraBomba = tile.esBomba;

        // Limpiar flags ANTES de recursión
        tile.esBomba = false;
        tile.esSupergema = false;

        Destroy(tile.joyaActual);
        tile.joyaActual = null;

        // Explotar DESPUÉS de destruir para evitar loop infinito
        if (eraBomba) ExplotarArea(tile);
    }

    // ─── Explosiones ─────────────────────────────────────────────────────────

    private void ExplotarArea(GeneradorJoyas centro)
    {
        // Guardamos columna y fila ANTES de cualquier destrucción
        int col = centro.columnas;
        int fil = centro.filas;

        List<GeneradorJoyas> aDestruir = new List<GeneradorJoyas>();

        for (int i = col - 1; i <= col + 1; i++)
            for (int j = fil - 1; j <= fil + 1; j++)
            {
                if (i < 0 || i >= tablero.ancho || j < 0 || j >= tablero.largo) continue;
                GeneradorJoyas vecino = tablero.allTiles[i, j];
                if (vecino == centro) continue;
                if (vecino.joyaActual != null) aDestruir.Add(vecino);
            }

        // Destruir todos de una vez para evitar problemas de recursión
        foreach (var tile in aDestruir) DestruirJoya(tile);
    }

    private void ExplotarColor(TipoJoya tipo)
    {
        for (int i = 0; i < tablero.ancho; i++)
            for (int j = 0; j < tablero.largo; j++)
            {
                GeneradorJoyas tile = tablero.allTiles[i, j];
                if (tile.joyaActual == null || tile.esSupergema) continue;
                // FIX: usamos helper que lee tanto Joya como Bomba
                if (ObtenerTipo(tile) == tipo) DestruirJoya(tile);
            }
    }

    // ─── Match detection ─────────────────────────────────────────────────────

    private List<GeneradorJoyas> GetMatch(GeneradorJoyas tile)
    {
        if (tile.joyaActual == null || tile.esSupergema)
            return new List<GeneradorJoyas>();

        TipoJoya tipo = ObtenerTipo(tile);

        List<GeneradorJoyas> horizontal = new List<GeneradorJoyas> { tile };
        horizontal.AddRange(BuscarIgnorandoBombas(tile, 1, 0, tipo));
        horizontal.AddRange(BuscarIgnorandoBombas(tile, -1, 0, tipo));

        List<GeneradorJoyas> vertical = new List<GeneradorJoyas> { tile };
        vertical.AddRange(BuscarIgnorandoBombas(tile, 0, 1, tipo));
        vertical.AddRange(BuscarIgnorandoBombas(tile, 0, -1, tipo));

        List<GeneradorJoyas> resultado = new List<GeneradorJoyas>();
        if (horizontal.Count >= 3) resultado.AddRange(horizontal);
        if (vertical.Count >= 3) resultado.AddRange(vertical);

        return resultado;
    }

    private List<GeneradorJoyas> BuscarIgnorandoBombas(GeneradorJoyas origen, int dc, int df, TipoJoya tipo)
    {
        List<GeneradorJoyas> lista = new List<GeneradorJoyas>();
        int c = origen.columnas + dc;
        int f = origen.filas + df;

        while (true)
        {
            if (c < 0 || c >= tablero.ancho || f < 0 || f >= tablero.largo) break;
            GeneradorJoyas vecino = tablero.allTiles[c, f];
            if (vecino.joyaActual == null || vecino.esSupergema) break;
            if (ObtenerTipo(vecino) != tipo) break;

            // Incluir tanto joyas normales como bombas del mismo color
            lista.Add(vecino);
            c += dc;
            f += df;
        }
        return lista;
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
            if (vecino.joyaActual == null || vecino.esSupergema) break;

            // ← Bomba del mismo color cuenta como parte del match
            if (ObtenerTipo(vecino) != tipo) break;

            lista.Add(vecino);

            // ← Si el vecino es bomba, no seguimos buscando más allá
            if (vecino.esBomba) break;

            c += dc;
            f += df;
        }
        return lista;
    }


    // ─── Conversiones ────────────────────────────────────────────────────────

    private void ConvertirEnBomba(GeneradorJoyas tile)
    {
        TipoJoya tipoAnterior = ObtenerTipo(tile);
        GameObject prefabBomba = tile.GetPrefabBomba();
        if (prefabBomba == null) { Debug.LogWarning("Falta prefabBomba en " + tile.name); return; }

        Destroy(tile.joyaActual);
        tile.joyaActual = null;
        tile.esBomba = false;
        tile.esSupergema = false;

        GameObject bombaObj = Instantiate(prefabBomba, tile.transform.position, Quaternion.identity, tile.transform);
        bombaObj.GetComponent<Bomba>().tipo = tipoAnterior;
        tile.joyaActual = bombaObj;
        tile.esBomba = true;
    }

    private void ConvertirEnSupergema(GeneradorJoyas tile)
    {
        if (tile.superGema == null) { Debug.LogWarning("Falta superGema prefab en " + tile.name); return; }

        Destroy(tile.joyaActual);
        tile.joyaActual = null;
        tile.esBomba = false;
        tile.esSupergema = false;

        GameObject superObj = Instantiate(tile.superGema, tile.transform.position, Quaternion.identity, tile.transform);
        tile.joyaActual = superObj;
        tile.esSupergema = true;
    }

    // ─── Utilidades ──────────────────────────────────────────────────────────

    /// <summary>
    /// Lee el TipoJoya de un tile sin importar si es Joya normal o Bomba.
    /// </summary>
    private TipoJoya ObtenerTipo(GeneradorJoyas tile)
    {
        if (tile.joyaActual == null) return default;

        Joya j = tile.joyaActual.GetComponent<Joya>();
        if (j != null) return j.tipo;

        Bomba b = tile.joyaActual.GetComponent<Bomba>();
        if (b != null) return b.tipo;

        return default;
    }

    // ─── Relleno ─────────────────────────────────────────────────────────────

    private void RellenarTablero()
    {
        for (int i = 0; i < tablero.ancho; i++)
            for (int j = 0; j < tablero.largo; j++)
            {
                if (tablero.allTiles[i, j].joyaActual != null) continue;

                for (int k = j + 1; k < tablero.largo; k++)
                {
                    GeneradorJoyas superior = tablero.allTiles[i, k];
                    if (superior.joyaActual == null) continue;

                    GeneradorJoyas hueco = tablero.allTiles[i, j];
                    superior.joyaActual.transform.SetParent(hueco.transform);
                    superior.joyaActual.transform.position = hueco.transform.position;
                    hueco.joyaActual = superior.joyaActual;
                    hueco.esBomba = superior.esBomba;
                    hueco.esSupergema = superior.esSupergema;
                    superior.joyaActual = null;
                    superior.esBomba = false;
                    superior.esSupergema = false;
                    break;
                }
            }

        for (int i = 0; i < tablero.ancho; i++)
            for (int j = 0; j < tablero.largo; j++)
                if (tablero.allTiles[i, j].joyaActual == null)
                    tablero.allTiles[i, j].SpawnJoya();
    }
}