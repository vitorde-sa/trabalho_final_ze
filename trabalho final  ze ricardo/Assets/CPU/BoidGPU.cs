using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class BoidGPU : MonoBehaviour
{
    public GameObject boidPrefab;
    public int boidCount = 100;
    public float speed = 2f;
    public float separationDistance = 2f;
    public float alignmentDistance = 5f;
    public float cohesionDistance = 5f;

    public GameObject targetPoint;

    private List<Boid> boids;
    private Stopwatch stopwatch;
    private float previousTime;

    void Start()
    {
        boids = new List<Boid>();
        stopwatch = new Stopwatch();
        previousTime = Time.time;

        for (int i = 0; i < boidCount; i++)
        {
            Vector3 position = new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10));
            GameObject boidObj = Instantiate(boidPrefab, position, Quaternion.identity);
            Boid boid = new Boid(boidObj.transform, speed); // Criação do objeto Boid
            boids.Add(boid); // Adiciona o objeto Boid à lista
        }
    }

    void Update()
    {
        if (targetPoint == null) return;

        stopwatch.Restart();

        foreach (Boid boid in boids)
        {
            boid.UpdateBoid(boids, separationDistance, alignmentDistance, cohesionDistance, targetPoint.transform.position);
        }

        stopwatch.Stop();

        float elapsedTime = stopwatch.ElapsedMilliseconds / 1000f;
        float currentTime = Time.time - previousTime;
        float speedup = currentTime / elapsedTime;

        UnityEngine.Debug.Log("Tempo de Execução: " + elapsedTime + " segundos");
        UnityEngine.Debug.Log("Speedup: " + speedup);

        previousTime = Time.time;
    }
}

public class Boid
{
    public Transform transform;
    public Vector3 velocity;
    private float speed;

    public Boid(Transform transform, float speed)
    {
        this.transform = transform;
        this.speed = speed;
        velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * speed;
    }

    public void UpdateBoid(List<Boid> boids, float separationDist, float alignmentDist, float cohesionDist, Vector3 targetPoint)
    {
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        Vector3 attraction = (targetPoint - transform.position).normalized;
        int neighborCount = 0;

        foreach (Boid otherBoid in boids)
        {
            if (otherBoid == this) continue;
            float distance = Vector3.Distance(this.transform.position, otherBoid.transform.position);

            // Separação
            if (distance < separationDist)
            {
                separation += (this.transform.position - otherBoid.transform.position).normalized / distance;
            }

            // Alinhamento
            if (distance < alignmentDist)
            {
                alignment += otherBoid.velocity;
                neighborCount++;
            }

            // Coesão
            if (distance < cohesionDist)
            {
                cohesion += otherBoid.transform.position;
            }
        }

        if (neighborCount > 0)
        {
            alignment /= neighborCount;
            cohesion /= neighborCount;
            cohesion = (cohesion - this.transform.position).normalized;
        }

        velocity += (separation + alignment + cohesion + attraction).normalized * speed * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
    }
}
