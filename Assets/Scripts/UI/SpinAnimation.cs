using UnityEngine;

public class SpinAnimation : MonoBehaviour
{
    public float rotateSpeed = 400f; // Velocidad de giro

    void Update()
    {
        // Gira el objeto en el eje Z continuamente
        transform.Rotate(0f, 0f, -rotateSpeed * Time.deltaTime);
    }
}