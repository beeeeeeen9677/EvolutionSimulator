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


    public string GetHeader()
    {
        return "Day,MoveSpeed,MaxEnergy,Size,SensorProbability";
    }

    public string GetRecord()
    {
        return $"{day},{moveSpeed},{maxEnergy},{size},{animalSensorProb}";
    }   
}
