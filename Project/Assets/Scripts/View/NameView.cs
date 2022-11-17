using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XFlow.Utils;

namespace Game.View
{
    public class NameView : MonoBehaviour
    {
        public TextMeshProUGUI Name;
        public void SetValue(string name)
        {
            Name.text = name;
        }
    }
}