using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;
using System;
using UnityEngine.UIElements;

// 방의 구성요소를 생성자로 구현
[System.Serializable]
public class Dungeon
{
    public Dungeon(Vector2Int _LeftBenchmark, bool _HiddenRoom = false, bool _BossRoom = false, GameObject _MiniMap_Obj = null, int _Monster = 0)
    {
        LeftBenchmark = _LeftBenchmark;
        HidenRoom = _HiddenRoom;
        BossRoom = _BossRoom;
        MiniMap_Obj = _MiniMap_Obj;
        Monster = _Monster;
    }

    public Vector2Int LeftBenchmark;
    public bool HidenRoom, BossRoom;
    public GameObject MiniMap_Obj;
    public int Monster;
}

public class Map : MonoBehaviour
{
    public static Map Instance;

    public List<Dungeon> _DungeonList = new List<Dungeon>();

    List<Vector2Int> NomarRoom = new List<Vector2Int>();

    HashSet<Vector3Int> GroundPos = new HashSet<Vector3Int>();
    public HashSet<Vector3Int> ObstaclePos = new HashSet<Vector3Int>();
    public List<GameObject> Soul, Scroll, AllMonster, PortalArray;

    [Header("오브젝트")]
    public GameObject[] MonsterArray;
    public GameObject _Portal, HiddenPortal, BossPortal, SoulObj, KeyObj, MiniMapNormal, MiniMapHidden, MiniMapBoss, BossObj;

    [Space][Header("타일")]
    public Tilemap GroundTileMap, ObstacleTileMap;
    public RuleTile GroundTile, ObstacleTile;

    [Space][Header("맵 수치")]
    public int RoomSize_X, RoomSize_Y, RoomCount, HiddenCount, MaxMonster, RoomCountCheck = 0, DoingMapInstace = 0;

    Vector2Int[] TileDirection;

    public bool Reset = false;

    bool FinishTile = false;

    private void Awake()
    {
        Instance = this;
        Screen.SetResolution(1920, 1080, true);
    }

    // 현재 방위치 기준으로 좌, 우, 위, 아래 범위 배열
    void TitleDirectionReset()
    {
        TileDirection = new Vector2Int[]
        {
               new Vector2Int(RoomSize_X + 10, 0),
               new Vector2Int(0, RoomSize_Y + 10),
               new Vector2Int(-RoomSize_X - 10, 0),
               new Vector2Int(0, -RoomSize_Y - 10)
        };
    }

    // 첫 시작방 생성
    public void StartTile()
    {
        RoomCountCheck = RoomCount;

        TitleDirectionReset();

        // 카메라의 사이즈를 방의 사이즈에 따라 설정
        Camera.main.orthographicSize = 3;

        RoomCount -= 1;

        // 바닥에 해당하는 타일을 설정하고 바닥 HashSet에 대입
        for (int i = 0; i < RoomSize_X; i++)
        {
            for (int j = 0; j < RoomSize_Y; j++)
            {
                GroundTileMap.SetTile(new Vector3Int(i, j, 0), GroundTile);
                GroundPos.Add(new Vector3Int(i, j, 0));
            }
        }

        // 던전에 해당 요소를 대입
        _DungeonList.Add(new Dungeon(Vector2Int.zero));

        Vector2 Center = new Vector3(RoomSize_X / 2 - 0.5f, RoomSize_Y / 2 - 0.5f);

        // 방의 위치에 맞게 포탈을 생성
        // 포탈에 각 이동 위치 대입
        PortalArray.Add(Instantiate(_Portal, Center + new Vector2(0, RoomSize_Y / 2 - 1.5f), Quaternion.identity));
        PortalArray[0].GetComponent<PorTarDirection>().Direction = "Up";

        PortalArray.Add(Instantiate(_Portal, Center - new Vector2(0, RoomSize_Y / 2 - 1.5f), Quaternion.identity));
        PortalArray[1].GetComponent<PorTarDirection>().Direction = "Down";

        PortalArray.Add(Instantiate(_Portal, Center + new Vector2(RoomSize_X / 2 - 1.5f, 0), Quaternion.identity));
        PortalArray[2].GetComponent<PorTarDirection>().Direction = "Right";

        PortalArray.Add(Instantiate(_Portal, Center - new Vector2(RoomSize_X / 2 - 1.5f, 0), Quaternion.identity));
        PortalArray[3].GetComponent<PorTarDirection>().Direction = "Left";

        // 카메라의 위치를 방의 중앙에 올 수 있도록 설정
        Camera.main.transform.position = new Vector3(RoomSize_X / 2 - 0.5f, RoomSize_Y / 2 - 0.5f, -10);

        // 시작 방 기준으로 각 4방향으로 타일을 생성
        StartCoroutine(StartMap(TileDirection[0], Vector2Int.zero));
        StartCoroutine(StartMap(TileDirection[1], Vector2Int.zero));
        StartCoroutine(StartMap(TileDirection[2], Vector2Int.zero));
        StartCoroutine(StartMap(TileDirection[3], Vector2Int.zero));
    }

    // 생성 위치를 받아와 방을 생성
    IEnumerator StartMap(Vector2Int Room_Direction, Vector2Int Pos)
    {
        while (RoomCount > 0)
        {
            // 던전 리스트에 생성할려고 하는 위치에 방이 있을 경우 다시 전의 위치에서 다른 방향으로 생성 가능한지 확인
            // 던전에 포함된 위치정보를 확인하기 위해 람다식을 이용하여 위치정보 확인
            if (_DungeonList.FindIndex(item => item.LeftBenchmark.Equals(Room_Direction)) != -1)
            {
                for (int i = 0; i < TileDirection.Length; i++)
                {
                    // 다른 방향에 생성 가능할 경우 위치를 다시 대입하여 While문 반복
                    if (_DungeonList.FindIndex(item => item.LeftBenchmark.Equals(Pos + TileDirection[i])) == -1)
                    {
                        Room_Direction = Pos + TileDirection[i];
                        break;
                    }
                    // 마지막 배열까지 확인후 생성가능 좌표가 없을 경우 DoingMapInstance 값을 더하여 4방향 모두 생성가능한 좌표가 없고
                    // 생성되어야 하는 방의 갯수와 현재 방의 갯수가 다를 경우 맵을 다시 초기화
                    else if (i == 3 && _DungeonList.FindIndex(item => item.LeftBenchmark.Equals(Pos + TileDirection[i])) != -1)
                    {
                        DoingMapInstace += 1;

                        if (DoingMapInstace == 4 && RoomCount != RoomCountCheck)
                            TileReset();

                        yield break;
                    }
                }
            }

            // 던전리스트에 해당 좌표가 존재하지 않을 경우 타일을 생성하고 랜덤 수에 맞춰 장애물 생성
            if (_DungeonList.FindIndex(item => item.LeftBenchmark.Equals(Room_Direction)) == -1)
            {

                for (int i = 0; i < RoomSize_X; i++)
                {
                    for (int j = 0; j < RoomSize_Y; j++)
                    {
                        GroundTileMap.SetTile(new Vector3Int(Room_Direction.x + i, Room_Direction.y + j, 0), GroundTile);
                        GroundPos.Add(new Vector3Int(Room_Direction.x + i, Room_Direction.y + j, 0));

                        if (i > 3 && j > 3 && i < RoomSize_X - 3 && j < RoomSize_Y - 3 && RoomCount != 1)
                        {
                            if (UnityEngine.Random.Range(0, 20) == 0)
                            {
                                ObstacleTileMap.SetTile(new Vector3Int(Room_Direction.x + i, Room_Direction.y + j, 0), ObstacleTile);
                                ObstaclePos.Add(new Vector3Int(Room_Direction.x + i, Room_Direction.y + j, 0));
                            }
                        }
                    }
                }

                // 마지막 순서의 방일 경우 보스룸으로 설정
                bool Boss = RoomCount == 1 ? true : false;

                RoomCount -= 1;

                int Direction = UnityEngine.Random.Range(0, 4);

                GameObject Monster = null;
                int Monster_Setting = UnityEngine.Random.Range(2, MaxMonster + 1);
                int MonsterCount = 0;

                Enemy CurMonster;

                // 새로운 방향의 좌표를 대입 후 반복
                Pos = Room_Direction;
                Room_Direction = Room_Direction + TileDirection[Direction];

                // 해당 방에 몬스터 생성
                // 생성된 몬스터 오브젝트는 리스트에서 관리
                // 방에 생성된 총 몬스터 수 던전 리스트에 대입
                if (!Boss)
                {
                    for (int i = 0; i < Monster_Setting; i++)
                    {

                        MonsterCount++;

                        int _X = UnityEngine.Random.Range(Pos.x + 4, Pos.x + RoomSize_X - 4);
                        int _Y = UnityEngine.Random.Range(Pos.y + 4, Pos.y + RoomSize_Y - 4);

                        Monster = Instantiate(MonsterArray[UnityEngine.Random.Range(0, MonsterArray.Length)], new Vector3(_X, _Y, 0), Quaternion.identity);
                        AllMonster.Add(Monster);

                        CurMonster = Monster.transform.GetChild(0).GetComponent<Enemy>();

                        CurMonster.LeftPos = new Vector3Int(Pos.x, Pos.y, 0);
                        CurMonster.RightPos = new Vector3Int(Pos.x + RoomSize_X, Pos.y + RoomSize_Y, 0);
                    }
                }
                else
                {
                    MonsterCount++;

                    Monster = Instantiate(BossObj, new Vector3(RoomSize_X / 2 - 0.5f + Pos.x, RoomSize_Y / 2 - 0.5f + Pos.y, 0), Quaternion.identity);
                    AllMonster.Add(Monster);

                    CurMonster = Monster.transform.GetChild(0).GetComponent<Enemy>();

                    CurMonster.LeftPos = new Vector3Int(Pos.x, Pos.y, 0);
                    CurMonster.RightPos = new Vector3Int(Pos.x + RoomSize_X, Pos.y + RoomSize_Y, 0);
                }

                // 던전리스트에 추가
                _DungeonList.Add(new Dungeon(Pos, false, Boss, null, MonsterCount));

                yield return null;

            }
        }
        // 모든 방 생성이 완료될 경우 실행
        if (!FinishTile)
        {
            FinishTile = true;

            HidenRoomKeyInstace();

            ButtonManager.instance.Success = true;
        }
    }

    private void HidenRoomKeyInstace()
    {
        // 난이도에 따른 아이템 생성
        if (HiddenCount > 0)
        {
            int ScrollCount = HiddenCount;

            // 첫번째 방과 이어진 총 5개의 방과 보스 방을 제외한 나머지 방을 리스트에 대입
            for (int i = 5; i < _DungeonList.Count; i++)
            {
                if (_DungeonList[i].BossRoom == false)
                    NomarRoom.Add(_DungeonList[i].LeftBenchmark);
            }

            // 히든룸의 갯수 만큼 방에 스크롤을 생성한 후 리스트에서 제외
            while (NomarRoom.Count > HiddenCount)
            {
                int s = UnityEngine.Random.Range(0, NomarRoom.Count);

                if (ScrollCount > 0)
                {
                    Scroll.Add(Instantiate(KeyObj, new Vector3(NomarRoom[s].x, NomarRoom[s].y) + new Vector3(RoomSize_X / 2, RoomSize_Y / 2), Quaternion.identity));
                    ScrollCount--;
                }

                NomarRoom.RemoveAt(s);
            }

            // 제거되고 남은 방에서 소울 오브젝트를 생성
            // 던전리스트의 히든룸 조건을 True로 변경
            for (int i = 0; i < NomarRoom.Count; i++)
            {
                _DungeonList[_DungeonList.FindIndex(item => item.LeftBenchmark.Equals(NomarRoom[i]))].HidenRoom = true;
                Soul.Add(Instantiate(SoulObj, new Vector3(NomarRoom[i].x, NomarRoom[i].y) + new Vector3(RoomSize_X / 2, RoomSize_Y / 2), Quaternion.identity));
            }

            // 방에 출입하지 않았을때 미니맵에 표시되는 각 방의 조건이 맞는 색의 이미지를 방의 위치에 맞게 생성
            // 방에 출입하였을 경우 이미지를 삭제하여 출입한 방과 출입하지 않은 방을 구분
            for (int i = 1; i < _DungeonList.Count; i++)
            {
                GameObject MiniMap = null;

                if (_DungeonList[i].BossRoom == false)
                {
                    MiniMap = _DungeonList[i].HidenRoom ?
                        Instantiate(MiniMapHidden, (new Vector3(_DungeonList[i].LeftBenchmark.x, _DungeonList[i].LeftBenchmark.y, 0) + new Vector3(RoomSize_X / 2, RoomSize_Y / 2, -5)), Quaternion.identity) :
                        Instantiate(MiniMapNormal, (new Vector3(_DungeonList[i].LeftBenchmark.x, _DungeonList[i].LeftBenchmark.y, 0) + new Vector3(RoomSize_X / 2, RoomSize_Y / 2, -5)), Quaternion.identity);
                }
                else
                {
                    MiniMap =
                        Instantiate(MiniMapBoss, (new Vector3(_DungeonList[i].LeftBenchmark.x, _DungeonList[i].LeftBenchmark.y, 0) + new Vector3(RoomSize_X / 2, RoomSize_Y / 2, -5)), Quaternion.identity);
                }

                // 이미지의 사이즈를 방의 사이즈에 맞게 바꾸어 미니맵에서 방을 가릴 수 있도록 구현
                // 해당 좌표를 가진 던전리스트의 구성요소에 이미지를 대입하여 관리
                MiniMap.transform.localScale = new Vector3(RoomSize_X, RoomSize_Y, 0);
                _DungeonList[i].MiniMap_Obj = MiniMap;
            }

            for (int i = 0; i < AllMonster.Count; i++)
            {
                AllMonster[i].transform.GetChild(0).GetComponent<Enemy>().CurPosDungeon();
            }
        }
    }

    private void OBJDestroy()
    {
        for (int i = 0; i < PortalArray.Count; i++)
            Destroy(PortalArray[i]);

        for (int i = 0; i < Soul.Count; i++)
            Destroy(Soul[i]);

        for (int i = 0; i < AllMonster.Count; i++)
            Destroy(AllMonster[i]);

        for (int i = 0; i < Scroll.Count; i++)
            Destroy(Scroll[i]);

        for (int i = 0; i < _DungeonList.Count; i++)
        {
            if (_DungeonList[i].MiniMap_Obj != null)
                Destroy(_DungeonList[i].MiniMap_Obj);
        }
    }

    // 생성된 오브젝트 삭제 및 리스트 정리
    public void TileReset()
    {

        Reset = true;

        GameObject.Find("Canvas").transform.Find("MiniMap").gameObject.SetActive(false);

        OBJDestroy();

        GroundTileMap.ClearAllTiles();
        ObstacleTileMap.ClearAllTiles();

        PortalArray.Clear();

        Soul.Clear();

        Scroll.Clear();

        AllMonster.Clear();

        NomarRoom.Clear();

        _DungeonList.Clear();

        GroundPos.Clear();
        ObstaclePos.Clear();

        RoomCountCheck = 0;

        HiddenCount = 0;

        FinishTile = false;

        StartCoroutine(ButtonManager.instance.ImageFadeIn());

        TitleDirectionReset();

    }
}
