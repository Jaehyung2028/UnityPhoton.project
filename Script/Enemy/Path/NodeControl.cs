using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 길 찾기는 A* 알고리즘을 이용하여 구현
// G : 시작으로부터 이동했던 거리, H : |가로|+|세로| 장애물 무시하여 목표까지의 거리, F : G + H 을 이용하여 가장 빠른 길을 찾는다.
[System.Serializable]
public class Node
{
    public Node(bool _isWall, int _x, int _y)
    { isWall = _isWall; x = _x; y = _y; }

    public bool isWall;
    public Node ParentNode;

    public int x, y, G, H;
    public int F { get { return G + H; } }
}


public class NodeControl : MonoBehaviour
{
    public Stack<Node> DestinationNode;

    int sizeX, sizeY;

    Node[,] NodeArray;

    Node StartNode, TargetNode, CurNode;

    List<Node> OpenList, ClosedList;

    public void PathFind(Vector3Int Left, Vector3Int Right, Vector3 Pos, Vector3 TargetPos, out Stack<Node> FinalNode, out Node Destination)
    {
        // 타일의 중앙 지점을 기준으로 이동하기 위해 타일맵의 위치를 -0.5, -0.5 이동시켜 정수 기준의 좌표가 타일의 중앙에 위치하게 하였고
        // 몬스터와 플레이어 위치또한 타일 중앙기준을 맞추기 위해 반올림
        Pos = Vector3Int.RoundToInt(Pos);
        TargetPos = Vector3Int.RoundToInt(TargetPos);

        // NodeArray에 몬스터 자신이 이동할수 있는 좌표 범위를 대입해주고 배열의 크기를 설정
        sizeX = Right.x - Left.x + 1;
        sizeY = Right.y - Left.y + 1;

        NodeArray = new Node[sizeX, sizeY];


        // 맵을 생성하때 장애물이 있는 타일맵의 좌표를 찾아 벽으로 인식시켜 대입
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                bool isWall = false;

                if (Map.Instance.ObstaclePos.Contains(new Vector3Int(Left.x + i, Left.y + j, 0)))
                    isWall = true;

                NodeArray[i, j] = new Node(isWall, Left.x + i, Left.y + j);
            }
        }

        // 시작 노드를 자신의 위치를 좌측하단 좌표 기준으로 초기화
        StartNode = NodeArray[(int)Pos.x - Left.x, (int)Pos.y - Left.y];

        // 타켓의 위치가 자신의 이동 범위에 있을 경우 타겟의 위치를 노드에 초기화
        // 범위에 포함되어 있지 않을 경우 null 값을 리턴해준다. 몬스터는 null 값이 리턴될 경우 이동코루틴을 종료
        if (TargetPos.x >= Left.x && TargetPos.x <= Right.x)
        {
            if (TargetPos.y >= Left.y && TargetPos.y <= Right.y)
            {
                TargetNode = NodeArray[(int)TargetPos.x - Left.x, (int)TargetPos.y - Left.y];
            }
        }
        else
        {
            Destination = null;
            FinalNode = new Stack<Node>();
            return;
        }

        Destination = TargetNode;

        // 나머지 리스트들을 초기화
        OpenList = new List<Node>() { StartNode };
        ClosedList = new List<Node>();
        DestinationNode = new Stack<Node>();


        // 이동할수 있는 오픈 리스트가 있을 경우 계속해서 경로를 탐색
        while (OpenList.Count > 0)
        {
            CurNode = OpenList[0];

            // 첫순서의 오픈 노드보다 다른 로드의 F가 작고 F가 같다면 H가 작은 걸 현재노드로 하여 닫힌리스트로 변경
            for (int i = 1; i < OpenList.Count; i++)
                if (OpenList[i].F <= CurNode.F && OpenList[i].H < CurNode.H) 
                    CurNode = OpenList[i];

            OpenList.Remove(CurNode);
            ClosedList.Add(CurNode);



            // 현재 노드가 타겟 노드와 같을 경우 이동경로를 전부 대입
            // 시작점부터 이동 해야 되기 때문에 스택을 활용
            if (CurNode == TargetNode)
            {
                Node TargetCurNode = TargetNode;

                while (TargetCurNode != StartNode)
                {
                    DestinationNode.Push(TargetCurNode);
                    TargetCurNode = TargetCurNode.ParentNode;
                }

                DestinationNode.Push(StartNode);
            }

            // 좌, 우, 위, 아래, 대각선 총 8개의 방향으로 경로를 탐색하여 오픈노드에 대입
            OpenListAdd(CurNode.x + 1, CurNode.y + 1, Left, Right);
            OpenListAdd(CurNode.x - 1, CurNode.y + 1, Left, Right);
            OpenListAdd(CurNode.x - 1, CurNode.y - 1, Left, Right);
            OpenListAdd(CurNode.x + 1, CurNode.y - 1, Left, Right);
            OpenListAdd(CurNode.x, CurNode.y + 1, Left, Right);
            OpenListAdd(CurNode.x + 1, CurNode.y, Left, Right);
            OpenListAdd(CurNode.x, CurNode.y - 1, Left, Right);
            OpenListAdd(CurNode.x - 1, CurNode.y, Left, Right);
        }

        // 최단 이동 경로를 담은 노드를 대입해주어 넘겨줌
        FinalNode = DestinationNode;
    }

    void OpenListAdd(int checkX, int checkY, Vector3Int Left, Vector3Int Right)
    {
        // 이동 범위를 벗어나지 않고, 벽 또는 닫힌리스트가 아닐 경우 실행
        if (checkX >= Left.x && checkX <= Right.x && checkY >= Left.y && checkY <= Right.y)
        {
            if (!NodeArray[checkX - Left.x, checkY - Left.y].isWall && !ClosedList.Contains(NodeArray[checkX - Left.x, checkY - Left.y]))
            {
                // 대각선 이동시 벽사이로 이동 불가
                if (NodeArray[CurNode.x - Left.x, checkY - Left.y].isWall && NodeArray[checkX - Left.x, CurNode.y - Left.y].isWall) return;

                // 대각선 이동시 코너를 가로 질러 가지 않고 수직수평에 벽이 있을 경우 이동 불가
                if (NodeArray[CurNode.x - Left.x, checkY - Left.y].isWall || NodeArray[checkX - Left.x, CurNode.y - Left.y].isWall) return;


                // 노드에 대입 시킨 후 직선, 대각선에 따른 비용 지정
                Node NeighborNode = NodeArray[checkX - Left.x, checkY - Left.y];
                int MoveCost = CurNode.G + (CurNode.x - checkX == 0 || CurNode.y - checkY == 0 ? 10 : 14);


                // 이동비용이 이웃노드G보다 작거나 또는 열린리스트에 이웃노드가 없다면 G, H, ParentNode를 설정 후 열린리스트에 추가
                if (MoveCost < NeighborNode.G || !OpenList.Contains(NeighborNode))
                {
                    NeighborNode.G = MoveCost;
                    NeighborNode.H = (Mathf.Abs(NeighborNode.x - TargetNode.x) + Mathf.Abs(NeighborNode.y - TargetNode.y)) * 10;
                    NeighborNode.ParentNode = CurNode;

                    OpenList.Add(NeighborNode);
                }
            }
        }
    }
}

