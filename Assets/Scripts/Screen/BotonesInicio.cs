using UnityEngine;
using UnityEngine.SceneManagement;

public class BotonesInicio : MonoBehaviour
{
    public void JugarClasico()
    {
        SceneManager.LoadScene("NivelClasico");
    }

    /*public void JugarTempo()
    {
        SceneManager.LoadScene("Tempo");
    }*/

    public void Salir()
    {
        Application.Quit();
    }
}
