using UnityEngine; // Librería principal de Unity

public class PaddleController : MonoBehaviour
{
    // Velocidad de movimiento de la barra
    [SerializeField] float speed = 10f;

    // Límite horizontal para que la barra no salga de pantalla
    [SerializeField] float limitX = 7.5f;

    void Update()
    {
        // Capturo el movimiento horizontal del teclado o mando
        // Devuelve valores entre -1 y 1
        float input = Input.GetAxis("Horizontal");

        // Creo el movimiento hacia derecha o izquierda
        // Time.deltaTime hace que el movimiento sea fluido e independiente de los FPS
        Vector3 movement = Vector3.right * input * speed * Time.deltaTime;

        // Muevo la barra usando ese vector calculado
        transform.Translate(movement);

        // Limito la posición X para evitar que la barra atraviese los bordes
        float clampedX = Mathf.Clamp(transform.position.x, -limitX, limitX);

        // Aplico la nueva posición limitada
        // Mantengo Y y Z igual para que solo se mueva horizontalmente
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }
}