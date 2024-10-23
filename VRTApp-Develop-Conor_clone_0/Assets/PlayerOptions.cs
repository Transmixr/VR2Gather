using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerOptions : MonoBehaviour
{

    public TMP_Text playerText;
       
    private static int instanceCount = 0;  // Shared across all instances
    public Renderer objectRenderer;

    public GameObject sphere;

    // Start is called before the first frame update
    void Start()
    {
    
    


    // Increment the count when a new instance is spawned
    instanceCount++;  
    // Get the Renderer
    objectRenderer = sphere.GetComponent<Renderer>();

    if (instanceCount > 1)
    {
        if (objectRenderer != null)
        {
            // Generate a random color
            Color randomColor = new Color(Random.value, Random.value, Random.value);
            
            // Apply the random color to the object's material
            objectRenderer.material.color = randomColor;

            // Alter the Players Name if they are not the the Second Player
            playerText.text = "Connecting\nPlayer";
            
            gameObject.name = "Connected Player";
        }

    }
    else {
            
            playerText.text = "Player";
            gameObject.name = "Player";
    }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
