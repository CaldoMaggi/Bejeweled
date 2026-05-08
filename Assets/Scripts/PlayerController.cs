using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private GeneradorJoyas tileSeleccionado;
    public DetectorMatch detector;

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
                    if (tileSeleccionado == null)
                    {
                        tileSeleccionado = tileClickeado;
                        Debug.Log($"Seleccionado: {tileSeleccionado.name}");
                    }
                    else
                    {
                        int dx = Mathf.Abs(tileSeleccionado.columnas - tileClickeado.columnas);
                        int dy = Mathf.Abs(tileSeleccionado.filas - tileClickeado.filas);

                        // Solo vecinos directos, nada diagonal
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
        EjecutarSwap(a, b);
        detector.ChequearDespuesDeSwap(a, b, () => EjecutarSwap(a, b));
    }

    // Separamos el swap en su propia función para reutilizarla al revertir
    public void EjecutarSwap(GeneradorJoyas a, GeneradorJoyas b)
    {
        Vector3 posA = a.joyaActual.transform.position;
        Vector3 posB = b.joyaActual.transform.position;

        a.joyaActual.transform.position = posB;
        b.joyaActual.transform.position = posA;

        a.joyaActual.transform.SetParent(b.transform);
        b.joyaActual.transform.SetParent(a.transform);

        GameObject temp = a.joyaActual;
        a.joyaActual = b.joyaActual;
        b.joyaActual = temp;

        // ← Esto es lo nuevo
        bool tempBomba = a.esBomba;
        bool tempSuper = a.esSupergema;
        a.esBomba = b.esBomba;
        a.esSupergema = b.esSupergema;
        b.esBomba = tempBomba;
        b.esSupergema = tempSuper;
    }
}