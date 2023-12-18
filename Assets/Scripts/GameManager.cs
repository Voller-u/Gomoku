using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject blackPiece;
    public GameObject whitePiece;
    public GameObject border;
    public float xDis;
    public float yDis;
    public bool isMyTurn;
    public bool isBlack;

    public static int[,] chessBoard = new int[15, 15];

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isMyTurn)
        {
            DisplayBorder();
        }
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

    Vector3 ClosestPoint(int xCo,int yCo)
    {
        Vector3 pointPos = new Vector3(xCo * xDis, yCo * yDis, 0);
        if (xCo > 7 || xCo < -7 || yCo > 7 || yCo < -7) pointPos.z = -1;
        return pointPos;
    }

    void GeneratePiece()
    {
        Vector3 mousePos = TransScreenToWorld(Input.mousePosition);
        int xCo = (int)Mathf.Round(mousePos.x / xDis), yCo = (int)Mathf.Round(mousePos.y / yDis);
        Vector3 pos = ClosestPoint(xCo,yCo);
        if (pos.z < 0) return;
        if (chessBoard[xCo+7, yCo+7] > 0) return;
        LayPiece(xCo + 7, yCo + 7);
    }

    void LayPiece(int x,int y)
    {
        chessBoard[x, y] = isBlack ? 1 : 2;
        Vector3 pos = ClosestPoint(x - 7, y - 7);
        GameObject piece = Instantiate(isBlack ? blackPiece : whitePiece);
        piece.transform.position = pos;
        if (CheckWin(x, y))
        {
            //TODO 赢了之后的事情
            Debug.Log("Winner:" + (isBlack ? "Black" : "White"));
        }
    }

    Vector3 TransScreenToWorld(Vector3 pos)
    {
        Vector3 t = Camera.main.ScreenToWorldPoint(pos);
        return new Vector3(t.x, t.y, 0);
    }

    void DisplayBorder()
    {
        Vector3 mousePos = TransScreenToWorld(Input.mousePosition);
        int xCo = (int)Mathf.Round(mousePos.x / xDis), yCo = (int)Mathf.Round(mousePos.y / yDis);
        Vector3 pos = ClosestPoint(xCo,yCo);
        if (pos.z < 0) return;
        if (chessBoard[xCo+7, yCo+7] > 0) return;
        GameObject bdr = Instantiate(border);
        bdr.transform.position = pos;
        Destroy(bdr, 0.05f);
    }

    bool CheckWin(int x,int y)
    {
        int pieceColor = chessBoard[x, y];
        if (CheckHorizontal(x,y,pieceColor) || CheckVertical(x,y,pieceColor)
            || CheckLeft(x,y,pieceColor)|| CheckRight(x,y,pieceColor)) return true;
        return false;
    }

    bool CheckHorizontal(int x,int y,int c)
    {
        //检查行
        int l = 0, r = 0;
        while (x - l >= 0 && chessBoard[x - l, y] == c) l++;
        while (x + r <= 14 && chessBoard[x + r, y] == c) r++;
        //因为该点重复计算所以-1
        if (l + r - 1 >= 5) return true;
        return false;
    }

    bool CheckVertical(int x,int y,int c)
    {
        //检查列
        int u = 0, d = 0;
        while (y + u <= 14 && chessBoard[x, y + u] == c) u++;
        while (y - d >= 0 && chessBoard[x, y - d] == c) d++;
        if (u + d - 1 >= 5) return true;
        return false;
    }

    bool CheckLeft(int x,int y,int c)
    {
        //检查左斜线
        int l = 0, r = 0;
        while (x - l >= 0 && y + l <= 14 && chessBoard[x - l, y + l] == c) l++;
        while (x + r <= 14 && y - r >= 0 && chessBoard[x + r, y - r] == c) r++;
        if (l + r - 1 >= 5) return true;
        return false;
    }
    
    bool CheckRight(int x,int y,int c)
    {
        //检查右斜线
        int l = 0, r = 0;
        while (x - l >= 0 && y - l >= 0 && chessBoard[x - l, y - l] == c) l++;
        while (x + r <= 14 && y + r <= 14 && chessBoard[x + r, y + r] == c) r++;
        if (l + r - 1 >= 5) return true;
        return false;
    }

}
