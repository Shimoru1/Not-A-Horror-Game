using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace SnoozyRat
{
    public class SR_DemoCharacterMaterialRandomizer : MonoBehaviour
    {
        [SerializeField] private MaterialRandomizationProperty[] propertiesToRandomize;
        [SerializeField] private bool randomizeOnStart = false;
        [SerializeField] private bool randomizeOnSpacebar = true;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (randomizeOnStart)
                RandomizeAllProperties();
        }

        // Update is called once per frame
        void Update()
        {
            if (randomizeOnSpacebar && Input.GetKeyUp(KeyCode.Space))
            {
                RandomizeAllProperties();
            }
        }

        public void RandomizeAllProperties()
        {
            if (propertiesToRandomize.Length < 1)
            {
                Debug.LogError("No properties set for randomization!");
                return;
            }

            for (int i = 0; i < propertiesToRandomize.Length; i++)
            {
                RandomizeProperty(propertiesToRandomize[i]);
            }
        }

        public void RandomizeProperty(MaterialRandomizationProperty property)
        {
            if (property.meshes.Length < 1)
            {
                Debug.LogError("No meshes set for randomization property!");
                return;
            }

            int randomMat = property.materialOptions.Length > 0 ? Random.Range(0, property.materialOptions.Length) : -1;

            for (int i = 0; i < property.meshes.Length; i++)
            {
                MaterialRandomizationMesh matMesh = property.meshes[i];

                Renderer mesh = matMesh.mesh;

                if (randomMat >= 0)
                {
                    Material mat = property.materialOptions[randomMat];
                    List<Material> mats = mesh.materials.ToList();
                    mats[matMesh.materialIndex] = mat;
                    mesh.SetMaterials(mats);
                }
            }
        }
    }

    [System.Serializable]
    public class MaterialRandomizationProperty
    {
        [Tooltip("Name is just for visual organization in the editor")]
        public string name;
        [Tooltip("Mesh to be affected by this randomization property")]
        public MaterialRandomizationMesh[] meshes;
        [Tooltip("Material options to randomly be chosen for the meshes (Leave empty for no change to material(s)")]
        public Material[] materialOptions;
    }

    [System.Serializable]
    public class MaterialRandomizationMesh
    {
        public Renderer mesh;
        [Tooltip("The material index to be affected by randomization (Leave at 0 if there is only one material applied on your mesh)")]
        public int materialIndex;
    }
}

