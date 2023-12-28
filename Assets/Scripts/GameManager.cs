using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public struct Point
    {
        public int x, y;
        public Point(int x,int y) { this.x = x; this.y = y; }
    }
    public List<Point> stepList = new List<Point>();
    public GameObject blackPiece;
    public GameObject whitePiece;
    public GameObject border;
    public float xDis;
    public float yDis;
    public bool isMyTurn;
    public bool playerIsBlack;
    public int maxDepth;//递归的最大深度
    public static int[,] chessBoard = new int[15, 15];
    public bool isThinking;
    public Point nextStep = new();
    public GameObject TutorialPanel;
    public GameObject winPanel;
    public Button revokeButton;
    public Button remakeButton;
    public Text winText;
    public bool gameStart;
    public bool gameEnd;
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

    public static List<List<string>> blackChessTypes = new();
    public static List<List<string>> whiteChessTypes = new();

    //白棋价值表
    public Dictionary<string, int> whiteEvalTable = new();
    //黑棋价值表
    public Dictionary<string, int> blackEvalTable = new();

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    // Update is called once per frame
    void Update()
    { 
        SetButton();
        if (isMyTurn && !gameEnd && gameStart)
        {
            DisplayBorder();
        }
        if (Input.GetMouseButtonDown(0) && isMyTurn && gameStart && !gameEnd)
        {
            GeneratePiece();
        }
        if (Input.GetKeyDown(KeyCode.Z) && isMyTurn && gameStart && !gameEnd)
        {
            Revoke();
        }
        if (gameStart && !gameEnd &&!isMyTurn && !isThinking)
        {
            AI();
            isMyTurn = true;
        }
    }

    void StartGame()
    {
        isMyTurn = true;
        TutorialPanel.SetActive(true);
        winPanel.SetActive(false);
        gameEnd = false;
        gameStart = false;
        InitChessTypesList();
    }

    void SetButton()
    {
        if (isMyTurn && gameStart && !gameEnd && stepList.Count > 0)
            revokeButton.interactable = true;
        else 
            revokeButton.interactable = false;

        if (gameStart)
            remakeButton.interactable = true;
        else
            remakeButton.interactable = false;
    }

    void InitChessTypesList()
    {
        blackChessTypes.Clear();
        whiteChessTypes.Clear();
        for(int i = 0; i < 7; i++)
        {
            blackChessTypes.Add(new List<string>());
            whiteChessTypes.Add(new List<string>());
        }
    }

    public void RevokeBtn()
    {
        Revoke();Revoke();
    }

    private void Revoke()
    {  
        if(isMyTurn && gameStart && !gameEnd && stepList.Count > 0)
        {
            GameObject t  = GameObject.Find("chess" + (stepList.Count-1).ToString());
            Destroy(t);
            chessBoard[stepList[^1].x, stepList[^1].y] = 0;
            stepList.RemoveAt(stepList.Count - 1);
        }
    }

    public void Remake()
    {
        if (gameStart)
        {
            while (stepList.Count > 0)
            {
                GameObject t = GameObject.Find("chess" + (stepList.Count - 1).ToString());
                Destroy(t);
                chessBoard[stepList[^1].x, stepList[^1].y] = 0;
                stepList.RemoveAt(stepList.Count - 1);
            }
            
        }
        StartGame();
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
        LayPiece(xCo + 7, yCo + 7,playerIsBlack);
    }

    void LayPiece(int x,int y,bool isBlack)
    {
        chessBoard[x, y] = isBlack ? 1 : 2;
        Vector3 pos = ClosestPoint(x - 7, y - 7);
        GameObject piece = Instantiate(isBlack ? blackPiece : whitePiece);
        piece.transform.position = pos;
        piece.name = "chess" + stepList.Count;
        stepList.Add(new Point(x, y));
        if (CheckWin(x, y))
        {
            //TODO 赢了之后的事情
            Debug.Log("Winner:" + (isBlack ? "Black" : "White"));
            gameEnd = true;
            winPanel.SetActive(true);
            winText.text = (isBlack ? "黑" : "白") + "棋赢";
        }
        if(isBlack == playerIsBlack)  Invoke(nameof(SwitchTurn), 0.5f);
    }

    void SwitchTurn()
    {
        isMyTurn = false;
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

    static bool IsPalindrome(string str)
    {
        int min = 0;
        int max = str.Length - 1;
        while (true)
        {
            if (min > max)
            {
                return true;
            }
            char a = str[min];
            char b = str[max];
            if (a != b) return false;
            min++;
            max--;
        }
    }
    static string Reverse(string str)
    {
        char[] arr = str.ToCharArray();
        Array.Reverse(arr);
        return new string(arr);
    }

    public void InitLevel(int level)
    {
        InitEvalTable((DifficultyLevel)level);
    }

    public void InitEvalTable(DifficultyLevel level)
    {
        #region 初始化计分表
        //冲二
        blackChessTypes[(int)ChessType.STwo].Add("211000");
        blackChessTypes[(int)ChessType.STwo].Add("01010");
        blackChessTypes[(int)ChessType.STwo].Add("210100");
        blackChessTypes[(int)ChessType.STwo].Add("210010");
        //冲三
        blackChessTypes[(int)ChessType.SThree].Add("21110");
        blackChessTypes[(int)ChessType.SThree].Add("010110");
        blackChessTypes[(int)ChessType.SThree].Add("0100110");
        blackChessTypes[(int)ChessType.SThree].Add("201110");
        blackChessTypes[(int)ChessType.SThree].Add("211100");
        blackChessTypes[(int)ChessType.SThree].Add("210110");
        blackChessTypes[(int)ChessType.SThree].Add("2100110");
        blackChessTypes[(int)ChessType.SThree].Add("2001112");
        blackChessTypes[(int)ChessType.SThree].Add("2011102");
        blackChessTypes[(int)ChessType.SThree].Add("2101102");
        blackChessTypes[(int)ChessType.SThree].Add("2101012");
        blackChessTypes[(int)ChessType.SThree].Add("0101010");
        blackChessTypes[(int)ChessType.SThree].Add("2110100");
        blackChessTypes[(int)ChessType.SThree].Add("2110102");

        //冲四
        blackChessTypes[(int)ChessType.SFour].Add("211110");
        blackChessTypes[(int)ChessType.SFour].Add("0111010");
        blackChessTypes[(int)ChessType.SFour].Add("0110110");
        blackChessTypes[(int)ChessType.SFour].Add("2111010");
        blackChessTypes[(int)ChessType.SFour].Add("2101112");
        blackChessTypes[(int)ChessType.SFour].Add("2110112");

        //活二
        blackChessTypes[(int)ChessType.Two].Add("01100");

        //活三
        blackChessTypes[(int)ChessType.Three].Add("0011100");

        //活四
        blackChessTypes[(int)ChessType.Four].Add("011110");

        //五成
        blackChessTypes[(int)ChessType.Five].Add("11111");
        for (int i = 0; i <= 6; i++)
        {
            int num = blackChessTypes[i].Count;
            for (int j = 0; j < num; j++)
                if (!IsPalindrome(blackChessTypes[i][j]))
                    blackChessTypes[i].Add(Reverse(blackChessTypes[i][j]));
        }


        for (int i = 0; i <= 6; i++)
            for (int j = 0; j < blackChessTypes[i].Count; j++)
                whiteChessTypes[i].Add(blackChessTypes[i][j].
                    Replace('2', '-').Replace('1', '2').Replace('-', '1'));
        #endregion
        //根据不同难度初始化价值表
        switch (level)
        {
            case DifficultyLevel.Easy:
                {
                    Debug.Log("简单难度");
                    maxDepth = 0;
                    break;
                }
            case DifficultyLevel.Middle:
                {
                    break;
                }
            case DifficultyLevel.Hard:
                {
                    Debug.Log("困难难度");
                    maxDepth = 1;
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
        TutorialPanel.SetActive(false);
        gameStart = true;
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
    void Analyse(int[,] board, int[] whiteCnt, int[] blackCnt, bool isBlack)
    {
        AnalyseHorizontal(board, whiteCnt, blackCnt, isBlack);
        AnalyseVertical(board, whiteCnt, blackCnt, isBlack);
        AnalyseLeft(board,  whiteCnt, blackCnt, isBlack);
        AnalyseRight(board,  whiteCnt, blackCnt, isBlack);
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
    void AnalyseHorizontal(int[,] board, int[] whiteCnt, int[] blackCnt, bool isBlack)
    {
        for(int i = 0; i < 15; i++)
        {
            string line = "";
            for (int j = 0; j < 15; j++) line += j.ToString();
            TypeCount(line, blackCnt, whiteCnt);
        }
        
    }
    void AnalyseVertical(int[,] board, int x, int y, int[] whiteCnt, int[] blackCnt, bool isBlack)
    {
        string colume = "";
        for (int i = 0; i < 14; i++) colume += board[x, i].ToString();
        TypeCount(colume, blackCnt, whiteCnt);
    }
    void AnalyseVertical(int[,] board,int[] whiteCnt, int[] blackCnt, bool isBlack)
    {
        for(int j = 0; j < 15; j++)
        {
            string colume = "";
            for (int i = 0; i < 15; i++) colume += board[j, i].ToString();
            TypeCount(colume, blackCnt, whiteCnt);
        }
    }
    void AnalyseLeft(int[,] board, int x, int y, int[] whiteCnt, int[] blackCnt, bool isBlack)
    {
        string leftSlash = "";
        int b = x + y;//纵截距
        if (x + y <= 14)
        {//在下半段
            for(int i = 0; i <= b; i++)
            {
                leftSlash += board[i, b - i].ToString();
            }
        }
        else
        {//在上半段
            for(int i = 0; i <= b - 28; i++)
            {
                leftSlash += board[b - 14 - i, 14 + i].ToString();
            }
        }
        TypeCount(leftSlash, blackCnt, whiteCnt);
    }
    void AnalyseLeft(int[,] board, int[] whiteCnt, int[] blackCnt, bool isBlack)
    {
        
        for(int b = 0; b <= 28; b++)
        {
            string leftSlash = "";
            if (b <= 14)
            {//在下半段
                for (int i = 0; i <= b; i++)
                {
                    leftSlash += board[i, b - i].ToString();
                }
            }
            else
            {//在上半段
                for (int i = 0; i <= b - 28; i++)
                {
                    leftSlash += board[b - 14 - i, 14 + i].ToString();
                }
            }
            TypeCount(leftSlash, blackCnt, whiteCnt);
        }
    }
    void AnalyseRight(int[,] board, int x, int y, int[] whiteCnt, int[] blackCnt, bool isBlack)
    {
        string rightSlash = "";
        int b = y - x;//纵截距
        if(y-x <= 0)
        {//在下半段
            for(int i = 0; i <= 14 + b; i++)
            {
                rightSlash += board[-1 * b + i, i].ToString();
            }
        }
        else
        {
            for(int i = 0; i <= 14 - b; i++)
            {
                rightSlash += board[i, b + i].ToString();
            }
        }
        TypeCount(rightSlash, blackCnt, whiteCnt);
    }

    void AnalyseRight(int[,] board, int[] whiteCnt, int[] blackCnt, bool isBlack)
    {
        
        //int b = y - x;//纵截距
        for(int b = -14; b <= 14; b++)
        {
            string rightSlash = "";
            if (b <= 0)
            {//在下半段
                for (int i = 0; i <= 14 + b; i++)
                {
                    rightSlash += board[-1 * b + i, i].ToString();
                }
            }
            else
            {
                for (int i = 0; i <= 14 - b; i++)
                {
                    rightSlash += board[i, b + i].ToString();
                }
            }
            TypeCount(rightSlash, blackCnt, whiteCnt);
        }
        
    }

    bool NoEmptyCell(int[,] board)
    {
        for (int i = 0; i < 15; i++)
            for (int j = 0; j < 15; j++)
                if (board[i,j] == 0) return false;
        return true;
    }

    int Evaluate(int[,] board,bool isBlack)
    {
        int[] whiteCnt = new int[7];
        int[] blackCnt = new int[7];
        //计算白棋和黑棋各种棋形的数量
        Analyse(board, stepList[^1].x, stepList[^1].y, whiteCnt, blackCnt, isBlack);
        //Debug.Log("Point:" + stepList[^1].x.ToString() + "," + stepList[^1].y.ToString()
        //    + "BlackFour cnt:" + blackCnt[(int)ChessType.Four] + "BlackThree cnt:" +
        //    blackCnt[(int)ChessType.Three]);
        //Debug.Log("Point:" + stepList[^1].x.ToString() + "," + stepList[^1].y.ToString()
        //    + "WhiteFour cnt:" + whiteCnt[(int)ChessType.Four] + "WhiteThree cnt:" +
        //    whiteCnt[(int)ChessType.Three]);
        //计算分数
        if (isBlack)
        {
            if (ChessTypeExists(blackCnt,ChessType.Five)) 
                return 9999;
            if (ChessTypeExists(whiteCnt,ChessType.Five))
                return -9999;
            if (ChessTypeExists(blackCnt,ChessType.Four) || blackCnt[(int)ChessType.SFour] > 2)
                return 9990;
            if (ChessTypeExists(blackCnt,ChessType.SFour))
                return 9980;
            if (ChessTypeExists(whiteCnt,ChessType.Four) || whiteCnt[(int)ChessType.SFour] >2)
                return -9970;
            if (ChessTypeExists(whiteCnt,ChessType.SFour) && ChessTypeExists(whiteCnt,ChessType.Three))
                return -9960;
            if (ChessTypeExists(blackCnt, ChessType.Three) && ChessTypeExists(whiteCnt, ChessType.SFour))
                return 9950;
            if (whiteCnt[(int)ChessType.Three] >1 && !ChessTypeExists(blackCnt,ChessType.SFour)
                && !ChessTypeExists(blackCnt,ChessType.Three) && !ChessTypeExists(blackCnt,ChessType.SThree))
                return -9940;
            int blackScore = CalFewScores(blackCnt);
            int whiteScore = CalFewScores(whiteCnt);
            //Debug.Log("whiteScore:" + whiteScore + " blackScore:" + blackScore);
            return blackScore - whiteScore;
        }
        else
        {
            if (ChessTypeExists(whiteCnt, ChessType.Five))
                return 9999;
            if (ChessTypeExists(blackCnt, ChessType.Five))
                return -9999;
            if (ChessTypeExists(whiteCnt, ChessType.Four) || whiteCnt[(int)ChessType.SFour] > 2)
                return 9990;
            if (ChessTypeExists(whiteCnt, ChessType.SFour))
                return 9980;
            if (ChessTypeExists(blackCnt, ChessType.Four) || blackCnt[(int)ChessType.SFour] > 2)
                return -9970;
            if (ChessTypeExists(blackCnt, ChessType.SFour) && ChessTypeExists(blackCnt, ChessType.Three))
                return -9960;
            if (ChessTypeExists(whiteCnt, ChessType.Three) && ChessTypeExists(blackCnt, ChessType.SFour))
                return 9950;
            if (blackCnt[(int)ChessType.Three] > 1 && !ChessTypeExists(whiteCnt, ChessType.SFour)
                && !ChessTypeExists(whiteCnt, ChessType.Three) && !ChessTypeExists(whiteCnt, ChessType.SThree))
                return -9940;
            int blackScore = CalFewScores(blackCnt) + CalWeight(1);
            int whiteScore = CalFewScores(whiteCnt) + CalWeight(2);
            //for (int i = 0; i < 7; i++) Debug.Log(whiteCnt[i]);
            //Debug.Log("whiteScore:" + whiteScore + " blackScore:" + blackScore);
            return whiteScore - blackScore;
        }
    }

    bool ChessTypeExists(int[] colorCnt, ChessType chessType)
    {
        return colorCnt[(int)chessType] > 0;
    }

    int CalWeight(int color)
    {
        int weightSum = 0;
        for (int i = 0; i < 15; i++)
            for (int j = 0; j < 15; j++)
                if (chessBoard[i, j] == color)
                    weightSum += Mathf.Min(Mathf.Min(i, 14 - i), Mathf.Min(j, 14 - j));
        return weightSum;
    }

    int CalFewScores(int[] colorCnt)
    {
        int scoreSum = 0;
        if (colorCnt[(int)ChessType.Three] > 1) scoreSum += 2000;
        if (colorCnt[(int)ChessType.Three] == 1) scoreSum += 200;
        scoreSum += colorCnt[(int)ChessType.SFour] * 10;
        scoreSum += colorCnt[(int)ChessType.Two] * 4;
        scoreSum += colorCnt[(int)ChessType.STwo] * 1;
        return scoreSum;
    }

    void PrintBoard()
    {
        string str = "";
        for(int i = 0; i < 15; i++)
        {
            for(int j = 0; j < 15; j++)
            {
                str += chessBoard[i, j].ToString();
            }
            str += '\n';
        }
        Debug.Log(str);
    }

    int AlphaBeta(int[,] board,int depth,int alpha,int beta,bool maximizingPlayer)
    {
        if (depth == 0 || NoEmptyCell(board) || CheckWin(stepList[^1].x, stepList[^1].y))
        {
            //返回价值
            //return Evaluate(board, playerIsBlack ? (maximizingPlayer ? false : true) :
            //    (maximizingPlayer ? true : false));
            int v = Evaluate(board, playerIsBlack ? false : true);
            Debug.Log("Leap v = " + v);
            //if(v==0) PrintBoard();
            return v;
        }
        if (maximizingPlayer)
        {
            int v = int.MaxValue;
            bool loopFlag = true;
            //搜刮子节点过程
            for (int i = 0; i < 15 && loopFlag; i++)
            {
                for (int j = 0; j < 15; j++)
                    if (board[i, j] == 0) 
                    {
                        board[i, j] = playerIsBlack ? 1 : 2;
                        stepList.Add(new Point(i, j));
                        v = Mathf.Min(v, AlphaBeta(board, depth - 1, alpha, beta, false));
                        board[i, j] = 0;
                        stepList.RemoveAt(stepList.Count - 1);
                        beta = Mathf.Min(beta, v);
                        if (beta <= alpha)
                        {
                            loopFlag = false;
                            break;
                        }
                    }
            }
            return v;
        }
        else
        {
            int v = 0-int.MaxValue;
            bool loopFlag = true;
            for (int i = 0; i < 15 && loopFlag; i++)
                for (int j = 0; j < 15; j++)
                    if (board[i, j] == 0)
                    {
                        board[i, j] = playerIsBlack ? 2 : 1;
                        stepList.Add(new Point(i, j));
                        v = Mathf.Max(v, AlphaBeta(board, depth - 1, alpha, beta, true));
                        board[i, j] = 0;
                        stepList.RemoveAt(stepList.Count - 1);
                        alpha = Mathf.Max(alpha, v);
                        if (beta <= alpha)
                        {
                            loopFlag = false;
                            break;
                        }
                    }
            return v;
        }
    }

    void AI()
    {
        isThinking = true;
        int val = 0 - int.MaxValue;
        Point nextStep = new Point(0, 0);
        for (int i = 0; i < 15; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (chessBoard[i, j] == 0)
                {
                    chessBoard[i, j] = playerIsBlack ? 2 : 1;
                    stepList.Add(new Point(i, j));
                    int v = AlphaBeta(chessBoard, maxDepth, 0 - int.MaxValue, int.MaxValue, true);
                    Debug.Log("v = " + v);
                    if (v > val)
                    {
                        val = v;
                        nextStep.x = i;
                        nextStep.y = j;
                    }
                    chessBoard[i, j] = 0;
                    stepList.RemoveAt(stepList.Count - 1);
                }
            }
        }
        Debug.Log("MAX Value:" + val);
        LayPiece(nextStep.x, nextStep.y, !playerIsBlack);
        //Debug.Log("Max Score:" + AlphaBeta(chessBoard, maxDepth, 0 - int.MaxValue, int.MaxValue, true));
        isThinking = false;
    }
}
