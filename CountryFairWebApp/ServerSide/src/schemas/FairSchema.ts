
 import { Schema} from "@colyseus/schema";

 /**
  * Synced state of the fair hub scene — the network contract between the Unity headset and
  * the React client.
  *
  * Still empty: the tent order currently lives only in the web client's local React state
  * (`GameScreen`), so nothing is patched to the other side yet. Every field added here must
  * be marked with `@type` and state which side is allowed to mutate it.
  */
 export class FairState extends Schema {
  }
