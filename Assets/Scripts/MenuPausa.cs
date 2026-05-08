using UnityEngine;

public class MenuPausa : MonoBehaviour
{
    public GameObject menuPausa;
    bool pausado = false;

    public void Pausa()
    {
        pausado = !pausado;
        menuPausa.SetActive(pausado);
        Time.timeScale = pausado ? 0f : 1f; // 0 = congela todo
    }

    public void Reanudar()
    {
        pausado = false;
        menuPausa.SetActive(false);
        Time.timeScale = 1f;
    }
}
