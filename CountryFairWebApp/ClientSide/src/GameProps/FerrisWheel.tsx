import { useEffect, useMemo, useRef } from "react";
import { useAnimations, useGLTF } from "@react-three/drei";
import { LoopRepeat } from "three";
import type { Group } from "three";
import { clone as cloneSkinned } from "three/examples/jsm/utils/SkeletonUtils.js";

const Ferris_WHEEL_MODEL = "/models/FerrisWheel.glb";

const MODEL_SCALE = 1;

type FerrisWheelProps = {
    position: [number, number, number];
    rotation?: [number, number, number];
    scale?: number;
    animationSpeed?: number;
};


export function FerrisWheel({
    position,
    rotation = [0, 0, 0],
    scale = 1,
    animationSpeed = 1,
}: FerrisWheelProps) {
    const { scene, animations } = useGLTF(Ferris_WHEEL_MODEL);
    // scene.clone() partilha o esqueleto entre clones: todos os trabalhadores
    // ficariam presos aos ossos do original, no mesmo sitio. SkeletonUtils faz o rebind.
    const model = useMemo(() => cloneSkinned(scene), [scene]);

    // O mixer tem de apontar para este clone, senao anima o modelo original.
    const modelRoot = useRef<Group>(null);
    const { actions, names } = useAnimations(animations, modelRoot);

    useEffect(() => {
        // O modelo traz um clip de rotacao por peca (a roda e cada cabine, que
        // contra-roda para ficar direita), por isso tocam todos em simultaneo.
        const playing = names
            .map((name) => actions[name])
            .filter((action) => action !== null && action !== undefined);

        for (const action of playing) {
            action.reset().setLoop(LoopRepeat, Infinity).play();
            action.timeScale = animationSpeed;
        }

        return () => {
            for (const action of playing) {
                action.stop();
            }
        };
    }, [actions, names, animationSpeed]);

    return (
        <group ref={modelRoot} position={position} rotation={rotation} scale={scale}>
            <primitive object={model} scale={MODEL_SCALE} />
        </group>
    );
}

useGLTF.preload(Ferris_WHEEL_MODEL);
