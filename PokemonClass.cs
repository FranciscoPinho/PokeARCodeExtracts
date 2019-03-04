using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows.Speech;


public class PokemonClass : MonoBehaviour {
    //Pokemon metadata
    public string Pokename;
    public int HP;
    public bool focused = false;
    public Attack[] attacks = new Attack[] {};
    public int trainer;
    protected string location;
    //Graphical components
    public GameObject portal;
    protected Transform initialParent;
    protected Animator animator;
    Color32 originalColor;
    protected SkinnedMeshRenderer pokerenderer;
    Color focusedColor = new Color(0.4f, 0.6f, 0.9f, 0.6f);
    Color deadColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);

    Scene current_scene;

    public void setTrainer(int tr)
    {
        trainer = tr;
    }

    public void setFocus(bool foc)
    {
        if (foc == focused)
            return;
        focused = foc;
        foreach (Material mat in pokerenderer.materials)
        {
            if (foc)
                mat.color = focusedColor;
            else mat.color = originalColor;
        }
    }

    public int getTrainer()
    {
        return trainer;
    }

    public string getLocation()
    {
        return location;
    }

    public bool isDead()
    {
        return HP <= 0;
    }

    public void setHP(int hp)
    {
        if (hp <= 0)
        {
            HP = 0;
            foreach (Material mat in pokerenderer.materials)
                mat.color = deadColor;
            StartCoroutine(teleportObject("Swap"));
        }
        else
        {
            HP = hp;
            if(!focused)
                foreach (Material mat in pokerenderer.materials)
                    mat.color = originalColor;
        }
    }

    override public string ToString()
    {
        return "Name: " + Pokename + "| HP: " + HP + "| Visibility: " + isVisible()+ "| Trainer: "+trainer;
    }
  
    protected virtual void Start()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
        pokerenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        originalColor=pokerenderer.material.color;
        SceneManager.activeSceneChanged += ChangedActiveScene;
        if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            switch (Pokename)
            {
                case "Bulbasaur":
                    initialParent = GameObject.Find("bulbaTarget").transform;
                    break;
                case "Ivysaur":
                    initialParent = GameObject.Find("ivyTarget").transform;
                    break;
                case "Venusaur":
                    initialParent = GameObject.Find("venuTarget").transform;
                    break;
                default:
                    break;
            }
            ChangedActiveScene(SceneManager.GetActiveScene(), SceneManager.GetActiveScene());
        }
    }

    private void ChangedActiveScene(Scene current, Scene next)
    {
        if (next.buildIndex == 3)
        {
            Debug.Log("Adding to game manager");
            if(trainer==1)
                Camera.main.GetComponent<BattleSceneControl>().AddPokemon1(this);
            else Camera.main.GetComponent<BattleSceneControl>().AddPokemon2(this);
            setPortalActive(false);
        }
    }

    public void setInitialParent(Transform parent)
    {
        initialParent = parent;
    }

    public bool isVisible()
    {
        return pokerenderer.isVisible;
    }

    public SkinnedMeshRenderer getPokemonRenderer()
    {
        return pokerenderer;
    }

    public void disappearFromLocationAnimation()
    {
        portal.SetActive(true);
        animator.SetTrigger("desummon");
    }

    public void setPortalActive(bool act)
    {
        portal.SetActive(act);
    }

    public void setAnimatorActive(bool act)
    {
        animator.enabled = act;
    }

    public void appearAtNewLocationAnimation()
    {
        animator.SetTrigger("summon");
    }

    public void backToImageTarget()
    {
        gameObject.transform.parent = initialParent;
        portal.transform.parent = initialParent;
        location = "target";
        pokerenderer.enabled = false;
        portal.GetComponent<SpriteRenderer>().enabled = false;
        if (gameObject.name == "Venusaur")
        {
            portal.transform.localPosition = new Vector3(0, 0.01f, 0);
            portal.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            gameObject.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
        }
        else
        {
            portal.transform.localPosition = new Vector3(0, 0, 0);
            portal.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
        gameObject.transform.localPosition = new Vector3(0, 0, 0);
        gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }


    public void setPokemonLocation(string location)
    {
        if (location.Equals("Arena"))
        {
            GameObject newparent = GameObject.Find("arena");
            if (newparent != null)
            {
                if (gameObject.GetComponent<PokemonClass>().getTrainer() == 2)
                {
                    gameObject.transform.parent = newparent.transform;
                    gameObject.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
                    gameObject.transform.localPosition = new Vector3(5, 1, 0);
                    portal.transform.parent = newparent.transform;
                    pokerenderer.enabled = true;
                    portal.GetComponent<SpriteRenderer>().enabled = true;
                    portal.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    portal.transform.localPosition = new Vector3(5, 1, 0);
                }
                else
                {
                    gameObject.transform.parent = newparent.transform;
                    gameObject.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                    gameObject.transform.localPosition = new Vector3(-5, 1, 0);
                    portal.transform.parent = newparent.transform;
                    pokerenderer.enabled = true;
                    portal.GetComponent<SpriteRenderer>().enabled = true;
                    portal.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    portal.transform.localPosition = new Vector3(-5, 1, 0);
                }
                location = "arena";
            }
        }
        if (location.Equals("Swap"))
        {
            backToImageTarget();
        }
    }
    public IEnumerator teleportObject(string location)
    {
        disappearFromLocationAnimation();
        yield return new WaitForSeconds(2f);
        setPokemonLocation(location);
        appearAtNewLocationAnimation();
        yield return new WaitForSeconds(1.65f);   
        setPortalActive(false);
    }
    
    public Attack getAttackByVoice(string command)
    {
        foreach(Attack att in attacks){
            if (att.voice == command)
            {
                return att;
            }
        }
        return default(Attack);
    }
}

[System.Serializable]
public struct Attack
{
    public string name;
    public int damage;
    public string effect;
    public string voice;
    public float animationTime;
	public GameObject particle;
} 