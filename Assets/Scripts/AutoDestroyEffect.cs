using UnityEngine; // Librería principal de Unity

public class AutoDestroyEffect : MonoBehaviour
{
    // Tiempo que permanecerá visible el efecto antes de destruirse
    [SerializeField] float lifeTime = 0.3f;

    void Start()
    {
        // Destruyo automáticamente este objeto tras unos segundos
        // Esto evita acumular efectos visuales innecesarios en la escena
        Destroy(gameObject, lifeTime);
    }
}