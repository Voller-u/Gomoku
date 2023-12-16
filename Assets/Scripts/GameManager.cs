using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject whitePiece;
    public float xDis;
    public float yDis;
    // Start is called before the first frame update
    void Start()
    {
   
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GeneratePiece();
        }
    }

    //void Test()
    //{
    //    for(int i = -7; i <= 7; i++)
    //    {
    //        for(int j = -7; j <= 7; j++)
    //        {
    //            GameObject t_piece = Instantiate(whitePiece);
    //            t_piece.transform.position = new Vector3(i * xDis, j * yDis, 0);
    //            Debug.Log(t_piece.transform.position);
    //        }
    //    }
    //}

    void GeneratePiece()
    {
        Vector3 mousePos = TransScreenToWorld(Input.mousePosition);
        Vector3 piecePos = new Vector3(Mathf.Round(mousePos.x / xDis) * xDis,
            Mathf.Round(mousePos.y / yDis) * yDis, 0);
        GameObject piece = Instantiate(whitePiece);
        piece.transform.position = new Vector3(piecePos.x, piecePos.y, 0);
    }

    Vector3 TransScreenToWorld(Vector3 pos)
    {
        Vector3 t = Camera.main.ScreenToWorldPoint(pos);
        return new Vector3(t.x, t.y, 0);
    }
}
