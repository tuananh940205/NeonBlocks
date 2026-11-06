using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace NeonBlocks.Editor
{
    /// <summary>Chạy test liên quan bài 13 trong Unity thật và lưu kết quả XML làm bằng chứng.</summary>
    public sealed class Lesson13TestRun : ICallbacks
    {
        private static TestRunnerApi runner;
        /// <summary>Chạy toàn bộ EditMode test của dự án để kiểm tra regression Hold/input/board.</summary>
        [MenuItem("NeonBlocks/Lesson 13/Run EditMode tests")]
        public static void Run()
        {
            runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            runner.RegisterCallbacks(new Lesson13TestRun());
            runner.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode, assemblyNames = new[] { "NeonBlocks.Tests.EditMode" } }));
        }
        /// <summary>Không thay đổi state trước khi test chạy.</summary>
        public void RunStarted(ITestAdaptor testsToRun) { }
        /// <summary>Lưu báo cáo test thật và ghi số pass/fail.</summary>
        public void RunFinished(ITestResultAdaptor result)
        {
            Directory.CreateDirectory("docs/evidence");
            TestRunnerApi.SaveResultToFile(result, "docs/evidence/lesson13-editmode.xml");
            Debug.Log($"Bài 13 EditMode: {result.PassCount} pass, {result.FailCount} fail.");
        }
        /// <summary>Không can thiệp từng test.</summary>
        public void TestStarted(ITestAdaptor test) { }
        /// <summary>Giữ nguyên kết quả từng test do Test Runner cung cấp.</summary>
        public void TestFinished(ITestResultAdaptor result) { }
    }
}
