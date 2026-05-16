using UnityEngine;

public class JoyaTiempo : MonoBehaviour
{
    [Header("Tiempo que otorga al hacer match")]
    public float segundosBonus = 5f; 
    public float ObtenerBonus()
    {
        return segundosBonus;
    }
}