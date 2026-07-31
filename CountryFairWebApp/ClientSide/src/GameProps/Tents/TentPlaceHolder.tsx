import { useState, type RefObject } from "react";
import { useFrame } from "@react-three/fiber";
import type { Vector3 } from "three";
import { Text3D } from "../Text3D";
import { DROP_RADIUS } from "./tentSlots";

type TentPlaceHolderProps = {
    position: readonly [number, number, number];
    number: number;
    /* O ponto de arrasto partilhado; e daqui que sai o destaque do anel sob o ponteiro. */
    dragPoint: RefObject<Vector3>;
};

/* Levanta o anel do chao o suficiente para nao haver z-fighting com o plano da relva. */
const GROUND_OFFSET = 0.02;

/*
  Altura a que o numero flutua acima do anel. Deitado no chao ficava quase ilegivel:
  a camara esta a 1.8 de altura a olhar para 1.5, ou seja quase na horizontal, por isso
  via o texto de esguelha. Fica de pe (sem rotacao, virado para +Z, que e o lado da
  camara) e acima do anel, mas abaixo do topo das tendas (1.58) para nao competir com elas.
*/
const NUMBER_HEIGHT = 0.8;

export function TentPlaceHolder({ position, number, dragPoint }: TentPlaceHolderProps) {
    const [highlighted, setHighlighted] = useState(false);

    useFrame(() => {
        const deltaX = dragPoint.current.x - position[0];
        const deltaZ = dragPoint.current.z - position[2];
        const inside = deltaX * deltaX + deltaZ * deltaZ <= DROP_RADIUS * DROP_RADIUS;

        /* O updater devolve o mesmo valor quando nada muda e o React sai sem renderizar,
           por isso isto nao custa um render por frame. */
        setHighlighted((current) => (current === inside ? current : inside));
    });

    return (
        <group position={[position[0], position[1] + GROUND_OFFSET, position[2]]}>
            {/* O ringGeometry nasce no plano XY, por isso e deitado para ficar no chao.
                depthWrite a false evita que o anel tape o numero desenhado por cima. */}
            <mesh rotation={[-Math.PI / 2, 0, 0]}>
                <ringGeometry args={[DROP_RADIUS * 0.72, DROP_RADIUS, 48]} />
                <meshBasicMaterial
                    color={highlighted ? "#ffd166" : "#ff9700"}
                    transparent
                    opacity={highlighted ? 0.9 : 0.35}
                    depthWrite={false}
                />
            </mesh>

            <Text3D
                position={[0, NUMBER_HEIGHT, 0]}
                fontSize={0.6}
                anchorY="middle"
                color={highlighted ? "#ffd166" : "#ff9700"}
            >
                {String(number)}
            </Text3D>
        </group>
    );
}
