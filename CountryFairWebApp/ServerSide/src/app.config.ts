import {defineServer,defineRoom,} from "colyseus";
/**
 * Import your Room files
 */
import {FairSceneRoom} from "./rooms/FairSceneRoom.js";


/**
 * Colyseus server definition for the Country Fair companion platform.
 *
 * Both room handlers point at the same {@link FairSceneRoom}: `my_room` is the name the
 * Colyseus tooling (playground, load tests) expects, while `fairsceneroom` is the name the
 * real clients ask for — the React client in `ClientSide/src/network/client.ts` and the
 * Unity headset build both call `joinOrCreate("fairsceneroom", { platform })`.
 */
const server = defineServer({
    /**
     * Define your room handlers:
     */

    rooms: {
        my_room: defineRoom(FairSceneRoom)

}});

server.define("fairsceneroom", FairSceneRoom);

export default server;
