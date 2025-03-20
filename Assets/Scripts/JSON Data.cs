using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

public class JSONData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TextView;
    [SerializeField] private UnityEngine.Object jsonFile;

    public void DisplayData(){
        JObject jsonObject = JObject.Parse(jsonFile.ToString());
        string jsonString = jsonObject["preferences"]["theme"].ToString();
        TextView.text = jsonString;
    }
}
