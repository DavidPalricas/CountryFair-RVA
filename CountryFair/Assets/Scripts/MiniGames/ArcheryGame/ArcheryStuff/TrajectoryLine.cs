using UnityEngine;

/// <summary>
/// Renders a parabolic arrow trajectory preview using a <see cref="LineRenderer"/>.
/// Samples <see cref="points"/> positions along the predicted path, applying scaled gravity to the Y axis.
/// </summary>
public class TrajectoryLine : MonoBehaviour
{
    /// <summary>LineRenderer used to draw the trajectory arc.</summary>
    public LineRenderer lineRenderer;
    /// <summary>Number of sample points along the predicted trajectory.</summary>
    public int points = 50;
    /// <summary>Time interval in seconds between consecutive trajectory samples.</summary>
    public float timeStep = 0.05f;
    /// <summary>Multiplier applied to <see cref="Physics.gravity"/> when computing the arc (1 = real gravity).</summary>
    public float gravityScale = 1f;

    /// <summary>
    /// Computes and displays a parabolic arc for an arrow launched from <paramref name="startPos"/>
    /// with initial velocity <paramref name="startVelocity"/>.
    /// </summary>
    /// <param name="startPos">World-space launch position.</param>
    /// <param name="startVelocity">Initial velocity vector of the arrow at launch.</param>
    public void ShowTrajectory(Vector3 startPos, Vector3 startVelocity)
    {
        lineRenderer.positionCount = points;

        for (int i = 0; i < points; i++)
        {
            float t = i * timeStep;
            Vector3 point = startPos + startVelocity * t;
            point.y += Physics.gravity.y * gravityScale * t * t / 2f;

            lineRenderer.SetPosition(i, point);
        }

        lineRenderer.enabled = true;
    }

    /// <summary>Disables the LineRenderer, hiding the trajectory preview.</summary>
    public void HideTrajectory()
    {
        lineRenderer.enabled = false;
    }
}
