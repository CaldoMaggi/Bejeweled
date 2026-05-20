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

    public GameObject prefabTextoFlotante;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ActualizarUI();
    }

    public void AgregarPuntos(int puntos)
    {
        puntaje += puntos;

        // Solo limita a 500 en modo clásico
        if (GestorContrarreloj.Instancia == null)
            puntaje = Mathf.Min(puntaje, puntajeMax);

        ActualizarUI();

        if (puntaje >= puntajeMax && GestorContrarreloj.Instancia == null)
            NivelGanado();
    }
    public void MostrarTextoFlotante(int puntos, Vector3 posicion)
    {
        GameObject obj = Instantiate(prefabTextoFlotante, posicion, Quaternion.identity);
        obj.GetComponent<ValoresMatchTexto>().Iniciar(puntos, posicion);
    }

    private int puntajeNivelAnterior = 0; // Puntos con los que empezaste el nivel actual

    private void ActualizarUI()
    {
        if (textoPuntaje != null)
            textoPuntaje.text = $"{puntaje}";
        if (barraProgreso != null)
        {
            if (GestorContrarreloj.Instancia != null)
            {
                barraProgreso.gameObject.SetActive(false); // oculta la barra en contrarreloj
            }
            else
            {
                barraProgreso.gameObject.SetActive(true);
                float puntosEnEsteNivel = puntaje - puntajeNivelAnterior;
                float metaEsteNivel = puntajeMax - puntajeNivelAnterior;
                barraProgreso.fillAmount = puntosEnEsteNivel / metaEsteNivel;
            }
        }
    }

    private void NivelGanado()
    {
        Debug.Log("¡Nivel ganado!");

        // Antes de subir la meta, se guarda el puntaje actual como base
        puntajeNivelAnterior = puntaje;
        puntajeMax += 500; // La nueva meta será 1000, luego 1500, etc.
        ActualizarUI(); // Ahora la barra dará 0 / 500 -> 0%
        ReinicioModoClasico.Instancia.ReiniciarTablero();
    }
    public int GetPuntaje() => puntaje;
}