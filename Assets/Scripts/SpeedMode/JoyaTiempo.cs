using UnityEngine;

public class JoyaTiempo : MonoBehaviour
{
    [Header("Tiempo que otorga al hacer match")]
    public float segundosBonus = 5f; // cambia a 2f o 5f en el Inspector por prefab

    // El tablero/match system llama esto cuando detecta que esta joya fue parte de un match
    public float ObtenerBonus()
    {
        return segundosBonus;
    }
}