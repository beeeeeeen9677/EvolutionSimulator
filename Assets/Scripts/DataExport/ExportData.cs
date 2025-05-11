using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExportData
{
    // store the average data of all animals


    public int day; // simulationDay
    public float moveSpeed;
    public float maxEnergy;
    public float size;
    public float animalSensorProb;
    public float sensorSize;
    public float warningRange;


    public string separateData;


    public string GetHeader()
    {
        return "Day,MoveSpeed,MaxEnergy,Size,SensorProbability,SensorSize,WarningRange,EntityID";
    }

    public string GetRecord()
    {
        if (separateData != null)
            return separateData;
        else
            return $"{day},{moveSpeed},{maxEnergy},{size},{animalSensorProb},{sensorSize},{warningRange},0";
    }
}
