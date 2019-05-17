using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace cdv
{
    public static class Build
    {
        /// <summary>
        /// This function builds the current state of the game for windows x64
        /// and immediately starts three instances of the game for testing.
        /// Can be used in the editor by selecting the Build/Build and Run 3x option
        /// or by pressing F5
        /// </summary>
        [MenuItem("Build/Build and Run 3x _F5")]
        public static void BuildAndRun3X()
        {
            var buildOptions = new BuildPlayerOptions();
            string scenesPath = "Assets/Scenes/";
            buildOptions.scenes = new[]
            {
                $"{scenesPath}TitleScreen.unity",
                $"{scenesPath}Game.unity"
            };
            buildOptions.locationPathName = "../build/win_x64/cdv.exe";
            buildOptions.target = BuildTarget.StandaloneWindows64;
            buildOptions.options = BuildOptions.None;

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if(summary.result == BuildResult.Succeeded)
            {
                UnityEngine.Debug.Log($"Build succeeded: {summary.totalTime.Seconds} seconds");

                UnityEngine.Debug.Log($"{Application.dataPath}/../../build/win_x64/cdv.exe");
                for(int i = 0; i < 3; i++)
                {
                    var gameInstance = new Process();
                    gameInstance.StartInfo.FileName = $"{Application.dataPath}/../../build/win_x64/cdv.exe";
                    gameInstance.Start();
                }
            }
            else if(summary.result == BuildResult.Failed)
            {
                UnityEngine.Debug.Log("Build failed");
            }
        }

        [MenuItem("Build/Run Previous Build #F5")]
        public static void RunPreviousBuild()
        {
            var gameInstance = new Process();
            gameInstance.StartInfo.FileName = $"{Application.dataPath}/../../build/win_x64/cdv.exe";
            gameInstance.Start();
        }
    }
}