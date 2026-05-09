using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        if (a.esSupergema || b.esSupergema)
        {
            GeneradorJoyas supergema = a.esSupergema ? a : b;
            GeneradorJoyas otra = a.esSupergema ? b : a;
            ExplotarColor(ObtenerTipo(otra));
            DestruirJoya(supergema);
            GestorPuntaje.Instancia.AgregarPuntos(5);
            yield return StartCoroutine(ProcesoPostDestruccion());
            yield break;
        }

        List<GeneradorJoyas> matchA = GetMatch(a);
        List<GeneradorJoyas> matchB = GetMatch(b);

        if (matchA.Count == 0 && matchB.Count == 0)
        {
            revertir();
            yield break;
        }

        HashSet<GeneradorJoyas> todos = new HashSet<GeneradorJoyas>(matchA);
        todos.UnionWith(matchB);
        List<GeneradorJoyas> matchPrincipal = matchA.Count >= matchB.Count ? matchA : matchB;
        GeneradorJoyas centro = matchA.Count >= matchB.Count ? a : b;
        int largo = matchPrincipal.Count;

        if (largo >= 5)
        {
            foreach (var tile in todos)
                if (tile != centro) DestruirJoya(tile);
            ConvertirEnSupergema(centro);
            GestorPuntaje.Instancia.AgregarPuntos(30);
        }
        else if (largo == 4)
        {
            foreach (var tile in todos)
                if (tile != centro) DestruirJoya(tile);
            ConvertirEnBomba(centro);
            GestorPuntaje.Instancia.AgregarPuntos(20);
        }
        else
        {
            foreach (var tile in todos) DestruirJoya(tile);
            GestorPuntaje.Instancia.AgregarPuntos(10);
        }

        yield return StartCoroutine(ProcesoPostDestruccion());
    }

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
            Dictionary<GeneradorJoyas, List<GeneradorJoyas>> matchPorTile = new Dictionary<GeneradorJoyas, List<GeneradorJoyas>>();

            for (int i = 0; i < tablero.ancho; i++)
            {
                for (int j = 0; j < tablero.largo; j++)
                {
                    GeneradorJoyas tile = tablero.allTiles[i, j];
                    if (tile.joyaActual == null || tile.esSupergema) continue;

                    List<GeneradorJoyas> match = GetMatch(tile);
                    if (match.Count >= 3)
                    {
                        todosLosMatches.UnionWith(match);
                        matchPorTile[tile] = match;
                        huboCambios = true;
                    }
                }
            }

            if (!huboCambios) break;

            GeneradorJoyas centroBomba = null;
            GeneradorJoyas centroSuper = null;
            int maxMatch = 0;

            foreach (var par in matchPorTile)
            {
                if (par.Value.Count > maxMatch)
                {
                    maxMatch = par.Value.Count;
                    if (par.Value.Count >= 5)
                        centroSuper = par.Key;
                    else if (par.Value.Count == 4)
                        centroBomba = par.Key;
                }
            }

            foreach (var tile in todosLosMatches)
            {
                if (tile != centroBomba && tile != centroSuper)
                    DestruirJoya(tile);
            }

            if (centroSuper != null)
            {
                ConvertirEnSupergema(centroSuper);
                GestorPuntaje.Instancia.AgregarPuntos(5);
            }
            else if (centroBomba != null)
            {
                ConvertirEnBomba(centroBomba);
                GestorPuntaje.Instancia.AgregarPuntos(4);
            }
            else
            {
                GestorPuntaje.Instancia.AgregarPuntos(3);
            }
            yield return new WaitForSeconds(0.3f);
            RellenarTablero();
            yield return new WaitForSeconds(0.4f);
        }
    }

    public void DestruirJoya(GeneradorJoyas tile)
    {
        if (tile == null || tile.joyaActual == null) return;

        bool eraBomba = tile.esBomba;
        tile.esBomba = false;
        tile.esSupergema = false;

        Destroy(tile.joyaActual);
        tile.joyaActual = null;

        if (eraBomba) ExplotarArea(tile);
    }

    private void ExplotarArea(GeneradorJoyas centro)
    {
        int col = centro.columnas;
        int fil = centro.filas;

        List<GeneradorJoyas> aDestruir = new List<GeneradorJoyas>();

        for (int i = col - 1; i <= col + 1; i++)
            for (int j = fil - 1; j <= fil + 1; j++)
            {
                if (i < 0 || i >= tablero.ancho || j < 0 || j >= tablero.largo) continue;
                GeneradorJoyas vecino = tablero.allTiles[i, j];
                if (vecino == centro || vecino.joyaActual == null) continue;
                aDestruir.Add(vecino);
            }

        foreach (var tile in aDestruir) DestruirJoya(tile);
    }

    private void ExplotarColor(TipoJoya tipo)
    {
        for (int i = 0; i < tablero.ancho; i++)
            for (int j = 0; j < tablero.largo; j++)
            {
                GeneradorJoyas tile = tablero.allTiles[i, j];
                if (tile.joyaActual == null || tile.esSupergema) continue;
                if (ObtenerTipo(tile) == tipo) DestruirJoya(tile);
            }
    }

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

            lista.Add(vecino);
            c += dc;
            f += df;
        }
        return lista;
    }

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
        if (tile.superGema == null) { Debug.LogWarning("Falta superGema en " + tile.name); return; }

        Destroy(tile.joyaActual);
        tile.joyaActual = null;
        tile.esBomba = false;
        tile.esSupergema = false;

        GameObject superObj = Instantiate(tile.superGema, tile.transform.position, Quaternion.identity, tile.transform);
        tile.joyaActual = superObj;
        tile.esSupergema = true;
    }

    private TipoJoya ObtenerTipo(GeneradorJoyas tile)
    {
        if (tile.joyaActual == null) return default;

        Joya j = tile.joyaActual.GetComponent<Joya>();
        if (j != null) return j.tipo;

        Bomba b = tile.joyaActual.GetComponent<Bomba>();
        if (b != null) return b.tipo;

        return default;
    }

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