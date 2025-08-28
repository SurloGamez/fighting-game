using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;

public class Shadow : MonoBehaviour
{

    /*Box Collider, CircleCollider, etc. are all children of the Collider class
     * check if the component is an instance of one of the collider types instead of
     * using try get component method
     */
    [SerializeField] Transform POV;
    [SerializeField] Material material;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    MeshRenderer mr;
    //colliders
    EdgeCollider2D col;
    BoxCollider2D boxcol;
    CircleCollider2D circol;
    Mesh mesh;
    Vector2[] points;
    GameObject shadow;

    void Start()
    {
        TryGetComponent<EdgeCollider2D>(out col);
        TryGetComponent<BoxCollider2D>(out boxcol);
        TryGetComponent<CircleCollider2D>(out circol);
        shadow = new GameObject();
        shadow.transform.position = transform.position + (Vector3.forward);
        mesh = shadow.AddComponent<MeshFilter>().mesh;
        mr = shadow.AddComponent<MeshRenderer>();
        mr.material = material;
        points = CopyColliderPoints(col, boxcol, circol);
    }

    Vector2[] CopyColliderPoints(EdgeCollider2D col, BoxCollider2D boxcol, CircleCollider2D circol)
    {
        if(col != null){
            return col.points;
        }
        if(boxcol != null){
            float dir = this.transform.eulerAngles.z;
            Vector2[] points = new Vector2[5];
            points[0] = RotatePoint(boxcol.size, boxcol.offset, dir, new Vector2(1, 1));
            points[1] = RotatePoint(boxcol.size, boxcol.offset, dir, new Vector2(-1, 1));
            points[2] = RotatePoint(boxcol.size, boxcol.offset, dir, new Vector2(-1, -1));
            points[3] = RotatePoint(boxcol.size, boxcol.offset, dir, new Vector2(1, -1));
            points[4] = points[0];
            return points;
        }
        return new Vector2[0];
    }

    Vector2 RotatePoint(Vector2 dimensions, Vector2 offset, float rotateby, Vector2 signs)
    {
        return  new Vector2(signs.x * (dimensions.x / 2) * Mathf.Cos(rotateby * Mathf.Deg2Rad), signs.x * (dimensions.x / 2) * Mathf.Sin(rotateby * Mathf.Deg2Rad)) +
                new Vector2(offset.x * Mathf.Cos(rotateby * Mathf.Deg2Rad), offset.x * Mathf.Sin(rotateby * Mathf.Deg2Rad)) +
                new Vector2(signs.y * (dimensions.y / 2) * Mathf.Cos((rotateby + 90) * Mathf.Deg2Rad), signs.y * (dimensions.y / 2) * Mathf.Sin((rotateby + 90) * Mathf.Deg2Rad)) +
                new Vector2(offset.y * Mathf.Cos((rotateby + 90) * Mathf.Deg2Rad), offset.y * Mathf.Sin((rotateby + 90) * Mathf.Deg2Rad));
    }

    //segments are stored in vector2 where each point is a vector component
    Vector2Int[] GetSegments(int pointCount)
    {
        List<Vector2Int> segmentsInd = new List<Vector2Int>();
        for (int i = 0; i < pointCount - 1; i++)
        {
            segmentsInd.Add(new Vector2Int(i, i + 1));   
        }
        return segmentsInd.ToArray();
    }
    bool IfShouldRender(Vector2 point, Vector2 playerpos)
    {
        return Vector2.Distance(point, playerpos) <= 14;
    }
    Vector2 GetPerpendicular(Vector2 cross, Vector2 playerpos, Vector2 diff, Vector2 diff1){
        Vector2 result = cross.Perpendicular1();
        if(Vector2.Dot(result, GetResulant(diff, diff1)) < 0){result *= -1;}
        return result;
    }
    Vector2 GetResulant(Vector2 vec, Vector2 vec1)
    {
        return (vec + vec1).normalized * 10;
    }
    void CalculatePolyMesh(Vector3 playerpos, Vector2[] points, Vector2Int[] segments)
    {
        
        for(int i = 0; i < segments.Length; i++)
        {
            int startIndex = vertices.Count;

            bool inbound = IfShouldRender(points[segments[i].x] + (Vector2)this.transform.position, playerpos);
            bool inbound1 = IfShouldRender(points[segments[i].y] + (Vector2)this.transform.position, playerpos);
            if (!(inbound || inbound1)) { continue; }

            Vector2 pointwrldpos = points[segments[i].x] + (Vector2)this.transform.position;
            Vector2 pointpos = points[segments[i].x];
            Vector2 diff = pointwrldpos - (Vector2)playerpos;
            Vector2 project1 = new Vector2();
            Vector2 project2 = new Vector2();
            RaycastHit2D ray = Physics2D.Raycast(pointwrldpos + (diff.normalized * -0.02f), -diff);
            if (ray.collider.transform == this.transform){continue;}
            project1 = pointpos + (diff.normalized * 16);
            vertices.Add(pointpos);
            vertices.Add(project1);

            pointwrldpos = points[segments[i].y] + (Vector2)this.transform.position;
            pointpos = points[segments[i].y];
            Vector2 diff1 = pointwrldpos - (Vector2)playerpos;
            RaycastHit2D ray1 = Physics2D.Raycast(pointwrldpos + (diff1.normalized * -0.02f), -diff1);
            if (ray1.collider.transform == this.transform){continue;}
            project2 = pointpos + (diff1.normalized * 16);
            vertices.Add(pointpos);
            vertices.Add(project2);

            AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            AddTriangle(startIndex + 1, startIndex + 2, startIndex + 3);

            //create plane orthagonal to projected points 
            vertices.Add(project1 + GetPerpendicular(project1 - project2, playerpos, diff, diff1));
            vertices.Add(project2 + GetPerpendicular(project1 - project2, playerpos, diff, diff1));
            AddTriangle(startIndex + 1, startIndex + 4, startIndex + 3);
            AddTriangle(startIndex + 3, startIndex + 4, startIndex + 5);

        }   
    }

    void CalculateCircleMesh(Vector2 playerpos, Vector2 offset, float radius)
    {
        bool inbound = IfShouldRender(offset + (Vector2)this.transform.position, playerpos);
        if (!inbound) { return; }
        Vector2 diff = (offset + (Vector2)transform.position) - playerpos;
        Vector2 cross = diff.normalized.Perpendicular1();
        Vector2 contact = offset + (cross * radius);
        vertices.Add(contact);
        Vector2 project = (contact + (Vector2)transform.position) - playerpos;
        vertices.Add(contact + (project.normalized * 16));
        contact = offset + (-cross * radius);
        vertices.Add(contact);
        project = (contact + (Vector2)transform.position) - playerpos;
        vertices.Add(contact + (project.normalized * 16));
        AddTriangle(0, 1, 2);
        AddTriangle(1, 2, 3);
    }

    void AddTriangle(int A, int B, int C)
    {
        triangles.Add(A);
        triangles.Add(B);
        triangles.Add(C);
    }

    void UpdateMesh()
    {
        vertices.Clear();
        triangles.Clear();
        mesh.Clear();
        if(circol != null){
            CalculateCircleMesh(POV.position, circol.offset, circol.radius);
        }
        else{
            CalculatePolyMesh(POV.position, points, GetSegments(points.Length));
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
    }

    void FixedUpdate()
    {
        UpdateMesh();
    }

    
}
