using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GestorPuntaje : MonoBehaviour
{
    public static GestorPuntaje Instancia;

    public TextMeshProUGUI textoPuntaje;
    public Image barraProgreso;
    private int puntajeMax = 500;

    private int puntaje = 0;

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
        puntaje += puntos;
        puntaje = Mathf.Min(puntaje, puntajeMax);
        ActualizarUI();

        // Si el puntaje llegó al máximo, llama al método que está opaco
        if (puntaje >= puntajeMax)
        {
            NivelGanado();
        }
    }

    private int puntajeNivelAnterior = 0; // Puntos con los que empezaste el nivel actual

    private void ActualizarUI()
    {
        if (textoPuntaje != null)
            textoPuntaje.text = $"{puntaje}";

        if (barraProgreso != null)
        {
            // Calculamos cuánto hemos progresado RELATIVO al nivel actual
            float puntosEnEsteNivel = puntaje - puntajeNivelAnterior;
            float metaEsteNivel = puntajeMax - puntajeNivelAnterior;

            barraProgreso.fillAmount = puntosEnEsteNivel / metaEsteNivel;
        }
    }
    public int GetPuntaje() => puntaje;

    private void NivelGanado()
    {
        Debug.Log("¡Nivel ganado!");

        // Antes de subir la meta, guardamos el puntaje actual como base
        puntajeNivelAnterior = puntaje;

        puntajeMax += 500; // La nueva meta será 1000, luego 1500, etc.

        ActualizarUI(); // Ahora la barra dará 0 / 500 -> 0%
        ReinicioModoClasico.Instancia.ReiniciarTablero();
    }
}