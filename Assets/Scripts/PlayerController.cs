using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private GeneradorJoyas tileSeleccionado;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                GeneradorJoyas tileClickeado = hit.collider.GetComponentInParent<GeneradorJoyas>();

                if (tileClickeado != null)
                {
                    // Primer click — seleccionar
                    if (tileSeleccionado == null)
                    {
                        tileSeleccionado = tileClickeado;
                        Debug.Log($"Seleccionado: {tileSeleccionado.name}");
                    }
                    // Segundo click — intentar swap
                    else
                    {
                        int dx = Mathf.Abs(tileSeleccionado.columnas - tileClickeado.columnas);
                        int dy = Mathf.Abs(tileSeleccionado.filas - tileClickeado.filas);

                        // Solo permite vecinos directos (arriba, abajo, izquierda, derecha)
                        if (dx + dy == 1)
                        {
                            SwapJoyas(tileSeleccionado, tileClickeado);
                        }

                        tileSeleccionado = null;
                    }
                }
            }
        }
    }

    private void SwapJoyas(GeneradorJoyas a, GeneradorJoyas b)
    {
        // Intercambiar posiciones
        Vector3 posA = a.joyaActual.transform.position;
        Vector3 posB = b.joyaActual.transform.position;

        a.joyaActual.transform.position = posB;
        b.joyaActual.transform.position = posA;

        // Intercambiar parents
        a.joyaActual.transform.SetParent(b.transform);
        b.joyaActual.transform.SetParent(a.transform);

        // Intercambiar referencias
        GameObject temp = a.joyaActual;
        a.joyaActual = b.joyaActual;
        b.joyaActual = temp;

        Debug.Log($"Swap: {a.name} <-> {b.name}");
    }
}