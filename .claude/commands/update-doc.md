Act as a senior Unity game developer specialized in VR (Meta Quest, XR Interaction Toolkit) with C#.

## Task

Update and complete the XML documentation of the project's C# scripts, following Unity/C# standard documentation conventions.

## Scope

- **If files are provided as arguments**, document **only those files**
- **If no arguments are provided**, document all `.cs` files in the project
- Do not refactor logic, do not rename variables
- If existing documentation is correct, leave it unchanged

## Documentation Rules

- Use `/// <summary>`, `/// <param>`, `/// <returns>`, `/// <remarks>` as appropriate
- Document all `[SerializeField]` fields and public properties with `/// <summary>`
- For Unity methods (`Awake`, `Start`, `Update`, etc.), document the specific purpose in the context of the script, not the generic behaviour
- Do not document the obvious — the comment should add context, not repeat the name

## Inspector-driven Files

For scripts with events wired in the Inspector (UnityEvent, UI buttons, XR Interactable callbacks):

- Add `/// <remarks>Invoked via Inspector in [PrefabName/Scene]</remarks>` on public methods with no direct call in code
- Identify these methods by looking for public signatures with no internal references

## Output

- Return the changed files with complete documentation
- At the end, list which public methods appear to be Inspector-driven and need confirmation
