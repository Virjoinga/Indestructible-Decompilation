using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Glu.ABTesting;
using UnityEngine;

public class TestABTestingBehaviour : MonoBehaviour
{
	private class Logger : LoggerSingleton<Logger>
	{
		public Logger()
		{
			LoggerSingleton<Logger>.SetLoggerName("App.TestABTesting");
		}
	}

	private void Start()
	{
		Logging.GetLogger("App.TestABTesting").SetLevel(10);
		StartCoroutine(ExampleForEditor());
	}

	private IEnumerator ExampleForEditor()
	{
		bool isSet3 = false;
		object obj = null;
		isSet3 = ABTesting.parameters.TryGetValue("LastRun", out obj);
		ABTesting.parameters.Set("LastRun", DateTime.Now);
		isSet3 = ABTesting.parameters.TryGetValue("LastRun", out obj);
		Dictionary<string, string> initialData = new Dictionary<string, string> { { "AssetBundlesUrls", "file://url-0/" } };
		string filepath = Path.Combine(Application.dataPath, "Packages/ABTesting/Examples/DecisionTable.xml").Replace('\\', '/');
		string url = "file://" + filepath;
		Glu.ABTesting.Resolution resolution = Glu.ABTesting.Resolution.Retrieve(initialData, url);
		while (!resolution.Ready)
		{
			yield return null;
		}
		foreach (KeyValuePair<string, string> kvp in resolution.Data)
		{
		}
		resolution.Commit();
	}
}
