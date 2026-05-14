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
        seleccionOrigen = Instantiate(prefabSeleccion);
        seleccionOrigen.SetActive(false);
        seleccionObjetivo = Instantiate(prefabSeleccion);
        seleccionObjetivo.SetActive(false);
    }

    void Update()
    {
        if (MenuPausa.Pausado) return;

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
            int dx = Mathf.Abs(tileSeleccionado.Columnas - tileClickeado.Columnas);
            int dy = Mathf.Abs(tileSeleccionado.Filas - tileClickeado.Filas);

            if (dx + dy == 1)
            {
                seleccionObjetivo.transform.position = tileClickeado.transform.position;
                seleccionObjetivo.SetActive(true);

                SwapJoyas(tileSeleccionado, tileClickeado);

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
        if (a.joyaActual == null || b.joyaActual == null) return; // ← esto es todo

        Vector3 posA = a.joyaActual.transform.position;
        Vector3 posB = b.joyaActual.transform.position;

        a.joyaActual.transform.position = posB;
        b.joyaActual.transform.position = posA;

        a.joyaActual.transform.SetParent(b.transform);
        b.joyaActual.transform.SetParent(a.transform);

        GameObject temp = a.joyaActual;
        a.joyaActual = b.joyaActual;
        b.joyaActual = temp;

        bool tempBomba = a.EsBomba;
        bool tempSuper = a.EsSupergema;
        a.EsBomba = b.EsBomba;
        a.EsSupergema = b.EsSupergema;
        b.EsBomba = tempBomba;
        b.EsSupergema = tempSuper;
    }
}