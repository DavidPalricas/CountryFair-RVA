
 import { Schema, type, MapSchema } from "@colyseus/schema";

 /**
  * Synced state of the fair hub scene ({@link FairSceneRoom}) — the network contract between
  * the Unity headset (`platform: "game"`) and the React client (`platform: "web"`).
  */
 export class FairState extends Schema {
   /**
    * Tent id -> slot/order value, mirroring the web client's local `TentOrder` (see
    * `ClientSide/src/GameProps/Tents/tentOrder.ts`). `MapSchema` is required instead of a plain
    * `Map`/`Record` so Colyseus can track and patch per-key mutations to clients.
    *
    * Not yet wired to a message handler: no `onMessage` currently writes to this map, so it is
    * still broadcast empty. Once wired, the intent is for the web client to mutate it and the
    * game client to read it, matching how `FairSceneRoom` prompts the web side to pick the order.
    */
   @type({ map: "string" }) tentOrders: MapSchema<string> = new MapSchema<string>();
  }
