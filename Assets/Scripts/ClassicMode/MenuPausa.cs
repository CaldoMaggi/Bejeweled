using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public static MenuPausa Instancia; // ← singleton para q no moleste en los dos niveles

    public GameObject menuPausa;
    public static bool Pausado = false;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoBoton;

    private void Awake()
    {
        Instancia = this;
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void Pausa()
    {
        Pausado = !Pausado;
        menuPausa.SetActive(Pausado);
        Time.timeScale = Pausado ? 0f : 1f;
        if (audioSource != null && sonidoBoton != null)
        {
            audioSource.PlayOneShot(sonidoBoton);
        }
    }

    public void Reanudar()
    {
        Pausado = false;
        menuPausa.SetActive(false);
        Time.timeScale = 1f;
        if (audioSource != null && sonidoBoton != null)
        {
            audioSource.PlayOneShot(sonidoBoton);
        }
    }

    public void HomeBotonContrareloj()
    {
        ReinicioModoContrareloj.Instancia.ReiniciarPartidaCompleta();
        Pausado = false;
        Time.timeScale = 1f;
        if (audioSource != null && sonidoBoton != null)
        {
            audioSource.PlayOneShot(sonidoBoton);
        }
        SceneManager.LoadScene("PantallaInicio");
    }

    public void HomeBoton()
    {
        Pausado = false;
        Time.timeScale = 1f;
        if (audioSource != null && sonidoBoton != null)
        {
            audioSource.PlayOneShot(sonidoBoton);
        }
        SceneManager.LoadScene("PantallaInicio");
    }
}