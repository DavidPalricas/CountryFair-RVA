import {Tent} from "./Tent";
import {ArcheryProps} from "../MiniGamesProps/ArcheryProps";
import { FrisbeeProps } from "../MiniGamesProps/FrisbeeProps";


type MiniGameType = "fishing" | "archery" | "frisbee" | "duckgame";

type MiniGameTentProps = {
    position: [number, number, number];
    rotation?: [number, number, number];
    scale?: number;
    type: MiniGameType;
};


export function MiniGameTent({ position, rotation = [0, 0, 0], scale = 1, type }: MiniGameTentProps) {
    const miniGamePropsPos: [number, number, number] = [0.6, 0, 2];

    return (
        <group position={position} rotation={rotation} scale={scale}>
            <Tent position={[0, 0, 0]} rotation={[0, 0, 0]} scale={1} />
            {type === "frisbee" && <FrisbeeProps position={miniGamePropsPos} />}
            {type === "archery" && <ArcheryProps position={miniGamePropsPos} />}
        </group>
    );
}
