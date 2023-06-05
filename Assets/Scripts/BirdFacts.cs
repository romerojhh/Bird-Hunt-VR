using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BirdFacts : MonoBehaviour
{
    public Text factText;
    public float displayInterval = 10f;
    
    private string[] randomFacts = {
        "Birds evolved from dinosaurs.",
        "The first bird domesticated by humans was the goose.",
        "Some birds sleep with one eye open ",
        "Kiwi birds are blind, so they hunt by smell.",
        "The ostrich has the largest eyes of any land animal",
        "Penguins can jump so high as 9 feet ",
        "Crows can recognize human faces ",
        "Birds communicate with color and sound ",
        "Birds do not have teeth",
        "All birds lay eggs.",
        "The smallest bird is a Bee Hummingbird.",
        "Chickens have over 200 distinct noises they make for communicating.",
        "The penguin is the only bird that can swim, but not fly. It is also the only bird that walks upright.",
        "The smallest bird egg belongs to the hummingbird and is the size of a pea."
        // can keep adding more
        // Credits: 
        // https://www.trvst.world/biodiversity/bird-facts/
        // mspca.org/pet_resources/interesting-facts-about-birds/
    };

    private void Start()
    {
        StartCoroutine(ChangeFact());
    }

    private IEnumerator ChangeFact()
    {
        while (true)
        {
            factText.text = GetRandomFact();
            yield return new WaitForSeconds(displayInterval);
        }
    }

    private string GetRandomFact()
    {
        int randomIndex = Random.Range(0, randomFacts.Length);
        return randomFacts[randomIndex];
    }
}