import { Suspense } from "react";
import { Canvas } from "@react-three/fiber";
import { Plane } from "../GameProps/Plane";
import {GameLight} from "../GameProps/GameLight";
import { Tent } from "../GameProps/Tents/Tent";
import {FerrisWheel} from "../GameProps/FerrisWheel";
import{Camera} from "../GameProps/Camera";
import "./GameScreen.css";

export function GameScreen() {
    return (
            <Canvas>
                <Camera position={[0, 3, 21]} lookAt={[0, 0, 0]} />
                <ambientLight intensity={0.4} />
                <GameLight position={[10, 10, 10]} color="white" intensity={1.5} />
                <Plane position={[0, 0, 0]} size={40} texture="/textures/grass.jpg" textureRepeat={10} />
                <FerrisWheel position={[0, 0, -10]} rotation={[0, Math.PI / 2, 0]} scale={0.4} animationSpeed={0.5} />
                <Tent position={[-4.2, 0.05, 13]} rotation={[0, Math.PI / 6, 0]} />
                <Tent position={[-1.6, 0.05, 13]} rotation={[0, Math.PI / 12, 0]} />
                <Tent position={[1, 0.05, 13]} rotation={[0,  Math.PI/ 200, 0]} />
                <Tent position={[3.6, 0.05, 13]} rotation={[0,- Math.PI/8, 0]} />

            </Canvas>
    );
}
