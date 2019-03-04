using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegistrationSceneControl : MonoBehaviour {
    public GameObject p1frame;
    public GameObject p2frame;
    int currentPlayer = 1;
    int assignedPokemons = 0;
    int assigned1 = 0;
    int assigned2 = 0;
    public GameObject[] pokemons = new GameObject[] { };
    public GameObject[] anchors = new GameObject[] { };
    private List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
    private List<int> processed_renderers = new List<int>();

    void Start()
    {
        //get all the renderers
        foreach (GameObject poke in pokemons)
        { 
            renderers.Add(poke.GetComponentInChildren<SkinnedMeshRenderer>(true));
        }
    }
    // Use this for initialization
    void Awake() {
        if (GameObject.Find("PictureHolder") != null)
        {
            PictureHolderVariables picture_holder = GameObject.Find("PictureHolder").GetComponent<PictureHolderVariables>();
            p1frame.GetComponent<RawImage>().texture = picture_holder.player1;
            p2frame.GetComponent<RawImage>().texture = picture_holder.player2;
        }
    }
	
    void Update()
    {
        for(int i=0; i<renderers.Count; i++)
        {
            if (renderers[i].isVisible && !processed_renderers.Contains(i))
            {
                processed_renderers.Add(i);
                RegisterPokemon(pokemons[i]);
            }
        }
    }

    public void RegisterPokemon(GameObject pokemon)
    {
        switch (currentPlayer)
        {
            case 1:
                pokemon.GetComponent<PokemonClass>().setTrainer(1);
                StartCoroutine(moveModelToUI(pokemon,assignedPokemons));
                assignedPokemons++;
                assigned1++;
                if (assignedPokemons == 2)
                    clickRegister();
                break;
            case 2:
                int anchor = assignedPokemons;
                if (assignedPokemons == 1 || (assigned2 == 1 && assignedPokemons==2))
                    anchor++;
                pokemon.GetComponent<PokemonClass>().setTrainer(2);
                StartCoroutine(moveModelToUI(pokemon,anchor));
                assignedPokemons++;
                assigned2++;
                if (assignedPokemons == 4)
                    clickRegister();
                break;
            default:
                break;
        }
    }

    IEnumerator moveModelToUI(GameObject pokemon,int anchor)
    {
        PokemonClass script_pokemon = pokemon.GetComponent<PokemonClass>();
        script_pokemon.disappearFromLocationAnimation();
        yield return new WaitForSeconds(2f);
        script_pokemon.setPortalActive(false);
        script_pokemon.setInitialParent(pokemon.transform.parent);
        pokemon.transform.parent = anchors[anchor].transform;
        pokemon.transform.localPosition = new Vector3(anchors[anchor].transform.position.x, anchors[anchor].transform.position.y, -10);
        if (pokemon.name == "Bulbasaur")
        {
            pokemon.transform.localScale = new Vector3(20, 20, 20);
        }
        else pokemon.transform.localScale = new Vector3(10, 10, 10);
        pokemon.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        script_pokemon.getPokemonRenderer().enabled = true;
    }
        
    public void clickRegister()
    {
        switch (currentPlayer)
        {
            case 1:
                if (assigned1 == 0)
                    return;
                GameObject.Find("whoRegistering").GetComponent<Text>().text = "Player 2 please register your monsters";
                currentPlayer++;
                break;
            case 2:
                if (assigned2 == 0)
                    return;
                foreach (GameObject poke in pokemons)
                {
                    poke.GetComponent<PokemonClass>().backToImageTarget();
                }
                GameObject app = GameObject.Find("ApplicationManager");
                if (app != null)
                    //transfer data from registrations to battlescene later
                    app.GetComponent<ApplicationManager>().ChangeScene(3);
                break;
        }
    }
	
}
