using UnityEngine;
using System.Collections.Generic;

public class ReinicioModoClasico : MonoBehaviour
{
    public static ReinicioModoClasico Instancia;
    public Tablero tablero;

    private void Awake()
    {
        Instancia = this;
    }

    public void ReiniciarTablero()
    {
        // Guardar potenciadores existentes
        List<PotenciadorGuardado> potenciadores = new List<PotenciadorGuardado>();

        for (int i = 0; i < tablero.ancho; i++)
            for (int j = 0; j < tablero.largo; j++)
            {
                GeneradorJoyas tile = tablero.allTiles[i, j];
                if (tile.esBomba || tile.esSupergema)
                {
                    potenciadores.Add(new PotenciadorGuardado
                    {
                        columna = i,
                        fila = j,
                        esBomba = tile.esBomba,
                        esSupergema = tile.esSupergema,
                        tipo = tile.GetTipo()
                    });
                }
            }

        // Destruir todas las joyas normales
        for (int i = 0; i < tablero.ancho; i++)
            for (int j = 0; j < tablero.largo; j++)
            {
                GeneradorJoyas tile = tablero.allTiles[i, j];
                if (!tile.esBomba && !tile.esSupergema && tile.joyaActual != null)
                {
                    Object.Destroy(tile.joyaActual);
                    tile.joyaActual = null;
                }
            }

        // Respawnear joyas normales en los huecos
        for (int i = 0; i < tablero.ancho; i++)
            for (int j = 0; j < tablero.largo; j++)
                if (tablero.allTiles[i, j].joyaActual == null)
                    tablero.allTiles[i, j].SpawnJoya();
    }
}

public class PotenciadorGuardado
{
    public int columna;
    public int fila;
    public bool esBomba;
    public bool esSupergema;
    public TipoJoya tipo;
}