using System;
using System.Collections;
using System.Linq;
using System.Threading;
using UnityEngine;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.Generic;

public class Audio_Manager : MonoBehaviour
{
    #region Variables
    readonly String projectPath = "C:\\Users\\anton\\repos\\MartinFk\\VRGiO\\vrgio\\vrgio_unityprojecct\\"; // TODO: Change to your project path

    int headphoneDeviceNumber = 0;
    int cubeDeviceNumber = 2;

    private ISampleProvider _sine5Seconds = new SignalGenerator()
    {
        Gain = 0.2,
        Frequency = 500,
        Type = SignalGeneratorType.Sin
    }.Take(TimeSpan.FromSeconds(5));

    private Dictionary<String, Tuple<AudioFileReader, bool, double>> _audioFileReaderDict = new Dictionary<string, Tuple<AudioFileReader, bool, double>>();
    #endregion

    void Start()
    {
        headphoneDeviceNumber = 0;
        cubeDeviceNumber = 2;
        StartCoroutine(PlayAudio(projectPath + "Assets\\Audiofiles\\20220518_220311-converted.mp3", headphoneDeviceNumber));
        Thread.Sleep(2000);
        StartCoroutine(PlayAudio(projectPath + "Assets\\Audiofiles\\20220518_215950-converted.mp3", cubeDeviceNumber));
    }

    public IEnumerator PlayAudio(String filepath, int deviceNumber = -1)
    {
        using (var audioFile = new AudioFileReader(filepath))
        using (var waveOut = new WaveOutEvent())
        {
            waveOut.DeviceNumber = deviceNumber;
            waveOut.DesiredLatency = 300;
            waveOut.Init(audioFile);
            _audioFileReaderDict.Add(filepath, new Tuple<AudioFileReader, bool, double>(audioFile, true, -1));
            waveOut.Play();
            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                if (_audioFileReaderDict[filepath].Item3 != -1)
                {
                    audioFile.CurrentTime = TimeSpan.FromSeconds(_audioFileReaderDict[filepath].Item3);
                    _audioFileReaderDict[filepath] = new Tuple<AudioFileReader, bool, double>(audioFile, true, -1);
                }
                if (_audioFileReaderDict[filepath].Item2 == false) waveOut.Stop();

                yield return new WaitForEndOfFrame();
            }
        }
    }

    public void JumpTo(String filepath, double time)
    {
        var dictEntry = _audioFileReaderDict[filepath];
        _audioFileReaderDict[filepath] = new Tuple<AudioFileReader, bool, double>(dictEntry.Item1, dictEntry.Item2, time);
    }
}