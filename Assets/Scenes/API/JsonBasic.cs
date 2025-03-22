using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;
public class JsonBasic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private TextMeshProUGUI  textJson; 
    [SerializeField] private UnityEngine.Object jsonFile;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void jsonData(){
       textJson.text = jsonFile.ToString();  
    }
    public void OnClickJsonByValue()
    {
        JObject jsonObject = JObject.Parse(jsonFile.ToString());
        string name = jsonObject["user"]["firstName"].ToString();
        textJson.text = name;  
    }
}
