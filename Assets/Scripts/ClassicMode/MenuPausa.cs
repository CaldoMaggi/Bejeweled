using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public static MenuPausa Instancia; // ← singleton

    public GameObject menuPausa;
    public static bool Pausado = false;

    private void Awake()
    {
        Instancia = this;
    }

    public void Pausa()
    {
        Pausado = !Pausado;
        menuPausa.SetActive(Pausado);
        Time.timeScale = Pausado ? 0f : 1f;
    }

    public void Reanudar()
    {
        Pausado = false;
        menuPausa.SetActive(false);
        Time.timeScale = 1f;
    }

    public void HomeBoton()
    {
        Pausado = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("PantallaInicio");
    }
}