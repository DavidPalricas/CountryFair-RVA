import {defineServer,defineRoom,} from "colyseus";
/**
 * Import your Room files
 */
import {FairSceneRoom} from "./rooms/FairSceneRoom.js";


const server = defineServer({
    /**
     * Define your room handlers:
     */

    rooms: {
        my_room: defineRoom(FairSceneRoom)
    }
});

export default server;