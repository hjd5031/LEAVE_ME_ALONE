// using System.Collections.Generic;
// using UnityEngine;
// using System.Timers;
// using UnityEngine.SceneManagement;
//
// public enum GameSceneState
// {
//     GameStartScene,
//     MainGame,
//     GameEnd
// }
//
// public class GameManager : Singleton<GameManager>
// {
//     public int PlayerScore;
//     public int EnemyScore;
//
//     public bool PLayerHasVehicleItem;
//     public bool PLayerHasDroneItem;
//     public bool EnemyHasVehicleItem;
//     public bool enemyHasDroneItem;
//
//     public bool isGameOver;
//     public bool isGameStart;
//
//     public Timer timer;
//
//     // ✅ 현재 게임 씬 상태
//     public GameSceneState CurrentSceneState { get; private set; }
//
//     // 🍅 태그별 토마토 리스트
//     public Dictionary<string, List<GameObject>> TomatoByTag = new Dictionary<string, List<GameObject>>()
//     {
//         { "PlayerisPlantable", new List<GameObject>() },
//         { "UnripePlayerTomato", new List<GameObject>() },
//         { "RipePlayerTomato", new List<GameObject>() },
//         { "PlayerisSunning", new List<GameObject>() },
//         { "PickedPlayerTomato", new List<GameObject>() },
//
//         { "EnemyisPlantable", new List<GameObject>() },
//         { "UnripeEnemyTomato", new List<GameObject>() },
//         { "RipeEnemyTomato", new List<GameObject>() },
//         { "EnemyisSunning", new List<GameObject>() },
//         { "PickedEnemyTomato", new List<GameObject>() }
//     };
//
//     void Start()
//     {
//         RegisterAllTomatoesByTag();
//
//         // ✅ 기본 상태는 GameStart로 설정
//         SceneManager.LoadScene("01.Scenes/MainGameScene");
//     }
//
//     void Update()
//     {
//         // 필요하면 상태 전환 테스트 가능
//         // if (Input.GetKeyDown(KeyCode.F1)) SetSceneState(GameSceneState.MainGame);
//     }
//
//     // ✅ 씬 상태 변경 함수
//     public void SetSceneState(GameSceneState newState)
//     {
//         if (CurrentSceneState == newState) return;
//
//         CurrentSceneState = newState;
//         Debug.Log($"[GameManager] Scene changed to: {newState}");
//
//         // 상태 변경에 따른 추가 로직
//         switch (newState)
//         {
//             case GameSceneState.GameStartScene:
//                 // 예: 초기 준비 화면 처리
//                 break;
//             case GameSceneState.MainGame:
//                 // 예: 게임 플레이 로직 시작
//                 isGameStart = true;
//                 break;
//             case GameSceneState.GameEnd:
//                 // 예: 결과 화면 및 정산 처리
//                 isGameOver = true;
//                 break;
//         }
//     }
//
//     /// <summary>
//     /// 씬 내 모든 토마토를 태그 기준으로 분류합니다.
//     /// </summary>
//     public void RegisterAllTomatoesByTag()
//     {
//         foreach (var key in TomatoByTag.Keys)
//         {
//             TomatoByTag[key].Clear();
//             GameObject[] found = GameObject.FindGameObjectsWithTag(key);
//             TomatoByTag[key].AddRange(found);
//         }
//     }
//
//     /// <summary>
//     /// 태그 변경 시 토마토를 리스트에서 제거/추가
//     /// </summary>
//     public void UpdateTomatoTag(GameObject tomato, string oldTag, string newTag)
//     {
//         if (TomatoByTag.ContainsKey(oldTag))
//             TomatoByTag[oldTag].Remove(tomato);
//
//         tomato.tag = newTag;
//
//         if (TomatoByTag.ContainsKey(newTag))
//             TomatoByTag[newTag].Add(tomato);
//     }
//
//     /// <summary>
//     /// 특정 태그의 토마토 리스트 반환
//     /// </summary>
//     public List<GameObject> GetTomatoesByTag(string tag)
//     {
//         if (TomatoByTag.ContainsKey(tag))
//             return TomatoByTag[tag];
//         return new List<GameObject>();
//     }
// }
