using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ApplicationManager : MonoBehaviour {

    private AsyncOperation sceneAsync;
    public string location_apikey = "34063694c8ade602b778d153f295421b";
    public string weather_apikey = "f88947fd7fd6a014836d4b8be1105484";
    //Clear sky is sunny: Clouds is cloudy: Rain for rain
    public string weather = "Clear";
    int winner = 0;
    public void Quit() 
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}
  
    public void Play()
    {
        StartCoroutine(loadScene(1));
    }

    public void ChangeScene(int index)
    {
        StartCoroutine(loadScene(index));
    }

    void Awake()
    {
        DontDestroyOnLoad(this);
        StartCoroutine(getLocation());
    }

    public void setWinner(int w)
    {
        winner = w;
    }

    public int getWinner()
    {
        return winner;
    }

    IEnumerator getLocation()
    {
        WWW myExtIPWWW = new WWW("https://api.ipify.org/?format=text");
        yield return myExtIPWWW;
        string IP = myExtIPWWW.text;
        if (IP == "")
            yield break;
        string requestURL = "http://api.ipstack.com/" + IP + "?access_key=" + location_apikey;
        WWW locationWWW = new WWW(requestURL);
        yield return locationWWW;
        string json_location = locationWWW.text;
        string[] location_bits = json_location.Split(',');
        string city_subst = location_bits[8];
        string[] city_bits = city_subst.Split(':');
        string city = city_bits[1];
        string clean_city=city.Replace("\"","");
        GameObject.Find("Location").GetComponent<Text>().text = "Location: " + clean_city;
        WWW weatherWWW = new WWW("http://api.openweathermap.org/data/2.5/weather?q="+clean_city+"&APPID=f88947fd7fd6a014836d4b8be1105484");
        yield return weatherWWW;
        string json_weather = weatherWWW.text;
        string[] weather_bits = json_weather.Split(',');
        string weather_subst = weather_bits[3];
        string[] forecast_bits = weather_subst.Split(':');
        string forecast = forecast_bits[1];
        string clean_forecast = forecast.Replace("\"", "");
        this.weather = clean_forecast;
        GameObject.Find("Weather").GetComponent<Text>().text = "Weather: " + clean_forecast;
    }

    IEnumerator loadScene(int index)
    {
        AsyncOperation scene = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
        scene.allowSceneActivation = false;
        sceneAsync = scene;
        //Wait until we are done loading the scene
        while (scene.progress < 0.9f)
        {
            Debug.Log("Loading scene " + " [][] Progress: " + scene.progress);
            yield return null;
        }

        int i = 0;
        while (i == 0)
        {
            i++;
            yield return null;
        }
        enableScene(index);
        yield break;
    }

    void enableScene(int index)
    {
        //Activate the Scene
        sceneAsync.allowSceneActivation = true;
        Scene sceneToLoad = SceneManager.GetSceneByBuildIndex(index);
        if (sceneToLoad.IsValid())
        {
            SceneManager.SetActiveScene(sceneToLoad);
        }
    }
}
