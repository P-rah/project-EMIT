using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneDatabase
{
    /*
        지금 Slot이 총 4개 정의되어 있음
        즉 동시에 4개의 씬을 로딩해놓고 별도 로딩 지연 없이 왔다갔다 할 수 있음
    */

    public class Slots
    {
        public const string Title = "Title";
        public const string Stage = "Stage";
        public const string CutSceneStage = "CutSceneStage";
        public const string Pause = "Pause";
        public const string StageContent = "StageContent";
        public const string Settings = "Settings";
    }

    public class Scenes
    {
        /*
            이게 실제 씬의 이름이 돼야 함.
            예를 들어서 GamePlayScene1.Unity를 연결하고 싶으면,
            문자열에 "GamePlayScene1"을 정확히 입력해두면 됨.

            <사용법>
            
            SceneControlManager에 함수를 만들거나, API 사용 시,
            const string이 자동완성으로 뜨니까 그걸 사용해서 씬 로드하면 됨.

            실제로 게임 빌드에 들어가는 모든 씬들을 각각 고유 문자열로 이렇게 저장하면 됨

            잊지 말고 빌드 설정에 있는 Scene List에도 씬을 추가해주면 됨. 
        */
        public const string MainMenu = "TitleScene";
        public const string Ch1_1 = "Ch1_G1";
        public const string Ch1_2 = "Ch1_G2";
        public const string Ch1_Boss = "Ch1_B1";
        public const string Clear = "ClearScene";
        //public const string Ch1_3 = "GamePlayScene3";
        //public const string Ch1_4 = "GamePlayScene4";
        //public const string CutSceneStage = "GamePlayScene2";
        public const string SettingsScene = "SettingsScene";
        public const string PauseScene = "PauseScene";
    }
    
    public static readonly Dictionary<string, string> NextStages = new()
    {
        [Scenes.Ch1_1] = Scenes.Ch1_2,
        [Scenes.Ch1_2] = Scenes.Ch1_Boss,
        [Scenes.Ch1_Boss] = Scenes.Clear
    };

    public static int GetBuildIndex(string sceneName)
    {
        return SceneManager.GetSceneByName(sceneName).buildIndex;
    }
}
