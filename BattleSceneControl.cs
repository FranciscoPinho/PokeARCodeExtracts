using System.Collections;
using System.Collections.Generic;
using UnityEngine.Windows.Speech;
using UnityEngine;
using System;
using UnityEngine.UI;
using static BattleSceneControl;

public class BattleSceneControl : MonoBehaviour {
    public ConfidenceLevel confidence = ConfidenceLevel.Medium;
    public enum GameState{InitialSummons,P1Turn,P1Attacking,P2Turn,P2Attacking,P1Won,P2Won,Swapping,Undoing};
    public GameState state = GameState.InitialSummons;
    public GameState prev_state;
    StateHistory history;
    protected MeshRenderer arena_renderer;
    private List<PokemonClass> pokemons_1 = new List<PokemonClass>();
    private List<PokemonClass> pokemons_2 = new List<PokemonClass>();
    PokemonClass activePokemon1 = null;
    PokemonClass activePokemon2 = null;
    PokemonClass toSwap = null;
    ApplicationManager manager;
    PictureHolderVariables pictures;
    Slider hp_slider1, hp_slider2;

    protected PhraseRecognizer recognizer;
    protected string[] keywords = new string[] { "swap","redo","undo","beam","focus","constrict"};
    protected string voiceCommand = "";

    #region INITIALIZERS
    void Start () {
        arena_renderer = GameObject.Find("arena").GetComponentInChildren<MeshRenderer>(true);
        history = new StateHistory();
        if (keywords != null)
        {
            recognizer = new KeywordRecognizer(keywords, confidence);
            recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
            recognizer.Start();
        }
        hp_slider1 = GameObject.Find("health_slider").GetComponent<Slider>();
        hp_slider2 = GameObject.Find("health_slider2").GetComponent<Slider>();
        if (GameObject.Find("ApplicationManager") != null)
        {
            manager = GameObject.Find("ApplicationManager").GetComponent<ApplicationManager>();
            Debug.Log(manager.weather);
            if (manager.weather == "Clear")
            {
                GameObject.Find("Sun").GetComponent<ParticleSystem>().Play();
                GameObject.Find("Directional Light").GetComponent<Light>().intensity = 1.25f;
            }
            else
            {
                GameObject.Find("Sun").SetActive(false);
                GameObject.Find("Directional Light").GetComponent<Light>().intensity = 0.65f;
            }
        }
        if (GameObject.Find("PictureHolder") != null)
        {
            pictures = GameObject.Find("PictureHolder").GetComponent<PictureHolderVariables>();
            GameObject.Find("player1_image").GetComponent<RawImage>().texture = pictures.player1;
            GameObject.Find("player2_image").GetComponent<RawImage>().texture = pictures.player2;
        }
    }

   
    public void AddPokemon1(PokemonClass poke)
    {
        pokemons_1.Add(poke);
    }

    public void AddPokemon2(PokemonClass poke)
    {
        pokemons_2.Add(poke);
    }

    private void OnApplicationQuit()
    {
        if (recognizer != null && recognizer.IsRunning)
        {
            recognizer.OnPhraseRecognized -= Recognizer_OnPhraseRecognized;
            recognizer.Stop();
        }
    }

    private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        voiceCommand = args.text;
        if (state == GameState.P1Turn)
            HandlePlayerTurn(1, "");

        else if (state == GameState.P2Turn)
            HandlePlayerTurn(2, "");
        else voiceCommand = "";
    }
    #endregion INITIALIZERS




    #region STATE_MACHINE

    bool isArenaVisible()
    {
        return arena_renderer.isVisible;
    }

    // Update is called once per frame
    void Update () {
        switch (state)
        {
            case GameState.InitialSummons:
                SetStateP1Buttons(false);
                SetStateP2Buttons(false);
                HandleInitialSummons();
                break;
            case GameState.P1Turn:
                if (activePokemon1 != null)
                    SetStateP1Buttons(true);
                else HandlePlayerTurn(1, "");
                SetStateP2Buttons(false);
                
                break;
            case GameState.P2Turn:
                if (activePokemon2 != null)
                    SetStateP2Buttons(true);
                else HandlePlayerTurn(2, "");
                SetStateP1Buttons(false);
                break;
            case GameState.P1Attacking:
                break;
            case GameState.P2Attacking:
                break;
            default:
                break;
        }
    }


    void SetStateP1Buttons(bool active)
    {
        if (state == GameState.P1Turn)
        {
            GameObject.Find("border_player1").GetComponent<SpriteRenderer>().enabled = true;
            GameObject.Find("border_player2").GetComponent<SpriteRenderer>().enabled = false;
        }
        GameObject[] p1stuff = GameObject.FindGameObjectsWithTag("player1");
        foreach(GameObject button in p1stuff)
        {
            if (button.name == "Attack1")
            {
                if (activePokemon1 == null)
                {
                    button.GetComponentInChildren<Text>().text = "";
                }
                else button.GetComponentInChildren<Text>().text = activePokemon1.attacks[0].name;
            }
            button.GetComponent<Button>().interactable = active;
            if (button.name == "Swap1")
            {
                if (getNumberAlivePokemon(1) < 2)
                {
                    button.SetActive(false);
                }

            }
        }
    }

    void SetStateP2Buttons(bool active)
    {
        if (state == GameState.P2Turn)
        {
            GameObject.Find("border_player2").GetComponent<SpriteRenderer>().enabled = true;
            GameObject.Find("border_player1").GetComponent<SpriteRenderer>().enabled = false;
        }
        GameObject[] p2stuff = GameObject.FindGameObjectsWithTag("player2");
        foreach (GameObject button in p2stuff)
        {
            if (button.name == "Attack2")
            {
                if (activePokemon2 == null)
                {
                    button.GetComponentInChildren<Text>().text = "";
                }
                else button.GetComponentInChildren<Text>().text = activePokemon2.attacks[0].name;
            }
            button.GetComponent<Button>().interactable = active;
            if (button.name == "Swap2")
            {
                if (getNumberAlivePokemon(2) < 2)
                {
                    button.SetActive(false);
                }

            }
        }   
    }

    void HandleInitialSummons()
    {
        SearchActivePokemon1();
        SearchActivePokemon2();
        if (activePokemon1 != null && activePokemon2 != null)
        {
            Debug.Log(activePokemon1.ToString());
            Debug.Log(activePokemon2.ToString());
            ChangeState(GameState.P1Turn);
        }
    }

    void SearchActivePokemon1()
    {
        for (int i = 0; i < pokemons_1.Count; i++)
        {
            PokemonClass temp_pokemon1 = pokemons_1[i];
            if (isArenaVisible() && temp_pokemon1.isVisible() && activePokemon1 == null && !temp_pokemon1.isDead())
            {
                activePokemon1 = temp_pokemon1;
                hp_slider1.value = temp_pokemon1.HP;
                StartCoroutine(activePokemon1.teleportObject("Arena"));
            }
        }
    }

    void SearchActivePokemon2()
    {
        for (int i = 0; i < pokemons_2.Count; i++)
        {
            PokemonClass temp_pokemon2 = pokemons_2[i];
            if (isArenaVisible() && temp_pokemon2.isVisible() && activePokemon2 == null && !temp_pokemon2.isDead())
            {
                activePokemon2 = temp_pokemon2;
                hp_slider2.value = temp_pokemon2.HP;
                StartCoroutine(activePokemon2.teleportObject("Arena"));
            }
        }
    }

    public void HandlePlayerTurn(int player,string button)
    {
        if (player == 1 && activePokemon1 == null)
        {
            SearchActivePokemon1();
            return;
        }

        if (player == 2 && activePokemon2 == null)
        {
            SearchActivePokemon2();
            return;
        } 
        Attack attack;
        string command = voiceCommand;
       
        if (button != "")
            command = button.ToLower();
        Debug.Log("BUTTON CLICKED IS " + command);
        switch (command)
        {
            case "swap":
                voiceCommand = "";
                prev_state = state;
                CheckSwap(player);
                break;
            case "undo":
                voiceCommand = "";
                Undo();
                break;
            case "redo":
                voiceCommand = "";
                Undo();
                break;
            case "focus":
                voiceCommand = "";
                if (player == 1)
                {
                    attack = activePokemon1.getAttackByVoice("focus");
                    if (attack.name != null)
                        Player1Attack(attack);
                }  
                else{
                    attack = activePokemon2.getAttackByVoice("focus");
                    if (attack.name != null)
                        Player2Attack(attack);
                }
                break;
            case "beam":
                voiceCommand = "";
                if (player == 1)
                {
                    attack = activePokemon1.getAttackByVoice("beam");
                    if (attack.name != null)
                        Player1Attack(attack);
                }
                else
                {
                    attack = activePokemon2.getAttackByVoice("beam");
                    if (attack.name != null)
                        Player2Attack(attack);
                }
                break;
            case "constrict":
                voiceCommand = "";
                if (player == 1)
                {
                    attack = activePokemon1.getAttackByVoice("constrict");
                    if (attack.name != null)
                        Player1Attack(attack);
                }
                else
                {
                    attack = activePokemon2.getAttackByVoice("constrict");
                    if (attack.name != null)
                        Player2Attack(attack);
                }
                break;
            default:
                break;
        }
    }

    public void Player1Attack(Attack attack)
    {
        SetStateP1Buttons(false);
        SetStateP2Buttons(false);
        SaveGameState();
        ChangeState(GameState.P1Attacking);
        if (attack.effect == "doubledamage"){
            activePokemon1.setFocus(true);
			attack.particle.SetActive(true);
			attack.particle.GetComponent<Animator> ().SetTrigger ("Active");
            attack.particle.GetComponent<AudioSource>().Play(0);
            StartCoroutine(Focus_timer(attack));
            
            return;
        }
		attack.particle.SetActive(true);
		attack.particle.GetComponent<Animator>().SetTrigger("Active");
        attack.particle.GetComponent<AudioSource>().Play(0);
		StartCoroutine (Attack1_executable(attack));
        return;
    }

    public void Player2Attack(Attack attack)
    {
        SetStateP1Buttons(false);
        SetStateP2Buttons(false);
        SaveGameState();
        ChangeState(GameState.P2Attacking);
        if (attack.effect == "doubledamage") {
            activePokemon2.setFocus(true);
			attack.particle.SetActive(true);
			attack.particle.GetComponent<Animator> ().SetTrigger ("Active");
            attack.particle.GetComponent<AudioSource>().Play(0);
            StartCoroutine(Focus_timer(attack));
            return;
        }
		attack.particle.SetActive(true);
		attack.particle.GetComponent<Animator>().SetTrigger ("Active");
        attack.particle.GetComponent<AudioSource>().Play(0);
        StartCoroutine (Attack2_executable(attack));
    }

    public void UpdatePokemonHP(int trainer,int hp)
    {
        if (trainer == 1)
        {
            activePokemon1.setHP(hp);
            hp_slider1.value = activePokemon1.HP;
        }
        else
        {
            activePokemon2.setHP(hp);
            hp_slider2.value = activePokemon2.HP;
        }
    }

    private void CheckSwap(int player)
    {
        SetStateP1Buttons(false);
        SetStateP2Buttons(false);
        if (getNumberAlivePokemon(player) > 1)
        {
            ChangeState(GameState.Swapping);
            if (!toSwap.isVisible())
                ChangeState(prev_state);
            if (player == 1)
                StartCoroutine(SwapTrainer1());
            else StartCoroutine(SwapTrainer2());
        }
        else ChangeState(prev_state);
    }

    private IEnumerator SwapTrainer1()
    {
        StartCoroutine(activePokemon1.teleportObject("Swap"));
        yield return new WaitForSeconds(2f);
        hp_slider1.value = toSwap.HP;
        StartCoroutine(toSwap.teleportObject("Arena"));
        activePokemon1 = toSwap;
        yield return new WaitForSeconds(3.60f);
        ChangeState(prev_state);
    }

    private IEnumerator SwapTrainer2()
    {
        StartCoroutine(activePokemon2.teleportObject("Swap"));
        yield return new WaitForSeconds(2f);
        hp_slider2.value = toSwap.HP;
        StartCoroutine(toSwap.teleportObject("Arena"));
        activePokemon2 = toSwap;
        yield return new WaitForSeconds(3.60f);
        ChangeState(prev_state);
    }

    private int getNumberAlivePokemon(int player)
    {
        int alivePokemon = 0;
        if (player == 1)
        {
            for (int i = 0; i < pokemons_1.Count; i++)
            {
                if (!pokemons_1[i].isDead())
                    alivePokemon++;
                if(activePokemon1!=null)
                    if (pokemons_1[i].Pokename != activePokemon1.Pokename)
                    {
                        if (pokemons_1[i].isVisible())
                        {
                            toSwap = pokemons_1[i];
                        }
                    }
            }
        }
        else
        {
            for (int i = 0; i < pokemons_2.Count; i++)
            {
                if (!pokemons_2[i].isDead())
                    alivePokemon++;
                if(activePokemon2!=null)
                    if (pokemons_2[i].Pokename != activePokemon2.Pokename)
                    {
                        if (pokemons_2[i].isVisible())
                        {
                            toSwap = pokemons_2[i];
                        }
                    }
            }
        }
        return alivePokemon;
    }

    public void ChangeState(GameState newstate)
    {
        state = newstate;
    }

	IEnumerator Attack1_executable(Attack attack){
		yield return new WaitForSeconds (attack.animationTime);
		attack.particle.SetActive(false);

		int damageDealt = activePokemon1.focused ? attack.damage * 2 : attack.damage;
		if (activePokemon1.focused)
		{
			activePokemon1.setFocus(false);
		}
		UpdatePokemonHP(2, activePokemon2.HP - damageDealt);
		if (activePokemon2.isDead())
		{
			SetStateP2Buttons(false);
			if (getNumberAlivePokemon(2) < 1)
			{
				ChangeState(GameState.P1Won);
                GameObject.Find("Undo").SetActive(false);
                if (manager != null)
                {
                    manager.setWinner(1);
                    manager.ChangeScene(4);
                }
				yield break;
			}
			activePokemon2 = null; 
		}
		ChangeState(GameState.P2Turn);
    }

    IEnumerator Attack2_executable(Attack attack){
		yield return new WaitForSeconds (attack.animationTime);
		attack.particle.SetActive (false);

		int damageDealt = activePokemon2.focused ? attack.damage * 2 : attack.damage;
		if (activePokemon2.focused)
		{
			activePokemon2.setFocus(false);
		}
		UpdatePokemonHP(1, activePokemon1.HP - damageDealt);
		if (activePokemon1.isDead())
		{
			SetStateP1Buttons(false);
			if (getNumberAlivePokemon(1) < 1)
			{
				ChangeState(GameState.P2Won);
                GameObject.Find("Undo").SetActive(false);
                if (manager != null)
                {
                    manager.setWinner(2);
                    manager.ChangeScene(4);
                }
                yield break;
			}
			activePokemon1 = null;
		}
        ChangeState(GameState.P1Turn);
       
    }

    IEnumerator Focus_timer(Attack attack){
		yield return new WaitForSeconds (attack.animationTime);
        attack.particle.SetActive(false);
        if(state==GameState.P1Attacking)
		    ChangeState(GameState.P2Turn);
        if (state == GameState.P2Attacking)
            ChangeState(GameState.P1Turn);
	}
    #endregion STATE_MACHINE

    #region STATE_HISTORY_MANAGEMENT
    void SaveGameState()
    {
        StateDetails deets = new StateDetails(activePokemon1.Pokename, activePokemon2.Pokename, state);
        for (int i = 0; i < pokemons_1.Count; i++)
        {
            deets.AddP1Records(new PokemonRecord(pokemons_1[i].HP, pokemons_1[i].focused));
        }
        for (int i = 0; i<pokemons_2.Count; i++)
        {
            deets.AddP2Records(new PokemonRecord(pokemons_2[i].HP, pokemons_2[i].focused));
        }
        history.AddToHistory(deets);
    }
    
     public void Undo(){
        if (state != GameState.P1Turn && state != GameState.P2Turn)
            return;
        ChangeState(GameState.Undoing);
        SetStateP1Buttons(false);
        SetStateP2Buttons(false);
        StateDetails deets;
        if(history.game_history.Count==0){
            if (activePokemon1 != null)
            {
                activePokemon1.setHP(100);
                activePokemon1.setFocus(false);
                activePokemon1.backToImageTarget();
                hp_slider1.value = 100;
                activePokemon1 = null;
            }
            if (activePokemon2 != null)
            {
                activePokemon2.setHP(100);
                activePokemon2.setFocus(false);
                activePokemon2.backToImageTarget();
                hp_slider2.value = 100;
                activePokemon2 = null;
            }
            ChangeState(GameState.InitialSummons);
            return;
        }

        deets = history.PopHistory();
        bool foundActive = false;
        for (int i = 0; i < pokemons_1.Count; i++)
        {
            if (deets.player1_records.Count > 0)
            {
                pokemons_1[i].HP = deets.player1_records[i].hp;
                pokemons_1[i].setFocus(deets.player1_records[i].isFocused);
            }
            if (pokemons_1[i].Pokename == deets.activePokemon1)
            {
                if(pokemons_1[i].getLocation()!="arena")
                    pokemons_1[i].setPokemonLocation("Arena");
                activePokemon1 = pokemons_1[i];
                hp_slider1.value = activePokemon1.HP;
                foundActive = true;
            }
            else
            {
                if (pokemons_1[i].getLocation()=="arena")
                    pokemons_1[i].backToImageTarget();
            }
            
           
        }
        if (!foundActive)
        { 
           hp_slider1.value = 100;
        }
        foundActive = false;
        for (int i = 0; i < pokemons_2.Count; i++)
        {
            if (deets.player2_records.Count > 0)
            {
                pokemons_2[i].HP = deets.player2_records[i].hp;
                pokemons_2[i].setFocus(deets.player2_records[i].isFocused);
            }
            if (pokemons_2[i].Pokename == deets.activePokemon2)
            {
                if (pokemons_2[i].getLocation() != "arena")
                    pokemons_2[i].setPokemonLocation("Arena");
                activePokemon2 = pokemons_2[i];
                hp_slider2.value = activePokemon2.HP;
                foundActive = true;
            }
            else
            {
                if (pokemons_2[i].getLocation() == "arena")
                    pokemons_2[i].backToImageTarget();
            }
        }
        if (!foundActive)
        {
            hp_slider2.value = 100;
        }
        ChangeState(deets.state);
        if (deets.state == GameState.P1Turn)
        {
            SetStateP1Buttons(true);
            SetStateP2Buttons(false);
        }
        else
        {
            SetStateP2Buttons(true);
            SetStateP1Buttons(false);
        }
    }
    #endregion STATE_HISTORY_MANAGEMENT

    #region BUTTON_CLICK_HANDLERS
    public void Attack1()
    {
        if (state == GameState.P1Turn && activePokemon1 != null)
            HandlePlayerTurn(1, activePokemon1.attacks[0].voice);
    }
    public void Attack2()
    {
        if (state == GameState.P2Turn && activePokemon2 != null)
            HandlePlayerTurn(2, activePokemon2.attacks[0].voice);
    }
    public void Focus1()
    {
        if (state == GameState.P1Turn && activePokemon1 != null)
            HandlePlayerTurn(1, "focus");
    }
    public void Focus2()
    {
        if (state == GameState.P2Turn && activePokemon2 != null)
            HandlePlayerTurn(2, "focus");
    }
    public void Swap1()
    {
        if (state == GameState.P1Turn && activePokemon1 != null)
            HandlePlayerTurn(1, "swap");
    }
    public void Swap2()
    {
        if (state == GameState.P2Turn && activePokemon2 != null)
            HandlePlayerTurn(2, "swap");

    }
    #endregion BUTTON_CLICK_HANDLERS
}

public class StateDetails
{
    public string activePokemon1;
    public string activePokemon2;
    public GameState state;
    public List<PokemonRecord> player1_records = new List<PokemonRecord>();
    public List<PokemonRecord> player2_records = new List<PokemonRecord>();

    public StateDetails(string poke1,string poke2,GameState st)
    {
        activePokemon1 = poke1;
        activePokemon2 = poke2;
        state = st;
    }
    public void AddP1Records(PokemonRecord newrecord)
    {
        player1_records.Add(newrecord);
    }
    public void AddP2Records(PokemonRecord newrecord)
    {
        player2_records.Add(newrecord);
    }
}

public class PokemonRecord
{
    public int hp;
    public bool isFocused;

    public PokemonRecord(int h,bool foc)
    {
        hp = h;
        isFocused = foc;
    }
}

public class StateHistory
{
    public Stack<StateDetails> game_history = new Stack<StateDetails>();
    public void AddToHistory(StateDetails newdeets)
    {
        game_history.Push(newdeets);
    }
    public StateDetails PopHistory()
    {
        return game_history.Pop();
    }
}