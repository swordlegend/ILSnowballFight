using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ILSnowballFight
{
    public class Combine : MonoBehaviour
    {
        public void CombineCubes()
        {
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                //meshFilters[i].gameObject.SetActive(false);
                i++;
            }
            gameObject.AddComponent<MeshFilter>();

            transform.GetComponent<MeshFilter>().mesh = new Mesh();
            transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            transform.GetComponent<MeshCollider>().sharedMesh = transform.GetComponent<MeshFilter>().mesh;
            transform.gameObject.SetActive(true);

            i = 0;
            while (i < meshFilters.Length)
            {
                if (meshFilters[i].gameObject != gameObject)
                    Destroy(meshFilters[i].gameObject);
                i++;
            }
        }
    }
}
