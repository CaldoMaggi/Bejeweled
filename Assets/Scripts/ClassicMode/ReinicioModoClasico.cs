using UnityEngine;
using System.Collections.Generic;

public class ReinicioModoClasico : MonoBehaviour
{
    public static ReinicioModoClasico Instancia; //singleton para acceder desde otros scripts
    public Tablero tablero;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    public void ReiniciarTablero()
    {
        // Guardar potenciadores existentes
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

        // Destruir todas las joyas normales
        for (int i = 0; i < tablero.Ancho; i++)
            for (int j = 0; j < tablero.Largo; j++)
            {
                GeneradorJoyas tile = tablero.allTiles[i, j];
                if (!tile.EsBomba && !tile.EsSupergema && tile.joyaActual != null)
                {
                    Object.Destroy(tile.joyaActual);
                    tile.joyaActual = null;
                }
            }

        // Respawnear joyas normales en los huecos
        for (int i = 0; i < tablero.Ancho; i++)
            for (int j = 0; j < tablero.Largo; j++)
                if (tablero.allTiles[i, j].joyaActual == null)
                    tablero.allTiles[i, j].SpawnJoya();
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