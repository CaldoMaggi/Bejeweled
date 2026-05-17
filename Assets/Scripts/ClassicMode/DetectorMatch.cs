using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectorMatch : MonoBehaviour
{
    public Tablero tablero;
    private bool esMovimientoManual = false;

    public void ChequearDespuesDeSwap(GeneradorJoyas a, GeneradorJoyas b, System.Action revertir)
    {
        esMovimientoManual = true;
        StartCoroutine(ChequearYResolver(a, b, revertir));
    }

    // ─── Helper: revisa si hay JoyaTiempo en el grupo y suma al banco ───
    private void RegistrarBonusTiempo(IEnumerable<GeneradorJoyas> tiles)
    {
        if (GestorContrarreloj.Instancia == null) return;

        foreach (GeneradorJoyas tile in tiles)
        {
            if (tile.joyaActual == null) continue;
            JoyaTiempo bonus = tile.joyaActual.GetComponent<JoyaTiempo>();
            if (bonus != null)
            {
                float segundos = bonus.ObtenerBonus();
                Vector3 pos = tile.transform.position;
                ReinicioModoContrareloj.Instancia.AgregarTiempo(segundos);
                GestorPuntaje.Instancia.MostrarTextoFlotante((int)segundos, pos);
            }
        }
    }

    private IEnumerator ChequearYResolver(GeneradorJoyas a, GeneradorJoyas b, System.Action revertir)
    {
        yield return new WaitForSeconds(0.2f);

        if (a.EsSupergema || b.EsSupergema)
        {
            GeneradorJoyas supergema = a.EsSupergema ? a : b;
            GeneradorJoyas otra = a.EsSupergema ? b : a;
            TipoJoya tipoObjetivo = ObtenerTipo(otra);

            List<GeneradorJoyas> aExplotar = new List<GeneradorJoyas>();
            for (int i = 0; i < tablero.Ancho; i++)
                for (int j = 0; j < tablero.Largo; j++)
                {
                    GeneradorJoyas tile = tablero.allTiles[i, j];
                    if (tile.joyaActual == null || tile.EsSupergema) continue;
                    if (ObtenerTipo(tile) == tipoObjetivo)
                    {
                        GestorPuntaje.Instancia.MostrarTextoFlotante(
                            GestorPuntaje.Instancia.puntosSuperjoyaPorGema,
                            tile.transform.position
                        );
                        aExplotar.Add(tile);
                    }
                }

            RegistrarBonusTiempo(aExplotar); // ← antes de destruir

            int gemasExplotadas = ContarGemasDeColor(tipoObjetivo);
            ExplotarColor(tipoObjetivo);
            DestruirJoya(supergema);
            GestorPuntaje.Instancia.AgregarPuntos(
                gemasExplotadas * GestorPuntaje.Instancia.puntosSuperjoyaPorGema
            );
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

        RegistrarBonusTiempo(todos); // ← antes de destruir, aplica a match 3, 4 y 5

        if (largo >= 5)
        {
            foreach (var tile in todos)
                if (tile != centro) DestruirJoya(tile);
            ConvertirEnSupergema(centro);
            GestorPuntaje.Instancia.AgregarPuntos(GestorPuntaje.Instancia.puntosMatch4);
            GestorPuntaje.Instancia.MostrarTextoFlotante(
                GestorPuntaje.Instancia.puntosMatch4,
                centro.transform.position
            );
        }
        else if (largo == 4)
        {
            foreach (var tile in todos)
                if (tile != centro) DestruirJoya(tile);
            ConvertirEnBomba(centro);
            GestorPuntaje.Instancia.AgregarPuntos(GestorPuntaje.Instancia.puntosMatch4);
            GestorPuntaje.Instancia.MostrarTextoFlotante(
                GestorPuntaje.Instancia.puntosMatch4,
                centro.transform.position
            );
            if (SoundManager.Instancia != null)
                SoundManager.Instancia.ReproducirMatch4();
        }
        else
        {
            foreach (var tile in todos) DestruirJoya(tile);
            int pts = esMovimientoManual
                ? GestorPuntaje.Instancia.puntosMatch3Manual
                : GestorPuntaje.Instancia.puntosMatch3Cascada;
            GestorPuntaje.Instancia.AgregarPuntos(pts);
            GestorPuntaje.Instancia.MostrarTextoFlotante(pts, CentroDelMatch(todos));
            if (SoundManager.Instancia != null)
                SoundManager.Instancia.ReproducirMatch3();
        }

        esMovimientoManual = false;
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

            for (int i = 0; i < tablero.Ancho; i++)
                for (int j = 0; j < tablero.Largo; j++)
                {
                    GeneradorJoyas tile = tablero.allTiles[i, j];
                    if (tile.joyaActual == null || tile.EsSupergema) continue;

                    List<GeneradorJoyas> match = GetMatch(tile);
                    if (match.Count >= 3)
                    {
                        todosLosMatches.UnionWith(match);
                        matchPorTile[tile] = match;
                        huboCambios = true;
                    }
                }

            if (!huboCambios) break;

            RegistrarBonusTiempo(todosLosMatches); // ← antes de destruir en cascada

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
                GestorPuntaje.Instancia.AgregarPuntos(GestorPuntaje.Instancia.puntosMatch4);
                GestorPuntaje.Instancia.MostrarTextoFlotante(
                    GestorPuntaje.Instancia.puntosMatch4,
                    centroSuper.transform.position
                );
            }
            else if (centroBomba != null)
            {
                ConvertirEnBomba(centroBomba);
                GestorPuntaje.Instancia.AgregarPuntos(GestorPuntaje.Instancia.puntosMatch4);
                GestorPuntaje.Instancia.MostrarTextoFlotante(
                    GestorPuntaje.Instancia.puntosMatch4,
                    centroBomba.transform.position
                );
            }
            else
            {
                GestorPuntaje.Instancia.AgregarPuntos(GestorPuntaje.Instancia.puntosMatch3Cascada);
                GestorPuntaje.Instancia.MostrarTextoFlotante(
                    GestorPuntaje.Instancia.puntosMatch3Cascada,
                    CentroDelMatch(todosLosMatches)
                );
            }
            if (SoundManager.Instancia != null)
                SoundManager.Instancia.ReproducirMatch3();

            yield return new WaitForSeconds(0.3f);
            RellenarTablero();
            yield return new WaitForSeconds(0.4f);
        }
    }

    private Vector3 CentroDelMatch(HashSet<GeneradorJoyas> match)
    {
        Vector3 suma = Vector3.zero;
        foreach (var tile in match)
            suma += tile.transform.position;
        return suma / match.Count;
    }

    private int ContarGemasDeColor(TipoJoya tipo)
    {
        int count = 0;
        for (int i = 0; i < tablero.Ancho; i++)
            for (int j = 0; j < tablero.Largo; j++)
            {
                GeneradorJoyas tile = tablero.allTiles[i, j];
                if (tile.joyaActual == null || tile.EsSupergema) continue;
                if (ObtenerTipo(tile) == tipo) count++;
            }
        return count;
    }

    public void DestruirJoya(GeneradorJoyas tile)
    {
        if (tile == null || tile.joyaActual == null) return;

        bool eraBomba = tile.EsBomba;
        tile.EsBomba = false;
        tile.EsSupergema = false;

        Destroy(tile.joyaActual);
        tile.joyaActual = null;

        if (eraBomba)
        {
            GestorPuntaje.Instancia.AgregarPuntos(GestorPuntaje.Instancia.puntosBomba);
            if (SoundManager.Instancia != null)
                SoundManager.Instancia.ReproducirBomba();
            ExplotarArea(tile);
        }
    }

    private void ExplotarArea(GeneradorJoyas centro)
    {
        int col = centro.Columnas;
        int fil = centro.Filas;

        List<GeneradorJoyas> aDestruir = new List<GeneradorJoyas>();

        for (int i = col - 1; i <= col + 1; i++)
            for (int j = fil - 1; j <= fil + 1; j++)
            {
                if (i < 0 || i >= tablero.Ancho || j < 0 || j >= tablero.Largo) continue;
                GeneradorJoyas vecino = tablero.allTiles[i, j];
                if (vecino == centro || vecino.joyaActual == null) continue;
                aDestruir.Add(vecino);
            }

        foreach (var tile in aDestruir) DestruirJoya(tile);
    }

    private void ExplotarColor(TipoJoya tipo)
    {
        for (int i = 0; i < tablero.Ancho; i++)
            for (int j = 0; j < tablero.Largo; j++)
            {
                GeneradorJoyas tile = tablero.allTiles[i, j];
                if (tile.joyaActual == null || tile.EsSupergema) continue;
                if (ObtenerTipo(tile) == tipo) DestruirJoya(tile);
            }
    }

    private List<GeneradorJoyas> GetMatch(GeneradorJoyas tile)
    {
        if (tile.joyaActual == null || tile.EsSupergema)
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
        int c = origen.Columnas + dc;
        int f = origen.Filas + df;

        while (true)
        {
            if (c < 0 || c >= tablero.Ancho || f < 0 || f >= tablero.Largo) break;
            GeneradorJoyas vecino = tablero.allTiles[c, f];
            if (vecino.joyaActual == null || vecino.EsSupergema) break;
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
        tile.EsBomba = false;
        tile.EsSupergema = false;

        GameObject bombaObj = Instantiate(prefabBomba, tile.transform.position, Quaternion.identity, tile.transform);
        bombaObj.GetComponent<Bomba>().tipo = tipoAnterior;
        tile.joyaActual = bombaObj;
        tile.EsBomba = true;

        // ← sonido al crear
        if (SoundManager.Instancia != null)
            SoundManager.Instancia.ReproducirSpawnBomba();
    }

    private void ConvertirEnSupergema(GeneradorJoyas tile)
    {
        if (tile.superGema == null) { Debug.LogWarning("Falta superGema en " + tile.name); return; }

        Destroy(tile.joyaActual);
        tile.joyaActual = null;
        tile.EsBomba = false;
        tile.EsSupergema = false;

        GameObject superObj = Instantiate(tile.superGema, tile.transform.position, Quaternion.identity, tile.transform);
        tile.joyaActual = superObj;
        tile.EsSupergema = true;

        if (SoundManager.Instancia != null)
            SoundManager.Instancia.ReproducirSupergema();
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
        for (int i = 0; i < tablero.Ancho; i++)
            for (int j = 0; j < tablero.Largo; j++)
            {
                if (tablero.allTiles[i, j].joyaActual != null) continue;

                for (int k = j + 1; k < tablero.Largo; k++)
                {
                    GeneradorJoyas superior = tablero.allTiles[i, k];
                    if (superior.joyaActual == null) continue;

                    GeneradorJoyas hueco = tablero.allTiles[i, j];
                    superior.joyaActual.transform.SetParent(hueco.transform);
                    superior.joyaActual.transform.position = hueco.transform.position;
                    hueco.joyaActual = superior.joyaActual;
                    hueco.EsBomba = superior.EsBomba;
                    hueco.EsSupergema = superior.EsSupergema;
                    superior.joyaActual = null;
                    superior.EsBomba = false;
                    superior.EsSupergema = false;
                    break;
                }
            }

        for (int i = 0; i < tablero.Ancho; i++)
            for (int j = 0; j < tablero.Largo; j++)
                if (tablero.allTiles[i, j].joyaActual == null)
                    tablero.allTiles[i, j].SpawnJoya();

        // ← sonido al rellenar
        if (SoundManager.Instancia != null)
            SoundManager.Instancia.ReproducirCaida();
    }
}