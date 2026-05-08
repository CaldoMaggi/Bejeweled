using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tablero : MonoBehaviour
{
    public int ancho = 8;
    public int largo = 8;
    public GameObject PrefabTile;
    public GeneradorJoyas[,] allTiles;

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
                Vector2 tempPosition = new Vector2(i, j);
                GameObject backgroundTile = Instantiate(PrefabTile, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform;
                backgroundTile.name = "(" + i + "," + j + ")";

                GeneradorJoyas tile = backgroundTile.GetComponent<GeneradorJoyas>();
                tile.columnas = i;
                tile.filas = j;
                tile.tablero = this;
                allTiles[i, j] = tile;
            }
        }
    }
}