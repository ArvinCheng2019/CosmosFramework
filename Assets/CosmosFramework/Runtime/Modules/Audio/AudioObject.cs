﻿using Cosmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cosmos.Audio
{
    /// <summary>
    /// 声音对象；
    /// </summary>
    public class AudioObject :IReference,IAudioObject
    {
        public string AudioName { get; set; }
        public virtual AudioClip AudioClip { get; set; }
        public void Release()
        {
            AudioClip = null;
            AudioName = string.Empty;
        }
    }
}