/// <summary>
/// Empty base class for all JSON-deserialized dialogue data structures.
/// Acts as a shared target type so <see cref="UIDialog"/> can deserialize into specific subclasses via Newtonsoft.Json.
/// </summary>
[System.Serializable]
public class JSONData{}