using UnityEngine;

[CreateAssetMenu(fileName = "New Gadget", menuName = "Gadget Data")]
    public class GadgetData : ScriptableObject
    {
        public string gadgetName;
        public Sprite gadgetIcon;
        public GameObject gadgetPrefab;
        public float cooldownTime = 1f;
        public float duration;
        public float range;
        public float damage;
    }

