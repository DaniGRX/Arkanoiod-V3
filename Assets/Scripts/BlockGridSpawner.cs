using UnityEngine; // Librería principal de Unity

public class BlockGridSpawner : MonoBehaviour
{
    [Header("Prefab y contenedor")]

    // Prefab del bloque que voy a instanciar múltiples veces
    [SerializeField] GameObject blockPrefab;

    // Objeto padre donde se guardarán todos los bloques generados
    [SerializeField] Transform parent;

    [Header("Grid")]

    // Número de filas de bloques
    [SerializeField] int rows = 5;

    // Número de columnas de bloques
    [SerializeField] int cols = 10;

    // Distancia horizontal y vertical entre bloques
    [SerializeField] Vector2 spacing = new Vector2(1.1f, 0.6f);

    // Posición inicial donde empezará a generarse la cuadrícula
    [SerializeField] Vector2 startPosition = new Vector2(-5f, 3f);

    [Header("Colores por fila")]

    // Array de colores para asignar un color distinto a cada fila
    [SerializeField]
    Color[] rowColors = new Color[]
    {
        Color.red,                     // Primera fila roja
        new Color(1f, 0.5f, 0f),      // Segunda fila naranja
        Color.yellow,                 // Tercera fila amarilla
        Color.green,                  // Cuarta fila verde
        Color.cyan                    // Quinta fila azul claro
    };

    /*
    // Esto sirve para probar automáticamente el spawn al iniciar
    void Start()
    {
        Spawn();
    }
    */

    public void Spawn()
    {
        // Compruebo que exista un prefab asignado
        if (blockPrefab == null)
        {
            // Muestro error en consola si falta el prefab
            Debug.LogError("No hay blockPrefab asignado en BlockGridSpawner.");

            // Salgo del método para evitar errores posteriores
            return;
        }

        // Si no he asignado un parent manualmente
        if (parent == null)

            // Uso este mismo objeto como contenedor
            parent = transform;

        // Elimino bloques anteriores antes de generar nuevos
        ClearParent();

        // Recorro todas las filas
        for (int r = 0; r < rows; r++)
        {
            // Recorro todas las columnas
            for (int c = 0; c < cols; c++)
            {
                // Calculo la posición exacta de cada bloque
                Vector2 pos = startPosition + new Vector2(c * spacing.x, -r * spacing.y);

                // Instancio el prefab en la posición calculada
                // Quaternion.identity significa rotación normal
                // parent organiza los bloques dentro del contenedor
                GameObject go = Instantiate(blockPrefab, pos, Quaternion.identity, parent);

                // Compruebo que exista el array de colores
                if (rowColors != null && rowColors.Length > 0)
                {
                    // Selecciono el color correspondiente a la fila
                    // El módulo % evita errores si hay más filas que colores
                    Color cRow = rowColors[r % rowColors.Length];

                    // Obtengo el SpriteRenderer del bloque recién creado
                    SpriteRenderer sr = go.GetComponent<SpriteRenderer>();

                    // Compruebo que realmente exista
                    if (sr != null)
                    {
                        // Cambio el color del sprite
                        sr.color = cRow;
                    }
                }
            }
        }
    }

    public int GetTotalBlocks()
    {
        // Devuelvo el número total de bloques generados
        return rows * cols;
    }

    void ClearParent()
    {
        // Recorro todos los hijos del parent desde el final hacia atrás
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            // Destruyo cada bloque existente
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}