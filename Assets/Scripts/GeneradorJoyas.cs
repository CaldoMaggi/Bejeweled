using UnityEngine;

public enum TipoJoya { Roja, Azul, Verde, Amarilla, Morada, Naranja }

public class GeneradorJoyas : MonoBehaviour
{
    public GameObject[] joyas;// lista de joyas

    public TipoJoya tipo;
    public bool Bomba4 = false; // match de 4 (destruye 3x3 tiles)
    public bool Cubo5 = false; // match de 5 (destruye joyas del mismo valor)
    public int filas;
    public int columnas;
    public GameObject joyaActual;

    private void Start()
    {
        Iniciar();
    }   

    private void Iniciar()
    {
        int joyaUsada = Random.Range(0, joyas.Length);
        joyaActual = Instantiate(joyas[joyaUsada], transform.position, Quaternion.identity);
        joyaActual.transform.parent = this.transform;
        joyaActual.name = this.gameObject.name;
    }
}
