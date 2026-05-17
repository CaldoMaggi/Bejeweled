using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BotonesInicio : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip animacionFinal;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoBoton;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void JugarClasico()
    {
        StartCoroutine(CambioEscena("NivelClasico"));
    }

    public void JugarTempo()
    {
        StartCoroutine(CambioEscena("NivelContrareloj"));
    }

    IEnumerator CambioEscena(string nombreEscena)
    {
        if (audioSource != null && sonidoBoton != null)
        {
            audioSource.PlayOneShot(sonidoBoton);
        }

        if (animator != null)
        {
            animator.SetTrigger("Iniciar");
            yield return new WaitForSeconds(animacionFinal.length);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        SceneManager.LoadScene(nombreEscena);
    }

    public void Salir()
    {
        Application.Quit();
    }
}