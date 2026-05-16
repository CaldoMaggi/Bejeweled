using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReinicioModoClasico : MonoBehaviour
{
    public static ReinicioModoClasico Instancia;
    public Tablero tablero;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    public void ReiniciarTablero()
    {
        StartCoroutine(ReiniciarConAnimacion());
    }

    private IEnumerator ReiniciarConAnimacion()
    {
        // 1. Guardar potenciadores existentes
        List<PotenciadorGuardadoClasico> potenciadores = new List<PotenciadorGuardadoClasico>();

        for (int i = 0; i < tablero.Ancho; i++)
            for (int j = 0; j < tablero.Largo; j++)
            {
                GeneradorJoyas tile = tablero.allTiles[i, j];
                if (tile.EsBomba || tile.EsSupergema)
                {
                    potenciadores.Add(new PotenciadorGuardadoClasico
                    {
                        columna = i,
                        fila = j,
                        esBomba = tile.EsBomba,
                        esSupergema = tile.EsSupergema,
                        tipo = tile.GetTipo()
                    });
                }
            }

        // 2. Lanzar el refresco con animación (destruye todo y respawnea)
        yield return StartCoroutine(tablero.RefrescarTablero());

        // 3. Restaurar potenciadores encima de las nuevas gemas
        foreach (var p in potenciadores)
        {
            GeneradorJoyas tile = tablero.allTiles[p.columna, p.fila];

            // Destruir la gema normal que acaba de spawnear RefrescarTablero
            if (tile.joyaActual != null)
            {
                Destroy(tile.joyaActual);
                tile.joyaActual = null;
            }

            tile.EsBomba = p.esBomba;
            tile.EsSupergema = p.esSupergema;

            // Respawnear el potenciador en su posición final (ya animada)
            tile.SpawnJoya();
        }
    }


    public class PotenciadorGuardadoClasico
    {
        public int columna;
        public int fila;
        public bool esBomba;
        public bool esSupergema;
        public TipoJoya tipo;
    }
}