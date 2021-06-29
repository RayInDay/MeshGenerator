using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshEditor : MonoBehaviour
{
    private Mesh Meshcontainer ;
    private MeshCollider meshCollider;
    private MeshFilter filter;

    void Start()
    {
        Meshcontainer = new Mesh();
           meshCollider = GetComponent<MeshCollider>();
        filter = GetComponent<MeshFilter>();
        //transform.localRotation = Quaternion.Euler(0,180,180);
        
    }

    public void SetMesh(Mesh mesh) {
        Meshcontainer.Clear();
        Meshcontainer.vertices = mesh.vertices;
        Meshcontainer.triangles=mesh.triangles;
        updateMesh();
    }
   private void updateMesh()
    {
        filter.mesh = Meshcontainer;
        meshCollider.sharedMesh = Meshcontainer;
    }
}
