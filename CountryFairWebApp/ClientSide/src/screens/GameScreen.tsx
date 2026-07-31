import { useCallback, useRef, useState } from "react";
import { Canvas } from "@react-three/fiber";
import { Vector3 } from "three";
import { Plane } from "../GameProps/Plane";
import {GameLight} from "../GameProps/GameLight";
import { MiniGameTent } from "../GameProps/Tents/MiniGameTent";
import { TentPlaceHolder } from "../GameProps/Tents/TentPlaceHolder";
import { TENT_SLOTS, type MiniGameType } from "../GameProps/Tents/tentSlots";
import { slotIndexAtPoint, swapIntoSlot, type TentOrder } from "../GameProps/Tents/tentOrder";
import {FerrisWheel} from "../GameProps/FerrisWheel";
import{Camera} from "../GameProps/Camera";
import "./GameScreen.css";

const INITIAL_ORDER: TentOrder = ["fishing", "archery", "frisbee", "duckgame"];

/*
  Este ecra faz o papel do PlaceHolderManager do Unity: e ele que sabe a ordem das
  tendas e quem esta a ser arrastado. O Dictionary<element, placeholder> do Unity aqui
  e so o array `order`, em que o indice do array e o indice do slot.
*/
export function GameScreen() {
    const [order, setOrder] = useState<TentOrder>(INITIAL_ORDER);
    const [dragging, setDragging] = useState<MiniGameType | null>(null);

    /* Partilhado entre a tenda agarrada e os aneis; escrito a cada pointermove, por isso
       vive num ref e nao em estado. */
    const dragPoint = useRef(new Vector3());

    const handleDragEnd = useCallback((type: MiniGameType, x: number, z: number) => {
        const targetSlot = slotIndexAtPoint(x, z);

        /* null = largou na zona morta entre slots; o useFrame da tenda leva-a de volta
           ao slot de origem sozinho, porque a ordem nao mudou. */
        if (targetSlot !== null) {
            setOrder((current) => swapIntoSlot(current, type, targetSlot));
        }

        setDragging(null);
    }, []);

    return (
        <div className="game-screen">

            <h1 className="game-screen__title"> Clique e arraste nas tendas para trocar a ordem delas</h1>

            <Canvas>
                <Camera position={[0, 1.8, 17.5]} lookAt={[0, 1.5, 0]} />
                <ambientLight intensity={0.4} />
                <GameLight position={[10, 10, 10]} color="white" intensity={1.5} />
                <Plane position={[0, 0, 0]} size={40} texture="/textures/grass.jpg" textureRepeat={10} />
                <FerrisWheel position={[0, 0, -10]} rotation={[0, Math.PI / 2, 0]} scale={0.4} animationSpeed={0.5} />

                {/*
                  A key e o tipo de mini-jogo e nao o slot: e isso que faz a identidade do
                  componente seguir a tenda quando a ordem muda (em vez de o React reciclar
                  o componente do slot e recarregar os modelos). E o equivalente a casar por
                  miniGame no OnOtherManagerUpdate do Unity.
                */}
                {order.map((type, slot) => (
                    <MiniGameTent
                        key={type}
                        type={type}
                        slot={slot}
                        isDragging={dragging === type}
                        hidden={dragging !== null && dragging !== type}
                        dragPoint={dragPoint}
                        onDragStart={setDragging}
                        onDragEnd={handleDragEnd}
                    />
                ))}

                {dragging !== null && TENT_SLOTS.map((slot) => (
                    <TentPlaceHolder
                        key={slot.number}
                        position={slot.position}
                        number={slot.number}
                        dragPoint={dragPoint}
                    />
                ))}

            </Canvas>
        </div>
    );
}
