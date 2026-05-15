using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIContrarreloj : MonoBehaviour
{
    public TextMeshProUGUI timerText;       // el texto del cronómetro en pantalla
    public GameObject panelGameOver;        // el panel que aparece al perder
    public TextMeshProUGUI textoBonusPopup; // el "+5s" flotante (puede ser null si no lo tienes)
    public Image barraProgreso; // arrastra el fill de la barra amarilla
    private float tiempoMaximo;

    void ActualizarBarra(float tiempoActual)
    {
        barraProgreso.fillAmount = tiempoActual / tiempoMaximo; // va de 1 a 0
    }

    void Start()
    {
        panelGameOver.SetActive(false);
        tiempoMaximo = ReinicioModoContrareloj.Instancia.TiempoLimite; // ← TiempoLimite, no TiempoRestante
        ReinicioModoContrareloj.Instancia.OnTiempoActualizado += ActualizarBarra;
        ReinicioModoContrareloj.Instancia.OnTiempoActualizado += ActualizarTimer;
        ReinicioModoContrareloj.Instancia.OnBonusTiempo += MostrarPopup;
        ReinicioModoContrareloj.Instancia.OnGameOver += MostrarPantallaGameOver;
    }

    void OnDestroy()
    {
        if (ReinicioModoContrareloj.Instancia == null) return;

        ReinicioModoContrareloj.Instancia.OnTiempoActualizado -= ActualizarBarra;  // ← faltaba esta
        ReinicioModoContrareloj.Instancia.OnTiempoActualizado -= ActualizarTimer;
        ReinicioModoContrareloj.Instancia.OnBonusTiempo -= MostrarPopup;
        ReinicioModoContrareloj.Instancia.OnGameOver -= MostrarPantallaGameOver;
    }

    void ActualizarTimer(float tiempo)
    {
        timerText.text = $"{Mathf.CeilToInt(tiempo)}s";
    }

    void MostrarPopup(float bonus)
    {
        if (textoBonusPopup == null) return;
        textoBonusPopup.text = $"+{bonus}s";
        // si tienes animación o coroutine para que desaparezca, la llamas aquí
    }

    void MostrarPantallaGameOver()
    {
        panelGameOver.SetActive(true);
    }
}