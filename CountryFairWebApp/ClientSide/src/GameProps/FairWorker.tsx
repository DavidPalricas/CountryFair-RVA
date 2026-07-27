import { useEffect, useMemo, useRef } from "react";
import { useAnimations, useGLTF } from "@react-three/drei";
import { LoopRepeat } from "three";
import type { Group } from "three";
import { clone as cloneSkinned } from "three/examples/jsm/utils/SkeletonUtils.js";

const FAIR_WORKER_MODEL = "/models/CountryFairWorker.glb";

// Este modelo veio do Blender ja em metros (2.3 de altura), ao contrario da
// tenda, que o obj2gltf exportou com 790. Por isso nao precisa de normalizacao.
const MODEL_SCALE = 1;

// Os clips vem do Blender com o prefixo da armature ("Armature|Idle").
type FairWorkerAnimation = "Grounded" | "Idle" | "Jump" | "Sprint" | "Walk";

type FairWorkerProps = {
    position: [number, number, number];
    rotation?: [number, number, number];
    scale?: number;
    animation?: FairWorkerAnimation;
    animationSpeed?: number;
};


export function FairWorker({
    position,
    rotation = [0, 0, 0],
    scale = 1,
    animation = "Idle",
    animationSpeed = 1,
}: FairWorkerProps) {
    const { scene, animations } = useGLTF(FAIR_WORKER_MODEL);
    // scene.clone() partilha o esqueleto entre clones: todos os trabalhadores
    // ficariam presos aos ossos do original, no mesmo sitio. SkeletonUtils faz o rebind.
    const model = useMemo(() => cloneSkinned(scene), [scene]);

    // O mixer tem de apontar para este clone, senao anima o esqueleto original.
    const modelRoot = useRef<Group>(null);
    const { actions, names } = useAnimations(animations, modelRoot);

    useEffect(() => {
        const clipName = names.find((name) => name === animation || name.split("|").pop() === animation);
        const action = clipName ? actions[clipName] : undefined;

        if (!action) {
            console.warn(`FairWorker: animacao "${animation}" nao existe no modelo. Disponiveis: ${names.join(", ")}`);
            return;
        }

        action.reset().setLoop(LoopRepeat, Infinity).fadeIn(0.3).play();
        action.timeScale = animationSpeed;

        // fadeOut em vez de stop() para o crossfade quando a animacao muda.
        return () => {
            action.fadeOut(0.3);
        };
    }, [actions, names, animation, animationSpeed]);

    return (
        <group ref={modelRoot} position={position} rotation={rotation} scale={scale}>
            <primitive object={model} scale={MODEL_SCALE} />
        </group>
    );
}

useGLTF.preload(FAIR_WORKER_MODEL);
