using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Entities;
using UnityEngine;

public class CSV_Exporter : MonoBehaviour
{
    private string filePath;
    private StreamWriter writer;
    private bool headersWritten = false;




    private void Start()
    {
        /*
        TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1)); // record the start time for creating file (use as filename postfix)
        int startTime = (int)t.TotalSeconds;
        */

        DateTime now = DateTime.Now;
        string formattedDate = now.ToString("ddMMyyyy_HHmmss");



        // Create directory if it doesn't exist
        string directoryPath = Path.Combine(Application.dataPath, "TestExportData");
        Directory.CreateDirectory(directoryPath);

        // Set up file path
        filePath = Path.Combine(directoryPath, $"EvolData_{formattedDate}.csv");

        // Initialize file
        writer = new StreamWriter(filePath, append: true);

        GridUpdateSystem gridUpdateSystem =
            World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<GridUpdateSystem>();
        gridUpdateSystem.OnAvgDataExport += ExportCSV;
    }

    private void ExportCSV(ExportData data)
    {
        //return; // disable export for now

        try
        {
            if (!headersWritten)
            {
                // writer.WriteLine(GetAllParameters());
                writer.WriteLine(data.GetHeader());
                headersWritten = true;
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh(); // Make file visible in Editor
#endif
            }

            //string newLine = $"{data.day},{data.moveSpeed},{data.maxEnergy},{data.size},{data.animalSensorProb}";
            string newLine = data.GetRecord();
            writer.WriteLine(newLine);
            Debug.Log("csv: " + newLine);

            writer.Flush();

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh(); // Update Editor view
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CSV Export failed: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        if (writer != null)
        {
            writer.Close();
            writer.Dispose();
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }

    /*
    private string GetAllParameters()
    {
        // loop through all parameter settings (PlayerPrefs), return Key-Pair value
        List<SettingInfo> settingInfo = MenuSettingInit.instance.GetAllParamSettings();

        string output = "";

        foreach (SettingInfo setting in settingInfo)
        {
            //output += $"{setting.propertyName}: {PlayP},";
        }
    }
    */
}