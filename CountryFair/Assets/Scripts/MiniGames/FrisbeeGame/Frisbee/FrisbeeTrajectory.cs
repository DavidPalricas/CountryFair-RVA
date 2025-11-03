using UnityEngine;

/// <summary>
/// Handles the visualization of the frisbee's trailing trajectory during flight.
/// 
/// This component displays a visual line that shows the recent flight path of the frisbee.
/// It creates a trailing effect by continuously updating a LineRenderer with the frisbee's
/// current position, shifting previous positions backward to maintain a history of movement.
/// 
/// The trajectory line is rendered as a separate GameObject and can be enabled/disabled
/// independently from the frisbee physics simulation.
/// 
/// The <see cref="ThrowFrisbee"/> class controls when this script is active by toggling its enabled state,
/// enabling trajectory visualization only when the frisbee is in flight.
/// </summary>
public class FrisbeeTrajectory : MonoBehaviour
{
    /// <summary>Trajectory Visualization - Settings for the trajectory line renderer</summary>
    [Header("Trajectory Visualization")]
    /// <summary>Number of points to calculate for the trajectory preview.</summary>
    [SerializeField]
    private int trajectoryPoints = 100;

    /// <summary>Color of the trajectory line with alpha transparency.</summary>
    [SerializeField]
    private Color trajectoryColor = new(0f, 1f, 0f, 0.8f);
    
    /// <summary>Width of the trajectory line renderer in world units.</summary>
    [SerializeField]
    private float trajectoryWidth = 0.05f;

    /// <summary>Line renderer component that displays the frisbee trajectory.</summary>
    private LineRenderer _line;

    /// <summary>
    /// Initializes the trajectory visualization system on startup.
    /// Creates the LineRenderer and disables this component by default.
    /// </summary>
    private void Awake()
    {
        SetupTrajectoryLine();

        enabled = false;
    }
    
    /// <summary>
    /// Updates the trajectory visualization each frame.
    /// Called while the component is enabled during frisbee flight.
    /// </summary>
    private void Update()
    {
        DrawTrajectory();
    }

    /// <summary>
    /// Creates and configures the LineRenderer component for displaying the trajectory visualization.
    /// Sets up a separate GameObject with appropriate shader, colors, and width properties.
    /// </summary>
    private void SetupTrajectoryLine()
    {
        // Create a separate GameObject for the trajectory line
        GameObject trajectoryObj = new("TrajectoryLine");
        _line = trajectoryObj.AddComponent<LineRenderer>();
        _line.material = new Material(Shader.Find("Sprites/Default"));
        _line.startColor = trajectoryColor;
        _line.endColor = trajectoryColor;
        _line.startWidth = trajectoryWidth;
        _line.endWidth = trajectoryWidth;
        _line.positionCount = 0;
        _line.useWorldSpace = true;
    }

    /// <summary>
    /// Updates the trajectory visualization line each frame by shifting previous positions backward
    /// and adding the current frisbee position at the front of the line.
    /// 
    /// This creates a trailing effect that shows the frisbee's recent flight path. The line starts
    /// from behind the frisbee and extends backward, displaying where the disc has been.
    /// 
    /// On the first call, initializes all trajectory points to the current position.
    /// On subsequent calls, shifts all existing points back by one index and updates the front point.
    /// </summary>
    private void DrawTrajectory()
    {
        for (int i = trajectoryPoints - 1; i > 0; i--)
        {
            if (_line.positionCount > i)
            {
                _line.SetPosition(i, _line.GetPosition(i - 1));
            }
        }

        // Add current position at the front (index 0)
        if (_line.positionCount == 0)
        {
            _line.positionCount = trajectoryPoints;

            // Initialize all points to current position
            for (int i = 0; i < trajectoryPoints; i++)
            {
                _line.SetPosition(i, transform.position);
            }

            return;
        }

        _line.SetPosition(0, transform.position);
    }
   
    /// <summary>
    /// Clears the trajectory line when this component is disabled.
    /// Called when the component or GameObject is disabled to clean up the visual.
    /// </summary>
    private void OnDisable()
    {
        _line.positionCount = 0; 
    }
}
