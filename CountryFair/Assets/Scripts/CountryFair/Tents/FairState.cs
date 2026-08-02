// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 4.0.28
// 

using Colyseus.Schema;
#if UNITY_5_3_OR_NEWER
using UnityEngine.Scripting;
#endif

public partial class FairState : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public FairState() { }
	[Type(0, "map", typeof(MapSchema<string>), "string")]
	public MapSchema<string> tentOrders = null;
}

