using System.Diagnostics;
using UnityEngine;

public class teste1 : MonoBehaviour
{
    public ComputeShader computeShader;
    public GameObject boidPrefab;
    public int boidCount = 100;
    public float maxSpeed = 5f;
    public float neighborRadius = 3f;
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;
    public float separationDistance = 1.0f;
    public float alignmentDistance = 2.0f;
    public float cohesionDistance = 2.5f;

    private Boid[] boids;
    private ComputeBuffer boidBuffer;
    private int kernelHandle;
    private GameObject[] boidGameObjects;
    private Stopwatch stopwatch;
    private float parallelTime;
    private float serialTime = 1000f; // Exemplo de tempo serial em milissegundos

    struct Boid
    {
        public Vector3 position;
        public Vector3 velocity;
    }

    void Start()
    {
        boids = new Boid[boidCount];
        boidGameObjects = new GameObject[boidCount];
        for (int i = 0; i < boidCount; i++)
        {
            boids[i] = new Boid
            {
                position = Random.insideUnitSphere * 10,
                velocity = Random.insideUnitSphere
            };
            boidGameObjects[i] = Instantiate(boidPrefab, boids[i].position, Quaternion.identity);
        }

        boidBuffer = new ComputeBuffer(boidCount, sizeof(float) * 6);
        boidBuffer.SetData(boids);

        kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(kernelHandle, "boids", boidBuffer);
        computeShader.SetInt("boidCount", boidCount);
        computeShader.SetFloat("maxSpeed", maxSpeed);
        computeShader.SetFloat("neighborRadius", neighborRadius);
        computeShader.SetFloat("separationWeight", separationWeight);
        computeShader.SetFloat("alignmentWeight", alignmentWeight);
        computeShader.SetFloat("cohesionWeight", cohesionWeight);
        computeShader.SetFloat("separationDistance", separationDistance);
        computeShader.SetFloat("alignmentDistance", alignmentDistance);
        computeShader.SetFloat("cohesionDistance", cohesionDistance);

        stopwatch = new Stopwatch();
    }

    void Update()
    {
        stopwatch.Start();

        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.Dispatch(kernelHandle, boidCount / 256 + 1, 1, 1);

        boidBuffer.GetData(boids);
        for (int i = 0; i < boidCount; i++)
        {
            boidGameObjects[i].transform.position = boids[i].position;
        }

        stopwatch.Stop();
        parallelTime = stopwatch.ElapsedMilliseconds;
        UnityEngine.Debug.Log("Tempo de execução (paralelo): " + parallelTime + " ms");

        float speedup = serialTime / parallelTime;
        UnityEngine.Debug.Log("Speedup: " + speedup);

        stopwatch.Reset();
    }

    void OnDestroy()
    {
        boidBuffer.Release();
    }
}
