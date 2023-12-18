using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    public enum ChessType
    {
        STwo,//冲二
        SThree,//冲三
        SFour,//冲四
        Two,//活二
        Three,//活三
        Four,//活四
        Five,//五成
    }
    public enum DifficultyLevel { Easy,Middle,Hard,Expert};

    public static List<List<string>> blackChessTypes = new List<List<string>>();
    public static List<List<string>> whiteChessTypes = new List<List<string>>();

    //白棋价值表
    public Dictionary<string, int> whiteEvalTable = new Dictionary<string, int>();
    //黑棋价值表
    public Dictionary<string, int> blackEvalTable = new Dictionary<string, int>();

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

    void InitEvalTable(DifficultyLevel level)
    {
        //根据不同难度初始化价值表
        switch (level)
        {
            case DifficultyLevel.Easy:
                {
                    break;
                }
            case DifficultyLevel.Middle:
                {
                    break;
                }
            case DifficultyLevel.Hard:
                {
                    blackChessTypes[(int)ChessType.STwo].Add("21100");//冲二
                    
                    blackChessTypes[(int)ChessType.SThree].Add("21110");//冲三
                    blackChessTypes[(int)ChessType.SFour].Add("122220");//冲四
                    blackChessTypes[(int)ChessType.Two].Add("01100");//活二
                    blackChessTypes[(int)ChessType.Three].Add("01110");//活三
                    blackChessTypes[(int)ChessType.Four].Add("011110");//活四
                    blackChessTypes[(int)ChessType.Five].Add("11111");//五成
                    for (int i = 0; i <= 6; i++)
                        for (int j = 0; j < blackChessTypes[i].Count; j++)
                            whiteChessTypes[i].Add(blackChessTypes[i][j].
                                Replace('2','-').Replace('1','2').Replace('2','1'));
                    break;
                }
            case DifficultyLevel.Expert:
                {
                    //whiteEvalTable.Clear();
                    //blackEvalTable.Clear();
                    //blackEvalTable.Add( "11111", 10000 );
                    
                    break;
                }
            default:
                break;
        }
    }

    /// <summary>
    /// 分析落子后的棋局并返回棋形计数数组
    /// </summary>
    /// <param name="board">落子后的虚拟棋盘</param>
    /// <param name="x">落子的x坐标</param>
    /// <param name="y">落子的y坐标</param>
    /// <param name="whiteCnt">白棋棋形计数数组</param>
    /// <param name="blackCnt">黑棋棋形计数数组</param>
    /// <param name="isBlack">布尔变量，判断此时是为哪一方计算分数</param>
    void Analyse(int[,] board,int x,int y, int[] whiteCnt, int[] blackCnt,bool isBlack)
    {
        AnalyseHorizontal(board, x, y, whiteCnt,blackCnt,isBlack);
        AnalyseVertical(board, x, y, whiteCnt, blackCnt, isBlack);
        AnalyseLeft(board, x, y, whiteCnt, blackCnt, isBlack);
        AnalyseRight(board, x, y, whiteCnt, blackCnt, isBlack);
    }

    void TypeCount(string s,int[] blackCnt, int[] whiteCnt)
    {
        for (int i = 0; i < blackChessTypes.Count; i++)
        {
            for(int j = 0; j < blackChessTypes[i].Count;j++)
                blackCnt[i] += Regex.Matches(s, blackChessTypes[i][j]).Count;
        }
        for (int i = 0; i < whiteChessTypes.Count; i++)
        {
            for (int j = 0; j < whiteChessTypes[i].Count; j++)
                whiteCnt[i] += Regex.Matches(s, whiteChessTypes[i][j]).Count;
        }
    }

    void AnalyseHorizontal(int[,] board, int x, int y, int[] whiteCnt, int[] blackCnt, bool isBlack)
    {
        string line = "";
        for (int i = 0; i < 14; i++) line += board[i, y].ToString();
        TypeCount(line, blackCnt, whiteCnt);
    }
    void AnalyseVertical(int[,] board, int x, int y, int[] whiteCnt, int[] blackCnt, bool isBlack)
    {
        string colume = "";
        for (int i = 0; i < 14; i++) colume += board[x, i].ToString();
        TypeCount(colume, blackCnt, whiteCnt);
    }
    void AnalyseLeft(int[,] board, int x, int y, int[] whiteCnt, int[] blackCnt, bool isBlack)
    {
        string leftSlash = "";
        if (x + y <= 14)
        {//在下半段
            for(int i = 0; y >= 0; y--, i++)
            {
                leftSlash += board[i, y].ToString();
            }
        }
        else
        {//在上半段
            for(int i = 14; y <= 14; y++, i--)
            {
                leftSlash += board[i, y].ToString();
            }
        }
        TypeCount(leftSlash, blackCnt, whiteCnt);
    }
    void AnalyseRight(int[,] board, int x, int y, int[] whiteCnt, int[] blackCnt, bool isBlack)
    {
        string rightSlash = "";
        if(y-x <= 0)
        {//在下半段
            for(int i = 0; x <= 1; x++, i++)
            {
                rightSlash += board[x, i].ToString();
            }
        }
        else
        {
            for(int i = 14; x >= 0; x--, i--)
            {
                rightSlash += board[x, i].ToString();
            }
        }
        TypeCount(rightSlash, blackCnt, whiteCnt);
    }

    int AlphaBeta()
    {

    }

}
