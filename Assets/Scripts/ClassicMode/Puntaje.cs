using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GestorPuntaje : MonoBehaviour
{
    public static GestorPuntaje Instancia; //singleton para acceder desde otros scripts

    public int puntosMatch3Manual = 10;
    public int puntosMatch3Cascada = 20;
    public int puntosMatch4 = 30;
    public int puntosBomba = 35;
    public int puntosSuperjoyaPorGema = 15;

    public TextMeshProUGUI textoPuntaje;
    public Image barraProgreso;
    private int puntajeMax = 500;

    private int puntaje = 0;
    private int nivelActual = 1;

    public GameObject prefabTextoFlotante;

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        ActualizarUI();
    }

    public void AgregarPuntos(int puntos)
    {
        puntaje += puntos * nivelActual; // Suma los puntos obtenidos al puntaje total multiplicado por el nivel actual
        puntaje = Mathf.Min(puntaje, puntajeMax);
        ActualizarUI();

        if (puntaje >= puntajeMax)
        {
            NivelGanado();
        }
    }
    public void MostrarTextoFlotante(int puntos, Vector3 posicion)
    {
        GameObject obj = Instantiate(prefabTextoFlotante, posicion, Quaternion.identity);
        obj.GetComponent<ValoresMatchTexto>().Iniciar(puntos * nivelActual, posicion);
    }

    private int puntajeNivelAnterior = 0; // Puntos con los que empezaste el nivel actual

    private void ActualizarUI()
    {
        if (textoPuntaje != null)
            textoPuntaje.text = $"{puntaje}";

        if (barraProgreso != null)
        {
            // Calcula cuánto se ha progresado RELATIVO al nivel actual
            float puntosEnEsteNivel = puntaje - puntajeNivelAnterior;
            float metaEsteNivel = puntajeMax - puntajeNivelAnterior;

            barraProgreso.fillAmount = puntosEnEsteNivel / metaEsteNivel;
        }
    }

    private void NivelGanado()
    {
        Debug.Log("¡Nivel ganado!");
        nivelActual++;

        // Antes de subir la meta, se guarda el puntaje actual como base
        puntajeNivelAnterior = puntaje;
        puntajeMax += 500; // La nueva meta será 1000, luego 1500, etc.
        ActualizarUI(); // Ahora la barra dará 0 / 500 -> 0%
        ReinicioModoClasico.Instancia.ReiniciarTablero();
    }
    public int GetPuntaje() => puntaje;
}