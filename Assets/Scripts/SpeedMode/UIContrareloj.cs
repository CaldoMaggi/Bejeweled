using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIContrarreloj : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI textoBancoTiempo;
    public GameObject panelGameOver;
    public TextMeshProUGUI textoBonusPopup;
    public Image barraProgreso;
    private float tiempoMaximo;

    void Start()
    {
        panelGameOver.SetActive(false);
        tiempoMaximo = ReinicioModoContrareloj.Instancia.TiempoLimite;
        ReinicioModoContrareloj.Instancia.OnTiempoActualizado += ActualizarBarra;
        ReinicioModoContrareloj.Instancia.OnTiempoActualizado += ActualizarTimer;
        ReinicioModoContrareloj.Instancia.OnBonusTiempo += MostrarPopup;
        ReinicioModoContrareloj.Instancia.OnGameOver += MostrarPantallaGameOver;
    }

    void OnDestroy()
    {
        if (ReinicioModoContrareloj.Instancia == null) return;
        ReinicioModoContrareloj.Instancia.OnTiempoActualizado -= ActualizarBarra;
        ReinicioModoContrareloj.Instancia.OnTiempoActualizado -= ActualizarTimer;
        ReinicioModoContrareloj.Instancia.OnBonusTiempo -= MostrarPopup;
        ReinicioModoContrareloj.Instancia.OnGameOver -= MostrarPantallaGameOver;
    }

    void Update()
    {
        if (textoBancoTiempo != null && ReinicioModoContrareloj.Instancia != null)
            textoBancoTiempo.text = $"Tiempo extra: {ReinicioModoContrareloj.Instancia.TiempoAcumulado:F1}s";
    }

    void ActualizarBarra(float tiempoActual)
    {
        barraProgreso.fillAmount = tiempoActual / tiempoMaximo;
    }

    void ActualizarTimer(float tiempo)
    {
        timerText.text = $"{Mathf.CeilToInt(tiempo)}s";
    }

    void MostrarPopup(float bonus)
    {
        if (textoBonusPopup == null) return;
        textoBonusPopup.text = $"+{bonus}s";
    }

    void MostrarPantallaGameOver()
    {
        panelGameOver.SetActive(true);
    }
}