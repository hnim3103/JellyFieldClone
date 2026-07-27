using System.Collections.Generic;
using UnityEngine;

public class JellyPopVFX : MonoBehaviour {

    public static JellyPopVFX Instance { get; private set; }

    [Header("Particle Settings")]
    [SerializeField] private int particleCount = 18;
    [SerializeField] private float particleSpeed = 3.5f;
    [SerializeField] private float particleSize = 0.12f;
    [SerializeField] private float particleLifetime = 0.45f;

    [Header("Materials & Prefabs")]
    [SerializeField] private Material particleMaterial;
    [SerializeField] private GameObject shrinkCubePrefab;
    [SerializeField] private ColorMaterialMapping[] colorMaterials;

    private Dictionary<JellyColor, Material> matDict;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } 
        else if (Instance != this) {
            Destroy(gameObject);
            return;
        }

        InitMaterialDict();
    }

    private void InitMaterialDict() {
        matDict = new Dictionary<JellyColor, Material>();
        if (colorMaterials == null) return;
        foreach (var mapping in colorMaterials) {
            if (!matDict.ContainsKey(mapping.color)) {
                matDict.Add(mapping.color, mapping.material);
            }
        }
    }


    public void PlayPopFX(Vector3 worldPos, JellyColor color) {
        if (color == JellyColor.None) return;

        Material colorMat = GetMaterial(color);
        Color mainColor = colorMat != null ? colorMat.color : Color.white;

        // Spawn Particle Burst
        CreateParticleBurst(worldPos, mainColor);

        // Spawn Shrinking Visual Cube
        CreateShrinkCube(worldPos, colorMat);
    }

    private Material GetMaterial(JellyColor color) {
        if (matDict != null && matDict.TryGetValue(color, out Material mat)) {
            return mat;
        }
        return null;
    }

    private void CreateParticleBurst(Vector3 position, Color color) {
        GameObject pObj = new GameObject("PopParticleBurst");
        pObj.transform.position = position;

        ParticleSystem ps = pObj.AddComponent<ParticleSystem>();

        // Stop the auto-playing system before configuring
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystemRenderer renderer = pObj.GetComponent<ParticleSystemRenderer>();

        if (particleMaterial != null) {
            Material matClone = new Material(particleMaterial);
            matClone.color = color;
            renderer.material = matClone;
        }

        var main = ps.main;
        main.duration = particleLifetime;
        main.loop = false;
        main.startLifetime = particleLifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(particleSpeed * 0.5f, particleSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.6f, particleSize * 1.3f);
        main.startColor = color;
        main.gravityModifier = 1.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, (short)particleCount)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 1f);
        curve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        ps.Play();
    }

    private void CreateShrinkCube(Vector3 position, Material colorMat) {
        if (shrinkCubePrefab == null) return;

        GameObject cube = Instantiate(shrinkCubePrefab);
        cube.name = "ShrinkCube";
        cube.transform.position = position;
        cube.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);

        // Apply color material from mapping
        if (colorMat != null) {
            Renderer r = cube.GetComponent<Renderer>();
            if (r == null) r = cube.GetComponentInChildren<Renderer>();
            if (r != null) {
                r.sharedMaterial = colorMat;
            }
        }

        // Animate shrink and destroy
        StartCoroutine(AnimateShrinkAndDestroy(cube));
    }

    private System.Collections.IEnumerator AnimateShrinkAndDestroy(GameObject obj) {
        float duration = 0.18f;
        float elapsed = 0f;
        Vector3 initialScale = obj.transform.localScale;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scaleMultiplier = Mathf.Lerp(1.25f, 0f, t * t);
            if (obj != null) {
                obj.transform.localScale = initialScale * scaleMultiplier;
            }
            yield return null;
        }

        if (obj != null) {
            Destroy(obj);
        }
    }
}
