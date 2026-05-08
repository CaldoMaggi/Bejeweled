using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private GeneradorJoyas tileSeleccionado;
    public DetectorMatch detector;

    public GameObject prefabSeleccion;
    private GameObject seleccionVisual;

    private GameObject seleccionOrigen;    // Primer clic
    private GameObject seleccionObjetivo; //segundo click

    void Start()
    {
        seleccionVisual = Instantiate(prefabSeleccion);
        seleccionVisual.SetActive(false);

        seleccionOrigen = Instantiate(prefabSeleccion);
        seleccionObjetivo = Instantiate(prefabSeleccion);

        seleccionOrigen.SetActive(false);
        seleccionObjetivo.SetActive(false);
    }

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
                    ManejarSeleccion(tileClickeado);
                }
            }
        }

        // Cancelar con clic derecho
        if (Input.GetMouseButtonDown(1)) DesactivarVisuales();
    }

    private void ManejarSeleccion(GeneradorJoyas tileClickeado)
    {
        if (tileSeleccionado == null)
        {
            // --- PRIMER MOVIMIENTO (ORIGEN) ---
            tileSeleccionado = tileClickeado;
            seleccionOrigen.transform.position = tileSeleccionado.transform.position;
            seleccionOrigen.SetActive(true);
        }
        else
        {
            // --- SEGUNDO MOVIMIENTO (DESTINO) ---
            int dx = Mathf.Abs(tileSeleccionado.columnas - tileClickeado.columnas);
            int dy = Mathf.Abs(tileSeleccionado.filas - tileClickeado.filas);

            if (dx + dy == 1)
            {
                // Es un vecino válido: mostramos el segundo cuadro un instante
                seleccionObjetivo.transform.position = tileClickeado.transform.position;
                seleccionObjetivo.SetActive(true);

                // Ejecutamos el swap
                SwapJoyas(tileSeleccionado, tileClickeado);

                // Limpiamos después de un breve momento o inmediatamente
                Invoke("DesactivarVisuales", 0.2f);
            }
            else
            {
                // Si toca una gema lejana, cambiamos el origen a esa nueva gema
                tileSeleccionado = tileClickeado;
                seleccionOrigen.transform.position = tileSeleccionado.transform.position;
                seleccionObjetivo.SetActive(false);
            }
        }
    }

    private void DesactivarVisuales()
    {
        seleccionOrigen.SetActive(false);
        seleccionObjetivo.SetActive(false);
        tileSeleccionado = null;
    }

    private void SwapJoyas(GeneradorJoyas a, GeneradorJoyas b)
    {
        EjecutarSwap(a, b);
        detector.ChequearDespuesDeSwap(a, b, () => EjecutarSwap(a, b));
    }

    public void EjecutarSwap(GeneradorJoyas a, GeneradorJoyas b)
    {
        // ... (Tu lógica de EjecutarSwap se mantiene igual)
        Vector3 posA = a.joyaActual.transform.position;
        Vector3 posB = b.joyaActual.transform.position;

        a.joyaActual.transform.position = posB;
        b.joyaActual.transform.position = posA;

        a.joyaActual.transform.SetParent(b.transform);
        b.joyaActual.transform.SetParent(a.transform);

        GameObject temp = a.joyaActual;
        a.joyaActual = b.joyaActual;
        b.joyaActual = temp;

        // Intercambio de estados (Bomba/Supergema)
        bool tempBomba = a.esBomba;
        bool tempSuper = a.esSupergema;
        a.esBomba = b.esBomba;
        a.esSupergema = b.esSupergema;
        b.esBomba = tempBomba;
        b.esSupergema = tempSuper;
    }
}