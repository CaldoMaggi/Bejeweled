using TMPro;
using UnityEngine;
using System.Collections;   

public class ValoresMatchTexto : MonoBehaviour
{
    public TextMeshPro texto;
    public float duracion = 1f;
    public float velocidadSubida = 1f;

    public void Iniciar(int puntos, Vector3 posicion)
    {
        transform.position = posicion;
        texto.text = $"+{puntos}";
        StartCoroutine(Animar());
    }

    private IEnumerator Animar()
    {
        float tiempo = 0f;
        Color colorInicial = texto.color;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            transform.position += Vector3.up * velocidadSubida * Time.deltaTime;

            // Desvanece al final
            float alpha = Mathf.Lerp(1f, 0f, tiempo / duracion);
            texto.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, alpha);

            yield return null;
        }

        Destroy(gameObject);
    }
}
