import { useEffect, useLayoutEffect, useRef, useState } from "react";
import { useFrame, type ThreeEvent } from "@react-three/fiber";
import { MathUtils, type Group } from "three";
import {Tent} from "./Tent";
import {ArcheryProps} from "../MiniGamesProps/ArcheryProps";
import { FrisbeeProps } from "../MiniGamesProps/FrisbeeProps";
import { FishingProps } from "../MiniGamesProps/FishingProps";
import { DuckProps } from "../MiniGamesProps/DuckProps";
import { Text3D } from "../Text3D";
import {TentRibbon} from "./TentRibbon";


type MiniGameType = "fishing" | "archery" | "frisbee" | "duckgame";

const HOVER_SCALE = 1.2;

const SCALE_DAMPING = 12;

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
    const groupRef = useRef<Group>(null);
    const [hovered, setHovered] = useState(false);

    useLayoutEffect(() => {
        groupRef.current?.scale.setScalar(scale);
    }, [scale]);

    useEffect(() => {
        if (hovered){
            document.body.style.cursor = "pointer";
            return () => { document.body.style.cursor = "auto"; };
        } 
    }, [hovered]);

    useFrame((_, delta) => {
        const group = groupRef.current;

        if (group === null) {
            return;
        }

        const targetScale = hovered ? scale * HOVER_SCALE : scale;
        const step = 1 - Math.exp(-SCALE_DAMPING * delta);

        group.scale.setScalar(MathUtils.lerp(group.scale.x, targetScale, step));
    });

    /* O R3F entrega o evento a todas as interseccoes ao longo do raio, por isso sem o
       stopPropagation as tendas que ficam atras desta tambem entravam em hover. */
    const handlePointerOver = (event: ThreeEvent<PointerEvent>) => {
        event.stopPropagation();
        setHovered(true);
    };

    return (
        <group
            ref={groupRef}
            name="MiniGameTent"
            position={position}
            rotation={rotation}
            onPointerOver={handlePointerOver}
            onPointerOut={() => setHovered(false)}
        >
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
