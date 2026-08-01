import {Client } from "colyseus";
import {CountryFairRoom} from "./CountryFairRoom.js";
import {FairState } from "../schemas/FairSchema.js";


/**
 * Room for the fair hub scene, where the patient picks the order of the mini-game tents.
 *
 * Registered as `fairsceneroom` (see `app.config.ts`) and joined by exactly two clients:
 * the Unity headset build (`platform: "game"`) and the React client (`platform: "web"`).
 * Its synced state is a {@link FairState}.
 */
export class FairSceneRoom extends CountryFairRoom {
 /**
  * Admits a client and, once both platforms are present, hands the fair state to the web side.
  *
  * `options.platform` must be `"web"` or `"game"`:
  * - an unknown platform throws, which Colyseus turns into a failed join on the client;
  * - a duplicate platform, or any client arriving after the pair is complete, is accepted by
  *   the transport and then dropped with `client.leave()`.
  *
  * When the pair completes, `"gamejoined"` is sent to every client except the headset,
  * carrying the current {@link FairState}. That message is what flips the React client from
  * the waiting screen to the game screen.
  */
 onJoin(client: Client, options: any) {
   if(this.allPlatformsentered) {
      console.warn(`Rejecting client ${client.sessionId} because all platforms have already joined.`);
      client.leave();
      return;
   }

    if (!this.isPlatformValid(options.platform)) {
       throw new Error(`Invalid platform: ${options.platform}`);
    }

    var clientPlatform : string = options.platform;

    if (this.clientsEntered[clientPlatform]) {
      console.warn(`Rejecting client ${client.sessionId} because platform ${clientPlatform} has already joined.`);
      client.leave();
      return;
    }

    this.clientsEntered[clientPlatform] = true;

    if (clientPlatform === "game") {
      this.gameClientId = client.sessionId;
    }

    this.state = new FairState();
    this.message = "Seleciona a ordem das tendas";

     console.log(`Client ${client.sessionId} joined with options:`, options);

    if (Object.values(this.clientsEntered).every(client => client === true)) {
      this.allPlatformsentered = true;
      console.log('All platforms have joined. Streaming the game state to web device');

      this.clients.forEach((client) => {
        if (client.sessionId !== this.gameClientId) {
          console.log(`Sending game state to client ${client.sessionId}`);
          client.send("gamejoined", this.state);
        }
      });
    }
   }
}
