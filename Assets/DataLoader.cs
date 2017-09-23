using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;

public class DeathData
{
    public readonly List<TimesliceData> TimeData;
    public readonly int TotalDeaths;

    private DeathData(List<TimesliceData> timeData)
    {
        TimeData = timeData;
        TotalDeaths = timeData.Sum(item => item.TotalForTime);
    }

    public static DeathData LoadDeathData()
    {
        string maleDataSourcePath = Application.dataPath + "\\DeathDataMale.csv";
        string femaleDataSourcePath = Application.dataPath + "\\DeathDataFemale.csv";
        List<TimesliceData> timeData = LoadData(maleDataSourcePath, femaleDataSourcePath);
        return new DeathData(timeData);
    }

    private static List<CauseData> LoadData(string dataSourcePath, bool males)
    {
        List<RawDataByAge> rawData = new List<RawDataByAge>();
        RawDataByAge rawDataInProgress = null;
        System.IO.StreamReader fileReader = new System.IO.StreamReader(dataSourcePath);
        string line;
        
        while ((line = fileReader.ReadLine()) != null)
        {
            string[] splitString = line.Split(',');
            if(!string.IsNullOrEmpty(splitString[0])) // New age bracket
            {
                rawData.Add(rawDataInProgress);
                rawDataInProgress = new RawDataByAge(males);
            }
            rawDataInProgress.AgeRangeRows.Add(line);
        }
        rawData.Add(rawDataInProgress);
        rawData.RemoveAt(0); // Remove the first entry because it is null
        fileReader.Close();
        return LoadAllCauseData(rawData);
    }

    private static List<CauseData> LoadAllCauseData(List<RawDataByAge> rawData)
    {
        List<CauseData> ret = new List<CauseData>();
        foreach (RawDataByAge data in rawData)
        {
            foreach (RawDataLine line in data.ToDataLines())
            {
                foreach (CauseData item in line.CauseData)
                {
                    if(!string.IsNullOrEmpty(item.CauseLabel))
                    {
                        ret.Add(item);
                    }
                }
            }
        }
        return ret;
    }

    private static List<TimesliceData> LoadData(string maleDataSourcePath, string femaleDataSourcePath)
    {
        List<CauseData> allData = new List<CauseData>();
        allData.AddRange(LoadData(maleDataSourcePath, true));
        allData.AddRange(LoadData(femaleDataSourcePath, false));

        List<TimesliceData> ret = new List<TimesliceData>();
        Dictionary<int, List<CauseData>> dataByYear = new Dictionary<int, List<CauseData>>();
        for (int i = 0; i < 11; i++)
        {
            dataByYear.Add(i * 10 + 1915, new List<CauseData>());
        }

        foreach (CauseData datum in allData)
        {
            dataByYear[datum.Year].Add(datum);
        }
        return dataByYear.Select(item => new TimesliceData(item.Key, item.Value)).ToList();
    }
}

public class TimesliceData
{
    public int Year;
    public int TotalForTime;
    public List<AgeRangeData> ByEachAgeRange;

    public TimesliceData(int year, List<CauseData> causeData)
    {
        Dictionary<int, List<CauseData>> data = new Dictionary<int, List<CauseData>>();
        data.Add(1, new List<CauseData>()); // A stellar example of the wisdom of starting arrays at zero
        for (int i = 1; i < 17; i++)
        {
            data.Add(i * 5, new List<CauseData>());
        }
        foreach (CauseData datum in causeData)
        {
            data[datum.Age].Add(datum);
        }
        Year = year;
        ByEachAgeRange = data.Select(item => new AgeRangeData(item.Key, item.Value)).ToList();
        TotalForTime = ByEachAgeRange.Sum(item => item.Total);
    }
}

public class AgeRangeData
{
    public int Age;
    public readonly List<CauseData> Reasons;
    public readonly int Total;

    public AgeRangeData(int age, List<CauseData> reasons)
    {
        Age = age;
        Reasons = reasons;
        Total = reasons.Sum(item => item.Total);
    }
}

public class CauseData
{
    public readonly bool Males;
    public readonly int Year;
    public readonly int Age;
    public readonly string CauseLabel;
    public readonly int Total;

    public CauseData(
        bool males,
        int year,
        int age,
        string causeLabel, 
        int total)
    {
        Males = males;
        Year = year;
        Age = age;
        CauseLabel = causeLabel;
        Total = total;
    }
}

public class RawDataByAge
{
    public readonly bool Males;
    public List<string> AgeRangeRows = new List<string>();

    public RawDataByAge(bool males)
    {
        Males = males;
    }
    
    public List<RawDataLine> ToDataLines()
    {
        string ageRange = AgeRangeRows[0].Split(',')[0];
        string rawAge = ageRange.Split('-')[0].Split('+')[0];
        int age = Convert.ToInt32(rawAge);
        return AgeRangeRows.Select(item => new RawDataLine(Males, age, item)).ToList();
    }
}

public class RawDataLine
{
    public int Age;
    public CauseData[] CauseData;

    public RawDataLine(bool males, int age, string line)
    {
        CauseData = new CauseData[11];
        Age = age;
        string[] splitLine = line.Split(',');
        for (int yearRange = 0; yearRange < 11; yearRange++)
        {
            int year = yearRange * 10 + 1915;
            int startingIndex = yearRange * 5 + 1;
            CauseData[yearRange] = LoadCauseData(males, splitLine, startingIndex, year, age);
        }
    }

    private CauseData LoadCauseData(bool males, string[] rawLine, int startingIndex, int year, int age)
    {
        string causeLabel = rawLine[startingIndex + 2];
        string rawTotal = rawLine[startingIndex + 3];
        string rawPercentage = rawLine[startingIndex + 4];
        int total = string.IsNullOrEmpty(rawTotal) ? 0 : Convert.ToInt32(rawTotal);
        return new CauseData(males, year, age, causeLabel, total);
    }
}