import { Room } from "colyseus";
import { FairState } from "../schemas/FairSchema.js";

/**
 * Base room for every Country Fair scene.
 *
 * The session is a pairing between exactly two devices: the Meta Quest headset running the
 * Unity game (`platform: "game"`) and the therapist's browser running the React client
 * (`platform: "web"`). This class only holds the pairing bookkeeping; the concrete rooms
 * (see {@link FairSceneRoom}) enforce it in their lifecycle hooks.
 */
export class CountryFairRoom extends Room {
    /** The only accepted `options.platform` values — one client is expected per entry. */
    protected static platforms: Array<string> = ["web", "game"];

    /** Prompt shown by the web client for the current scene; set when the pairing is formed. */
    protected message  : string = "";

    /**
     * Which platforms are already in the room. Guards against a second browser tab or a
     * second headset stealing a slot that belongs to the other half of the pair.
     */
     protected clientsEntered: Record<string, boolean> = {
        ['web']: false,
        ['game']: false
    };

    /** True once every platform in {@link platforms} has joined; the room is then closed to newcomers. */
    protected allPlatformsentered: boolean = false;

    /**
     * Session id of the headset client, so state broadcasts can target everyone *but* the
     * game (the game is the source of truth, not a consumer of these updates).
     */
    protected gameClientId : string = '';

    maxClients = 10;

    /**
     * Whether `platform` is one the room accepts. The value arrives verbatim from the client
     * join options, so it is trimmed and lower-cased before being matched.
     */
    protected isPlatformValid(platform: string) : boolean {
        platform = platform.trim().toLowerCase()
         return CountryFairRoom.platforms.includes(platform);
    }

    onCreate() {
        this.state = new FairState();

        this.onMessage("updateFairState", (client, data) => {
            console.log("Received updated fair state from", client.sessionId, data);
        });
    }
}
