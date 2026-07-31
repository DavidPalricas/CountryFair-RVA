import {Tent} from "./Tent";
import {ArcheryProps} from "../MiniGamesProps/ArcheryProps";
import { FrisbeeProps } from "../MiniGamesProps/FrisbeeProps";
import { FishingProps } from "../MiniGamesProps/FishingProps";
import { DuckProps } from "../MiniGamesProps/DuckProps";
import { Text3D } from "../Text3D";
import {TentRibbon} from "./TentRibbon";


type MiniGameType = "fishing" | "archery" | "frisbee" | "duckgame";

const TENT_NAMES: Record<MiniGameType, string> = {
    fishing: "Tenda da Pesca",
    archery: "Tenda de Arco e Flecha",
    frisbee: "Tenda do Frisbee",
    duckgame: "Tenda do Jogo dos Patos",
};

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
            {/* A tenda tem 1.58 de altura, por isso o letreiro assenta em 1.7.
                maxWidth mantem o texto dentro da largura da tenda (1.63). */}
            <Text3D position={[0, 1.7, 0]}>
                {TENT_NAMES[type]}
            </Text3D>

            <TentRibbon position={[-1.2, 1.7, 0]}/>
            <Tent position={[0, 0, 0]}/>
            {type === "fishing" && <FishingProps position={miniGamePropsPos} />}
            {type === "archery" && <ArcheryProps position={miniGamePropsPos} />}
            {type === "frisbee" && <FrisbeeProps position={miniGamePropsPos} />}
            {type === "duckgame" && <DuckProps position={miniGamePropsPos} />}
           
        </group>
    );
}
