using System;
using UnityEngine;

[CreateAssetMenu(menuName ="Neon Overflow/Base/SoundData")]
public class Sounds : ScriptableObject
{

    [Serializable]
    public struct Data {
        public int id;
        public string name;
        public AudioClip clip;
    }

    public Data[] source;
}