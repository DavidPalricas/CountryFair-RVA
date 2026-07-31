import { useMemo } from "react";
import { useGLTF } from "@react-three/drei";
import{TentBg} from "./TentBg";
import { FairWorker } from "../FairWorker";


const TENT_MODEL = "/models/Tent.glb";

// The scale factor to apply to the tent model. Adjust this value based on the actual size of your model.
const MODEL_SCALE = 0.002;

type TentProps = {
    position: [number, number, number];
    rotation?: [number, number, number];
    scale?: number;
};

export function Tent({ position = [0, 0, 0], rotation = [0, 0, 0], scale = 1 }: TentProps) {
    const { scene } = useGLTF(TENT_MODEL);

    const model = useMemo(() => scene.clone(), [scene]);

    return (
        <group position={position} rotation={rotation} scale={scale}>
            <primitive object={model} scale={MODEL_SCALE} />
            <TentBg position={[0, 0.5, 0]} dimensions={[1.4, 1, 1.3]} />
            <FairWorker position={[-1, 0, 1]} rotation={[0, 0, 0]} scale={0.35} />
        </group>
    );
}


useGLTF.preload(TENT_MODEL);
