using System.Collections.Generic;
using UnityEngine;

namespace JellyGame {
    public class JellyGroup : MonoBehaviour {
        [SerializeField] private JellyColor topLeft = JellyColor.Aquamarine;
        [SerializeField] private JellyColor topRight = JellyColor.Aquamarine;
        [SerializeField] private JellyColor bottomLeft = JellyColor.Aquamarine;
        [SerializeField] private JellyColor bottomRight = JellyColor.Aquamarine;

        [SerializeField] private GameObject prefab1x1;
        [SerializeField] private GameObject prefab1x2H;
        [SerializeField] private GameObject prefab2x1V;
        [SerializeField] private GameObject prefab2x2;

        [SerializeField] private ColorMaterialMapping[] colorMaterials;

        [SerializeField] private JellySquish squishEffect;
        [SerializeField] private Transform visualContainer;

        private static readonly Vector3 PosTL = new Vector3(-0.25f, 0f, 0.25f);
        private static readonly Vector3 PosTR = new Vector3(0.25f, 0f, 0.25f);
        private static readonly Vector3 PosBL = new Vector3(-0.25f, 0f, -0.25f);
        private static readonly Vector3 PosBR = new Vector3(0.25f, 0f, -0.25f);

        private Dictionary<JellyColor, Material> matDict;

        private void Awake() {
            InitMaterialDict();
            RefreshVisual();
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

        public void Setup(JellyColor tl, JellyColor tr, JellyColor bl, JellyColor br) {
            topLeft = tl;
            topRight = tr;
            bottomLeft = bl;
            bottomRight = br;
            RefreshVisual();
        }

        public JellyColor GetSubColor(int localX, int localY) {
            if (localX == 0 && localY == 1) return topLeft;
            if (localX == 1 && localY == 1) return topRight;
            if (localX == 0 && localY == 0) return bottomLeft;
            if (localX == 1 && localY == 0) return bottomRight;
            return JellyColor.None;
        }
        public void SetSubColor(int localX, int localY, JellyColor color) {
            if (localX == 0 && localY == 1) topLeft = color;
            else if (localX == 1 && localY == 1) topRight = color;
            else if (localX == 0 && localY == 0) bottomLeft = color;
            else if (localX == 1 && localY == 0) bottomRight = color;
        }

        [ContextMenu("Refresh Visual")]
        public void RefreshVisual() {
            InitMaterialDict();

            if (visualContainer == null) return;

            for (int i = visualContainer.childCount - 1; i >= 0; i--) {
                if (Application.isPlaying)
                    Destroy(visualContainer.GetChild(i).gameObject);
                else
                    DestroyImmediate(visualContainer.GetChild(i).gameObject);
            }

            if (topLeft == topRight && topLeft == bottomLeft && topLeft == bottomRight) {
                SpawnSubMesh("Mesh_2x2", prefab2x2, Vector3.zero, topLeft);
                if (squishEffect != null) squishEffect.ApplySquish();
                return;
            }

            bool handledTL = false, handledTR = false, handledBL = false, handledBR = false;

            if (topLeft == topRight) {
                SpawnSubMesh("Mesh_1x2_Top", prefab1x2H, new Vector3(0f, 0f, 0.25f), topLeft);
                handledTL = true; handledTR = true;
            }
            if (bottomLeft == bottomRight) {
                SpawnSubMesh("Mesh_1x2_Bottom", prefab1x2H, new Vector3(0f, 0f, -0.25f), bottomLeft);
                handledBL = true; handledBR = true;
            }

            if (!handledTL && !handledBL && topLeft == bottomLeft) {
                SpawnSubMesh("Mesh_2x1_Left", prefab2x1V, new Vector3(-0.25f, 0f, 0f), topLeft);
                handledTL = true; handledBL = true;
            }
            if (!handledTR && !handledBR && topRight == bottomRight) {
                SpawnSubMesh("Mesh_2x1_Right", prefab2x1V, new Vector3(0.25f, 0f, 0f), topRight);
                handledTR = true; handledBR = true;
            }

            if (!handledTL) SpawnSubMesh("Mesh_1x1_TL", prefab1x1, PosTL, topLeft);
            if (!handledTR) SpawnSubMesh("Mesh_1x1_TR", prefab1x1, PosTR, topRight);
            if (!handledBL) SpawnSubMesh("Mesh_1x1_BL", prefab1x1, PosBL, bottomLeft);
            if (!handledBR) SpawnSubMesh("Mesh_1x1_BR", prefab1x1, PosBR, bottomRight);

            if (squishEffect != null) squishEffect.ApplySquish();
        }

        private void SpawnSubMesh(string name, GameObject prefab, Vector3 localPos, JellyColor color) {
            GameObject obj;
            if (prefab != null) {
                obj = Instantiate(prefab, visualContainer);
            } 
            else {
                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.SetParent(visualContainer, false);
            }

            obj.name = name;
            obj.transform.localPosition = localPos;

            if (matDict != null && matDict.TryGetValue(color, out Material mat)) {
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers) {
                    r.sharedMaterial = mat;
                }
            }
        }
    }
}
