using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicControl : MonoBehaviour
{
    [SerializeField] private AudioSource _chickenSong;
    private bool _isPlaying;

    private void Start()
    {
        _isPlaying = false;
    }

    public void Toggle()
    {
        if (_isPlaying)
        {
            _chickenSong.Stop();
            _isPlaying = false;
        }
        else
        {
            _chickenSong.Play();
            _isPlaying = true;
        }
    }
}
