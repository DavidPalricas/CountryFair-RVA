using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Loads the CountryFair hub scene, transitioning the player out of a mini-game.
/// </summary>
public class ReturnToFair : MonoBehaviour
{
    /// <summary>
    /// Immediately loads the CountryFair hub scene, discarding the current mini-game scene.
    /// </summary>
    /// <remarks>Invocado via Inspector em botões de retorno nas cenas de mini-jogo.</remarks>
    public void Return()
    {
        SceneManager.LoadScene("CountryFair");
    }
}
